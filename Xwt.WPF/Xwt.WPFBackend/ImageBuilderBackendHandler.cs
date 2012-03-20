//
// ImageBuilderBackendHandler.cs
//
// Author:
//       Eric Maupin <ermau@xamarin.com>
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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xwt.Backends;
using Xwt.Drawing;

namespace Xwt.WPFBackend
{
	public class ImageBuilderBackendHandler
		: IImageBuilderBackendHandler
	{
		public object CreateImageBuilder (int width, int height, ImageFormat format)
		{
			return new Bitmap (width, height, format.ToPixelFormat ());
		}

		public object CreateContext (object backend)
		{
			Bitmap bmp = (Bitmap) backend;
			return new DrawingContext (Graphics.FromImage (bmp));
		}

		public object CreateImage (object backend)
		{
			Bitmap bmp = (Bitmap) backend;
			IntPtr ptr = bmp.GetHbitmap ();

			try {
				return Imaging.CreateBitmapSourceFromHBitmap (ptr, IntPtr.Zero, Int32Rect.Empty,
				                                              BitmapSizeOptions.FromEmptyOptions ());
			}
			finally {
				DeleteObject (ptr);
			}
		}

		public void Dispose (object backend)
		{
			Bitmap bmp = (Bitmap) backend;
			bmp.Dispose();
		}

		[DllImport ("gdi32")]
		private static extern int DeleteObject (IntPtr o);
	}
}
