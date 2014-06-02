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
using System.Collections.Generic;

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
		
			var triStateCellView = new CheckBoxCellView {
				StateBinding = triState,
				Editable = true, 
				AllowMixed = true 
			};
			triStateCellView.Toggled += (object sender, WidgetEventArgs e) => {
				if (view.CurrentEventRow == null) {
					MessageDialog.ShowError("CurrentEventRow is null. This is not supposed to happen");
				}
				else {
					store.GetNavigatorAt(view.CurrentEventRow).SetValue(text, "TriState Toggled");
				}
			};
			var checkCellView = new CheckBoxCellView {
				ActiveBinding = check,
				Editable = true 
			};
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
				store.AddNode ().SetValue (text, "Added").SetValue (desc, "Desc");
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

			TreeView tree = new TreeView ();

			var template = new TreeItemTemplate {
				Views = {
					new TextCellView {
						TextBinding = new PropertyBinding<Country> ((c) => c.Name)
					}
				}
			};
			template.ItemsSource = new PropertyBinding<Country> (c => c.Cities);
			tree.ItemTemplates.Add (template);


			var col1 = new ListViewColumn ("Some column");
			var col2 = new ListViewColumn ("Some other column");
			Xwt.Drawing.Image img = null;

			tree.ItemTemplates.Add (
				new TreeItemTemplate {
					ItemType = typeof(Country),
					ItemsSource = new PropertyBinding<Country> (c => c.Cities),
					Views = {
						new ImageCellView { Image = img },
						new TextCellView {
							TextBinding = new PropertyBinding<Country> (c => c.Name),
							Column = col2
						}
					}
				}
			);
			tree.ItemTemplates.Add (
				new TreeItemTemplate {
					ItemType = typeof(Country),
					ItemsSource = new PropertyBinding<City> (c => c.Quarters),
					Views = {
						new ImageCellView { Image = img },
						new TextCellView {
							TextBinding = new PropertyBinding<City> (c => c.Name),
							Column = col2
						}
					}
				}
			);

			var selField = new DataField<bool> ();
			var dataField = new DataField<Country> ();
			var st = new TreeStore (selField, dataField);

			tree.ItemTemplates.Add (
				new TreeItemTemplate {
					Views = {
						new CheckBoxCellView {
							ActiveBinding = selField
						},
						new TextCellView {
							TextBinding = dataField.Select (c => c.Name)
						}
					}
				}
			);

			/*******************/

			PropertyExtension<City,bool> selProp = new PropertyExtension<City, bool> ();
			selProp.SetDefaultValue (true);

			tree.ItemTemplates.Add (
				new TreeItemTemplate {
					Views = {
						new CheckBoxCellView {
							ActiveBinding = selProp
						},
						new TextCellView {
							TextBinding = dataField.Select (c => c.Name),
							TextColorBinding = new CustomBinding<City> (c => c.Population >= 10000 ? Xwt.Drawing.Colors.Red : Xwt.Drawing.Colors.Black)
						}
					},
					ItemsSource = new CustomBinding<City>  (c => c.Population >= 100000 ? c.Quarters : null)
				}
			);
			City cc = null;
			selProp.GetValue (cc);
		}

		void HandleDragOver (object sender, DragOverEventArgs e)
		{
			e.AllowedAction = e.Action;
		}
	}

	class DataStore
	{
		public DataStore (params IDataField[] fields)
		{
		}

		public T GetValue<T> (object instance, IDataField<T> field)
		{
			return default(T);
		}
	}

	class PropertyExtension<TO,T>: Binding
	{

		public T GetValue (TO instance)
		{
			return default(T);
		}

		public void SetValue (TO instance, T value)
		{
		}

		public void SetDefaultValue (T value)
		{
		}
	}

	class Country
	{
		public string Name;
		public IEnumerable<City> Cities;
		public int Population;
	}

	class City
	{
		public string Name;
		public IEnumerable<City> Quarters;
		public int Population;
	}

	class Quarter
	{
		public string Name;
	}
}

