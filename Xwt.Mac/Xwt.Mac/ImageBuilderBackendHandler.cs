// 
// ImageBuilderBackendHandler.cs
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
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using System.Drawing;

namespace Xwt.Mac
{
	public class MacImageBuilderBackendHandler: ImageBuilderBackendHandler
	{
		public MacImageBuilderBackendHandler ()
		{
		}

		#region IImageBuilderBackendHandler implementation
		public override object CreateImageBuilder (int width, int height, ImageFormat format)
		{
			var flags = CGBitmapFlags.ByteOrderDefault;
			int bytesPerRow;
			switch (format) {

			case ImageFormat.ARGB32:
				bytesPerRow = width * 4;
				flags |= CGBitmapFlags.PremultipliedFirst;
				break;

			case ImageFormat.RGB24:
				bytesPerRow = width * 3;
				flags |= CGBitmapFlags.None;
				break;

			default:
				throw new NotImplementedException ("ImageFormat: " + format.ToString ());
			}
			return new CGContextBackend {
				Context = new CGBitmapContext (IntPtr.Zero, width, height, 8, bytesPerRow, Util.DeviceRGBColorSpace, flags),
				Size = new SizeF (width, height)
			};
		}

		public override object CreateContext (object backend)
		{
			return backend;
		}

		public override object CreateImage (object backend)
		{
			var gc = (CGContextBackend)backend;
			return new NSImage (((CGBitmapContext)gc.Context).ToImage (), gc.Size);
		}

		public override void Dispose (object backend)
		{
			((CGContextBackend)backend).Context.Dispose ();
		}
		#endregion


	}
}

