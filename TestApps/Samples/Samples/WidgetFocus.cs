//
// FocusWidget.cs
//
// Author:
//       Vsevolod Kukol <sevo@sevo.org>
//
// Copyright (c) 2015 Vsevolod Kukol
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
	public class WidgetFocus: VBox
	{
		DataField<string> value = new DataField<string> ();
		public WidgetFocus ()
		{
			var text = new TextEntry ();
			var check = new CheckBox ("CheckBox");
			var slider = new HSlider ();
			ListStore store = new ListStore (value);
			var list = new ListView (store);
			list.Columns.Add ("Value", value);
			list.HeadersVisible = false;
			for (int n=0; n<10; n++) {
				var r = store.AddRow ();
				store.SetValue (r, value, "Value " + n);
			}

			var btn1 = new Button ("TextEnty");
			var btn2 = new Button ("Checkbox");
			var btn3 = new Button ("Slider");
			var btn4 = new Button ("ListBox");
			var btn5 = new Button ("Button");

			btn1.Clicked += (sender, e) => text.SetFocus ();
			btn2.Clicked += (sender, e) => check.SetFocus ();
			btn3.Clicked += (sender, e) => slider.SetFocus ();
			btn4.Clicked += (sender, e) => list.SetFocus ();
			btn5.Clicked += (sender, e) => btn1.SetFocus ();

			var btnBox = new HBox ();
			btnBox.PackStart (btn1);
			btnBox.PackStart (btn2);
			btnBox.PackStart (btn3);
			btnBox.PackStart (btn4);
			btnBox.PackStart (btn5);

			var focusBox = new HBox ();
			var vbox = new VBox ();
			vbox.PackStart (text);
			vbox.PackStart (check);
			vbox.PackStart (slider);
			focusBox.PackStart (vbox);
			focusBox.PackStart (list, true);

			PackStart (btnBox);
			PackStart (focusBox, true);
		}
	}
}

