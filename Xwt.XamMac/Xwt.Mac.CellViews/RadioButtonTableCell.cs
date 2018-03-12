//
// RadioButtonTableCell.cs
//
// Author:
//       Vsevolod Kukol <sevoku@microsoft.com>
//
// Copyright (c) 2016 Microsoft Corporation
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
using AppKit;
using CoreGraphics;

namespace Xwt.Mac
{
	class RadioButtonTableCell: NSButton, ICellRenderer
	{
		NSTrackingArea trackingArea;

		public RadioButtonTableCell ()
		{
			SetButtonType (NSButtonType.Radio);
			AllowsMixedState = false;
			Activated += HandleActivated;
			Title = string.Empty;
		}

		public override NSCellStateValue State {
			get {
				return base.State;
			}
			set {
				// don't let Cocoa set the state for us
			}
		}

		void HandleActivated (object sender, EventArgs e)
		{
			Backend.Load (this);
			var cellView = Frontend;
			CellContainer.SetCurrentEventRow ();
			if (!cellView.RaiseToggled ()) {
				if (cellView.ActiveField != null)
					CellContainer.SetValue (cellView.ActiveField, true);
			}
		}

		IRadioButtonCellViewFrontend Frontend {
			get { return (IRadioButtonCellViewFrontend)Backend.Frontend; }
		}

		public CellViewBackend Backend { get; set; }

		public CompositeCell CellContainer { get; set; }

		public NSView CellView { get { return this; } }

		static CGSize defaultSize = CGSize.Empty;
		public override CGSize FittingSize {
			get {
				// Radio NSButton has always the same size, measure it only once
				if (defaultSize.IsEmpty)
					defaultSize = base.FittingSize;
				return defaultSize;
			}
		}

		public void Fill ()
		{
			var cellView = Frontend;
			base.State = cellView.Active ? NSCellStateValue.On : NSCellStateValue.Off;
			Enabled = cellView.Editable;
			Hidden = !cellView.Visible;
		}

		public void CopyFrom (object other)
		{
			var ob = (RadioButtonTableCell)other;
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
