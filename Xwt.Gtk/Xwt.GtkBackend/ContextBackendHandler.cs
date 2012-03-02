// 
// ContextBackendHandler.cs
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
using Xwt.Engine;
using Xwt.Drawing;

namespace Xwt.GtkBackend
{
	class GtkContext
	{
		public Cairo.Context Context;
		public Cairo.Surface TempSurface;
	}
	
	public class ContextBackendHandler: IContextBackendHandler
	{
		public ContextBackendHandler ()
		{
		}

		#region IContextBackendHandler implementation
		public object CreateContext (Widget w)
		{
			GtkContext ctx = new GtkContext ();
			var b = (IGtkWidgetBackend)WidgetRegistry.GetBackend (w);
			if (!b.Widget.IsRealized) {
				Cairo.Surface sf = new Cairo.ImageSurface (Cairo.Format.ARGB32, 1, 1);
				Cairo.Context c = new Cairo.Context (sf);
				ctx.Context = c;
				ctx.TempSurface = sf;
			} else {
				ctx.Context = Gdk.CairoHelper.Create (b.Widget.GdkWindow);
			}
			return ctx;
		}
		
		public void Save (object backend)
		{
			GtkContext gc = (GtkContext)backend;
			gc.Context.Save ();
		}
		
		public void Restore (object backend)
		{
			GtkContext gc = (GtkContext)backend;
			gc.Context.Restore ();
		}

		public void Arc (object backend, double xc, double yc, double radius, double angle1, double angle2)
		{
			GtkContext gc = (GtkContext) backend;
			gc.Context.Arc (xc, yc, radius, (angle1 * System.Math.PI) / 180, (angle2 * System.Math.PI) / 180);
		}

		public void Clip (object backend)
		{
			Cairo.Context ctx = ((GtkContext) backend).Context;
			ctx.Clip ();
		}

		public void ClipPreserve (object backend)
		{
			Cairo.Context ctx = ((GtkContext) backend).Context;
			ctx.ClipPreserve ();
		}

		public void ResetClip (object backend)
		{
			Cairo.Context ctx = ((GtkContext) backend).Context;
			ctx.ResetClip ();
		}

		public void ClosePath (object backend)
		{
			Cairo.Context ctx = ((GtkContext) backend).Context;
			ctx.ClosePath ();
		}

		public void CurveTo (object backend, double x1, double y1, double x2, double y2, double x3, double y3)
		{
			GtkContext gc = (GtkContext) backend;
			gc.Context.CurveTo (x1, y1, x2, y2, x3, y3);
		}

		public void Fill (object backend)
		{
			Cairo.Context ctx = ((GtkContext) backend).Context;
			ctx.Fill ();
		}

		public void FillPreserve (object backend)
		{
			Cairo.Context ctx = ((GtkContext) backend).Context;
			ctx.FillPreserve ();
		}

		public void LineTo (object backend, double x, double y)
		{
			GtkContext gc = (GtkContext) backend;
			gc.Context.LineTo (x, y);
		}

		public void MoveTo (object backend, double x, double y)
		{
			GtkContext gc = (GtkContext) backend;
			gc.Context.MoveTo (x, y);
		}

		public void NewPath (object backend)
		{
			Cairo.Context ctx = ((GtkContext) backend).Context;
			ctx.NewPath ();
		}

		public void Rectangle (object backend, double x, double y, double width, double height)
		{
			GtkContext gc = (GtkContext) backend;
			gc.Context.Rectangle (x, y, width, height);
		}

		public void RelCurveTo (object backend, double dx1, double dy1, double dx2, double dy2, double dx3, double dy3)
		{
			Cairo.Context ctx = ((GtkContext) backend).Context;
			ctx.RelCurveTo (dx1, dy1, dx2, dy2, dx3, dy3);
		}

		public void RelLineTo (object backend, double dx, double dy)
		{
			Cairo.Context ctx = ((GtkContext) backend).Context;
			ctx.RelLineTo (dx, dy);
		}

		public void RelMoveTo (object backend, double dx, double dy)
		{
			Cairo.Context ctx = ((GtkContext) backend).Context;
			ctx.RelMoveTo (dx, dy);
		}

		public void Stroke (object backend)
		{
			Cairo.Context ctx = ((GtkContext) backend).Context;
			ctx.Stroke ();
		}

		public void StrokePreserve (object backend)
		{
			Cairo.Context ctx = ((GtkContext) backend).Context;
			ctx.StrokePreserve ();
		}

		public void SetColor (object backend, Xwt.Drawing.Color color)
		{
			Cairo.Context ctx = ((GtkContext) backend).Context;
			ctx.Color = new Cairo.Color (color.Red, color.Green, color.Blue, color.Alpha);
		}
		
		public void SetLineWidth (object backend, double width)
		{
			Cairo.Context ctx = ((GtkContext) backend).Context;
			ctx.LineWidth = width;
		}
		
		public void SetLineDash (object backend, double offset, params double[] pattern)
		{
			Cairo.Context ctx = ((GtkContext) backend).Context;
			ctx.SetDash (pattern, offset);
		}
		
		public void SetPattern (object backend, object p)
		{
			Cairo.Context ctx = ((GtkContext)backend).Context;
			if (p != null)
				ctx.Pattern = (Cairo.Pattern) p;
			else
				ctx.Pattern = null;
		}
		
		public void SetFont (object backend, Font font)
		{
		}
		
		public void DrawTextLayout (object backend, TextLayout layout, double x, double y)
		{
			Pango.Layout pl = (Pango.Layout) WidgetRegistry.GetBackend (layout);
			GtkContext ctx = (GtkContext) backend;
			ctx.Context.MoveTo (x, y);
			Pango.CairoHelper.ShowLayout (ctx.Context, pl);
		}
		
		public void DrawImage (object backend, object img, double x, double y, double alpha)
		{
			Gdk.Pixbuf pb = (Gdk.Pixbuf)img;
			GtkContext ctx = (GtkContext)backend;
			Gdk.CairoHelper.SetSourcePixbuf (ctx.Context, pb, x, y);
			if (alpha == 1)
				ctx.Context.Paint ();
			else
				ctx.Context.PaintWithAlpha (alpha);
		}
		
		public void DrawImage (object backend, object img, double x, double y, double width, double height, double alpha)
		{
			Gdk.Pixbuf pb = (Gdk.Pixbuf)img;
			GtkContext ctx = (GtkContext)backend;
			ctx.Context.Save ();
			double sx = ((double) width) / pb.Width;
			double sy = ((double) height) / pb.Height;
			ctx.Context.Translate (x, y);
			ctx.Context.Scale (sx, sy);
			Gdk.CairoHelper.SetSourcePixbuf (ctx.Context, pb, 0, 0);
			if (alpha == 1)
				ctx.Context.Paint ();
			else
				ctx.Context.PaintWithAlpha (alpha);
			ctx.Context.Restore ();
			
/*			var imgs = new Cairo.ImageSurface (Cairo.Format.ARGB32, pb.Width, pb.Height);
			var ic = new Cairo.Context (imgs);
			Gdk.CairoHelper.SetSourcePixbuf (ic, pb, 0, 0);
			if (alpha == 1)
				ic.Paint ();
			else
				ic.PaintWithAlpha (alpha);
			
			var sp = new Cairo.SurfacePattern (imgs);
			sp.Extend = Cairo.Extend.None;
			ctx.Context.Rectangle (x, y, width, height);
			ctx.Context.Pattern = sp;
			ctx.Context.Fill ();*/
		}
		
		public void ResetTransform (object backend)
		{
			GtkContext gc = (GtkContext)backend;
			gc.Context.IdentityMatrix();
		}

        public void Rotate (object backend, double angle)
		{
			GtkContext gc = (GtkContext)backend;
			gc.Context.Rotate ((angle * System.Math.PI) / 180);
		}
		
		public void Translate (object backend, double tx, double ty)
		{
			GtkContext gc = (GtkContext)backend;
			gc.Context.Translate (tx, ty);
		}
		
		public void Dispose (object backend)
		{
			var ctx = (GtkContext) backend;
			IDisposable d = (IDisposable) ctx.Context;
			d.Dispose ();
            d = (IDisposable)ctx.TempSurface;
			if (d != null)
				d.Dispose ();
		}
		#endregion
	}
}

