//
// CheckBoxTableCell.cs
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
using Xwt.Backends;

namespace Xwt.Mac
{
	class CheckBoxTableCell: NSButtonCell, ICellRenderer
	{
		bool visible = true;

		public CheckBoxTableCell ()
		{
			SetButtonType (NSButtonType.Switch);
			Activated += HandleActivated;
			Title = "";
		}

		void HandleActivated (object sender, EventArgs e)
		{
			var cellView = Frontend;
			CellContainer.SetCurrentEventRow ();
			if (cellView.Editable && !cellView.RaiseToggled () && (cellView.StateField != null || cellView.ActiveField != null)) {
				if (cellView.StateField != null)
					CellContainer.SetValue (cellView.StateField, State.ToXwtState ());
				else if (cellView.ActiveField != null)
					CellContainer.SetValue (cellView.ActiveField, State != NSCellStateValue.Off);
			}
		}

		public CheckBoxTableCell (IntPtr p): base (p)
		{
		}

		ICheckBoxCellViewFrontend Frontend {
			get { return (ICheckBoxCellViewFrontend) Backend.Frontend; }
		}

		public CellViewBackend Backend { get; set; }

		public CompositeCell CellContainer { get; set; }

		public void Fill ()
		{
			var cellView = Frontend;
			AllowsMixedState = cellView.AllowMixed || cellView.State == CheckBoxState.Mixed;
			State = cellView.State.ToMacState ();
			Editable = cellView.Editable;
			visible = cellView.Visible;
		}

		public override CoreGraphics.CGSize CellSizeForBounds (CoreGraphics.CGRect bounds)
		{
			if (visible)
				return base.CellSizeForBounds (bounds);
			return CoreGraphics.CGSize.Empty;
		}

		public override void DrawInteriorWithFrame (CoreGraphics.CGRect cellFrame, NSView inView)
		{
			if (visible)
				base.DrawInteriorWithFrame (cellFrame, inView);
		}

		public void CopyFrom (object other)
		{
			var ob = (CheckBoxTableCell)other;
			Backend = ob.Backend;
		}
	}
}

