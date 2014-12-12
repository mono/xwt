//
// PopoverSample.cs
//
// Author:
//       Jérémie Laval <jeremie.laval@xamarin.com>
//
// Copyright (c) 2012 Xamarin, Inc.
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
	public class PopoverSample : VBox
	{
		Popover popover;
		Popover popover2;

		public PopoverSample ()
		{
			var btn = new Button ("Click me");
			btn.Clicked += HandleClicked;
			PackStart (btn);
			var btn2 = new Button ("Click me");
			btn2.Clicked += HandleClicked2;
			PackEnd (btn2);
		}
	
		void HandleClicked (object sender, EventArgs e)
		{
			if (popover == null) {
				popover = new Popover ();
				popover.Padding = 20;

				var table = new Table () { DefaultColumnSpacing = 20, DefaultRowSpacing = 10 };
//					table.Margin.SetAll (60);
				table.Add (new Label ("Font") { TextAlignment = Alignment.End }, 0, 0);
				table.Add (new ComboBox (), 1, 0, vexpand:true);

				table.Add (new Label ("Family")  { TextAlignment = Alignment.End }, 0, 1);
				table.Add (new ComboBox (), 1, 1, vexpand:true);

				var cmbStyle = new ComboBox ();
				cmbStyle.Items.Add ("Normal");
				cmbStyle.Items.Add ("Bold");
				cmbStyle.Items.Add ("Italic");

				table.Add (new Label ("Style")  { TextAlignment = Alignment.End }, 0, 2);
				table.Add (cmbStyle, 1, 2, vexpand:true);

				table.Add (new Label ("Size")  { TextAlignment = Alignment.End }, 0, 3);
				table.Add (new SpinButton (), 1, 3, vexpand:true);

				var b = new Button ("Add more");
				table.Add (b, 0, 4);
				int next = 5;
				b.Clicked += delegate {
					table.Add (new Label ("Row " + next), 0, next++);
				};

				table.Margin = 20;
				popover.Content = table;
			}
//			popover.Padding.SetAll (20);
			popover.BackgroundColor = Xwt.Drawing.Colors.Yellow.WithAlpha(0.9);
			popover.Show (Popover.Position.Top, (Button)sender, new Rectangle (50, 10, 5, 5));
		}

		void HandleClicked2 (object sender, EventArgs e)
		{
			if (popover2 == null) {
				popover2 = new Popover ();

				var table = new Table () { DefaultColumnSpacing = 20, DefaultRowSpacing = 10 };
				table.Add (new Label ("Font") { TextAlignment = Alignment.End }, 0, 0);
				table.Add (new ComboBox (), 1, 0, vexpand:true);

				table.Add (new Label ("Family")  { TextAlignment = Alignment.End }, 0, 1);
				table.Add (new ComboBox (), 1, 1, vexpand:true);

				table.Add (new Label ("Style")  { TextAlignment = Alignment.End }, 0, 2);
				table.Add (new ComboBox (), 1, 2, vexpand:true);

				table.Add (new Label ("Size")  { TextAlignment = Alignment.End }, 0, 3);
				table.Add (new SpinButton (), 1, 3, vexpand:true);

				var b = new Button ("Add more");
				table.Add (b, 0, 4);
				int next = 5;
				b.Clicked += delegate {
					table.Add (new Label ("Row " + next), 0, next++);
				};

				table.Margin = 6;
				popover2.Content = table;
			}

			var newRect = new Rectangle (((Button)sender).Size.Width * 0.66d, 0, 0, 0);
			popover2.Show (Popover.Position.Bottom, (Button)sender, newRect);
		}
	}
}

