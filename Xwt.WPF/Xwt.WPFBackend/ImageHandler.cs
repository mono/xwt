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
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xwt.Backends;
using Xwt.WPFBackend.Interop;
using SWM = System.Windows.Media;
using SWMI = System.Windows.Media.Imaging;
using System.Collections.Generic;

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

		public override object CreateCustomDrawn (ImageDrawCallback drawCallback)
		{
			return new WpfImage (drawCallback);
		}

		public override object CreateMultiResolutionImage (System.Collections.Generic.IEnumerable<object> images)
		{
			return new WpfImage (images.Cast<WpfImage> ().Select (i => i.Frames[0]));
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

			return ApplicationContext.Toolkit.WrapImage (CreateMultiSizeIcon (new object[] { img1, img2, img3, img4 }));
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
			BitmapSource img = wpfImage.Frames[0] as BitmapSource;
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
			var img = (BitmapSource) wpfImage.Frames[0];
			if (img == null)
				throw new NotSupportedException ("Invalid image format");
			if (img.Format.BitsPerPixel != 32)
				throw new NotSupportedException ("Image format not supported");

			var bitmapImage = img as WriteableBitmap;

			if (!(bitmapImage is WriteableBitmap)) {
				bitmapImage = new WriteableBitmap (img);
				((WpfImage)handle).Frames[0] = bitmapImage;
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

		public override object ConvertToBitmap (object img, int width, int height, Xwt.Drawing.ImageFormat format)
		{
			var wpfImage = (WpfImage)img;
			return new WpfImage (wpfImage.GetBestFrame (ApplicationContext, 1, width, height, true));
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
			var source = (WpfImage) handle;
			return source.Size;
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

		public override void CopyBitmapArea (object srcHandle, int srcX, int srcY, int width, int height, object destHandle, int destX, int destY)
		{
			throw new NotImplementedException ();
		}
	}

	class WpfImage
	{
		ImageDrawCallback drawCallback;

		public byte[] PixelData;
		public int Stride;
		public bool PixelWritePending;

		List<ImageSource> frames = new List<ImageSource> ();

		public WpfImage (ImageSource image)
		{
			if (image is BitmapFrame)
				frames.AddRange (((BitmapFrame)image).Decoder.Frames);
			else
				frames.Add (image);
		}

		public WpfImage (IEnumerable<ImageSource> images)
		{
			frames.AddRange (images);
		}

		public WpfImage (ImageDrawCallback drawCallback)
		{
			this.drawCallback = drawCallback;
		}

		public List<ImageSource> Frames
		{
			get { return frames; }
		}

		public bool HasMultipleSizes
		{
			get { return frames != null && frames.Count > 1 || drawCallback != null; }
		}

		public Size Size {
			get { return Frames.Count > 0 ? new Size (frames[0].Width, frames[0].Height) : Size.Zero; }
		}

		public int GetPixelOffset (int x, int y)
		{
			if (frames.Count == 0)
				throw new NotSupportedException ();
			BitmapSource img = frames[0] as BitmapSource;
			return y * Stride + x * ((img.Format.BitsPerPixel + 7) / 8);
		}

		public void AllocatePixelData ()
		{
			if (PixelData == null) {
				BitmapSource img = frames[0] as BitmapSource;
				var height = (int) ImageHandler.HeightToPixels (img);
				var width = (int) ImageHandler.WidthToPixels (img);
				Stride = (width * img.Format.BitsPerPixel + 7) / 8;
				PixelData = new byte[height * Stride];
				img.CopyPixels (PixelData, Stride, 0);
			}
		}

		ImageSource FindFrame (int width, int height)
		{
			if (frames == null)
				return null;
			if (frames.Count == 1)
				return frames[0];

			ImageSource bestSmall = null, bestBig = null;
			double bestSmallSize = 0, bestBigSize = 0;

			foreach (var p in frames) {
				var s = p.Width * p.Height;
				if (p.Width < width || p.Height < height) {
					if (bestSmall == null || s > bestSmallSize) {
						bestSmall = p;
						bestSmallSize = s;
					}
				}
				else {
					if (bestBig == null || s < bestBigSize) {
						bestBig = p;
						bestBigSize = s;
					}
				}
			}
			return bestBig ?? bestSmall;
		}

		public ImageSource GetBestFrame (ApplicationContext actx, Visual w, double width, double height, bool forceExactSize)
		{
			return GetBestFrame (actx, w.GetScaleFactor (), width, height, forceExactSize);
		}

		public ImageSource GetBestFrame (ApplicationContext actx, double scaleFactor, double width, double height, bool forceExactSize)
		{
			var f = FindFrame ((int)(width * scaleFactor), (int)(height * scaleFactor));
			if (f == null || (forceExactSize && (f.Width != (int)width || f.Height != (int)height)))
				return RenderFrame (actx, scaleFactor, width, height);
			else
				return f;
		}

		ImageSource RenderFrame (ApplicationContext actx, double scaleFactor, double width, double height)
		{
			ImageDescription idesc = new ImageDescription () {
				Alpha = 1,
				Size = new Size (width * scaleFactor, height * scaleFactor)
			};
			SWM.DrawingVisual visual = new SWM.DrawingVisual ();
			using (SWM.DrawingContext ctx = visual.RenderOpen ()) {
				Draw (actx, ctx, 1, 0, 0, idesc);
			}

			SWMI.RenderTargetBitmap bmp = new SWMI.RenderTargetBitmap ((int)idesc.Size.Width, (int)idesc.Size.Height, 96, 96, PixelFormats.Pbgra32);
			bmp.Render (visual);

			AddFrame (bmp);
			return bmp;
		}

		void AddFrame (ImageSource pix)
		{
			frames.Add (pix);
		}

		public void Draw (ApplicationContext actx, SWM.DrawingContext dc, double scaleFactor, double x, double y, ImageDescription idesc)
		{
			if (drawCallback != null) {
				DrawingContext c = new DrawingContext (dc, scaleFactor);
				actx.InvokeUserCode (delegate {
					drawCallback (c, new Rectangle (x, y, idesc.Size.Width, idesc.Size.Height));
				});
			}
			else {
				if (idesc.Alpha < 1)
					dc.PushOpacity (idesc.Alpha);

				var f = GetBestFrame (actx, scaleFactor, idesc.Size.Width, idesc.Size.Height, false);
				dc.DrawImage (f, new Rect (x, y, idesc.Size.Width, idesc.Size.Height));

				if (idesc.Alpha < 1)
					dc.Pop ();
			}
		}
	}

	public class ImageBox : System.Windows.Controls.Canvas
	{
		ApplicationContext actx;

		public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register ("ImageSource", typeof (ImageDescription), typeof (ImageBox), new PropertyMetadata (ImageDescription.Null));

		public ImageBox ()
		{
			this.actx = ToolkitEngineBackend.GetToolkitBackend<WPFEngine> ().ApplicationContext;
		}

		public ImageBox (ApplicationContext actx)
		{
			this.actx = actx;
		}

		protected override void OnRender (System.Windows.Media.DrawingContext dc)
		{
			var image = ImageSource;
			if (!image.IsNull) {
				var x = (RenderSize.Width - image.Size.Width) / 2;
				var y = (RenderSize.Height - image.Size.Height) / 2;
				((WpfImage)image.Backend).Draw (actx, dc, this.GetScaleFactor (), x, y, image);
			}
		}

		public ImageDescription ImageSource
		{
			get { return (ImageDescription)this.GetValue (ImageSourceProperty); }
			set { SetValue (ImageSourceProperty, value); }
		}

		protected override System.Windows.Size MeasureOverride (System.Windows.Size constraint)
		{
			var image = ImageSource;
			if (!image.IsNull)
				return new System.Windows.Size (image.Size.Width, image.Size.Height);
			else
				return new System.Windows.Size (0, 0);
		}
	}
}
