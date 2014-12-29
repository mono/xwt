//
// ListViewFilter.cs
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
using Xwt.Drawing;

namespace Samples
{
	public class ListViewFilter: VBox
	{
		DataField<string> name = new DataField<string> ();
		DataField<Image> icon = new DataField<Image> ();
		DataField<string> text = new DataField<string> ();
		DataField<Image> icon2 = new DataField<Image> ();
		DataField<CellData> progress = new DataField<CellData> ();

		public ListViewFilter ()
		{
			PackStart (new Label ("The listview should have a red background"));
			ListView list = new ListView ();
			list.GridLinesVisible = GridLines.Both;
			ListStore store = new ListStore (name, icon, text, icon2, progress);
			ListStoreFilter filter = new ListStoreFilter (store);

			list.DataSource = filter;
			list.Columns.Add ("Name", icon, name);
			list.Columns.Add ("Text", icon2, text);
			list.Columns.Add ("Progress", new TextCellView () { TextField = text }, new CustomCell () { ValueField = progress });

			var png = Image.FromResource (typeof(App), "class.png");

			Random rand = new Random ();

			for (int n=0; n<100; n++) {
				var r = store.AddRow ();
				store.SetValue (r, icon, png);
				store.SetValue (r, name, "Value " + n);
				store.SetValue (r, icon2, png);
				store.SetValue (r, text, "Text " + n);
				store.SetValue (r, progress, new CellData { Value = rand.Next () % 100 });
			}
			PackStart (list, true);

			list.RowActivated += delegate(object sender, ListViewRowEventArgs e) {
				int storeRow = filter.ConvertPositionToChildPosition(e.RowIndex);
				var valName = store.GetValue (storeRow, name);
				var valText = store.GetValue (storeRow, text);
				var valProgress = store.GetValue (storeRow, progress).Value;
				MessageDialog.ShowMessage ("Row " + e.RowIndex + " activated", 
				                           "Name: " + valName + "\nText: " + valText + "\nProgress: " + valProgress);
			};

			Menu contextMenu = new Menu ();
			contextMenu.Items.Add (new MenuItem ("Test menu"));
			list.ButtonPressed += delegate(object sender, ButtonEventArgs e) {
				int row = list.GetRowAtPosition(new Point(e.X, e.Y));
				if (e.Button == PointerButton.Right && row >= 0) {
					// Set actual row to selected
					list.SelectRow(row);
					contextMenu.Popup(list, e.X, e.Y);
				}
			};

			var spnValue = new SpinButton ();
			spnValue.MinimumValue = 0;
			spnValue.MaximumValue = 99;
			spnValue.IncrementValue = 1;
			spnValue.Digits = 0;
			var btnScrollVal = new Button ("Name");
			btnScrollVal.Clicked += (sender, e) =>
				list.ScrollToRow(filter.ConvertChildPositionToPosition((int)spnValue.Value));

			var btnScrollRow = new Button ("Row");
			btnScrollRow.Clicked += (sender, e) =>
				list.ScrollToRow((int)spnValue.Value);

			HBox scrollActBox = new HBox ();
			scrollActBox.PackStart (new Label("Scroll to: "));
			scrollActBox.PackStart (spnValue);
			scrollActBox.PackStart (btnScrollVal);
			scrollActBox.PackStart (btnScrollRow);
			PackStart (scrollActBox);

			var txtFilter = new TextEntry ();
			txtFilter.Changed += (sender, e) => filter.Refilter ();
			var spnFilter = new SpinButton ();
			spnFilter.Digits = 0;
			spnFilter.MinimumValue = 0;
			spnFilter.MaximumValue = 100;
			spnFilter.IncrementValue = 1;
			spnFilter.Value = 100;
			spnFilter.ValueChanged += (sender, e) => filter.Refilter ();

			filter.FilterFunction = delegate(int pos) {
				var valText = store.GetValue (pos, text);
				if (!valText.Contains (txtFilter.Text))
					return true;

				var valProgress = store.GetValue (pos, progress).Value;
				if (valProgress > spnFilter.Value)
					return true;
				return false;
			};

			var boxFilter = new HBox ();
			boxFilter.PackStart (new Label ("Filter Text:"));
			boxFilter.PackStart (txtFilter, true);
			boxFilter.PackStart (new Label (" & %Max:"));
			boxFilter.PackStart (spnFilter);
			PackStart (boxFilter);
		}
	}
}

