﻿//
// ListViewCombos.cs
//
// Author:
//       Lluis Sanchez Gual <lluis@xamarin.com>
//
// Copyright (c) 2016 Xamarin, Inc (http://www.xamarin.com)
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
using System.Linq;

namespace Samples
{
	public class ListViewCombos: VBox
	{
		ListStore store;
		ListView list;
		DataField<int> indexField;
		public ListViewCombos ()
		{
			list = new ListView ();
			indexField = new DataField<int> ();
			var indexField2 = new DataField<int> ();
			var itemsField = new DataField<ItemCollection> ();
		
			store = new ListStore (indexField, indexField2, itemsField);
			list.DataSource = store;
			list.GridLinesVisible = GridLines.Horizontal;

			ComboBoxCellView comboCellView = new ComboBoxCellView { Editable = true, SelectedIndexField = indexField };
			comboCellView.Items.Add (1, "one");
			comboCellView.Items.Add (2, "two");
			comboCellView.Items.Add (3, "three");
			comboCellView.EditingFinished += CellView_EditingFinished;
			list.Columns.Add (new ListViewColumn ("List 1", comboCellView));

			var comboCellView2 = new ComboBoxCellView { Editable = true, SelectedIndexField = indexField2, ItemsField = itemsField };
			comboCellView2.EditingFinished += CellView_EditingFinished;
			list.Columns.Add (new ListViewColumn ("List 2", comboCellView2));

			int p = 0;
			for (int n = 0; n < 10; n++) {
				var r = store.AddRow ();
				store.SetValue (r, indexField, n % 3);
				var col = new ItemCollection ();
				for (int i = 0; i < 3; i++) {
					col.Add (p, "p" + p);
					p++;
				}
				store.SetValues (r, indexField2, n % 3, itemsField, col);
			}
			PackStart (list, true);
		}

		void CellView_EditingFinished (object sender, CellEditingFinishedArgs<CheckBoxState> e)
		{
			Console.WriteLine("Your old value was '{0}' and now is '{1}'", e.OldValue, e.NewValue);
		}

		void CellView_EditingFinished(object sender, CellEditingFinishedArgs<string> e)
		{
			Console.WriteLine("Your old value was '{0}' and now is '{1}'", e.OldValue, e.NewValue);
		}
	}
}
