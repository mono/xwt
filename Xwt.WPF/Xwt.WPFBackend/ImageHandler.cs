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

			return LoadFromImageSource (img);
		}

		public static object LoadFromImageSource (ImageSource img)
		{
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

			return BitmapSource.Create (width, height, dpi, dpi, bitmapImage.Format, bitmapImage.Palette, pixelData, stride);
		}

		public override object CreateCustomDrawn (ImageDrawCallback drawCallback)
		{
			return new WpfImage (drawCallback);
		}

		public override object CreateMultiResolutionImage (System.Collections.Generic.IEnumerable<object> images)
		{
			var refImg = (WpfImage)images.First ();
			var f = refImg.Frames[0];
			var frames = images.Cast<WpfImage> ().Select (img => new WpfImage.ImageFrame (img.Frames[0].ImageSource, f.Width, f.Height));
			return new WpfImage (frames);
		}

		public override object CreateMultiSizeIcon (IEnumerable<object> images)
		{
			return new WpfImage (images.Cast<WpfImage> ());
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
					using (var s = typeof (ImageHandler).Assembly.GetManifestResourceStream ("Xwt.WPF.icons.add-16.png"))
						return LoadFromStream (s);
				case StockIconId.Remove:
					using (var s = typeof (ImageHandler).Assembly.GetManifestResourceStream ("Xwt.WPF.icons.remove-16.png"))
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
			BitmapSource img = wpfImage.MainFrame as BitmapSource;
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
			var img = (BitmapSource)wpfImage.MainFrame;
			if (img == null)
				throw new NotSupportedException ("Invalid image format");
			if (img.Format.BitsPerPixel != 32)
				throw new NotSupportedException ("Image format not supported");

			var bitmapImage = img as WriteableBitmap;

			if (bitmapImage == null) {
				bitmapImage = new WriteableBitmap (img);
				((WpfImage)handle).MainFrame = bitmapImage;
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

		public override object ConvertToBitmap (ImageDescription idesc, double scaleFactor, Xwt.Drawing.ImageFormat format)
		{
			var wpfImage = (WpfImage)idesc.Backend;
			return new WpfImage (wpfImage.GetBestFrame (ApplicationContext, scaleFactor, idesc, true));
		}

		public override bool HasMultipleSizes (object handle)
		{
			return ((WpfImage)handle).HasMultipleSizes;
		}

		public override bool IsBitmap (object handle)
		{
			return !HasMultipleSizes (handle);
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
			var bmp = new CroppedBitmap (oldImg, new Int32Rect (srcX, srcY, w, h));
			return new WpfImage (bmp);
		}

		public override void CopyBitmapArea (object srcHandle, int srcX, int srcY, int width, int height, object destHandle, int destX, int destY)
		{
			throw new NotImplementedException ();
		}
	}

	class WpfImage
	{
		public class ImageFrame
		{
			public ImageSource ImageSource { get; set; }
			public double Width { get; private set; }
			public double Height { get; private set; }
			public double Scale { get; set; }
			public ImageFrame (ImageSource pix)
			{
				ImageSource = pix;
				Width = pix.Width;
				Height = pix.Height;
				Scale = 1;
			}
			public ImageFrame (ImageSource pix, double width, double height)
			{
				ImageSource = pix;
				Width = width;
				Height = height;
				Scale = pix.Width / width;
			}
			public void Dispose ()
			{
			}
		}

		ImageDrawCallback drawCallback;

		public byte[] PixelData;
		public int Stride;
		public bool PixelWritePending;

		ImageFrame[] frames = new ImageFrame[0];

		public WpfImage (ImageSource image)
		{
			if (image is BitmapFrame)
				this.frames = ((BitmapFrame)image).Decoder.Frames.Select (f => new ImageFrame (f)).ToArray ();
			else
				this.frames = new ImageFrame[] { new ImageFrame (image) };
		}

		public WpfImage (IEnumerable<ImageSource> images)
		{
			this.frames = images.Select (f => new ImageFrame (f)).ToArray ();
			if (frames.Length == 0)
				throw new InvalidOperationException();
		}

		public WpfImage (IEnumerable<ImageFrame> frames)
		{
			this.frames = frames.ToArray ();
			if (this.frames.Length == 0)
				throw new InvalidOperationException();
		}

		public WpfImage (IEnumerable<WpfImage> images)
		{
			var first = images.First ();
			if (first.drawCallback != null)
				drawCallback = first.drawCallback;
			else {
				this.frames = images.SelectMany (i => i.Frames).ToArray ();
				if (this.frames.Length == 0)
					throw new InvalidOperationException ();
			}
		}

		public WpfImage (ImageDrawCallback drawCallback)
		{
			this.drawCallback = drawCallback;
		}

		public ImageFrame[] Frames
		{
			get { return frames; }
		}

		public ImageSource MainFrame
		{
			get { return frames.Length > 0 ? frames[0].ImageSource : null; }
			set { frames[0].ImageSource = value; }
		}

		public bool HasMultipleSizes
		{
			get { return frames != null && frames.Length> 1 || drawCallback != null; }
		}

		public Size Size {
			get { return frames.Length > 0 ? new Size (frames[0].Width, frames[0].Height) : Size.Zero; }
		}

		public int GetPixelOffset (int x, int y)
		{
			if (frames.Length == 0)
				throw new NotSupportedException ();
			BitmapSource img = frames[0].ImageSource as BitmapSource;
			return y * Stride + x * ((img.Format.BitsPerPixel + 7) / 8);
		}

		public void AllocatePixelData ()
		{
			if (PixelData == null) {
				BitmapSource img = frames[0].ImageSource as BitmapSource;
				var height = (int) ImageHandler.HeightToPixels (img);
				var width = (int) ImageHandler.WidthToPixels (img);
				Stride = (width * img.Format.BitsPerPixel + 7) / 8;
				PixelData = new byte[height * Stride];
				img.CopyPixels (PixelData, Stride, 0);
			}
		}

		ImageSource FindFrame (double width, double height, double scaleFactor)
		{
			if (frames == null)
				return null;
			if (frames.Length == 1)
				return frames[0].ImageSource;

			ImageSource best = null;
			int bestSizeMatch = 0;
			double bestResolutionMatch = 0;

			foreach (var f in frames) {
				int sizeMatch;
				if (f.Width == width && f.Height == height) {
					if (f.Scale == scaleFactor)
						return f.ImageSource; // Exact match
					sizeMatch = 2; // Exact size
				}
				else if (f.Width >= width && f.Height >= height)
					sizeMatch = 1; // Bigger size
				else
					sizeMatch = 0; // Smaller size

				var resolutionMatch = ((double)f.ImageSource.Width * (double)f.ImageSource.Height) / ((double)width * (double)height * scaleFactor);

				if (best == null ||
					(bestResolutionMatch < 1 && resolutionMatch > bestResolutionMatch) ||
					(bestResolutionMatch >= 1 && resolutionMatch >= 1 && resolutionMatch <= bestResolutionMatch && (sizeMatch >= bestSizeMatch))) {
					best = f.ImageSource;
					bestSizeMatch = sizeMatch;
					bestResolutionMatch = resolutionMatch;
				}
			}

			return best;
		}

		public ImageSource GetBestFrame (ApplicationContext actx, Visual w, double width, double height, bool forceExactSize)
		{
			return GetBestFrame (actx, w.GetScaleFactor (), width, height, forceExactSize);
		}

		public ImageSource GetBestFrame (ApplicationContext actx, double scaleFactor, double width, double height, bool forceExactSize)
		{
			return GetBestFrame (actx, scaleFactor, new ImageDescription { Alpha = 1.0, Size = new Size (width, height) }, forceExactSize);
		}

		public ImageSource GetBestFrame (ApplicationContext actx, double scaleFactor, ImageDescription idesc, bool forceExactSize)
		{
			double width = idesc.Size.Width;
			double height = idesc.Size.Height;
			var f = FindFrame (width, height, scaleFactor);
			if (f == null || (forceExactSize && (Math.Abs (f.Width - width * scaleFactor) > 0.01 || Math.Abs (f.Height - height * scaleFactor) > 0.01)))
				return RenderFrame (actx, scaleFactor, idesc);
			else
				return f;
		}

		ImageSource RenderFrame (ApplicationContext actx, double scaleFactor, ImageDescription idesc)
		{
			SWM.DrawingVisual visual = new SWM.DrawingVisual ();
			using (SWM.DrawingContext ctx = visual.RenderOpen ()) {
				ctx.PushTransform (new ScaleTransform (scaleFactor, scaleFactor));
				Draw (actx, ctx, scaleFactor, 0, 0, idesc);
				ctx.Pop ();
			}

			SWMI.RenderTargetBitmap bmp = new SWMI.RenderTargetBitmap ((int)(idesc.Size.Width * scaleFactor), (int)(idesc.Size.Height * scaleFactor), 96, 96, PixelFormats.Pbgra32);
			bmp.Render (visual);

			var f = new ImageFrame (bmp, idesc.Size.Width, idesc.Size.Height);
			if (drawCallback == null)
				AddFrame (f);
			return bmp;
		}

		void AddFrame (ImageFrame frame)
		{
			if (frames == null)
				frames = new ImageFrame[] { frame };
			else {
				Array.Resize (ref frames, frames.Length + 1);
				frames[frames.Length - 1] = frame;
			}
		}

		public void Draw (ApplicationContext actx, SWM.DrawingContext dc, double scaleFactor, double x, double y, ImageDescription idesc)
		{
			if (drawCallback != null) {
				DrawingContext c = new DrawingContext (dc, scaleFactor);
				actx.InvokeUserCode (delegate {
					drawCallback (c, new Rectangle (x, y, idesc.Size.Width, idesc.Size.Height), idesc, actx.Toolkit);
				});
			}
			else {
				if (idesc.Alpha < 1)
					dc.PushOpacity (idesc.Alpha);

				var f = GetBestFrame (actx, scaleFactor, idesc.Size.Width, idesc.Size.Height, false);
				var bmpImage = f as BitmapSource;

				// When an image is a single bitmap that doesn't have the same intrinsic size as the drawing size, dc.DrawImage makes a very poor job of down/up scaling it.
				// Thus we handle this manually by using a TransformedBitmap to handle the conversion in a better way when it's needed.

				var scaledWidth = idesc.Size.Width * scaleFactor;
				var scaledHeight = idesc.Size.Height * scaleFactor;
				if (bmpImage != null && (Math.Abs (bmpImage.PixelHeight - scaledHeight) > 0.001 || Math.Abs (bmpImage.PixelWidth - scaledWidth) > 0.001))
					f = new TransformedBitmap (bmpImage, new ScaleTransform (scaledWidth / bmpImage.PixelWidth, scaledHeight / bmpImage.PixelHeight));

				dc.DrawImage (f, new Rect (x, y, idesc.Size.Width, idesc.Size.Height));

				if (idesc.Alpha < 1)
					dc.Pop ();
			}
		}
	}

	public class ImageBox : System.Windows.Controls.Canvas
	{
		ApplicationContext actx;

		public static readonly DependencyProperty ImageSourceProperty =
			DependencyProperty.Register ("ImageSource", typeof (ImageDescription), typeof (ImageBox), new FrameworkPropertyMetadata (ImageDescription.Null) { AffectsMeasure = true, AffectsRender = true });

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
