// 
// ListBoxSample.cs
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
	public class ListBoxSample: VBox
	{
		DataField<string> name = new DataField<string> ();
		DataField<Image> icon = new DataField<Image> ();
		
		public ListBoxSample ()
		{
			// Default list box
			
			ListBox list = new ListBox ();
			
			for (int n=0; n<100; n++)
				list.Items.Add ("Value " + n);


			list.KeyPressed += (sender, e) => {
				if (e.Key == Key.Insert) {
					int r = list.SelectedRow + 1;
					list.Items.Insert(r, "Value " + list.Items.Count + 1);
					list.ScrollToRow (r);
					list.SelectRow (r);
					list.FocusedRow = r;
				}
			};
			
			PackStart (list, true);
			
			// Custom list box
			
			ListBox customList = new ListBox ();
			customList.GridLinesVisible = true;
			ListStore store = new ListStore (name, icon);
			customList.DataSource = store;
			customList.Views.Add (new ImageCellView (icon));
			customList.Views.Add (new TextCellView (name));
			
			var png = Image.FromResource (typeof(App), "class.png");
			
			for (int n=0; n<100; n++) {
				var r = store.AddRow ();
				store.SetValue (r, icon, png);
				store.SetValue (r, name, "Value " + n);
			}

			customList.KeyPressed += (sender, e) => {
				if (e.Key == Key.Insert) {
					var r = store.InsertRowAfter(customList.SelectedRow < 0 ? 0 : customList.SelectedRow);
					store.SetValue (r, icon, png);
					store.SetValue (r, name, "Value " + (store.RowCount + 1));
					customList.ScrollToRow (r);
					customList.SelectRow (r);
					customList.FocusedRow = r;
				}
			};

			PackStart (customList, true);

			var spnValue = new SpinButton ();
			spnValue.MinimumValue = 0;
			spnValue.MaximumValue = 99;
			spnValue.IncrementValue = 1;
			spnValue.Digits = 0;
			var btnScroll = new Button ("Go!");
			btnScroll.Clicked += (sender, e) => customList.ScrollToRow((int)spnValue.Value);

			HBox scrollActBox = new HBox ();
			scrollActBox.PackStart (new Label("Scroll to Value: "));
			scrollActBox.PackStart (spnValue);
			scrollActBox.PackStart (btnScroll);
			PackStart (scrollActBox);
		}	
	}
}

