//
// PopoverBackend.cs
//
// Author:
//       Jérémie Laval <jeremie.laval@xamarin.com>
//
// Copyright (c) 2012 Xamarin, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;

using Xwt.Backends;

using Gtk;
using Cairo;

namespace Xwt.GtkBackend
{
	public class PopoverBackend : IPopoverBackend
	{
		class PopoverWindow : Gtk.Window
		{
			const int arrowPadding = 20;

			bool supportAlpha;
			Xwt.Popover.Position arrowPosition;
			Gtk.Alignment alignment;

			public PopoverWindow (Gtk.Widget child, Xwt.Popover.Position orientation) : base (WindowType.Toplevel)
			{
				this.AppPaintable = true;
				this.Decorated = false;
				this.SkipPagerHint = true;
				this.SkipTaskbarHint = true;
				this.TypeHint = Gdk.WindowTypeHint.PopupMenu;
				//this.TransientFor = (Gtk.Window)child.Toplevel;
				this.AddEvents ((int)Gdk.EventMask.FocusChangeMask);
				//this.DefaultHeight = this.DefaultWidth = 400;
				this.arrowPosition = orientation;
				this.alignment = SetupAlignment ();
				this.Add (alignment);
				this.alignment.Add (child);
				this.FocusOutEvent += HandleFocusOutEvent;
				OnScreenChanged (null);
			}

			public Xwt.Popover.Position ArrowPosition {
				get {
					return arrowPosition;
				}
			}

			public void ReleaseInnerWidget ()
			{
				alignment.Remove (alignment.Child);
			}

			void HandleFocusOutEvent (object o, FocusOutEventArgs args)
			{
				this.HideAll ();
			}

			Gtk.Alignment SetupAlignment ()
			{
				const int defaultPadding = 20;
				var align = new Gtk.Alignment (0, 0, 1, 1);
				align.LeftPadding = align.RightPadding = defaultPadding;
				if (arrowPosition == Xwt.Popover.Position.Top) {
					align.TopPadding = arrowPadding + defaultPadding;
					align.BottomPadding = defaultPadding;
				} else {
					align.BottomPadding = arrowPadding + defaultPadding;
					align.TopPadding = defaultPadding;
				}

				return align;
			}

			protected override void OnScreenChanged (Gdk.Screen previous_screen)
			{
				// To check if the display supports alpha channels, get the colormap
				var colormap = this.Screen.RgbaColormap;
				if (colormap == null) {
					colormap = this.Screen.RgbColormap;
					supportAlpha = false;
				} else {
					supportAlpha = true;
				}
				this.Colormap = colormap;
				base.OnScreenChanged (previous_screen);
			}

			protected override bool OnExposeEvent (Gdk.EventExpose evnt)
			{
				int w, h;
				this.GdkWindow.GetSize (out w, out h);
				var bounds = new Xwt.Rectangle (5, 5, w - 6, h - 6);
				var backgroundColor = Xwt.Drawing.Color.FromBytes (230, 230, 230, 230);
				var black = Xwt.Drawing.Color.FromBytes (60, 60, 60);

				using (Context ctx = Gdk.CairoHelper.Create (this.GdkWindow)) {
					// We clear the surface with a transparent color if possible
					if (supportAlpha)
						ctx.Color = new Color (1.0, 1.0, 1.0, 0.0);
					else
						ctx.Color = new Color (1.0, 1.0, 1.0);
					ctx.Operator = Operator.Source;
					ctx.Paint ();

					var calibratedRect = RecalibrateChildRectangle (bounds);
					// Fill it with one round rectangle
					RoundRectangle (ctx, calibratedRect, 15);
					ctx.LineWidth = .8;
					ctx.Color = new Color (black.Red, black.Green, black.Blue, black.Alpha);
					ctx.StrokePreserve ();
					ctx.Color = new Color (backgroundColor.Red, backgroundColor.Green, backgroundColor.Blue, backgroundColor.Alpha);
					ctx.Fill ();

					// Triangle
					// We first begin by positionning ourselves at the top-center or bottom center of the previous rectangle
					var arrowX = bounds.Center.X;
					var arrowY = arrowPosition == Xwt.Popover.Position.Top ? calibratedRect.Top + ctx.LineWidth : calibratedRect.Bottom - ctx.LineWidth;
					ctx.NewPath ();
					ctx.MoveTo (arrowX, arrowY);
					// We draw the rectangle path
					DrawTriangle (ctx);
					// We use it
					ctx.Color = new Color (black.Red, black.Green, black.Blue, black.Alpha);
					ctx.StrokePreserve ();
					ctx.ClosePath ();
					ctx.Color = new Color (backgroundColor.Red, backgroundColor.Green, backgroundColor.Blue, backgroundColor.Alpha);
					ctx.Fill ();
				}

				base.OnExposeEvent (evnt);
				return false;
			}
			
			void DrawTriangle (Context ctx)
			{
				var triangleSide = 2 * arrowPadding / Math.Sqrt (3);
				var halfSide = triangleSide / 2;
				var verticalModifier = arrowPosition == Xwt.Popover.Position.Top ? -1 : 1;
				// Move to the left
				ctx.RelMoveTo (-halfSide, 0);
				ctx.RelLineTo (halfSide, verticalModifier * arrowPadding);
				ctx.RelLineTo (halfSide, verticalModifier * -arrowPadding);
			}

			void RoundRectangle (Context ctx, Rectangle rect, double radius)
			{
				radius = rect.Height / radius;
				double degrees = Math.PI / 180;
				var x = rect.X;
				var y = rect.Y;
				var height = rect.Height;
				var width = rect.Width;

				ctx.NewSubPath ();
				ctx.Arc (x + width - radius, y + radius, radius, -90 * degrees, 0 * degrees);
				ctx.Arc (x + width - radius, y + height - radius, radius, 0 * degrees, 90 * degrees);
				ctx.Arc (x + radius, y + height - radius, radius, 90 * degrees, 180 * degrees);
				ctx.Arc (x + radius, y + radius, radius, 180 * degrees, 270 * degrees);
				ctx.ClosePath ();
			}

			Xwt.Rectangle RecalibrateChildRectangle (Xwt.Rectangle bounds)
			{
				switch (arrowPosition) {
				case Xwt.Popover.Position.Top:
					return new Rectangle (bounds.X, bounds.Y + arrowPadding, bounds.Width, bounds.Height - arrowPadding);
				case Xwt.Popover.Position.Bottom:
					return new Rectangle (bounds.X, bounds.Y, bounds.Width, bounds.Height - arrowPadding);
				}
				return bounds;
			}
		}

		PopoverWindow popover;
		public event EventHandler Closed;

		public Xwt.Engine.WidgetRegistry PreferredRegistry {
			get {
				return GtkEngine.Registry;
			}
		}

		public void Run (Xwt.WindowFrame parent, Xwt.Popover.Position orientation, Func<Xwt.Widget> childSource, Xwt.Widget reference)
		{
			var child = childSource ();
			popover = new PopoverWindow ((Gtk.Widget)((WidgetBackend)GtkEngine.Registry.GetBackend (child)).NativeWidget, orientation);
			popover.TransientFor = ((WindowFrameBackend)GtkEngine.Registry.GetBackend (parent)).Window;
			popover.DestroyWithParent = true;
			popover.Hidden += (o, args) => {
				popover.ReleaseInnerWidget ();
				if (Closed != null)
					Closed (this, EventArgs.Empty);
			};

			var position = new Point (reference.ScreenBounds.Center.X, popover.ArrowPosition == Popover.Position.Top ? reference.ScreenBounds.Bottom : reference.ScreenBounds.Top);
			popover.ShowAll ();
			popover.GrabFocus ();
			int w, h;
			popover.GetSize (out w, out h);
			popover.Move ((int)position.X - w / 2, (int)position.Y);
			popover.SizeAllocated += (o, args) => { popover.Move ((int)position.X - args.Allocation.Width / 2, (int)position.Y); popover.GrabFocus (); };
		}

		public void Dispose ()
		{

		}
	}
}