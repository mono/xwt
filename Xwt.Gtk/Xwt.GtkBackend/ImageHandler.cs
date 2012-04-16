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

namespace Xwt.GtkBackend
{
	public class ImageHandler: ImageBackendHandler
	{
		public override object LoadFromStream (System.IO.Stream stream)
		{
			using (Gdk.PixbufLoader loader = new Gdk.PixbufLoader (stream))
				return loader.Pixbuf;
		}
		
		public override object LoadFromIcon (string id, IconSize size)
		{
			string stockId = Util.ToGtkStock (id);
			var gsize = Util.ToGtkSize (size);
			
			Gtk.IconSet iconset = Gtk.IconFactory.LookupDefault (stockId);
			if (iconset != null) 
				return iconset.RenderIcon (Gtk.Widget.DefaultStyle, Gtk.TextDirection.Ltr, Gtk.StateType.Normal, gsize, null, null);
			
			if (Gtk.IconTheme.Default.HasIcon (stockId)) {
				int w, h;
				Gtk.Icon.SizeLookup (gsize, out w, out h);
				Gdk.Pixbuf result = Gtk.IconTheme.Default.LoadIcon (stockId, h, (Gtk.IconLookupFlags)0);
				return result;
			}
			return null;
		}
		
		public override void SetPixel (object handle, int x, int y, Xwt.Drawing.Color color)
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
		
		public override Xwt.Drawing.Color GetPixel (object handle, int x, int y)
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
		
		public override Size GetSize (object handle)
		{
			var pix = (Gdk.Pixbuf)handle;
			return new Size (pix.Width, pix.Height);
		}
		
		public override object Resize (object handle, double width, double height)
		{
			var pix = (Gdk.Pixbuf)handle;
			return pix.ScaleSimple ((int)width, (int)height, Gdk.InterpType.Bilinear);
		}
		
		public override object Copy (object handle)
		{
			var pix = (Gdk.Pixbuf)handle;
			return pix.Copy ();
		}
		
		public override void CopyArea (object srcHandle, int srcX, int srcY, int width, int height, object destHandle, int destX, int destY)
		{
			var pixSrc = (Gdk.Pixbuf)srcHandle;
			var pixDst = (Gdk.Pixbuf)destHandle;
			pixSrc.CopyArea (srcX, srcY, width, height, pixDst, destX, destY);
		}
		
		public override object Crop (object handle, int srcX, int srcY, int width, int height)
		{
			var pix = (Gdk.Pixbuf)handle;
			Gdk.Pixbuf res = new Gdk.Pixbuf (pix.Colorspace, pix.HasAlpha, pix.BitsPerSample, width, height);
			res.Fill (0);
			pix.CopyArea (srcX, srcY, width, height, res, 0, 0);
			return res;
		}
		
		public override object ChangeOpacity (object backend, double opacity)
		{
			Gdk.Pixbuf image = (Gdk.Pixbuf) backend;
			Gdk.Pixbuf result = image.Copy ();
			result.Fill (0);
			result = result.AddAlpha (true, 0, 0, 0);
			image.Composite (result, 0, 0, image.Width, image.Height, 0, 0, 1, 1, Gdk.InterpType.Bilinear, (int)(255 * opacity));
			return result;
		}
	}
}

