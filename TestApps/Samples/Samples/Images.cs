// 
// Images.cs
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
using System.Reflection;
using Xwt;
using Xwt.Drawing;

namespace Samples
{
	public class Images: VBox
	{
		public Images ()
		{
			ImageView img = new ImageView ();
			img.Image = Image.FromResource (GetType (), "cow.jpg");
			PackStart (img);

			var stockIcons = typeof (StockIcons).GetProperties (BindingFlags.Public | BindingFlags.Static);
			var perRow = 6;

			HBox row = null;
			for (var i = 0; i < stockIcons.Length; i++) {
				if (stockIcons [i].PropertyType != typeof (Image))
					continue;

				if ((i % perRow) == 0) {
					if (row != null)
						PackStart (row);
					row = new HBox ();
				}

				var vbox = new VBox ();

				var stockImage = (Image)stockIcons [i].GetValue (typeof(StockIcons), null);
				var imageView = new ImageView ();
				var label = new Label (stockIcons [i].Name);

				try {
					var icon = stockImage.WithSize (IconSize.Medium);
					if (icon != null)
						imageView.Image = icon;
				} catch { }

				vbox.PackStart (imageView);
				vbox.PackEnd (label);

				row.PackStart (vbox);
			}
			if (row != null)
				PackStart (row);
		}
	}
}

