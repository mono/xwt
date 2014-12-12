//
// R.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc
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
	public class RadioButtonSample: VBox
	{
		public RadioButtonSample ()
		{
			var b1 = new RadioButton ("Item 1");
			var b2 = new RadioButton ("Item 2 (red background)");
			b2.BackgroundColor = Xwt.Drawing.Colors.Red;
			var b3 = new RadioButton ("Item 3");
			b2.Group = b3.Group = b1.Group;
			PackStart (b1);
			PackStart (b2);
			PackStart (b3);

			var la = new Label ();
			la.Hide ();
			b1.Group.ActiveRadioButtonChanged += delegate {
				la.Show ();
				la.Text = "Active: " + b1.Group.ActiveRadioButton.Label;
			};
			PackStart (la);

			PackStart (new HSeparator ());

			var box = new VBox ();
			box.PackStart (new Label ("First Option"));
			box.PackStart (new Label ("Second line"));

			var b4 = new RadioButton (box);
			var b5 = new RadioButton ("Second Option");
			var b6 = new RadioButton ("Disabled Option") { Sensitive = false };
			PackStart (b4);
			PackStart (b5);
			PackStart (b6);
			b4.Group = b5.Group = b6.Group;
		}
	}
}

