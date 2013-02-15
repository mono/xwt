// 
// ImageHandler.cs
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
using Xwt.Backends;
using Xwt.Drawing;

namespace Xwt.GtkBackend
{
	public class ImageHandler: ImageBackendHandler
	{
		public override object LoadFromStream (System.IO.Stream stream)
		{
			using (Gdk.PixbufLoader loader = new Gdk.PixbufLoader (stream))
				return loader.Pixbuf;
		}

		public override void SaveToStream (object backend, System.IO.Stream stream, ImageFileType fileType)
		{
			var pix = (Gdk.Pixbuf)backend;
			var buffer = pix.SaveToBuffer (GetFileType (fileType));
			stream.Write (buffer, 0, buffer.Length);
		}

		string GetFileType (ImageFileType type)
		{
			switch (type) {
			case ImageFileType.Bmp:
				return "bmp";
			case ImageFileType.Jpeg:
				return "jpeg";
			case ImageFileType.Png:
				return "png";
			default:
				throw new NotSupportedException ();
			}
		}

		public override Image GetStockIcon (string id)
		{
			return ApplicationContext.Toolkit.WrapImage (Util.ToGtkStock (id));
		}
		
		public override void SetBitmapPixel (object handle, int x, int y, Xwt.Drawing.Color color)
		{
			var pix = (Gdk.Pixbuf)handle;
			
			unsafe {
				byte* p = (byte*) pix.Pixels;
				p += y * pix.Rowstride + x * pix.NChannels;
				p[0] = (byte)(color.Red * 255);
				p[1] = (byte)(color.Green * 255);
				p[2] = (byte)(color.Blue * 255);
				p[3] = (byte)(color.Alpha * 255);
			}
		}
		
		public override Xwt.Drawing.Color GetBitmapPixel (object handle, int x, int y)
		{
			var pix = (Gdk.Pixbuf)handle;
			
			unsafe {
				byte* p = (byte*) pix.Pixels;
				p += y * pix.Rowstride + x * pix.NChannels;
				return Xwt.Drawing.Color.FromBytes (p[0], p[1], p[2], p[3]);
			}
		}
		
		public override void Dispose (object backend)
		{
			((Gdk.Pixbuf)backend).Dispose ();
		}

		public override bool HasMultipleSizes (object handle)
		{
			return handle is string;
		}

		public override Size GetSize (object handle)
		{
			var pix = handle as Gdk.Pixbuf;
			if (pix != null)
				return new Size (pix.Width, pix.Height);
			else if (handle is String)
				return new Size (48, 48); // Return a default size
			else
				return Size.Zero;
		}
		
		public override object ResizeBitmap (object handle, double width, double height)
		{
			var pix = (Gdk.Pixbuf)handle;
			return pix.ScaleSimple ((int)width, (int)height, Gdk.InterpType.Bilinear);
		}
		
		public override object CopyBitmap (object handle)
		{
			var pix = (Gdk.Pixbuf)handle;
			return pix.Copy ();
		}
		
		public override void CopyBitmapArea (object srcHandle, int srcX, int srcY, int width, int height, object destHandle, int destX, int destY)
		{
			var pixSrc = (Gdk.Pixbuf)srcHandle;
			var pixDst = (Gdk.Pixbuf)destHandle;
			pixSrc.CopyArea (srcX, srcY, width, height, pixDst, destX, destY);
		}
		
		public override object CropBitmap (object handle, int srcX, int srcY, int width, int height)
		{
			var pix = (Gdk.Pixbuf)handle;
			Gdk.Pixbuf res = new Gdk.Pixbuf (pix.Colorspace, pix.HasAlpha, pix.BitsPerSample, width, height);
			res.Fill (0);
			pix.CopyArea (srcX, srcY, width, height, res, 0, 0);
			return res;
		}
		
		public override object ChangeBitmapOpacity (object backend, double opacity)
		{
			Gdk.Pixbuf image = (Gdk.Pixbuf) backend;
			Gdk.Pixbuf result = image.Copy ();
			result.Fill (0);
			result = result.AddAlpha (true, 0, 0, 0);
			image.Composite (result, 0, 0, image.Width, image.Height, 0, 0, 1, 1, Gdk.InterpType.Bilinear, (int)(255 * opacity));
			return result;
		}

		public override bool IsBitmap (object handle)
		{
			return handle is Gdk.Pixbuf;
		}

		public override object ConvertToBitmap (object handle, double width, double height)
		{
			Gdk.Pixbuf result = CreateBitmap ((string)handle, width, height);

			if (result != null) {
				int bw = (int) width;
				int bh = (int) height;
				if (result.Width != bw || result.Height != bh)
					return ResizeBitmap (result, bw, bh);
			}

			return result;
		}

		internal static Gdk.Pixbuf CreateBitmap (string stockId, double width, double height)
		{
			Gdk.Pixbuf result = null;
			
			Gtk.IconSet iconset = Gtk.IconFactory.LookupDefault (stockId);
			if (iconset != null) {
				// Find the size that better fits the requested size
				Gtk.IconSize gsize = Util.GetBestSizeFit (width);
				result = iconset.RenderIcon (Gtk.Widget.DefaultStyle, Gtk.TextDirection.Ltr, Gtk.StateType.Normal, gsize, null, null);
			}
			
			if (result == null && Gtk.IconTheme.Default.HasIcon (stockId))
				result = Gtk.IconTheme.Default.LoadIcon (stockId, (int)width, (Gtk.IconLookupFlags)0);

			return result;
		}
	}
}

