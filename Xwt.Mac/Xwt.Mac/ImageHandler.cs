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
using MonoMac.AppKit;
using MonoMac.Foundation;
using System.Drawing;
using System.Collections.Generic;

namespace Xwt.Mac
{
	public class ImageHandler: ImageBackendHandler
	{
		static Dictionary<string, NSImage> stockIcons = new Dictionary<string, NSImage> ();
		
		public override object LoadFromStream (System.IO.Stream stream)
		{
			using (NSData data = NSData.FromStream (stream)) {
				return new NSImage (data);
			}
		}
		
		public override object LoadFromFile (string file)
		{
			return new NSImage (file);
		}
		
		public override object LoadFromIcon (string id, IconSize size)
		{
			NSImage img;
			if (!stockIcons.TryGetValue (id + size, out img)) {
				img = LoadStockIcon (id, size);
				stockIcons [id + size] = img;
			}
			return img;
		}
		
		public override Size GetSize (object handle)
		{
			NSImage img = (NSImage)handle;
			return new Size ((int)img.Size.Width, (int)img.Size.Height);
		}
		
		public override object Resize (object handle, double width, double height)
		{
			throw new NotImplementedException ();
		}
		
		public override object Copy (object handle)
		{
			throw new NotImplementedException ();
		}
		
		public override void CopyArea (object backend, int srcX, int srcY, int width, int height, object dest, int destX, int destY)
		{
			throw new NotImplementedException ();
		}
		
		public override object Crop (object backend, int srcX, int srcY, int width, int height)
		{
			throw new NotImplementedException ();
		}
		
		public override object ChangeOpacity (object backend, double opacity)
		{
			throw new NotImplementedException ();
		}
		
		static NSImage FromResource (string res)
		{
			var stream = typeof(ImageHandler).Assembly.GetManifestResourceStream (res);
			using (stream)
			using (NSData data = NSData.FromStream (stream)) {
				return new NSImage (data);
			}
		}
		
		static NSImage LoadStockIcon (String id, IconSize size)
		{
			switch (id) {
			case StockIcons.ZoomIn: return FromResource ("magnifier-zoom-in.png");
			case StockIcons.ZoomOut: return FromResource ("magnifier-zoom-out.png");
			}
			throw new NotSupportedException ();
		}
	}
}

