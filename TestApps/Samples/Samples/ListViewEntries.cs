//
// ListViewEntries.cs
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
using System.IO;

namespace Samples
{
	public class ListViewEntries: VBox
	{
		public ListViewEntries ()
		{
			ListView list = new ListView ();
			var editableTextField = new DataField<string> ();
			var nonEditableTextField = new DataField<string> ();

			ListStore store = new ListStore (editableTextField, nonEditableTextField);
			list.DataSource = store;
			list.GridLinesVisible = GridLines.Horizontal;

			var textCellView = new TextCellView { Editable = true, TextField = editableTextField };
			list.Columns.Add (new ListViewColumn ("Editable", textCellView));
			list.Columns.Add (new ListViewColumn ("Not Editable", new TextCellView { Editable = false, TextField = nonEditableTextField }));

			Random rand = new Random ();

			for (int n = 0; n < 10; n++) {
				var r = store.AddRow ();
				store.SetValue (r, editableTextField, "Editable value " + n);
				store.SetValue (r, nonEditableTextField, "Non-editable value " + n);
			}
			PackStart (list, true);
			var btn = new Button ("Add Row");
			btn.Clicked += delegate {
				var row = store.AddRow ();
				store.SetValues (row, editableTextField, "New editable text", nonEditableTextField, "New non-editable text");
				list.StartEditingCell (row, textCellView);
			};
			PackStart (btn, false, hpos: WidgetPlacement.Start);
		}
	}
}

