//
// TreeViewFilter.cs
//
// Author:
//       Vsevolod Kukol <sevo@sevo.org>
//
// Copyright (c) 2014 Vsevolod Kukol
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
	public class TreeViewFilter: VBox
	{
		int valueCount;
		DataField<string> text = new DataField<string> ();
		DataField<bool> check = new DataField<bool>();
		DataField<string> action = new DataField<string> ();
		DataField<string> position = new DataField<string> ();

		public TreeViewFilter ()
		{
			TreeView view = new TreeView ();
			TreeView fview = new TreeView ();
			TreeStore store = new TreeStore (check, text, action, position);

			var checkCellView = new CheckBoxCellView (check) { Editable = true };
			view.Columns.Add ("Item", text);
			view.Columns.Add ("Check", checkCellView);
			view.Columns.Add ("State", new TextCellView (check));
			view.Columns.Add ("Pos", position);
			view.Columns.Add ("Action", action);

			var checkCellView2 = new CheckBoxCellView (check) { Editable = true };
			fview.Columns.Add ("Item", text);
			fview.Columns.Add ("Check", checkCellView2);
			fview.Columns.Add ("State", new TextCellView (check));
			fview.Columns.Add ("Pos", position);
			fview.Columns.Add ("Action", action);

			store.AddNode ().SetValue (text, "Value 1").SetValue(position, "1");
			store.AddNode ().SetValue (text, "Value 2").SetValue(position, "2").AddChild ()
				.SetValue (text, "Value 3").SetValue(position, "2.2");
			store.AddNode ().SetValue (text, "Value 4").SetValue(position, "3").AddChild ()
				.SetValue (text, "Value 5").SetValue(position, "3.3");
			valueCount = 6;

			TreeStoreFilter filter = new TreeStoreFilter (store);

			checkCellView.Toggled += (object sender, WidgetEventArgs e) => {
				if (view.CurrentEventRow == null) {
					MessageDialog.ShowError("CurrentEventRow is null. This is not supposed to happen");
				}
				else {
					var newstate = !store.GetNavigatorAt(view.CurrentEventRow).GetValue (check);
					string saction = newstate ? "Checked" : "Unchecked";
					store.GetNavigatorAt(view.CurrentEventRow).SetValue(action, saction);
					store.GetNavigatorAt(view.CurrentEventRow).SetValue (check, newstate);
					e.Handled = true;
				}
			};

			checkCellView2.Toggled += (object sender, WidgetEventArgs e) => {
				if (fview.CurrentEventRow == null) {
					MessageDialog.ShowError("CurrentEventRow is null. This is not supposed to happen");
				}
				else {
					var newstate = !store.GetNavigatorAt(filter.ConvertPositionToChildPosition (fview.CurrentEventRow)).GetValue (check);
					string saction = newstate ? "Checked" : "Unchecked";
					store.GetNavigatorAt(filter.ConvertPositionToChildPosition (fview.CurrentEventRow)).SetValue(action, saction);
					store.GetNavigatorAt(filter.ConvertPositionToChildPosition (fview.CurrentEventRow)).SetValue (check, newstate);
					e.Handled = true;
				}
			};

			var chkFilter = new CheckBox ("Checked");
			chkFilter.AllowMixed = true;
			chkFilter.State = CheckBoxState.Mixed;
			chkFilter.Toggled += (sender, e) => filter.Refilter ();

			var txtFilter = new TextEntry ();
			txtFilter.Changed += (sender, e) => filter.Refilter ();

			filter.FilterFunction = delegate(TreePosition arg)
			{
				var checkedval = store.GetNavigatorAt (arg).GetValue (check);

				if (chkFilter.State == CheckBoxState.On && !checkedval)
					return true;
				if (chkFilter.State == CheckBoxState.Off && checkedval)
					return true;

				var val = store.GetNavigatorAt (arg).GetValue (text);
				if (val == null)
					return false;
				return !val.ToUpper ().Contains (txtFilter.Text.ToUpper ());
			};

			var filterBox = new HBox ();
			filterBox.PackStart (new Label("Filter:"));
			filterBox.PackStart (chkFilter);
			filterBox.PackStart (txtFilter, true);


			view.DataSource = store;
			fview.DataSource = filter;

			Label la = new Label ();

			view.SetDragDropTarget (DragDropAction.All, TransferDataType.Text);
			view.SetDragSource (DragDropAction.All, TransferDataType.Text);

			view.DragDrop += delegate(object sender, DragEventArgs e) {
				TreePosition node;
				RowDropPosition pos;
				view.GetDropTargetRow (e.Position.X, e.Position.Y, out pos, out node);
				var nav = store.GetNavigatorAt (node);
				var dest = nav.GetValue (text);

				string loc = String.Empty;
				if (pos == RowDropPosition.Before) 
					nav.MoveNext();
				else if (pos == RowDropPosition.After)
					nav.MovePrevious();
				else
					nav.MoveToParent();
				loc = pos + " \"" + nav.GetValue (text) + "\"";
				la.Text = "Dropped \"" + e.Data.Text + "\" into \"" + dest + "\" " + loc;
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
				e.DragOperation.SetDragImage (StockIcons.Add, 0, 0);
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
			Button addButton = new Button ("Add");
			addButton.Clicked += delegate(object sender, EventArgs e) {
				TreeNavigator n;
				if (view.SelectedRow != null)
					n = store.AddNode (view.SelectedRow);
				else
					n = store.AddNode ();
				n.SetValue (text, "Value " + valueCount++);
				view.ScrollToRow (n.CurrentPosition);
			};

			Button removeButton = new Button ("Remove Selection");
			removeButton.Clicked += delegate(object sender, EventArgs e) {
				foreach (TreePosition row in view.SelectedRows) {
					store.GetNavigatorAt (row).Remove ();
				}
			};

			PackStart (view, true);
			PackStart (la);
			PackStart (removeButton);
			PackStart (addButton);
			PackStart (fview, true);

			PackStart (filterBox);

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

