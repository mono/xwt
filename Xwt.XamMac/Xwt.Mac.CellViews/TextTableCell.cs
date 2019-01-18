// 
// TextTableCell.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2011 Xamarin Inc
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

using AppKit;
using Foundation;
using Xwt.Backends;

namespace Xwt.Mac
{
	class TextTableCell : NSTextField, ICellRenderer
	{
		NSTrackingArea trackingArea;
		public TextTableCell ()
		{
			Editable = false;
			Bezeled = false;
			DrawsBackground = false;
		}
		
		ITextCellViewFrontend Frontend {
			get { return (ITextCellViewFrontend) Backend.Frontend; }
		}

		public override bool AllowsVibrancy {
			get {
				// we don't support vibrancy
				if (EffectiveAppearance.AllowsVibrancy)
					return false;
				return base.AllowsVibrancy;
			}
		}
		
		public CellViewBackend Backend { get; set; }

		public CompositeCell CellContainer { get; set; }

		public NSView CellView { get { return this; } }

		public void Fill ()
		{
			if (Frontend.Markup != null)
				AttributedStringValue = FormattedText.FromMarkup (Frontend.Markup).ToAttributedString ();
			else
				StringValue = Frontend.Text ?? "";
			Hidden = !Frontend.Visible;
			this.ApplyAcessibilityProperties ();
		}
		
		public virtual NSBackgroundStyle BackgroundStyle {
			[Export ("backgroundStyle")]
			get {
				return Cell.BackgroundStyle;
			}
			[Export ("setBackgroundStyle:")]
			set {
				Cell.BackgroundStyle = value;
			}
		}

		public void CopyFrom (object other)
		{
			var ob = (TextTableCell)other;
			Backend = ob.Backend;
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
