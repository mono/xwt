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
using Xwt.Engine;

namespace Xwt.GtkBackend
{
	public class PopoverBackend : IPopoverBackend
	{
		class PopoverWindow : Gtk.Window
		{
			const int arrowPadding = 10;
			const int radius = 6;
			
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
				this.alignment = new Gtk.Alignment (0, 0, 1, 1);
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
			
			public void SetPadding (WidgetSpacing spacing)
			{
				alignment.LeftPadding = radius + (uint) spacing.Left;
				alignment.RightPadding = radius + (uint) spacing.Right;
				if (arrowPosition == Xwt.Popover.Position.Top) {
					alignment.TopPadding = radius + arrowPadding + (uint) spacing.Top;
					alignment.BottomPadding = radius + (uint) spacing.Bottom;
				} else {
					alignment.BottomPadding = radius + arrowPadding + (uint) spacing.Bottom;
					alignment.TopPadding = radius + (uint) spacing.Top;
				}
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
				var bounds = new Xwt.Rectangle (0.5, 0.5, w - 1, h - 1);
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
					RoundRectangle (ctx, calibratedRect, radius);
					ctx.LineWidth = 1;
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
		IPopoverEventSink sink;
		Popover frontend;

		public void Initialize (IPopoverEventSink sink)
		{
			this.sink = sink;
		}

		public void InitializeBackend (object frontend, ApplicationContext context)
		{
			this.frontend = (Popover) frontend;
		}
		
		public void EnableEvent (object eventId)
		{
		}
		
		public void DisableEvent (object eventId)
		{
		}

		public void Show (Xwt.Popover.Position orientation, Xwt.Widget reference, Xwt.Rectangle positionRect, Widget child)
		{
			var parent = reference.ParentWindow;
			popover = new PopoverWindow ((Gtk.Widget)((WidgetBackend)Xwt.Engine.Toolkit.GetBackend (child)).NativeWidget, orientation);
			popover.SetPadding (frontend.Padding);
			popover.TransientFor = ((WindowFrameBackend)Xwt.Engine.Toolkit.GetBackend (parent)).Window;
			popover.DestroyWithParent = true;
			popover.Hidden += (o, args) => {
				popover.ReleaseInnerWidget ();
				sink.OnClosed ();
				popover.Destroy ();
			};
			
			var position = new Point (reference.ScreenBounds.Center.X, popover.ArrowPosition == Popover.Position.Top ? reference.ScreenBounds.Bottom : reference.ScreenBounds.Top);
			popover.ShowAll ();
			popover.GrabFocus ();
			int w, h;
			popover.GetSize (out w, out h);
			popover.Move ((int)position.X - w / 2, (int)position.Y);
			popover.SizeAllocated += (o, args) => { popover.Move ((int)position.X - args.Allocation.Width / 2, (int)position.Y); popover.GrabFocus (); };
		}

		public void Hide ()
		{
			popover.Hide ();
		}
		
		public void Dispose ()
		{
		}
	}
}