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
using System.Drawing;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Xwt.Drawing;
using System.IO;

#if MONOMAC
using nint = System.Int32;
using nfloat = System.Single;
using CGRect = System.Drawing.RectangleF;
using CGPoint = System.Drawing.PointF;
using CGSize = System.Drawing.SizeF;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;
using MonoMac.CoreGraphics;
#else
using Foundation;
using AppKit;
using CoreGraphics;
using ObjCRuntime;
#endif

namespace Xwt.Mac
{
	public class ImageHandler: ImageBackendHandler
	{
		static readonly IntPtr sel_alloc = new Selector ("alloc").Handle;
		static readonly IntPtr sel_release = new Selector ("release").Handle;
		static readonly IntPtr sel_initWithIconRef = new Selector ("initWithIconRef:").Handle;
		static readonly IntPtr cls_NSImage = new Class (typeof (NSImage)).Handle;

		static Dictionary<string, NSImage> stockIcons = new Dictionary<string, NSImage> ();
		
		public override object LoadFromStream (Stream stream)
		{
			using (NSData data = NSData.FromStream (stream)) {
				return new NSImage (data);
			}
		}
		
		public override object LoadFromFile (string file)
		{
			return new NSImage (file);
		}

		public override object CreateMultiResolutionImage (IEnumerable<object> images)
		{
			NSImage res = new NSImage ();
			foreach (NSImage img in images)
				res.AddRepresentations (img.Representations ());
			return res;
		}

		public override object CreateMultiSizeIcon (IEnumerable<object> images)
		{
			if (images.Count () == 1)
				return images.First ();

			NSImage res = new NSImage ();
			foreach (NSImage img in images)
				res.AddRepresentations (img.Representations ());
			return res;
		}

		public override object CreateCustomDrawn (ImageDrawCallback drawCallback)
		{
			return new CustomImage (ApplicationContext, drawCallback);
		}

		public override Xwt.Drawing.Image GetStockIcon (string id)
		{
			NSImage img;
			if (!stockIcons.TryGetValue (id, out img)) {
				img = LoadStockIcon (id);
				stockIcons [id] = img;
			}
			return ApplicationContext.Toolkit.WrapImage (img);
		}

		public override void SaveToStream (object backend, System.IO.Stream stream, ImageFileType fileType)
		{
			NSImage img = backend as NSImage;
			if (img == null)
				throw new NotSupportedException ();

			var imageData = img.AsTiff ();
			var imageRep = (NSBitmapImageRep) NSBitmapImageRep.ImageRepFromData (imageData);
			var props = new NSDictionary ();
			imageData = imageRep.RepresentationUsingTypeProperties (fileType.ToMacFileType (), props);
			using (var s = imageData.AsStream ()) {
				s.CopyTo (stream);
			}
		}

		public override bool IsBitmap (object handle)
		{
			NSImage img = handle as NSImage;
			return img != null && img.Representations ().OfType<NSBitmapImageRep> ().Any ();
		}

		public override object ConvertToBitmap (object handle, double width, double height, double scaleFactor, ImageFormat format)
		{
			int pixelWidth = (int)(width * scaleFactor);
			int pixelHeight = (int)(height * scaleFactor);

			if (handle is CustomImage) {
				var flags = CGBitmapFlags.ByteOrderDefault;
				int bytesPerRow;
				switch (format) {
				case ImageFormat.ARGB32:
					bytesPerRow = pixelWidth * 4;
					flags |= CGBitmapFlags.PremultipliedFirst;
					break;

				case ImageFormat.RGB24:
					bytesPerRow = pixelWidth * 3;
					flags |= CGBitmapFlags.None;
					break;

				default:
					throw new NotImplementedException ("ImageFormat: " + format.ToString ());
				}

				var bmp = new CGBitmapContext (IntPtr.Zero, pixelWidth, pixelHeight, 8, bytesPerRow, Util.DeviceRGBColorSpace, flags);
				bmp.TranslateCTM (0, pixelHeight);
				bmp.ScaleCTM ((float)scaleFactor, (float)-scaleFactor);

				var ctx = new CGContextBackend {
					Context = bmp,
					Size = new SizeF ((float)width, (float)height),
					InverseViewTransform = bmp.GetCTM ().Invert (),
					ScaleFactor = scaleFactor
				};

				var ci = (CustomImage)handle;
				ci.DrawInContext (ctx);

				var img = new NSImage (((CGBitmapContext)bmp).ToImage (), new SizeF (pixelWidth, pixelHeight));
				var imageData = img.AsTiff ();
				var imageRep = (NSBitmapImageRep)NSBitmapImageRep.ImageRepFromData (imageData);
				var im = new NSImage ();
				im.AddRepresentation (imageRep);
				im.Size = new SizeF ((float)width, (float)height);
				bmp.Dispose ();
				return im;
			}
			else {
				NSImage img = (NSImage)handle;
				NSBitmapImageRep bitmap = img.Representations ().OfType<NSBitmapImageRep> ().FirstOrDefault ();
				if (bitmap == null) {
					var imageData = img.AsTiff ();
					var imageRep = (NSBitmapImageRep)NSBitmapImageRep.ImageRepFromData (imageData);
					var im = new NSImage ();
					im.AddRepresentation (imageRep);
					im.Size = new SizeF ((float)width, (float)height);
					return im;
				}
				return handle;
			}
		}
		
		public override Xwt.Drawing.Color GetBitmapPixel (object handle, int x, int y)
		{
			NSImage img = (NSImage)handle;
			NSBitmapImageRep bitmap = img.Representations ().OfType<NSBitmapImageRep> ().FirstOrDefault ();
			if (bitmap != null)
				return bitmap.ColorAt (x, y).ToXwtColor ();
			else
				throw new InvalidOperationException ("Not a bitmnap image");
		}
		
		public override void SetBitmapPixel (object handle, int x, int y, Xwt.Drawing.Color color)
		{
			NSImage img = (NSImage)handle;
			NSBitmapImageRep bitmap = img.Representations ().OfType<NSBitmapImageRep> ().FirstOrDefault ();
			if (bitmap != null)
				bitmap.SetColorAt (color.ToNSColor (), x, y);
			else
				throw new InvalidOperationException ("Not a bitmnap image");
		}

		public override bool HasMultipleSizes (object handle)
		{
			NSImage img = (NSImage)handle;
			return img.Size.Width == 0 && img.Size.Height == 0;
		}
		
		public override Size GetSize (object handle)
		{
			NSImage img = (NSImage)handle;
			NSBitmapImageRep bitmap = img.Representations ().OfType<NSBitmapImageRep> ().FirstOrDefault ();
			if (bitmap != null)
				return new Size (bitmap.PixelsWide, bitmap.PixelsHigh);
			else
				return new Size ((int)img.Size.Width, (int)img.Size.Height);
		}
		
		public override object CopyBitmap (object handle)
		{
			return ((NSImage)handle).Copy ();
		}
		
		public override void CopyBitmapArea (object backend, int srcX, int srcY, int width, int height, object dest, int destX, int destY)
		{
			throw new NotImplementedException ();
		}
		
		public override object CropBitmap (object backend, int srcX, int srcY, int width, int height)
		{
			NSImage img = (NSImage)backend;
			NSBitmapImageRep bitmap = img.Representations ().OfType<NSBitmapImageRep> ().FirstOrDefault ();
			if (bitmap != null) {
				var empty = CGRect.Empty;
				var cgi = bitmap.AsCGImage (ref empty, null, null).WithImageInRect (new CGRect (srcX, srcY, width, height));
				NSImage res = new NSImage (cgi, new CGSize (width, height));
				cgi.Dispose ();
				return res;
			}
			else
				throw new InvalidOperationException ("Not a bitmap image");
		}
		
		static NSImage FromResource (string res)
		{
			var stream = typeof(ImageHandler).Assembly.GetManifestResourceStream (res);
			using (stream)
			using (NSData data = NSData.FromStream (stream)) {
				return new NSImage (data);
			}
		}

		static NSImage NSImageFromResource (string id)
		{
			return (NSImage) Toolkit.GetBackend (Xwt.Drawing.Image.FromResource (typeof(ImageHandler), id));
		}
		
		static NSImage LoadStockIcon (string id)
		{
			switch (id) {
			case StockIconId.ZoomIn: return NSImageFromResource ("zoom-in.png");
			case StockIconId.ZoomOut: return NSImageFromResource ("zoom-out.png");
			}

			NSImage image = null;
			IntPtr iconRef;
			var type = Util.ToIconType (id);
			if (type != 0 && GetIconRef (-32768/*kOnSystemDisk*/, 1835098995/*kSystemIconsCreator*/, type, out iconRef) == 0) {
				try {
					var alloced = Messaging.IntPtr_objc_msgSend (cls_NSImage, sel_alloc);
					image = (NSImage) Runtime.GetNSObject (Messaging.IntPtr_objc_msgSend_IntPtr (alloced, sel_initWithIconRef, iconRef));
					// NSImage (IntPtr) ctor retains, but since it is the sole owner, we don't want that
					Messaging.void_objc_msgSend (image.Handle, sel_release);
				} finally {
					ReleaseIconRef (iconRef);
				}
			}

			return image;
		}

		[DllImport ("/System/Library/Frameworks/CoreServices.framework/Frameworks/LaunchServices.framework/LaunchServices")]
		static extern int GetIconRef (short vRefNum, int creator, int iconType, out IntPtr iconRef);
		[DllImport ("/System/Library/Frameworks/CoreServices.framework/Frameworks/LaunchServices.framework/LaunchServices")]
		static extern int ReleaseIconRef (IntPtr iconRef);
	}


	public class CustomImage: NSImage
	{
		ImageDrawCallback drawCallback;
		ApplicationContext actx;
		NSCustomImageRep imgRep;

		public CustomImage (ApplicationContext actx, ImageDrawCallback drawCallback)
		{
			this.actx = actx;
			this.drawCallback = drawCallback;
			imgRep = new NSCustomImageRep (new Selector ("drawIt:"), this);
			AddRepresentation (imgRep);
		}

		[Export ("drawIt:")]
		public void DrawIt (NSObject ob)
		{
			CGContext ctx = NSGraphicsContext.CurrentContext.GraphicsPort;
			if (!NSGraphicsContext.CurrentContext.IsFlipped) {
				// Custom drawing is done using flipped order, so if the target surface is not flipped, we need to flip it
				ctx.TranslateCTM (0, Size.Height);
				ctx.ScaleCTM (1, -1);
			}
			DrawInContext (ctx);
		}

		internal void DrawInContext (CGContext ctx)
		{
			var backend = new CGContextBackend {
				Context = ctx,
				InverseViewTransform = ctx.GetCTM ().Invert ()
			};
			DrawInContext (backend);
		}

		internal void DrawInContext (CGContextBackend ctx)
		{
			var s = ctx.Size != SizeF.Empty ? ctx.Size : Size;
			actx.InvokeUserCode (delegate {
				drawCallback (ctx, new Rectangle (0, 0, s.Width, s.Height));
			});
		}

		public CustomImage Clone ()
		{
			return new CustomImage (actx, drawCallback);
		}
	}
}

