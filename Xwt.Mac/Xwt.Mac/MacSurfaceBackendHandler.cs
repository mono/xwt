//
// MacSurfaceBackendHandler.cs
//
// Author:
//       Lluis Sanchez Gual <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin, Inc (http://www.xamarin.com)
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
using MonoMac.CoreGraphics;
using MonoMac.AppKit;

namespace Xwt.Mac
{
	public class MacSurfaceBackendHandler: SurfaceBackendHandler
	{
		public override object CreateSurface (double width, double height, double scaleFactor)
		{
			int pixelWidth = (int)(width * scaleFactor);
			int pixelHeight = (int)(height * scaleFactor);
			int bytesPerRow = pixelWidth * 4;
			var flags = CGBitmapFlags.ByteOrderDefault | CGBitmapFlags.PremultipliedFirst;

			var bmp = new CGBitmapContext (IntPtr.Zero, pixelWidth, pixelHeight, 8, bytesPerRow, Util.DeviceRGBColorSpace, flags);
			//			bmp.TranslateCTM (0, pixelHeight);
			//bmp.ScaleCTM ((float)scaleFactor, (float)-scaleFactor);
			return new MacSurface {
				Context = new CGContextBackend {
					Context = bmp
				}
			};
		}

		public override object CreateSurfaceCompatibleWithWidget (object widgetBackend, double width, double height)
		{
			ViewBackend view = (ViewBackend)widgetBackend;
			return CreateSurface (width, height, Util.GetScaleFactor (view.Widget));
		}

		public override object CreateSurfaceCompatibleWithSurface (object surfaceBackend, double width, double height)
		{
			throw new NotSupportedException ();
		}

		public override object CreateContext (object backend)
		{
			return ((MacSurface)backend).Context;
		}
	}

	class MacSurface
	{
		CGImage image;

		public CGContextBackend Context;

		public CGImage Image {
			get {
				if (image == null || Context.Damaged) {
					image = ((CGBitmapContext)Context.Context).ToImage ();
					Context.Damaged = false;
				}
				return image;
			}
		}
	}
}

