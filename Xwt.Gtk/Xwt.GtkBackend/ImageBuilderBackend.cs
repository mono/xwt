// 
// ImageBuilderBackend.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2012 Xamarin Inc
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
using Xwt.CairoBackend;

namespace Xwt.GtkBackend
{
	public class ImageBuilderBackend: ImageBuilderBackendHandler
	{
		public ImageBuilderBackend ()
		{
		}

		#region IImageBuilderBackendHandler implementation

		public override bool DisposeHandleOnUiThread {
			get {
				return true;
			}
		}

		public override object CreateImageBuilder (int width, int height, ImageFormat format)
		{
			Cairo.Format cformat;
			switch (format) {
			case ImageFormat.ARGB32: cformat = Cairo.Format.ARGB32; break;
			default: cformat = Cairo.Format.RGB24; break;
			}
			return new Cairo.ImageSurface (cformat, width, height);
		}

		public override object CreateContext (object backend)
		{
			Cairo.Surface sf = (Cairo.Surface) backend;
			CairoContextBackend ctx = new CairoContextBackend (1);
			ctx.Context = new Cairo.Context (sf);
			return ctx;
		}

		public override object CreateImage (object backend)
		{
			var pix = CreatePixbuf ((Cairo.ImageSurface)backend);
			return new GtkImage (pix);
		}

		public static Gdk.Pixbuf CreatePixbuf (Cairo.ImageSurface sf)
		{
			byte[] cdata = sf.Data;
			int nbytes = sf.Format == Cairo.Format.ARGB32 ? 4 : 3;
			byte[] data = new byte[(cdata.Length / 4) * nbytes];
			
			int i = 0;
			int n = 0;
			var stride = sf.Stride;
			int ncols = sf.Width;
			
			if (BitConverter.IsLittleEndian) {
				var row = sf.Height;
				while (row-- > 0) {
					var prevPos = n;
					var col = ncols;
					while (col-- > 0) {
						double alphaFactor = nbytes == 4 ? 255d / cdata [n + 3] : 1;
						data[i] = (byte) (cdata[n+2] * alphaFactor + 0.5);
						data[i + 1] = (byte) (cdata[n+1] * alphaFactor + 0.5);
						data[i + 2] = (byte) (cdata[n+0] * alphaFactor + 0.5);
						if (nbytes == 4)
							data[i + 3] = cdata [n + 3];
						n += 4;
						i += nbytes;
					}
					n = prevPos + stride;
				}
			} else {
				var row = sf.Height;
				while (row-- > 0) {
					var prevPos = n;
					var col = ncols;
					while (col-- > 0) {
						double alphaFactor = nbytes == 4 ? 255d / cdata [n + 3] : 1;
						data[i] = (byte) (cdata[n+1] * alphaFactor + 0.5);
						data[i + 1] = (byte) (cdata[n+2] * alphaFactor + 0.5);
						data[i + 2] = (byte) (cdata[n+3] * alphaFactor + 0.5);
						if (nbytes == 4)
							data[i + 3] = cdata [n + 0];
						n += 4;
						i += nbytes;
					}
					n = prevPos + stride;
				}
			}
			
			return new Gdk.Pixbuf (data, Gdk.Colorspace.Rgb, nbytes == 4, 8, sf.Width, sf.Height, sf.Width * nbytes, null);
		}

		public override void Dispose (object backend)
		{
			IDisposable sf = (IDisposable) backend;
			sf.Dispose ();
		}
		#endregion
	}
}

