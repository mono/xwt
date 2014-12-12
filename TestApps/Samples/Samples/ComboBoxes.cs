// 
// ComboBoxes.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2011 Xamarin Inc
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
	public class ComboBoxes: VBox
	{
		public ComboBoxes ()
		{
			HBox box = new HBox ();
			ComboBox c = new ComboBox ();
			c.Items.Add ("One");
			c.Items.Add ("Two");
			c.Items.Add ("Three");
			c.SelectedIndex = 1;
			box.PackStart (c);
			Label la = new Label ();
			box.PackStart (la);
			c.SelectionChanged += delegate {
				la.Text = "Selected: " + (string)c.SelectedItem;
			};
			PackStart (box);
			
			box = new HBox ();
			ComboBox c2 = new ComboBox ();
			box.PackStart (c2);
			Button b = new Button ("Fill combo (should grow)");
			box.PackStart (b);
			b.Clicked += delegate {
				for (int n=0; n<10; n++) {
					c2.Items.Add ("Item " + new string ('#', n));
				}
			};
			PackStart (box);
			
			// Combo with custom labels
			
			box = new HBox ();
			ComboBox c3 = new ComboBox ();
			c3.Items.Add (0, "Combo with custom labels");
			c3.Items.Add (1, "One");
			c3.Items.Add (2, "Two");
			c3.Items.Add (3, "Three");
			c3.Items.Add (ItemSeparator.Instance);
			c3.Items.Add (4, "Maybe more");
			var la3 = new Label ();
			box.PackStart (c3);
			box.PackStart (la3);
			c3.SelectionChanged += delegate {
				la3.Text = string.Format ("Selected item: {0} with label {1}",
				                          c3.SelectedItem,
				                          c3.SelectedText);
			};
			PackStart (box);

			var c4 = new ComboBoxEntry ();
			var la4 = new Label ("Selected text: ");
			PackStart (c4);
			PackStart (la4);
			
			c4.Items.Add (1, "One");
			c4.Items.Add (2, "Two");
			c4.Items.Add (3, "Three");
			c4.TextEntry.PlaceholderText = "This is an entry";
			c4.TextEntry.SelectionChanged += delegate {
				la4.Text = "Selected text: " + c4.TextEntry.SelectedText;
			};

			HBox selBox = new HBox ();
			Label las = new Label ("Selection:");
			selBox.PackStart (las);
			Button selReplace = new Button ("Replace");
			selReplace.Clicked += delegate {
				c4.TextEntry.SelectedText = "[TEST]";
			};
			selBox.PackEnd (selReplace);
			Button selAll = new Button ("Select all");
			selAll.Clicked += delegate {
				c4.TextEntry.SelectionStart = 0;
				c4.TextEntry.SelectionLength = c4.TextEntry.Text.Length;
			};
			selBox.PackEnd (selAll);
			Button selPlus = new Button ("+");
			selPlus.Clicked += delegate {
				c4.TextEntry.SelectionLength++;
			};
			selBox.PackEnd (selPlus);
			Button selRight = new Button (">");
			selRight.Clicked += delegate {
				c4.TextEntry.SelectionStart++;
			};
			selBox.PackEnd (selRight);
			PackStart (selBox);

			c4.TextEntry.SelectionChanged += delegate {
				las.Text = "Selection: " + c4.TextEntry.CursorPosition + " - " + c4.TextEntry.SelectionStart + " Length: " + c4.TextEntry.SelectionLength;
			};


			var c5 = new ComboBoxEntry ();
			c5.TextEntry.TextAlignment = Alignment.Center;
			c5.TextEntry.Text = "centered text with red background";
			c5.BackgroundColor = Colors.Red;
			c5.Items.Add (1, "One");
			c5.Items.Add (2, "Two");
			c5.Items.Add (3, "Three");
			PackStart (c5);

			// A complex combobox
			
			// Three data fields
			var imgField = new DataField<Image> ();
			var textField = new DataField<string> ();
			var descField = new DataField<string> ();
			
			ComboBox cbox = new ComboBox ();
			ListStore store = new ListStore (textField, imgField, descField);
			
			cbox.ItemsSource = store;
			var r = store.AddRow ();
			store.SetValue (r, textField, "Information");
			store.SetValue (r, descField, "Icons are duplicated on purpose");
			store.SetValue (r, imgField, StockIcons.Information);
			r = store.AddRow ();
			store.SetValue (r, textField, "Error");
			store.SetValue (r, descField, "Another item");
			store.SetValue (r, imgField, StockIcons.Error);
			r = store.AddRow ();
			store.SetValue (r, textField, "Warning");
			store.SetValue (r, descField, "A third item");
			store.SetValue (r, imgField, StockIcons.Warning);
			
			// Four views to show three data fields
			cbox.Views.Add (new ImageCellView (imgField));
			cbox.Views.Add (new TextCellView (textField));
			cbox.Views.Add (new ImageCellView (imgField));
			cbox.Views.Add (new TextCellView (descField));
			
			cbox.SelectedIndex = 0;
			
			PackStart (cbox);
		}
	}
}

