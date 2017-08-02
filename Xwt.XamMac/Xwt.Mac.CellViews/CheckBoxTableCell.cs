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
using Foundation;
using Xwt.Backends;

namespace Xwt.Mac
{
	class CheckBoxTableCell: NSButton, ICellRenderer
	{
		public CheckBoxTableCell ()
		{
			SetButtonType (NSButtonType.Switch);
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
			var nextState = State; // store new state internally set by Cocoa
			base.State = cellView.State.ToMacState (); // reset state to previous state from store
			CellContainer.SetCurrentEventRow ();
			if (!cellView.RaiseToggled ()) {
				if (cellView.StateField != null)
					CellContainer.SetValue (cellView.StateField, nextState.ToXwtState ());
				else if (cellView.ActiveField != null)
					CellContainer.SetValue (cellView.ActiveField, nextState != NSCellStateValue.Off);
			}
		}

		NSCellStateValue GetNextState ()
		{
			if (!AllowsMixedState) {
				switch (State) {
					case NSCellStateValue.Off:
					case NSCellStateValue.Mixed:
						return NSCellStateValue.On;
					default: return NSCellStateValue.Off;
				}
			} else {
				switch (State) {
					case NSCellStateValue.Off:
						return NSCellStateValue.Mixed;
					case NSCellStateValue.Mixed:
						return NSCellStateValue.On;
					default: return NSCellStateValue.Off;
				}
			}
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
			base.State = cellView.State.ToMacState ();
			Enabled = cellView.Editable;
			Hidden = !cellView.Visible;
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
			var ob = (CheckBoxTableCell)other;
			Backend = ob.Backend;
		}
	}
}

