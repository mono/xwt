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
		public override object LoadFromStream (System.IO.Stream stream)
		{
			using (Gdk.PixbufLoader loader = new Gdk.PixbufLoader (stream))
				return new GtkImage (loader.Pixbuf);
		}

		public override void SaveToStream (object backend, System.IO.Stream stream, ImageFileType fileType)
		{
			var pix = (GtkImage)backend;
			var buffer = pix.Frames[0].SaveToBuffer (GetFileType (fileType));
			stream.Write (buffer, 0, buffer.Length);
		}

		public override object CreateCustomDrawn (ImageDrawCallback drawCallback)
		{
			return new GtkImage (drawCallback);
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
			var pix = ((GtkImage)handle).Frames[0];
			
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
			var pix = ((GtkImage)handle).Frames[0];
			
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
			return handle is string;
		}

		public override Size GetSize (object handle)
		{
			var pix = handle as GtkImage;
			return pix.DefaultSize;
		}
		
		public override object ResizeBitmap (object handle, double width, double height)
		{
			var pix = ((GtkImage)handle).Frames[0];
			return new GtkImage(pix.ScaleSimple ((int)width, (int)height, Gdk.InterpType.Bilinear));
		}
		
		public override object CopyBitmap (object handle)
		{
			var pix = ((GtkImage)handle).Frames[0];
			return new GtkImage (pix.Copy ());
		}
		
		public override void CopyBitmapArea (object srcHandle, int srcX, int srcY, int width, int height, object destHandle, int destX, int destY)
		{
			var pixSrc = ((GtkImage)srcHandle).Frames[0];
			var pixDst = ((GtkImage)destHandle).Frames[0];
			pixSrc.CopyArea (srcX, srcY, width, height, pixDst, destX, destY);
		}
		
		public override object CropBitmap (object handle, int srcX, int srcY, int width, int height)
		{
			var pix = ((GtkImage)handle).Frames[0];
			Gdk.Pixbuf res = new Gdk.Pixbuf (pix.Colorspace, pix.HasAlpha, pix.BitsPerSample, width, height);
			res.Fill (0);
			pix.CopyArea (srcX, srcY, width, height, res, 0, 0);
			return new GtkImage (res);
		}
		
		public override object ChangeBitmapOpacity (object backend, double opacity)
		{
			Gdk.Pixbuf image = ((GtkImage)backend).Frames[0];
			Gdk.Pixbuf result = image.Copy ();
			result.Fill (0);
			result = result.AddAlpha (true, 0, 0, 0);
			image.Composite (result, 0, 0, image.Width, image.Height, 0, 0, 1, 1, Gdk.InterpType.Bilinear, (int)(255 * opacity));
			return new GtkImage (result);
		}

		public override bool IsBitmap (object handle)
		{
			var img = (GtkImage) handle;
			return !img.HasMultipleSizes;
		}

		public override object ConvertToBitmap (object handle, double width, double height)
		{
			throw new NotImplementedException ();
		}

		internal static Gdk.Pixbuf CreateBitmap (string stockId, double width, double height)
		{
			Gdk.Pixbuf result = null;
			
			Gtk.IconSet iconset = Gtk.IconFactory.LookupDefault (stockId);
			if (iconset != null) {
				// Find the size that better fits the requested size
				Gtk.IconSize gsize = Util.GetBestSizeFit (width);
				result = iconset.RenderIcon (Gtk.Widget.DefaultStyle, Gtk.TextDirection.Ltr, Gtk.StateType.Normal, gsize, null, null);
			}
			
			if (result == null && Gtk.IconTheme.Default.HasIcon (stockId))
				result = Gtk.IconTheme.Default.LoadIcon (stockId, (int)width, (Gtk.IconLookupFlags)0);

			return result;
		}
	}

	public class GtkImage: IDisposable
	{
		Gdk.Pixbuf[] frames;
		ImageDrawCallback drawCallback;
		string stockId;

		public Gdk.Pixbuf[] Frames {
			get { return frames; }
		}

		public GtkImage (Gdk.Pixbuf img)
		{
			this.frames = new Gdk.Pixbuf [] { img };
		}
		
		public GtkImage (string stockId)
		{
			this.stockId = stockId;
		}
		
		public GtkImage (IEnumerable<Gdk.Pixbuf> frames)
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
					return new Size (frames[0].Width, frames[0].Height);
				else
					return Size.Zero;
			}
		}

		public bool HasMultipleSizes {
			get { return frames.Length > 1 || drawCallback != null || stockId != null; }
		}
		
		public Gdk.Pixbuf GetBestFrame (Cairo.Context ctx, double width, double height)
		{
			return frames[0];
		}

		public Gdk.Pixbuf GetBestFrame (Gtk.Widget w, double width, double height)
		{
			return frames[0];
		}
		
		public Gdk.Pixbuf GetBestFrame ()
		{
			return frames[0];
		}
		
		public void Draw (ApplicationContext actx, Cairo.Context ctx, double x, double y, ImageDescription idesc)
		{
			if (stockId != null) {
				Gdk.Pixbuf img = null;
				if (frames != null)
					img = frames.FirstOrDefault (p => p.Width == (int) idesc.Size.Width && p.Height == (int) idesc.Size.Height);
				if (img == null) {
					img = ImageHandler.CreateBitmap (stockId, idesc.Size.Width, idesc.Size.Height);
					if (frames == null)
						frames = new Gdk.Pixbuf[] { img };
					else {
						Array.Resize (ref frames, frames.Length + 1);
						frames [frames.Length - 1] = img;
					}
				}
				Gdk.CairoHelper.SetSourcePixbuf (ctx, img, x, y);
				if (idesc.Alpha == 1)
					ctx.Paint ();
				else
					ctx.PaintWithAlpha (idesc.Alpha);
			}
			else if (drawCallback != null) {
				CairoContextBackend c = new CairoContextBackend () {
					Context = ctx
				};
				actx.InvokeUserCode (delegate {
					drawCallback (c, new Rectangle (x, y, idesc.Size.Width, idesc.Size.Height));
				});
			}
			else {
				Gdk.CairoHelper.SetSourcePixbuf (ctx, GetBestFrame (ctx, idesc.Size.Width, idesc.Size.Height), x, y);
				if (idesc.Alpha == 1)
					ctx.Paint ();
				else
					ctx.PaintWithAlpha (idesc.Alpha);
			}
		}
	}

	public class ImageBox: Gtk.DrawingArea
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
			WidgetFlags |= Gtk.WidgetFlags.AppPaintable;
			WidgetFlags |= Gtk.WidgetFlags.NoWindow;
			this.actx = actx;
		}

		public ImageDescription Image {
			get { return image; }
			set { image = value; QueueResize (); }
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

		protected override bool OnExposeEvent (Gdk.EventExpose evnt)
		{
			if (image.IsNull)
				return true;

			int x = Allocation.X + (int)(((float)Allocation.Width - (float)image.Size.Width) * xalign);
			int y = Allocation.Y + (int)(((float)Allocation.Height - (float)image.Size.Height) * yalign);
			if (x < 0) x = 0;
			if (y < 0) y = 0;
			using (var ctx = Gdk.CairoHelper.Create (GdkWindow)) {
				((GtkImage)image.Backend).Draw (actx, ctx, x, y, image);
				return true;
			}
		}
	}
}

