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
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xwt.Backends;
using Xwt.WPFBackend.Interop;
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

			var bmp = img as BitmapSource;
			if (bmp != null && (bmp.DpiX != 96 || bmp.DpiY != 96))
				return new WpfImage (ConvertBitmapTo96DPI (bmp));

			return new WpfImage (img);
		}

		public static BitmapSource ConvertBitmapTo96DPI (BitmapSource bitmapImage)
		{
			double dpi = 96;
			int width = bitmapImage.PixelWidth;
			int height = bitmapImage.PixelHeight;

			int stride = width * (bitmapImage.Format.BitsPerPixel + 7) / 8;
			byte[] pixelData = new byte[stride * height];
			bitmapImage.CopyPixels (pixelData, stride, 0);

			return BitmapSource.Create (width, height, dpi, dpi, bitmapImage.Format, null, pixelData, stride);
		}

		public override void SaveToStream (object backend, Stream stream, Drawing.ImageFileType fileType)
		{
			var image = DataConverter.AsImageSource(backend) as BitmapSource;
			BitmapEncoder encoder;
			switch (fileType) {
				case Drawing.ImageFileType.Png: encoder = new PngBitmapEncoder (); break;
				case Drawing.ImageFileType.Jpeg: encoder = new JpegBitmapEncoder (); break;
				case Drawing.ImageFileType.Bmp: encoder = new BmpBitmapEncoder (); break;
				default: throw new NotSupportedException ("Image format not supported");
			}
			encoder.Frames.Add (BitmapFrame.Create (image));
			encoder.Save (stream);
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
					return new WpfImage (NativeMethods.GetImage (NativeStockIcon.Error, options));
				case StockIconId.Information:
					return new WpfImage (NativeMethods.GetImage (NativeStockIcon.Info, options));
				case StockIconId.OrientationLandscape:
				case StockIconId.OrientationPortrait:
					return new WpfImage (NativeMethods.GetImage (NativeStockIcon.Help, options));
				//throw new NotImplementedException();
				case StockIconId.Question:
					return new WpfImage (NativeMethods.GetImage (NativeStockIcon.Help, options));
				case StockIconId.Warning:
					return new WpfImage (NativeMethods.GetImage (NativeStockIcon.Warning, options));
				case StockIconId.Zoom100:
				case StockIconId.ZoomFit:
				case StockIconId.ZoomIn:
				case StockIconId.ZoomOut:
					return new WpfImage (NativeMethods.GetImage (NativeStockIcon.Find, options));

				default:
					throw new ArgumentException ("Unknown icon id", "id");
			}
		}

		public override Xwt.Drawing.Color GetBitmapPixel (object handle, int x, int y)
		{
			var wpfImage = (WpfImage)handle;
			BitmapSource img = wpfImage.Image as BitmapSource;
			if (img == null)
				throw new NotSupportedException ("Invalid image format");
			if (img.Format.BitsPerPixel != 32)
				throw new NotSupportedException ("Image format not supported");

			wpfImage.AllocatePixelData ();
			var offset = wpfImage.GetPixelOffset (x, y);
			return Xwt.Drawing.Color.FromBytes (wpfImage.PixelData[offset + 2], wpfImage.PixelData[offset + 1], wpfImage.PixelData[offset], wpfImage.PixelData[offset + 3]);
		}

		public override void SetBitmapPixel (object handle, int x, int y, Drawing.Color color)
		{
			var wpfImage = (WpfImage)handle;
			var img = (BitmapSource) wpfImage.Image;
			if (img == null)
				throw new NotSupportedException ("Invalid image format");
			if (img.Format.BitsPerPixel != 32)
				throw new NotSupportedException ("Image format not supported");

			var bitmapImage = img as WriteableBitmap;

			if (!(bitmapImage is WriteableBitmap)) {
				bitmapImage = new WriteableBitmap (img);
				((WpfImage)handle).Image = bitmapImage;
			}

			wpfImage.AllocatePixelData ();
			var offset = wpfImage.GetPixelOffset (x, y);
			wpfImage.PixelData[offset] = (byte)(color.Blue * 255);
			wpfImage.PixelData[offset + 1] = (byte)(color.Green * 255);
			wpfImage.PixelData[offset + 2] = (byte)(color.Red * 255);
			wpfImage.PixelData[offset + 3] = (byte)(color.Alpha * 255);

			bitmapImage.Lock ();
			bitmapImage.WritePixels (new Int32Rect (x, y, 1, 1), wpfImage.PixelData, wpfImage.Stride, offset);
			bitmapImage.Unlock ();
		}

		private static double WidthToDPI (SWMI.BitmapSource img, double pixels)
		{
			return pixels * 96 / img.DpiX;
		}

		private static double HeightToDPI (SWMI.BitmapSource img, double pixels)
		{
			return pixels * 96 / img.DpiY;
		}

		public static double WidthToPixels (ImageSource img)
		{
			if (img is SWMI.BitmapSource) {
				var bs = (BitmapSource)img;
				return (bs.DpiX * bs.Width) / 96;
			}
			else
				return img.Width;
		}

		public static double HeightToPixels (ImageSource img)
		{
			if (img is SWMI.BitmapSource) {
				var bs = (BitmapSource)img;
				return (bs.DpiY * bs.Height) / 96;
			}
			else
				return img.Height;
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
			BitmapSource source = (BitmapSource) DataConverter.AsImageSource (handle);
			return new Size (source.PixelWidth, source.PixelHeight);
		}

		public override object ResizeBitmap (object handle, double width, double height)
		{
			var oldImg = (SWMI.BitmapSource)DataConverter.AsImageSource (handle);

			width = WidthToDPI (oldImg, width);
			height = HeightToDPI (oldImg, height);

			SWM.DrawingVisual visual = new SWM.DrawingVisual ();
			using (SWM.DrawingContext ctx = visual.RenderOpen ())
			{
				ctx.DrawImage (oldImg, new System.Windows.Rect (0, 0, width, height));
			}

			SWMI.RenderTargetBitmap bmp = new SWMI.RenderTargetBitmap ((int)width, (int)height, oldImg.DpiX, oldImg.DpiY, PixelFormats.Pbgra32);
			bmp.Render (visual);

			return new WpfImage (bmp);
		}

		public override object CopyBitmap (object handle)
		{
			return new WpfImage (((SWMI.BitmapSource)DataConverter.AsImageSource (handle)).Clone ());
		}

		public override object CropBitmap(object handle, int srcX, int srcY, int w, int h)
		{
			var oldImg = (SWMI.BitmapSource)DataConverter.AsImageSource (handle);

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

	class WpfImage
	{
		public WpfImage (ImageSource image)
		{
			Image = image;
		}

		public ImageSource Image;
		public byte[] PixelData;
		public int Stride;
		public bool PixelWritePending;

		public int GetPixelOffset (int x, int y)
		{
			BitmapSource img = Image as BitmapSource;
			return y * Stride + x * ((img.Format.BitsPerPixel + 7) / 8);
		}

		public void AllocatePixelData ()
		{
			if (PixelData == null) {
				BitmapSource img = Image as BitmapSource;
				var height = (int) ImageHandler.HeightToPixels (img);
				var width = (int) ImageHandler.WidthToPixels (img);
				Stride = (width * img.Format.BitsPerPixel + 7) / 8;
				PixelData = new byte[height * Stride];
				img.CopyPixels (PixelData, Stride, 0);
			}
		}
	}
}
