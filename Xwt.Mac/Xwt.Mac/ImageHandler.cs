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
using System.Linq;
using Xwt.Backends;
using MonoMac.AppKit;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;
using System.Drawing;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Xwt.Drawing;

namespace Xwt.Mac
{
	public class ImageHandler: ImageBackendHandler
	{
		static readonly IntPtr sel_alloc = new Selector ("alloc").Handle;
		static readonly IntPtr sel_release = new Selector ("release").Handle;
		static readonly IntPtr sel_initWithIconRef = new Selector ("initWithIconRef:").Handle;
		static readonly IntPtr cls_NSImage = new Class (typeof (NSImage)).Handle;

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
		
		public override Xwt.Drawing.Color GetPixel (object handle, int x, int y)
		{
			NSImage img = (NSImage)handle;
			NSBitmapImageRep bitmap = img.Representations ().OfType<NSBitmapImageRep> ().FirstOrDefault ();
			if (bitmap != null)
				return bitmap.ColorAt (x, y).ToXwtColor ();
			else
				throw new InvalidOperationException ("Not a bitmnap image");
		}
		
		public override void SetPixel (object handle, int x, int y, Xwt.Drawing.Color color)
		{
			NSImage img = (NSImage)handle;
			NSBitmapImageRep bitmap = img.Representations ().OfType<NSBitmapImageRep> ().FirstOrDefault ();
			if (bitmap != null)
				bitmap.SetColorAt (color.ToNSColor (), x, y);
			else
				throw new InvalidOperationException ("Not a bitmnap image");
		}
		
		public override Size GetSize (object handle)
		{
			NSImage img = (NSImage)handle;
			return new Size ((int)img.Size.Width, (int)img.Size.Height);
		}
		
		public override object Resize (object handle, double width, double height)
		{
			NSImage newImg = (NSImage)Copy (handle);
			newImg.Size = new SizeF ((float)width, (float)height);
			return newImg;
		}
		
		public override object Copy (object handle)
		{
			return ((NSImage)handle).Copy ();
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
			//http://stackoverflow.com/a/2868928/578190
			NSImage img = (NSImage)backend;
			NSImage newImg = new NSImage (img.Size);

			newImg.LockFocus ();
			img.Draw (PointF.Empty, RectangleF.Empty, NSCompositingOperation.SourceOver, (float)opacity);
			newImg.UnlockFocus ();

			return newImg;
		}
		
		static NSImage FromResource (string res)
		{
			var stream = typeof(ImageHandler).Assembly.GetManifestResourceStream (res);
			using (stream)
			using (NSData data = NSData.FromStream (stream)) {
				return new NSImage (data);
			}
		}
		
		static NSImage LoadStockIcon (string id, IconSize size)
		{
			NSImage image = null;

			switch (id) {
			case StockIcons.ZoomIn:  image = FromResource ("magnifier-zoom-in.png"); break;
			case StockIcons.ZoomOut: image = FromResource ("magnifier-zoom-out.png"); break;
			}

			IntPtr iconRef;
			var type = Util.ToIconType (id);
			if (type != 0 && GetIconRef (-32768/*kOnSystemDisk*/, 1835098995/*kSystemIconsCreator*/, type, out iconRef) == 0) {
				try {
					image = new NSImage (Messaging.IntPtr_objc_msgSend_IntPtr (Messaging.IntPtr_objc_msgSend (cls_NSImage, sel_alloc), sel_initWithIconRef, iconRef));
					// NSImage (IntPtr) ctor retains, but since it is the sole owner, we don't want that
					Messaging.void_objc_msgSend (image.Handle, sel_release);
				} finally {
					ReleaseIconRef (iconRef);
				}
			}

			if (image != null)
				image.Size = Util.ToIconSize (size);
			return image;
		}

		[DllImport ("/System/Library/Frameworks/CoreServices.framework/Frameworks/LaunchServices.framework/LaunchServices")]
		static extern int GetIconRef (short vRefNum, int creator, int iconType, out IntPtr iconRef);
		[DllImport ("/System/Library/Frameworks/CoreServices.framework/Frameworks/LaunchServices.framework/LaunchServices")]
		static extern int ReleaseIconRef (IntPtr iconRef);
	}
}

