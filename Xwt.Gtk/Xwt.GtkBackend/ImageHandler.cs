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
using Xwt.Drawing;
using System.Collections.Generic;
using Xwt.CairoBackend;
using System.Linq;

namespace Xwt.GtkBackend
{
	public class ImageHandler: ImageBackendHandler
	{
		public override object LoadFromFile (string file)
		{
			return new GtkImage (new Gdk.Pixbuf (file));
		}

		public override object LoadFromStream (System.IO.Stream stream)
		{
			using (Gdk.PixbufLoader loader = new Gdk.PixbufLoader (stream))
				return new GtkImage (loader.Pixbuf);
		}

		public override void SaveToStream (object backend, System.IO.Stream stream, ImageFileType fileType)
		{
			var pix = (GtkImage)backend;
			var buffer = pix.Frames[0].Pixbuf.SaveToBuffer (GetFileType (fileType));
			stream.Write (buffer, 0, buffer.Length);
		}

		public override object CreateCustomDrawn (ImageDrawCallback drawCallback)
		{
			return new GtkImage (drawCallback);
		}

		public override object CreateMultiResolutionImage (IEnumerable<object> images)
		{
			var refImg = (GtkImage) images.First ();
			var f = refImg.Frames [0];
			var frames = images.Cast<GtkImage> ().Select (img => new GtkImage.ImageFrame (img.Frames[0].Pixbuf, f.Width, f.Height, true));
			return new GtkImage (frames);
		}

		public override object CreateMultiSizeIcon (IEnumerable<object> images)
		{
			var frames = images.Cast<GtkImage> ().SelectMany (img => img.Frames);
			return new GtkImage (frames);
		}

		string GetFileType (ImageFileType type)
		{
			switch (type) {
			case ImageFileType.Bmp:
				return "bmp";
			case ImageFileType.Jpeg:
				return "jpeg";
			case ImageFileType.Png:
				return "png";
			default:
				throw new NotSupportedException ();
			}
		}

		public override Image GetStockIcon (string id)
		{
			return ApplicationContext.Toolkit.WrapImage (Util.ToGtkStock (id));
		}
		
		public override void SetBitmapPixel (object handle, int x, int y, Xwt.Drawing.Color color)
		{
			var pix = ((GtkImage)handle).Frames[0].Pixbuf;
			
			unsafe {
				byte* p = (byte*) pix.Pixels;
				p += y * pix.Rowstride + x * pix.NChannels;
				p[0] = (byte)(color.Red * 255);
				p[1] = (byte)(color.Green * 255);
				p[2] = (byte)(color.Blue * 255);
				p[3] = (byte)(color.Alpha * 255);
			}
		}
		
		public override Xwt.Drawing.Color GetBitmapPixel (object handle, int x, int y)
		{
			var pix = ((GtkImage)handle).Frames[0].Pixbuf;
			
			unsafe {
				byte* p = (byte*) pix.Pixels;
				p += y * pix.Rowstride + x * pix.NChannels;
				return Xwt.Drawing.Color.FromBytes (p[0], p[1], p[2], p[3]);
			}
		}
		
		public override void Dispose (object backend)
		{
			((GtkImage)backend).Dispose ();
		}

		public override bool HasMultipleSizes (object handle)
		{
			return ((GtkImage)handle).HasMultipleSizes;
		}

		public override Size GetSize (object handle)
		{
			var pix = handle as GtkImage;
			return pix.DefaultSize;
		}
		
		public override object CopyBitmap (object handle)
		{
			var pix = ((GtkImage)handle).Frames[0].Pixbuf;
			return new GtkImage (pix.Copy ());
		}
		
		public override void CopyBitmapArea (object srcHandle, int srcX, int srcY, int width, int height, object destHandle, int destX, int destY)
		{
			var pixSrc = ((GtkImage)srcHandle).Frames[0].Pixbuf;
			var pixDst = ((GtkImage)destHandle).Frames[0].Pixbuf;
			pixSrc.CopyArea (srcX, srcY, width, height, pixDst, destX, destY);
		}
		
		public override object CropBitmap (object handle, int srcX, int srcY, int width, int height)
		{
			var pix = ((GtkImage)handle).Frames[0].Pixbuf;
			Gdk.Pixbuf res = new Gdk.Pixbuf (pix.Colorspace, pix.HasAlpha, pix.BitsPerSample, width, height);
			res.Fill (0);
			pix.CopyArea (srcX, srcY, width, height, res, 0, 0);
			return new GtkImage (res);
		}
		
		public override bool IsBitmap (object handle)
		{
			var img = (GtkImage) handle;
			return !img.HasMultipleSizes;
		}

		public override object ConvertToBitmap (object handle, double width, double height, double scaleFactor, ImageFormat format)
		{
			var img = (GtkImage) handle;
			var f = new GtkImage.ImageFrame (img.GetBestFrame (ApplicationContext, scaleFactor, width, height, true), (int)width, (int)height, true);
			return new GtkImage (new GtkImage.ImageFrame [] { f });
		}

		internal static Gdk.Pixbuf CreateBitmap (string stockId, double width, double height, double scaleFactor)
		{
			Gdk.Pixbuf result = null;
			
			Gtk.IconSet iconset = Gtk.IconFactory.LookupDefault (stockId);
			if (iconset != null) {
				// Find the size that better fits the requested size
				Gtk.IconSize gsize = Util.GetBestSizeFit (width);
				result = iconset.RenderIcon (Gtk.Widget.DefaultStyle, Gtk.TextDirection.Ltr, Gtk.StateType.Normal, gsize, null, null, scaleFactor);
				if (result == null || result.Width < width * scaleFactor) {
					var gsize2x = Util.GetBestSizeFit (width * scaleFactor, iconset.Sizes);
					if (gsize2x != Gtk.IconSize.Invalid && gsize2x != gsize)
						// Don't dispose the previous result since the icon is owned by the IconSet
						result = iconset.RenderIcon (Gtk.Widget.DefaultStyle, Gtk.TextDirection.Ltr, Gtk.StateType.Normal, gsize2x, null, null);
				}
			}
			
			if (result == null && Gtk.IconTheme.Default.HasIcon (stockId))
				result = Gtk.IconTheme.Default.LoadIcon (stockId, (int)width, (Gtk.IconLookupFlags)0);

			if (result == null) {
				return CreateBitmap (Gtk.Stock.MissingImage, width, height, scaleFactor);
			}
			return result;
		}
	}

	public class GtkImage: IDisposable
	{
		public class ImageFrame {
			public Gdk.Pixbuf Pixbuf { get; private set; }
			public int Width { get; private set; }
			public int Height { get; private set; }
			public double Scale { get; set; }
			public bool Owned { get; set; }
			public ImageFrame (Gdk.Pixbuf pix, bool owned) {
				Pixbuf = pix;
				Width = pix.Width;
				Height = pix.Height;
				Scale = 1;
				Owned = owned;
			}
			public ImageFrame (Gdk.Pixbuf pix, int width, int height, bool owned) {
				Pixbuf = pix;
				Width = width;
				Height = height;
				Scale = (double)pix.Width / (double) width;
				Owned = owned;
			}
			public void Dispose () {
				if (Owned)
					Pixbuf.Dispose ();
			}
		}

		ImageFrame[] frames;
		ImageDrawCallback drawCallback;
		string stockId;

		public ImageFrame[] Frames {
			get { return frames; }
		}

		public GtkImage (Gdk.Pixbuf img)
		{
			this.frames = new ImageFrame [] { new ImageFrame (img, true) };
		}
		
		public GtkImage (string stockId)
		{
			this.stockId = stockId;
		}
		
		public GtkImage (IEnumerable<Gdk.Pixbuf> frames)
		{
			this.frames = frames.Select (p => new ImageFrame (p, true)).ToArray ();
		}

		public GtkImage (IEnumerable<ImageFrame> frames)
		{
			this.frames = frames.ToArray ();
		}

		public GtkImage (ImageDrawCallback drawCallback)
		{
			this.drawCallback = drawCallback;
		}

		public void Dispose ()
		{
			if (frames != null) {
				foreach (var f in frames)
					f.Dispose ();
			}
		}

		public Size DefaultSize {
			get {
				if (frames != null)
					return new Size (frames[0].Pixbuf.Width, frames[0].Pixbuf.Height);
				else
					return new Size (16, 16);
			}
		}

		public bool HasMultipleSizes {
			get { return frames != null && frames.Length > 1 || drawCallback != null || stockId != null; }
		}

		Gdk.Pixbuf FindFrame (int width, int height, double scaleFactor)
		{
			if (frames == null)
				return null;
			if (frames.Length == 1)
				return frames [0].Pixbuf;

			Gdk.Pixbuf best = null;
			int bestSizeMatch = 0;
			double bestResolutionMatch = 0;

			foreach (var f in frames) {
				int sizeMatch;
				if (f.Width == width && f.Height == height) {
					if (f.Scale == scaleFactor)
						return f.Pixbuf; // Exact match
					sizeMatch = 2; // Exact size
				}
				else if (f.Width >= width && f.Height >= height)
					sizeMatch = 1; // Bigger size
				else
					sizeMatch = 0; // Smaller size

				var resolutionMatch = ((double)f.Pixbuf.Width * (double)f.Pixbuf.Height) / ((double)width * (double)height * scaleFactor);

				if ( best == null ||
				    (bestResolutionMatch < 1 && resolutionMatch > bestResolutionMatch) ||
				    (bestResolutionMatch >= 1 && resolutionMatch >= 1 && resolutionMatch <= bestResolutionMatch && (sizeMatch >= bestSizeMatch))) 
				{
					best = f.Pixbuf;
					bestSizeMatch = sizeMatch;
					bestResolutionMatch = resolutionMatch;
				}
			}
			
			return best;
		}

		public Gdk.Pixbuf ToPixbuf (ApplicationContext actx, double width, double height)
		{
			return GetBestFrame (actx, 1, width, height, true);
		}

		public Gdk.Pixbuf ToPixbuf (ApplicationContext actx, Gtk.Widget w)
		{
			return GetBestFrame (actx, Util.GetScaleFactor (w), DefaultSize.Width, DefaultSize.Height, true);
		}

		public Gdk.Pixbuf GetBestFrame (ApplicationContext actx, Gtk.Widget w, double width, double height, bool forceExactSize)
		{
			return GetBestFrame (actx, Util.GetScaleFactor (w), width, height, forceExactSize);
		}

		public Gdk.Pixbuf GetBestFrame (ApplicationContext actx, double scaleFactor, double width, double height, bool forceExactSize)
		{
			var f = FindFrame ((int)width, (int)height, scaleFactor);
			if (f == null || (forceExactSize && (f.Width != (int)width || f.Height != (int)height)))
				return RenderFrame (actx, scaleFactor, width, height);
			else
				return f;
		}

		Gdk.Pixbuf RenderFrame (ApplicationContext actx, double scaleFactor, double width, double height)
		{
			var swidth = Math.Max ((int)(width * scaleFactor), 1);
			var sheight = Math.Max ((int)(height * scaleFactor), 1);

			using (var sf = new Cairo.ImageSurface (Cairo.Format.ARGB32, swidth, sheight))
			using (var ctx = new Cairo.Context (sf)) {
				ImageDescription idesc = new ImageDescription () {
					Alpha = 1,
					Size = new Size (width, height)
				};
				ctx.Scale (scaleFactor, scaleFactor);
				Draw (actx, ctx, scaleFactor, 0, 0, idesc);
				var f = new ImageFrame (ImageBuilderBackend.CreatePixbuf (sf), Math.Max((int)width,1), Math.Max((int)height,1), true);
				AddFrame (f);
				return f.Pixbuf;
			}
		}

		void AddFrame (ImageFrame frame)
		{
			if (frames == null)
				frames = new ImageFrame[] { frame };
			else {
				Array.Resize (ref frames, frames.Length + 1);
				frames [frames.Length - 1] = frame;
			}
		}
		
		public void Draw (ApplicationContext actx, Cairo.Context ctx, double scaleFactor, double x, double y, ImageDescription idesc)
		{
			if (stockId != null) {
				ImageFrame frame = null;
				if (frames != null)
					frame = frames.FirstOrDefault (f => f.Width == (int) idesc.Size.Width && f.Height == (int) idesc.Size.Height && f.Scale == scaleFactor);
				if (frame == null) {
					frame = new ImageFrame (ImageHandler.CreateBitmap (stockId, idesc.Size.Width, idesc.Size.Height, scaleFactor), (int)idesc.Size.Width, (int)idesc.Size.Height, false);
					frame.Scale = scaleFactor;
					AddFrame (frame);
				}
				DrawPixbuf (ctx, frame.Pixbuf, x, y, idesc);
			}
			else if (drawCallback != null) {
				CairoContextBackend c = new CairoContextBackend (scaleFactor) {
					Context = ctx
				};
				if (actx != null) {
					actx.InvokeUserCode (delegate {
						drawCallback (c, new Rectangle (x, y, idesc.Size.Width, idesc.Size.Height));
					});
				} else
					drawCallback (c, new Rectangle (x, y, idesc.Size.Width, idesc.Size.Height));
			}
			else {
				DrawPixbuf (ctx, GetBestFrame (actx, scaleFactor, idesc.Size.Width, idesc.Size.Height, false), x, y, idesc);
			}
		}

		void DrawPixbuf (Cairo.Context ctx, Gdk.Pixbuf img, double x, double y, ImageDescription idesc)
		{
			ctx.Save ();
			ctx.Translate (x, y);
			ctx.Scale (idesc.Size.Width / (double)img.Width, idesc.Size.Height / (double)img.Height);
			Gdk.CairoHelper.SetSourcePixbuf (ctx, img, 0, 0);

			#pragma warning disable 618
			using (var pattern = ctx.Source as Cairo.SurfacePattern) {
				if (pattern != null) {
					if (idesc.Size.Width > img.Width || idesc.Size.Height > img.Height) {
						// Fixes blur issue when rendering on an image surface
						pattern.Filter = Cairo.Filter.Fast;
					} else
						pattern.Filter = Cairo.Filter.Good;
				}
			}
			#pragma warning restore 618

			if (idesc.Alpha >= 1)
				ctx.Paint ();
			else
				ctx.PaintWithAlpha (idesc.Alpha);
			ctx.Restore ();
		}
	}

	public class ImageBox: GtkDrawingArea
	{
		ImageDescription image;
		ApplicationContext actx;
		float yalign = 0.5f, xalign = 0.5f;

		public ImageBox (ApplicationContext actx, ImageDescription img): this (actx)
		{
			Image = img;
		}

		public ImageBox (ApplicationContext actx)
		{
			this.SetHasWindow (false);
			this.SetAppPaintable (true);
			this.actx = actx;
		}

		public ImageDescription Image {
			get { return image; }
			set { 
				image = value;
				SetSizeRequest ((int)image.Size.Width, (int)image.Size.Height);
				QueueResize ();
			}
		}

		public float Yalign {
			get { return yalign; }
			set { yalign = value; QueueDraw (); }
		}

		public float Xalign {
			get { return xalign; }
			set { xalign = value; QueueDraw (); }
		}

		protected override void OnSizeRequested (ref Gtk.Requisition requisition)
		{
			base.OnSizeRequested (ref requisition);
			if (!image.IsNull) {
				requisition.Width = (int) image.Size.Width;
				requisition.Height = (int) image.Size.Height;
			}
		}

		protected override bool OnDrawn (Cairo.Context cr)
		{
			if (image.IsNull)
				return true;

			int x = (int)(((float)Allocation.Width - (float)image.Size.Width) * xalign);
			int y = (int)(((float)Allocation.Height - (float)image.Size.Height) * yalign);
			if (x < 0) x = 0;
			if (y < 0) y = 0;
			((GtkImage)image.Backend).Draw (actx, cr, Util.GetScaleFactor (this), x, y, image);
			return base.OnDrawn (cr);
		}
	}
}

