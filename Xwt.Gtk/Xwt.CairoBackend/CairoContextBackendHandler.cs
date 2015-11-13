// 
// ContextBackendHandler.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//       Hywel Thomas <hywel.w.thomas@gmail.com>
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

using Xwt.Drawing;
using Xwt.GtkBackend;
using System.Collections.Generic;

namespace Xwt.CairoBackend
{
	class CairoContextBackend : IDisposable
	{
		public double GlobalAlpha = 1;
		public Cairo.Context Context;
		public Cairo.Surface TempSurface;
		public double ScaleFactor = 1;
		public double PatternAlpha = 1;
		public StyleSet Styles;
		internal Point Origin = Point.Zero;

		Stack<Data> dataStack = new Stack<Data> ();

		struct Data {
			public double PatternAlpha;
			public double GlobalAlpha;
		}

		public CairoContextBackend (double scaleFactor)
		{
			ScaleFactor = scaleFactor;
		}

		public virtual void Dispose ()
		{
			IDisposable d = Context;
			if (d != null) {
				d.Dispose ();
			}
			d = TempSurface;
			if (d != null) {
				d.Dispose ();
			}
		}

		public void Save ()
		{
			Context.Save ();
			dataStack.Push (new Data () {
				PatternAlpha = PatternAlpha,
				GlobalAlpha = GlobalAlpha
			});
		}

		public void Restore ()
		{
			Context.Restore ();
			var d = dataStack.Pop ();
			PatternAlpha = d.PatternAlpha;
			GlobalAlpha = d.GlobalAlpha;
		}
	}
	
	public class CairoContextBackendHandler: ContextBackendHandler
	{
		public override bool DisposeHandleOnUiThread {
			get {
				return true;
			}
		}

		#region IContextBackendHandler implementation

		public override double GetScaleFactor (object backend)
		{
			CairoContextBackend gc = (CairoContextBackend)backend;
			return gc.ScaleFactor;
		}

		public override void Save (object backend)
		{
			CairoContextBackend gc = (CairoContextBackend)backend;
			gc.Save ();
		}
		
		public override void Restore (object backend)
		{
			CairoContextBackend gc = (CairoContextBackend)backend;
			gc.Restore ();
		}
		
		public override void SetGlobalAlpha (object backend, double alpha)
		{
			CairoContextBackend gc = (CairoContextBackend) backend;
			gc.GlobalAlpha = alpha;
		}
		
		public override void SetStyles (object backend, StyleSet styles)
		{
			CairoContextBackend gc = (CairoContextBackend) backend;
			gc.Styles = styles;
		}

		const double degrees = System.Math.PI / 180d;

		public override void Arc (object backend, double xc, double yc, double radius, double angle1, double angle2)
		{
			CairoContextBackend gc = (CairoContextBackend)backend;
			gc.Context.Arc (xc, yc, radius, angle1 * degrees, angle2 * degrees);
		}

		public override void ArcNegative (object backend, double xc, double yc, double radius, double angle1, double angle2)
		{
			CairoContextBackend gc = (CairoContextBackend)backend;
			gc.Context.ArcNegative (xc, yc, radius, angle1 * degrees, angle2 * degrees);
		}

		public override void Clip (object backend)
		{
			Cairo.Context ctx = ((CairoContextBackend) backend).Context;
			ctx.Clip ();
		}

		public override void ClipPreserve (object backend)
		{
			Cairo.Context ctx = ((CairoContextBackend) backend).Context;
			ctx.ClipPreserve ();
		}

		public override void ClosePath (object backend)
		{
			Cairo.Context ctx = ((CairoContextBackend) backend).Context;
			ctx.ClosePath ();
		}

		public override void CurveTo (object backend, double x1, double y1, double x2, double y2, double x3, double y3)
		{
			CairoContextBackend gc = (CairoContextBackend) backend;
			gc.Context.CurveTo (x1, y1, x2, y2, x3, y3);
		}

		public override void Fill (object backend)
		{
			var gtkc = (CairoContextBackend) backend;
			Cairo.Context ctx = gtkc.Context;
			var alpha = gtkc.GlobalAlpha * gtkc.PatternAlpha;

			if (alpha == 1)
				ctx.Fill ();
			else {
				ctx.Save ();
				ctx.Clip ();
				ctx.PaintWithAlpha (alpha);
				ctx.Restore ();
			}
		}

		public override void FillPreserve (object backend)
		{
			var gtkc = (CairoContextBackend) backend;
			Cairo.Context ctx = gtkc.Context;
			var alpha = gtkc.GlobalAlpha * gtkc.PatternAlpha;

			if (alpha == 1)
				ctx.FillPreserve ();
			else {
				ctx.Save ();
				ctx.ClipPreserve ();
				ctx.PaintWithAlpha (alpha);
				ctx.Restore ();
			}
		}

		public override void LineTo (object backend, double x, double y)
		{
			CairoContextBackend gc = (CairoContextBackend) backend;
			gc.Context.LineTo (x, y);
		}

		public override void MoveTo (object backend, double x, double y)
		{
			CairoContextBackend gc = (CairoContextBackend) backend;
			gc.Context.MoveTo (x, y);
		}

		public override void NewPath (object backend)
		{
			Cairo.Context ctx = ((CairoContextBackend) backend).Context;
			ctx.NewPath ();
		}

		public override void Rectangle (object backend, double x, double y, double width, double height)
		{
			CairoContextBackend gc = (CairoContextBackend) backend;
			gc.Context.Rectangle (x, y, width, height);
		}

		public override void RelCurveTo (object backend, double dx1, double dy1, double dx2, double dy2, double dx3, double dy3)
		{
			Cairo.Context ctx = ((CairoContextBackend) backend).Context;
			ctx.RelCurveTo (dx1, dy1, dx2, dy2, dx3, dy3);
		}

		public override void RelLineTo (object backend, double dx, double dy)
		{
			Cairo.Context ctx = ((CairoContextBackend) backend).Context;
			ctx.RelLineTo (dx, dy);
		}

		public override void RelMoveTo (object backend, double dx, double dy)
		{
			Cairo.Context ctx = ((CairoContextBackend) backend).Context;
			ctx.RelMoveTo (dx, dy);
		}

		public override void Stroke (object backend)
		{
			Cairo.Context ctx = ((CairoContextBackend) backend).Context;
			ctx.Stroke ();
		}

		public override void StrokePreserve (object backend)
		{
			Cairo.Context ctx = ((CairoContextBackend) backend).Context;
			ctx.StrokePreserve ();
		}

		public override void SetColor (object backend, Xwt.Drawing.Color color)
		{
			var gtkContext = (CairoContextBackend) backend;
			gtkContext.Context.SetSourceRGBA (color.Red, color.Green, color.Blue, color.Alpha * gtkContext.GlobalAlpha);
			gtkContext.PatternAlpha = 1;
		}
		
		public override void SetLineWidth (object backend, double width)
		{
			Cairo.Context ctx = ((CairoContextBackend) backend).Context;
			ctx.LineWidth = width;
		}
		
		public override void SetLineDash (object backend, double offset, params double[] pattern)
		{
			Cairo.Context ctx = ((CairoContextBackend) backend).Context;
			ctx.SetDash (pattern, offset);
		}
		
		public override void SetPattern (object backend, object p)
		{
			var cb = (CairoContextBackend)backend;
			var toolkit = ApplicationContext.Toolkit;

			Cairo.Context ctx = cb.Context;
			p = toolkit.GetSafeBackend (p);
			if (p is ImagePatternBackend) {
				cb.PatternAlpha = ((ImagePatternBackend)p).Image.Alpha;
				p = ((ImagePatternBackend)p).GetPattern (ApplicationContext, ((CairoContextBackend)backend).ScaleFactor);
			} else
				cb.PatternAlpha = 1;

			if (p != null)
				ctx.SetSource ((Cairo.Pattern) p);
			else
				ctx.SetSource ((Cairo.Pattern) null);
		}
		
		public override void DrawTextLayout (object backend, TextLayout layout, double x, double y)
		{
			var be = (GtkTextLayoutBackendHandler.PangoBackend)ApplicationContext.Toolkit.GetSafeBackend (layout);
			var pl = be.Layout;
			CairoContextBackend ctx = (CairoContextBackend)backend;
			ctx.Context.MoveTo (x, y);
			if (layout.Height <= 0) {
				Pango.CairoHelper.ShowLayout (ctx.Context, pl);
			} else {
				var lc = pl.LineCount;
				var scale = Pango.Scale.PangoScale;
				double h = 0;
				var fe = ctx.Context.FontExtents;
				var baseline = fe.Ascent / (fe.Ascent + fe.Descent);
				for (int i=0; i<lc; i++) {
					var line = pl.Lines [i];
					var ext = new Pango.Rectangle ();
					var extl = new Pango.Rectangle ();
					line.GetExtents (ref ext, ref extl);
					h += h == 0 ? (extl.Height / scale * baseline) : (extl.Height / scale);
					if (h > layout.Height)
						break;
					ctx.Context.MoveTo (x, y + h);
					Pango.CairoHelper.ShowLayoutLine (ctx.Context, line);
				}
			}
		}

		public override void DrawImage (object backend, ImageDescription img, double x, double y)
		{
			CairoContextBackend ctx = (CairoContextBackend)backend;

			img.Alpha *= ctx.GlobalAlpha;
			img.Styles = img.Styles.AddRange (ctx.Styles);

			var pix = (Xwt.GtkBackend.GtkImage) img.Backend;

			pix.Draw (ApplicationContext, ctx.Context, ctx.ScaleFactor, x, y, img);
		}
		
		public override void DrawImage (object backend, ImageDescription img, Rectangle srcRect, Rectangle destRect)
		{
			CairoContextBackend ctx = (CairoContextBackend)backend;
			ctx.Context.Save ();
			ctx.Context.NewPath();
			ctx.Context.Rectangle (destRect.X, destRect.Y, destRect.Width, destRect.Height);
			ctx.Context.Clip ();
			double sx = destRect.Width / srcRect.Width;
			double sy = destRect.Height / srcRect.Height;
			ctx.Context.Translate (destRect.X-srcRect.X*sx, destRect.Y-srcRect.Y*sy);
			ctx.Context.Scale (sx, sy);
			img.Alpha *= ctx.GlobalAlpha;
			img.Styles = img.Styles.AddRange (ctx.Styles);

			var pix = (Xwt.GtkBackend.GtkImage) img.Backend;
			pix.Draw (ApplicationContext, ctx.Context, ctx.ScaleFactor, 0, 0, img);
			ctx.Context.Restore ();
		}
		
		protected virtual Size GetImageSize (object img)
		{
			return new Size (0,0);
		}
		
		public override void Rotate (object backend, double angle)
		{
			CairoContextBackend gc = (CairoContextBackend)backend;
			gc.Context.Rotate ((angle * System.Math.PI) / 180);
		}
		
		public override void Scale (object backend, double scaleX, double scaleY)
		{
			CairoContextBackend gc = (CairoContextBackend)backend;
			gc.Context.Scale (scaleX, scaleY);
		}

		public override void Translate (object backend, double tx, double ty)
		{
			CairoContextBackend gc = (CairoContextBackend)backend;
			gc.Context.Translate (tx, ty);
		}

		public override void ModifyCTM (object backend, Matrix m)
		{
			CairoContextBackend gc = (CairoContextBackend)backend;
			Cairo.Matrix t = new Cairo.Matrix (m.M11, m.M12, m.M21, m.M22, m.OffsetX, m.OffsetY);
			gc.Context.Transform (t);
		}

		public override Matrix GetCTM (object backend)
		{
			var cb = (CairoContextBackend)backend;
			Cairo.Matrix t = cb.Context.Matrix;
			// Adjust CTM X0,Y0 for ContextBackend Origin (ensures that new CTM is Identity Matrix)
			Matrix ctm = new Matrix (t.Xx, t.Yx, t.Xy, t.Yy, t.X0-cb.Origin.X, t.Y0-cb.Origin.Y);
			return ctm;
		}

		public override object CreatePath ()
		{
			Cairo.Surface sf = new Cairo.ImageSurface (null, Cairo.Format.A1, 0, 0, 0);
			return new CairoContextBackend (1) { // scale doesn't matter here, we are going to use it only for creating a path
				TempSurface = sf,
				Context = new Cairo.Context (sf)
			};
		}

		public override object CopyPath (object backend)
		{
			var newPath = CreatePath ();
			AppendPath (newPath, backend);
			return newPath;
		}

		public override void AppendPath (object backend, object otherBackend)
		{
			Cairo.Context dest = ((CairoContextBackend)backend).Context;
			Cairo.Context src = ((CairoContextBackend)otherBackend).Context;

			using (var path = src.CopyPath ())
				dest.AppendPath (path);
		}

		public override bool IsPointInFill (object backend, double x, double y)
		{
			return ((CairoContextBackend)backend).Context.InFill (x, y);
		}

		public override bool IsPointInStroke (object backend, double x, double y)
		{
			return ((CairoContextBackend)backend).Context.InStroke (x, y);
		}

		public override void Dispose (object backend)
		{
			var ctx = (CairoContextBackend) backend;
			ctx.Dispose ();
		}

		#endregion
	}
}

