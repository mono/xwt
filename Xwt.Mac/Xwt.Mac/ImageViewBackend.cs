// 
// ImageViewBackend.cs
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
using MonoMac.AppKit;
using Xwt.Drawing;

namespace Xwt.Mac
{
	public class ImageViewBackend: ViewBackend<NSImageView,IWidgetEventSink>, IImageViewBackend
	{
		public ImageViewBackend ()
		{
		}
		
		public override void Initialize ()
		{
			base.Initialize ();
			ViewObject = new CustomNSImageView ();
		}

		public void SetImage (Image image)
		{
			if (image == null)
				throw new ArgumentNullException ("nativeImage");

			NSImage nativeImage = Toolkit.GetBackend (image) as NSImage;
			if (nativeImage == null)
				nativeImage = Toolkit.GetBackend (image.ToBitmap ()) as NSImage;
			if (nativeImage == null)
				throw new ArgumentException ("nativeImage is not of the expected type", "nativeImage");

			Widget.Image = nativeImage;
			Widget.SetFrameSize (Widget.Image.Size);
		}
	}
	
	class CustomNSImageView: NSImageView, IViewObject
	{
		public NSView View {
			get {
				return this;
			}
		}

		public ViewBackend Backend { get; set; }
	}
}

