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
using Xwt.CairoBackend;

using Gtk;
using Cairo;


namespace Xwt.GtkBackend
{
	public class PopoverBackend : IPopoverBackend
	{
		public sealed class PopoverWindow : GtkPopoverWindow
		{
			const int arrowPadding = 10;
			const int radius = 6;

			Popover.Position arrowPosition;
			WidgetSpacing padding;
			Gtk.Alignment alignment;
			int arrowDelta;

			public Color BackgroundColor { get; set; }

			public Xwt.Popover.Position ArrowPosition {
				get {
					return arrowPosition;
				}
				set {
					arrowPosition = value;
					Padding = padding;
					QueueDraw ();
				}
			}

			public int ArrowDelta {
				get { return arrowDelta; }
				set {
					if (arrowDelta == value)
						return;
					arrowDelta = value;
					QueueDraw ();
				}
			}

			public WidgetSpacing Padding {
				get {
					return padding;
				}
				set {
					padding = value;
					alignment.LeftPadding = radius + (uint) padding.Left;
					alignment.RightPadding = radius + (uint) padding.Right;
					if (arrowPosition == Xwt.Popover.Position.Top) {
						alignment.TopPadding = radius + arrowPadding + (uint) padding.Top;
						alignment.BottomPadding = radius + (uint) padding.Bottom;
					} else {
						alignment.BottomPadding = radius + arrowPadding + (uint) padding.Bottom;
						alignment.TopPadding = radius + (uint) padding.Top;
					}
				}
			}

			public Gtk.Widget Content {
				get {
					return alignment.Child;
				}
				set {
					if (alignment.Child != value) {
						if (alignment.Child != null)
							alignment.Remove (alignment.Child);
						if (value != null)
							alignment.Add (value);
					}
				}
			}
			
			public PopoverWindow () : base (WindowType.Toplevel)
			{
				this.AppPaintable = true;
				this.Decorated = false;
				this.SkipPagerHint = true;
				this.SkipTaskbarHint = true;
				this.TypeHint = Gdk.WindowTypeHint.PopupMenu;
				this.DestroyWithParent = true;
				this.AddEvents ((int)Gdk.EventMask.FocusChangeMask);
				this.alignment = new Gtk.Alignment (0, 0, 1, 1);
				this.alignment.Show ();
				this.Add (alignment);

				OnScreenChanged (null);
			}

			protected override void OnSizeAllocated (Gdk.Rectangle allocation)
			{
				base.OnSizeAllocated (allocation);
				QueueDraw ();
			}


			protected override bool OnDrawn (Context cr)
			{
				int w, h;
				this.GdkWindow.GetSize (out w, out h);
				
				// We clear the surface with a transparent color if possible
				if (supportAlpha)
					cr.SetSourceRGBA (1.0, 1.0, 1.0, 0.0);
				else
					cr.SetSourceRGB (1.0, 1.0, 1.0);
				cr.Operator = Operator.Source;
				cr.Paint ();

				cr.LineWidth = GtkWorkarounds.GetScaleFactor (Content) > 1 ? 2 : 1;
				var bounds = new Xwt.Rectangle (cr.LineWidth / 2, cr.LineWidth / 2, w - cr.LineWidth, h - cr.LineWidth);
				var calibratedRect = RecalibrateChildRectangle (bounds);
				// Fill it with one round rectangle
				RoundRectangle (cr, calibratedRect, radius);
				
				// Triangle
				// We first begin by positionning ourselves at the top-center or bottom center of the previous rectangle
				var arrowX = bounds.Center.X + arrowDelta;
				var arrowY = arrowPosition == Xwt.Popover.Position.Top ? calibratedRect.Top + cr.LineWidth : calibratedRect.Bottom;
				cr.MoveTo (arrowX, arrowY);
				// We draw the rectangle path
				DrawTriangle (cr);

				// We use it
				if (supportAlpha)
					cr.SetSourceRGBA (0.0, 0.0, 0.0, 0.2);
				else
					cr.SetSourceRGB (238d / 255d, 238d / 255d, 238d / 255d);
				cr.StrokePreserve ();
				cr.SetSourceRGBA (BackgroundColor.R, BackgroundColor.G, BackgroundColor.B, BackgroundColor.A);
				cr.Fill ();

				return base.OnDrawn (cr);
			}
			
			void DrawTriangle (Context ctx)
			{
				var halfSide = arrowPadding;
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

		public Xwt.Drawing.Color BackgroundColor { get; set; }

		public PopoverWindow Popover {
			get { return popover; }
			protected set { popover = value; }
		}

		public virtual void Initialize (IPopoverEventSink sink)
		{
			this.sink = sink;
			this.BackgroundColor = Xwt.Drawing.Color.FromBytes (0xee, 0xee, 0xee, 0xf9);
			this.popover = new PopoverWindow ();
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
			popover.Content = (Gtk.Widget)((WidgetBackend)Toolkit.GetBackend (child)).NativeWidget;
			popover.ArrowPosition = orientation;
			popover.BackgroundColor = BackgroundColor.ToCairoColor ();
			popover.Padding = frontend.Padding;

			var parent = (WindowFrameBackend)Toolkit.GetBackend (reference.ParentWindow);
			if (popover.TransientFor != parent.Window) {
				if (popover.TransientFor != null)
					popover.TransientFor.FocusInEvent -= HandleParentFocusInEvent;
				popover.TransientFor = parent.Window;
				popover.TransientFor.FocusInEvent += HandleParentFocusInEvent;
			}

			popover.Hidden += (o, args) => sink.OnClosed ();

			var screenBounds = reference.ScreenBounds;
			if (positionRect == Rectangle.Zero)
				positionRect = new Rectangle (Point.Zero, screenBounds.Size);
			positionRect = positionRect.Offset (screenBounds.Location);
			popover.Show ();
			popover.Present ();
			popover.GrabFocus ();
			int w, h;
			popover.GetSize (out w, out h);

			UpdatePopoverPosition (positionRect, w, h);
			popover.SizeAllocated += (o, args) => {
				UpdatePopoverPosition (positionRect, args.Allocation.Width, args.Allocation.Height);
				popover.GrabFocus ();
			};
		}

		void UpdatePopoverPosition (Rectangle positionRect, int width, int height)
		{
			var position = new Point (positionRect.Center.X, popover.ArrowPosition == Xwt.Popover.Position.Top ? positionRect.Bottom : positionRect.Top);
			var x = (int)position.X - width / 2;
			int wx, wy, ww, wh;
			popover.TransientFor.GetSize (out ww, out wh);
			popover.TransientFor.GetPosition (out wx, out wy);

			// If the popover height would overflow, we flip the arrow position if possible
			var arrowPos = popover.ArrowPosition;
			var overflowing = arrowPos == Xwt.Popover.Position.Top ? position.Y + height > wy + wh : position.Y - height < wy;
			var otherOverflow = arrowPos == Xwt.Popover.Position.Top ? position.Y - height < wy : position.Y + height > wy + wh;
			if (overflowing && !otherOverflow) {
				popover.ArrowPosition = arrowPos == Xwt.Popover.Position.Bottom ? Xwt.Popover.Position.Top : Xwt.Popover.Position.Bottom;
				position = new Point (positionRect.Center.X, popover.ArrowPosition == Xwt.Popover.Position.Top ? positionRect.Bottom : positionRect.Top);
			}

			// If the popover width would overflow out of the screen, we balance this
			// by translating and moving the arrow
			var delta = Math.Min (
				Math.Max (0, ((int)position.X + width / 2) - (wx + ww) - 2),
				((int)position.X - width / 2) - wx + 2
			);
			x -= delta;
			popover.ArrowDelta = delta;

			if (popover.ArrowPosition == Xwt.Popover.Position.Top)
				popover.Move (x, (int)position.Y);
			else
				popover.Move (x, (int)position.Y - height);
		}

		void HandleParentFocusInEvent (object o, FocusInEventArgs args)
		{
			Hide ();
		}

		public void Hide ()
		{
			popover.Hide ();
		}
		
		public void Dispose ()
		{
			if (popover.TransientFor != null)
				popover.TransientFor.FocusInEvent -= HandleParentFocusInEvent;

			popover.Destroy ();
			popover.Dispose ();
		}
	}
}