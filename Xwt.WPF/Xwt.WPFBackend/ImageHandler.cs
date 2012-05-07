// 
// ImageHandler.cs
//  
// Author:
//	   Luís Reis <luiscubal@gmail.com>
//	   Eric Maupin <ermau@xamarin.com>
// 
// Copyright (c) 2012 Luís Reis
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
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xwt.Backends;
using Xwt.WPFBackend.Interop;
using Color = System.Drawing.Color;
using SWM = System.Windows.Media;
using SWMI = System.Windows.Media.Imaging;

namespace Xwt.WPFBackend
{
	public class ImageHandler: ImageBackendHandler
	{
		public override object LoadFromStream (Stream stream)
		{
			var img = new SWMI.BitmapImage ();
			img.BeginInit();
			img.CacheOption = SWMI.BitmapCacheOption.OnLoad;
			img.StreamSource = stream;
			img.EndInit();

			return img;
		}

		public override object LoadFromIcon (string id, IconSize size)
		{
			if (Environment.OSVersion.Version.Major <= 5)
				throw new NotImplementedException();

			var options = GetNativeStockOptionsFromSize (size);

			switch (id) {
			case StockIcons.Add:
					using (var s = typeof (ImageHandler).Assembly.GetManifestResourceStream ("list-add.png"))
						return LoadFromStream (s);
				//throw new NotImplementedException();
			case StockIcons.Error:
				return NativeMethods.GetImage (NativeStockIcon.Error, options);
			case StockIcons.Information:
				return NativeMethods.GetImage (NativeStockIcon.Info, options);
			case StockIcons.OrientationLandscape:
			case StockIcons.OrientationPortrait:
				return NativeMethods.GetImage (NativeStockIcon.Help, options);
				//throw new NotImplementedException();
			case StockIcons.Question:
				return NativeMethods.GetImage (NativeStockIcon.Help, options);
			case StockIcons.Remove:
				using (var s = typeof (ImageHandler).Assembly.GetManifestResourceStream ("list-remove.png"))
					return LoadFromStream (s);
			case StockIcons.Warning:
				return NativeMethods.GetImage (NativeStockIcon.Warning, options);
			case StockIcons.Zoom100:
			case StockIcons.ZoomFit:
			case StockIcons.ZoomIn:
			case StockIcons.ZoomOut:
				return NativeMethods.GetImage (NativeStockIcon.Find, options);

			default:
				throw new ArgumentException ("Unknown icon id", "id");
			}
		}

		public override Xwt.Drawing.Color GetPixel (object handle, int x, int y)
		{
			throw new NotImplementedException ();
		}

		public override void SetPixel (object handle, int x, int y, Drawing.Color color)
		{
			throw new NotImplementedException ();
		}

		private static NativeStockIconOptions GetNativeStockOptionsFromSize (IconSize size)
		{
			switch (size) {
			case IconSize.Small:
				return NativeStockIconOptions.Small;

			default:
				return NativeStockIconOptions.Large;
			}
		}

		private static double WidthToDPI (SWMI.BitmapSource img, double pixels)
		{
			return pixels * 96 / img.DpiX;
		}

		private static double HeightToDPI (SWMI.BitmapSource img, double pixels)
		{
			return pixels * 96 / img.DpiY;
		}

		public override Size GetSize (object handle)
		{
			BitmapSource source = handle as BitmapSource;
			if (source != null)
				return new Size (source.PixelWidth, source.PixelHeight);

			Bitmap bitmp = handle as Bitmap;
			if (bitmp != null)
				return new Size (bitmp.Width, bitmp.Height);

			throw new ArgumentException();
		}

		public override object Resize (object handle, double width, double height)
		{
			var oldImg = (SWMI.BitmapSource)handle;

			width = WidthToDPI (oldImg, width);
			height = HeightToDPI (oldImg, height);

			SWM.DrawingVisual visual = new SWM.DrawingVisual ();
			using (SWM.DrawingContext ctx = visual.RenderOpen ())
			{
				ctx.DrawImage (oldImg, new System.Windows.Rect (0, 0, width, height));
			}

			SWMI.RenderTargetBitmap bmp = new SWMI.RenderTargetBitmap ((int)width, (int)height, oldImg.DpiX, oldImg.DpiY, oldImg.Format);
			bmp.Render (visual);

			return bmp;
		}

		public override object Copy (object handle)
		{
			return ((SWMI.BitmapSource)handle).Clone ();
		}

		public override object Crop(object handle, int srcX, int srcY, int w, int h)
		{
			var oldImg = (SWMI.BitmapSource)handle;

			double width = WidthToDPI (oldImg, w);
			double height = HeightToDPI (oldImg, h);

			SWM.DrawingVisual visual = new SWM.DrawingVisual ();
			using (SWM.DrawingContext ctx = visual.RenderOpen ())
			{
				//Not sure whether this actually works, untested
				ctx.DrawImage(oldImg, new System.Windows.Rect (-srcX, -srcY, srcX+width, srcY+height));
			}

			SWMI.RenderTargetBitmap bmp = new SWMI.RenderTargetBitmap ((int)width, (int)height, oldImg.DpiX, oldImg.DpiY, oldImg.Format);
			bmp.Render (visual);

			return bmp;
		}

		public override object ChangeOpacity (object backend, double opacity)
		{
			Bitmap bitmap = DataConverter.AsBitmap (backend);
			if (bitmap == null)
				throw new ArgumentException ();
			Bitmap result = new Bitmap (bitmap.Width, bitmap.Height, bitmap.PixelFormat);
			Graphics g = Graphics.FromImage (result);
			ContextBackendHandler.DrawImageCore (g, bitmap, 0, 0, bitmap.Width, bitmap.Height, (float)opacity);
			g.Dispose ();
			return result;
		}

		public override void CopyArea (object srcHandle, int srcX, int srcY, int width, int height, object destHandle, int destX, int destY)
		{
			throw new NotImplementedException ();
		}
	}
}
