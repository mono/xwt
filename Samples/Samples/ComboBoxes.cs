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
				la3.Text = "Selected item: " + c3.SelectedItem;
			};
			PackStart (box);
			
			box = new HBox ();
			var c4 = new ComboBoxEntry ();
			var la4 = new Label ();
			box.PackStart (c4);
			box.PackStart (la4);
			
			c4.Items.Add (1, "One");
			c4.Items.Add (2, "Two");
			c4.Items.Add (3, "Three");
			c4.TextEntry.PlaceholderText = "This is an entry";
			c4.TextEntry.Changed += delegate {
				la4.Text = "Selected text: " + c4.TextEntry.Text;
			};
			PackStart (box);
		}
	}
}

