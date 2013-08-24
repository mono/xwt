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
using MonoMac.AppKit;
using Xwt.Backends;

namespace Xwt.Mac
{
	class CheckBoxTableCell: NSButtonCell, ICellRenderer
	{
		ICheckBoxCellViewFrontend cellView;

		public CheckBoxTableCell ()
		{
			SetButtonType (NSButtonType.Switch);
			Activated += HandleActivated;
			Title = "";
		}

		void HandleActivated (object sender, EventArgs e)
		{
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

		public CheckBoxTableCell (ICheckBoxCellViewFrontend cellView): this ()
		{
			this.cellView = cellView;
		}

		public ICellViewFrontend Frontend {
			get { return cellView; }
		}

		public CompositeCell CellContainer { get; set; }

		public void Fill ()
		{
			AllowsMixedState = cellView.AllowMixed || cellView.State == CheckBoxState.Mixed;
			State = cellView.State.ToMacState ();
			Editable = cellView.Editable;
		}

		public void CopyFrom (object other)
		{
			var ob = (CheckBoxTableCell)other;
			cellView = ob.cellView;
		}

		public override void EditWithFrame (System.Drawing.RectangleF aRect, NSView inView, NSText editor, MonoMac.Foundation.NSObject delegateObject, NSEvent theEvent)
		{
			base.EditWithFrame (aRect, inView, editor, delegateObject, theEvent);
		}

		public override void DidChangeValue (string forKey)
		{
			base.DidChangeValue (forKey);
		}

		public override void WillChange (MonoMac.Foundation.NSString forKey, MonoMac.Foundation.NSKeyValueSetMutationKind mutationKind, MonoMac.Foundation.NSSet objects)
		{
			base.WillChange (forKey, mutationKind, objects);
		}

		public override void WillChangeValue (string forKey)
		{
			base.WillChangeValue (forKey);
		}

		public override void EndEditing (NSText textObj)
		{
			base.EndEditing (textObj);
		}

		public override bool CommitEditing ()
		{
			return base.CommitEditing ();
		}
	}
}

