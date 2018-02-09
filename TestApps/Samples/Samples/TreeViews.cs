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
using Xwt.Drawing;

namespace Samples
{
	public class TreeViews: VBox
	{
		DataField<CheckBoxState> triState = new DataField<CheckBoxState>();
		DataField<bool> check = new DataField<bool>();
		DataField<bool> option1 = new DataField<bool> ();
		DataField<bool> option2 = new DataField<bool> ();
		DataField<bool> option3 = new DataField<bool> ();
		DataField<string> text = new DataField<string> ();
		DataField<string> desc = new DataField<string> ();
		
		public TreeViews ()
		{
			TreeView view = new TreeView ();
			TreeStore store = new TreeStore (triState, check, option1, option2, option3, text, desc);
			view.GridLinesVisible = GridLines.Both;
			
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
					store.GetNavigatorAt(view.CurrentEventRow).SetValue(text, "Toggled " + checkCellView.Active);
				}
			};
			var optionCellView1 = new RadioButtonCellView (option1) { Editable = true };
			optionCellView1.Toggled += (object sender, WidgetEventArgs e) => {
				if (view.CurrentEventRow == null) {
					MessageDialog.ShowError ("CurrentEventRow is null. This is not supposed to happen");
				} else {
					store.GetNavigatorAt (view.CurrentEventRow).SetValue (option2, optionCellView1.Active);
				}
			};
			var optionCellView2 = new RadioButtonCellView (option2) { Editable = true };
			optionCellView2.Toggled += (object sender, WidgetEventArgs e) => {
				if (view.CurrentEventRow == null) {
					MessageDialog.ShowError ("CurrentEventRow is null. This is not supposed to happen");
				} else {
					store.GetNavigatorAt (view.CurrentEventRow).SetValue (option1, optionCellView2.Active);
				}
			};

			TreePosition initialActive = null;
			var optionCellView3 = new RadioButtonCellView (option3) { Editable = true };
			optionCellView3.Toggled += (object sender, WidgetEventArgs e) => {
				if (view.CurrentEventRow == null) {
					MessageDialog.ShowError ("CurrentEventRow is null. This is not supposed to happen");
				} else {
					if (initialActive != null)
						store.GetNavigatorAt (initialActive).SetValue (option3, false);
					initialActive = view.CurrentEventRow;
				}
			};

			view.Columns.Add ("TriCheck", triStateCellView);
			view.Columns.Add ("Check", checkCellView);
			view.Columns.Add ("Radio", optionCellView1, optionCellView2, optionCellView3);
			view.Columns.Add ("Item", text);
			view.Columns.Add ("Desc", desc);
			view.Columns[2].Expands = true; // expand third column, aligning last column to the right side
			view.Columns[2].CanResize = true;
			view.Columns[3].CanResize = true;
			
			store.AddNode ().SetValue (text, "One").SetValue (desc, "First").SetValue (triState, CheckBoxState.Mixed);
			store.AddNode ().SetValue (text, "Two").SetValue (desc, "Second").AddChild ()
				.SetValue (text, "Sub two").SetValue (desc, "Sub second");
			store.AddNode ().SetValue (text, "Three").SetValue (desc, "Third").AddChild ()
				.SetValue (text, "Sub three").SetValue (desc, "Sub third");
			PackStart (view, true);

			Menu contextMenu = new Menu ();
			contextMenu.Items.Add (new MenuItem ("Test menu"));
			view.ButtonPressed += delegate(object sender, ButtonEventArgs e) {
				TreePosition tmpTreePos;
				RowDropPosition tmpRowDrop;
				if ((e.Button == PointerButton.Right) && view.GetDropTargetRow (e.X, e.Y, out tmpRowDrop, out tmpTreePos)) {
					// Set actual row to selected
					view.SelectRow (tmpTreePos);
					contextMenu.Popup(view, e.X, e.Y);
				}
			};
				
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
				var img = Image.FromResource(GetType(), "class.png");
				e.DragOperation.SetDragImage(img, (int)img.Size.Width, (int)img.Size.Height);
				e.DragOperation.Finished += delegate(object s, DragFinishedEventArgs args) {
					Console.WriteLine ("D:" + args.DeleteSource);
				};
			};
			view.RowExpanding += delegate(object sender, TreeViewRowEventArgs e) {
				var val = store.GetNavigatorAt (e.Position).GetValue (text);
				Console.WriteLine("Expanding: " + val);
			};
			view.RowExpanded += delegate(object sender, TreeViewRowEventArgs e) {
				var val = store.GetNavigatorAt (e.Position).GetValue (text);
				Console.WriteLine("Expanded: " + val);
			};
			view.RowCollapsing += delegate(object sender, TreeViewRowEventArgs e) {
				var val = store.GetNavigatorAt (e.Position).GetValue (text);
				Console.WriteLine("Collapsing: " + val);
			};
			view.RowCollapsed += delegate(object sender, TreeViewRowEventArgs e) {
				var val = store.GetNavigatorAt (e.Position).GetValue (text);
				Console.WriteLine("Collapsed: " + val);
			};

			RadioButtonGroup group = new RadioButtonGroup ();
			foreach (SelectionMode mode in Enum.GetValues(typeof (SelectionMode))) {
				var radio = new RadioButton (mode.ToString ());
				radio.Group = group;
				radio.Activated += delegate {
					view.SelectionMode = mode;
				};
				PackStart (radio);
			}

			int addCounter = 0;
			view.KeyPressed += (sender, e) => {
				if (e.Key == Key.Insert) {
					TreeNavigator n;
					if (view.SelectedRow != null)
						n = store.InsertNodeAfter (view.SelectedRow).SetValue (text, "Inserted").SetValue (desc, "Desc");
					else
						n = store.AddNode ().SetValue (text, "Inserted").SetValue (desc, "Desc");
					view.ExpandToRow (n.CurrentPosition);
					view.ScrollToRow (n.CurrentPosition);
					view.UnselectAll ();
					view.SelectRow (n.CurrentPosition);
					view.FocusedRow = n.CurrentPosition;
				}
			};
			Button addButton = new Button ("Add");
			addButton.Clicked += delegate(object sender, EventArgs e) {
				addCounter++;
				TreeNavigator n;
				if (view.SelectedRow != null)
					n = store.AddNode (view.SelectedRow).SetValue (text, "Added " + addCounter).SetValue (desc, "Desc");
				else
					n = store.AddNode ().SetValue (text, "Added " + addCounter).SetValue (desc, "Desc");
				view.ExpandToRow (n.CurrentPosition);
				view.ScrollToRow (n.CurrentPosition);
				view.SelectRow (n.CurrentPosition);
			};
			PackStart (addButton);
			
			Button removeButton = new Button ("Remove Selection");
			removeButton.Clicked += delegate(object sender, EventArgs e) {
				foreach (TreePosition row in view.SelectedRows) {
					store.GetNavigatorAt (row).Remove ();
				}
			};
			PackStart (removeButton);

			Button clearButton = new Button("Clear");
			clearButton.Clicked += delegate (object sender, EventArgs e) {
				store.Clear();
			};
			PackStart(clearButton);

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

