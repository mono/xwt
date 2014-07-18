//
// CustomCellRendererImage.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc.
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
using System.Collections.Generic;
using System.Linq;
using Gtk;
using Xwt.Backends;

namespace Xwt.GtkBackend
{

	public class CustomCellRendererImage: CellViewBackend
	{
		ImageRenderer renderer;

		public CustomCellRendererImage ()
		{
			renderer = new ImageRenderer ();
			CellRenderer = renderer;
		}

		protected override void OnLoadData ()
		{
			var view = (IImageCellViewFrontend)Frontend;
			renderer.Context = ApplicationContext;
			renderer.Image = view.Image.ToImageDescription (ApplicationContext);
		}
	}

	class ImageRenderer: GtkCellRendererCustom
	{
		ImageDescription image;

		public ApplicationContext Context;

		[GLib.Property ("image")]
		public ImageDescription Image {
			get { return image; }
			set { image = value; }
		}

		protected override void OnRender (Cairo.Context cr, Gtk.Widget widget, Gdk.Rectangle background_area, Gdk.Rectangle cell_area, CellRendererState flags)
		{
			if (image.IsNull)
				return;
			var pix = ((GtkImage)image.Backend);
			pix.Draw (Context, cr, Util.GetScaleFactor (widget), cell_area.X, cell_area.Y, image);

		}

		protected override void OnGetSize (Gtk.Widget widget, ref Gdk.Rectangle cell_area, out int x_offset, out int y_offset, out int width, out int height)
		{
			if (image.IsNull) {
				width = height = 0;
			} else {
				width = (int) image.Size.Width;
				height = (int) image.Size.Height;
			}
			x_offset = y_offset = 0;
		}
	}
}
