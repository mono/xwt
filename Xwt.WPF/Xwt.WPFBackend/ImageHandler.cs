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

		public override Drawing.Image GetStockIcon (string id)
		{
			var img1 = RenderStockIcon (id, NativeStockIconOptions.Small);
			var img2 = RenderStockIcon (id, NativeStockIconOptions.Large);
			var img3 = RenderStockIcon (id, NativeStockIconOptions.ShellSize);
			var img4 = RenderStockIcon (id, default (NativeStockIconOptions));

			return Xwt.Drawing.Image.FromMultipleSizes (
				ApplicationContext.Toolkit.WrapImage (img1),
				ApplicationContext.Toolkit.WrapImage (img2),
				ApplicationContext.Toolkit.WrapImage (img3),
				ApplicationContext.Toolkit.WrapImage (img4)
			);
		}

		object RenderStockIcon (string id, NativeStockIconOptions options)
		{
			if (Environment.OSVersion.Version.Major <= 5)
				throw new NotImplementedException ();

			switch (id) {
				case StockIconId.Add:
					using (var s = typeof (ImageHandler).Assembly.GetManifestResourceStream ("Xwt.WPF.icons.list-add.png"))
						return LoadFromStream (s);
				case StockIconId.Remove:
					using (var s = typeof (ImageHandler).Assembly.GetManifestResourceStream ("Xwt.WPF.icons.list-remove.png"))
						return LoadFromStream (s);

				case StockIconId.Error:
					return NativeMethods.GetImage (NativeStockIcon.Error, options);
				case StockIconId.Information:
					return NativeMethods.GetImage (NativeStockIcon.Info, options);
				case StockIconId.OrientationLandscape:
				case StockIconId.OrientationPortrait:
					return NativeMethods.GetImage (NativeStockIcon.Help, options);
				//throw new NotImplementedException();
				case StockIconId.Question:
					return NativeMethods.GetImage (NativeStockIcon.Help, options);
				case StockIconId.Warning:
					return NativeMethods.GetImage (NativeStockIcon.Warning, options);
				case StockIconId.Zoom100:
				case StockIconId.ZoomFit:
				case StockIconId.ZoomIn:
				case StockIconId.ZoomOut:
					return NativeMethods.GetImage (NativeStockIcon.Find, options);

				default:
					throw new ArgumentException ("Unknown icon id", "id");
			}
		}

		public override Xwt.Drawing.Color GetBitmapPixel (object handle, int x, int y)
		{
			throw new NotImplementedException ();
		}

		public override void SetBitmapPixel (object handle, int x, int y, Drawing.Color color)
		{
			throw new NotImplementedException ();
		}

		private static double WidthToDPI (SWMI.BitmapSource img, double pixels)
		{
			return pixels * 96 / img.DpiX;
		}

		private static double HeightToDPI (SWMI.BitmapSource img, double pixels)
		{
			return pixels * 96 / img.DpiY;
		}

		public override object ConvertToBitmap (object handle, double width, double height)
		{
			throw new NotImplementedException ();
		}

		public override bool HasMultipleSizes (object handle)
		{
			return false;
		}

		public override bool IsBitmap (object handle)
		{
			return true;
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

		public override object ResizeBitmap (object handle, double width, double height)
		{
			var oldImg = (SWMI.BitmapSource)handle;

			width = WidthToDPI (oldImg, width);
			height = HeightToDPI (oldImg, height);

			SWM.DrawingVisual visual = new SWM.DrawingVisual ();
			using (SWM.DrawingContext ctx = visual.RenderOpen ())
			{
				ctx.DrawImage (oldImg, new System.Windows.Rect (0, 0, width, height));
			}

			SWMI.RenderTargetBitmap bmp = new SWMI.RenderTargetBitmap ((int)width, (int)height, oldImg.DpiX, oldImg.DpiY, PixelFormats.Pbgra32);
			bmp.Render (visual);

			return bmp;
		}

		public override object CopyBitmap (object handle)
		{
			return ((SWMI.BitmapSource)handle).Clone ();
		}

		public override object CropBitmap(object handle, int srcX, int srcY, int w, int h)
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

			SWMI.RenderTargetBitmap bmp = new SWMI.RenderTargetBitmap ((int)width, (int)height, oldImg.DpiX, oldImg.DpiY, PixelFormats.Pbgra32);
			bmp.Render (visual);

			return bmp;
		}

		public override object ChangeBitmapOpacity (object backend, double opacity)
		{
			throw new System.NotImplementedException ();
/*			Bitmap bitmap = DataConverter.AsBitmap (backend);
			if (bitmap == null)
				throw new ArgumentException ();
			Bitmap result = new Bitmap (bitmap.Width, bitmap.Height, bitmap.PixelFormat);
			Graphics g = Graphics.FromImage (result);
			WpfContextBackendHandler.DrawImageCore (g, bitmap, 0, 0, bitmap.Width, bitmap.Height, (float)opacity);
			g.Dispose ();
			return result;
*/		}

		public override void CopyBitmapArea (object srcHandle, int srcX, int srcY, int width, int height, object destHandle, int destX, int destY)
		{
			throw new NotImplementedException ();
		}
	}
}
