//
// ThemedImages.cs
//
// Author:
//       Vsevolod Kukol <sevo@xamarin.com>
//
// Copyright (c) 2015 Xamarin Inc. (http://www.xamarin.com)
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
	public class ThemedImages: VBox
	{
		public ThemedImages ()
		{
			Context.RegisterStyles ("dark", "sel");

			var img = Image.FromResource ("zoom-in-16.png");
			var img_sel = Image.FromResource ("zoom-in-16.png").WithStyles("sel");
			var img_dark = Image.FromResource ("zoom-in-16.png").WithStyles("dark");
			var img_dark_sel = Image.FromResource ("zoom-in-16.png").WithStyles("dark", "sel");


			var img_row = new HBox ();
			ImageView imgv = new ImageView () { Image = img };
			ImageView imgv_sel = new ImageView () { Image = img_sel };
			ImageView imgv_dark = new ImageView () { Image = img_dark };
			ImageView imgv_dark_sel = new ImageView () { Image = img_dark_sel };
			img_row.PackStart (imgv);
			img_row.PackStart (imgv_sel);
			img_row.PackStart (imgv_dark);
			img_row.PackStart (imgv_dark_sel);
			PackStart (img_row);

			var btn_row = new HBox ();
			Button btn = new Button (img);
			Button btn_sel = new Button (img_sel);
			Button btn_dark = new Button (img_dark);
			Button btn_dark_sel = new Button (img_dark_sel);
			btn_row.PackStart (btn);
			btn_row.PackStart (btn_sel);
			btn_row.PackStart (btn_dark);
			btn_row.PackStart (btn_dark_sel);
			PackStart (btn_row);
		}
	}
}

