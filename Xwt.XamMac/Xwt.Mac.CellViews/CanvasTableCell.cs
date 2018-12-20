//
// CanvasTableCell.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc.
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
using AppKit;
using CoreGraphics;
using Xwt.Backends;

namespace Xwt.Mac
{
	class CanvasTableCell: NSView, ICanvasCellRenderer
	{
		NSTrackingArea trackingArea;

		public CompositeCell CellContainer { get; set; }

		public CellViewBackend Backend { get; set; }

		public NSView CellView { get { return this; } }

		// Since 10.12 or 10.13 views inside tables flip on data reload.
		// IsFlipped enforces the correct orientation of the layer.
		public override bool IsFlipped { get { return true; } }

		public void CopyFrom (object other)
		{
			var ob = (CanvasTableCell)other;
			Backend = ob.Backend;
		}

		public void Fill ()
		{
			Hidden = !Frontend.Visible;
			this.ApplyAcessibilityProperties ();
		}
		
		ICanvasCellViewFrontend Frontend {
			get { return (ICanvasCellViewFrontend) Backend.Frontend; }
		}

		public override CGSize FittingSize {
			get {
				var size = CGSize.Empty;
				Frontend.ApplicationContext.InvokeUserCode (delegate {
					var s = Frontend.GetRequiredSize (SizeConstraint.Unconstrained);
					size = new CGSize ((nfloat)s.Width, (nfloat)s.Height);
				});
				return size;
			}
		}

		public Size GetRequiredSize(SizeConstraint widthConstraint)
		{
			var size = Size.Zero;
			Frontend.ApplicationContext.InvokeUserCode (delegate {
				size = Frontend.GetRequiredSize (widthConstraint);
			});
			return size;
		}

		public override void DrawRect (CGRect dirtyRect)
		{
			Backend.Load (this);
			Frontend.ApplicationContext.InvokeUserCode (delegate {
				CGContext ctx = NSGraphicsContext.CurrentContext.GraphicsPort;

				var backend = new CGContextBackend {
					Context = ctx,
					InverseViewTransform = ctx.GetCTM ().Invert ()
				};
				var bounds = Backend.CellBounds;
				backend.Context.ClipToRect (dirtyRect);
				backend.Context.TranslateCTM ((nfloat)(-bounds.X), (nfloat)(-bounds.Y));
				Frontend.Draw (backend, new Rectangle (bounds.X, bounds.Y, bounds.Width, bounds.Height));
			});
		}

		public override void UpdateTrackingAreas ()
		{
			if (trackingArea != null) {
				RemoveTrackingArea (trackingArea);
				trackingArea.Dispose ();
			}
			var options = NSTrackingAreaOptions.MouseMoved | NSTrackingAreaOptions.ActiveInKeyWindow | NSTrackingAreaOptions.MouseEnteredAndExited;
			trackingArea = new NSTrackingArea (Bounds, options, this, null);
			AddTrackingArea (trackingArea);
		}

		public override void RightMouseDown (NSEvent theEvent)
		{
			if (!this.HandleMouseDown (theEvent))
				base.RightMouseDown (theEvent); 
		}

		public override void RightMouseUp (NSEvent theEvent)
		{
			if (!this.HandleMouseUp (theEvent))
				base.RightMouseUp (theEvent); 
		}

		public override void MouseDown (NSEvent theEvent)
		{
			if (!this.HandleMouseDown (theEvent))
				base.MouseDown (theEvent); 
		}

		public override void MouseUp (NSEvent theEvent)
		{
			if (!this.HandleMouseUp (theEvent))
				base.MouseUp (theEvent); 
		}

		public override void OtherMouseDown (NSEvent theEvent)
		{
			if (!this.HandleMouseDown (theEvent))
				base.OtherMouseDown (theEvent);
		}

		public override void OtherMouseUp (NSEvent theEvent)
		{
			if (!this.HandleMouseUp (theEvent))
				base.OtherMouseUp (theEvent);
		}

		public override void MouseEntered (NSEvent theEvent)
		{
			this.HandleMouseEntered (theEvent);
				base.MouseEntered (theEvent);
		}

		public override void MouseExited (NSEvent theEvent)
		{
			this.HandleMouseExited (theEvent);
				base.MouseExited (theEvent);
		}

		public override void MouseMoved (NSEvent theEvent)
		{
			if (!this.HandleMouseMoved (theEvent))
				base.MouseMoved (theEvent);
		}

		public override void MouseDragged (NSEvent theEvent)
		{
			if (!this.HandleMouseMoved (theEvent))
				base.MouseDragged (theEvent);
		}

		public override void KeyDown (NSEvent theEvent)
		{
			if (!this.HandleKeyDown (theEvent))
				base.KeyDown (theEvent);
		}

		public override void KeyUp (NSEvent theEvent)
		{
			if (!this.HandleKeyUp (theEvent))
				base.KeyUp (theEvent);
		}
	}
}

