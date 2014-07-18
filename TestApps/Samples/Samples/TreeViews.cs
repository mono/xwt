// 
// TreeViews.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2012 Xamarin Inc
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
using Xwt;

namespace Samples
{
	public class TreeViews: VBox
	{
		DataField<CheckBoxState> triState = new DataField<CheckBoxState>();
		DataField<bool> check = new DataField<bool>();
		DataField<string> text = new DataField<string> ();
		DataField<string> desc = new DataField<string> ();
		
		public TreeViews ()
		{
			TreeView view = new TreeView ();
			TreeStore store = new TreeStore (triState, check, text, desc);
		
			var triStateCellView = new CheckBoxCellView (triState) { Editable = true, AllowMixed = true };
			triStateCellView.Toggled += (object sender, WidgetEventArgs e) => {
				if (view.CurrentEventRow == null) {
					MessageDialog.ShowError("CurrentEventRow is null. This is not supposed to happen");
				}
				else {
					store.GetNavigatorAt(view.CurrentEventRow).SetValue(text, "TriState Toggled");
				}
			};
			var checkCellView = new CheckBoxCellView (check) { Editable = true };
			checkCellView.Toggled += (object sender, WidgetEventArgs e) => {
				if (view.CurrentEventRow == null) {
					MessageDialog.ShowError("CurrentEventRow is null. This is not supposed to happen");
				}
				else {
					store.GetNavigatorAt(view.CurrentEventRow).SetValue(text, "Toggled");
				}
			};
			view.Columns.Add ("TriCheck", triStateCellView);
			view.Columns.Add ("Check", checkCellView);
			view.Columns.Add ("Item", text);
			view.Columns.Add ("Desc", desc);
			
			store.AddNode ().SetValue (text, "One").SetValue (desc, "First").SetValue (triState, CheckBoxState.Mixed);
			store.AddNode ().SetValue (text, "Two").SetValue (desc, "Second").AddChild ()
				.SetValue (text, "Sub two").SetValue (desc, "Sub second");
			store.AddNode ().SetValue (text, "Three").SetValue (desc, "Third").AddChild ()
				.SetValue (text, "Sub three").SetValue (desc, "Sub third");
			PackStart (view, true);
			
			view.DataSource = store;
			
			Label la = new Label ();
			PackStart (la);
			
			view.SetDragDropTarget (DragDropAction.All, TransferDataType.Text);
			view.SetDragSource (DragDropAction.All, TransferDataType.Text);
			
			view.DragDrop += delegate(object sender, DragEventArgs e) {
				TreePosition node;
				RowDropPosition pos;
				view.GetDropTargetRow (e.Position.X, e.Position.Y, out pos, out node);
				var nav = store.GetNavigatorAt (node);
				la.Text += "Dropped \"" + e.Data.Text + "\" into \"" + nav.GetValue (text) + "\" " + pos + "\n";
				e.Success = true;
			};
			view.DragOver += delegate(object sender, DragOverEventArgs e) {
				TreePosition node;
				RowDropPosition pos;
				view.GetDropTargetRow (e.Position.X, e.Position.Y, out pos, out node);
				if (pos == RowDropPosition.Into)
					e.AllowedAction = DragDropAction.None;
				else
					e.AllowedAction = e.Action;
			};
			view.DragStarted += delegate(object sender, DragStartedEventArgs e) {
				var val = store.GetNavigatorAt (view.SelectedRow).GetValue (text);
				e.DragOperation.Data.AddValue (val);
				e.DragOperation.Finished += delegate(object s, DragFinishedEventArgs args) {
					Console.WriteLine ("D:" + args.DeleteSource);
				};
			};
			
			Button addButton = new Button ("Add");
			addButton.Clicked += delegate(object sender, EventArgs e) {
				var n = store.AddNode ().SetValue (text, "Added").SetValue (desc, "Desc");
				view.ScrollToRow (n.CurrentPosition);
			};
			PackStart (addButton);
			
			Button removeButton = new Button ("Remove Selection");
			removeButton.Clicked += delegate(object sender, EventArgs e) {
				foreach (TreePosition row in view.SelectedRows) {
					store.GetNavigatorAt (row).Remove ();
				}
			};
			PackStart (removeButton);

			var label = new Label ();
			PackStart (label);

			view.RowExpanded += (sender, e) => label.Text = "Row expanded: " + store.GetNavigatorAt (e.Position).GetValue (text);
		}

		void HandleDragOver (object sender, DragOverEventArgs e)
		{
			e.AllowedAction = e.Action;
		}
	}
}

