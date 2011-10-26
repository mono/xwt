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
		public Gtk.Widget Widget;
		public Point Origin;
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
			ctx.Widget = b.Widget;
			return ctx;
		}

		public void Arc (object backend, double xc, double yc, double radius, double angle1, double angle2)
		{
			GtkContext gc = (GtkContext) backend;
			gc.Context.Arc (gc.Origin.X + xc, gc.Origin.Y + yc, radius, (angle1 * System.Math.PI) / 180, (angle2 * System.Math.PI) / 180);
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
			gc.Context.CurveTo (gc.Origin.X + x1, gc.Origin.Y + y1, gc.Origin.X + x2, gc.Origin.Y + y2, gc.Origin.X + x3, gc.Origin.Y + y3);
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
			gc.Context.LineTo (gc.Origin.X + x, gc.Origin.Y + y);
		}

		public void MoveTo (object backend, double x, double y)
		{
			GtkContext gc = (GtkContext) backend;
			gc.Context.MoveTo (gc.Origin.X + x, gc.Origin.Y + y);
		}

		public void NewPath (object backend)
		{
			Cairo.Context ctx = ((GtkContext) backend).Context;
			ctx.NewPath ();
		}

		public void Rectangle (object backend, double x, double y, double width, double height)
		{
			GtkContext gc = (GtkContext) backend;
			gc.Context.Rectangle (gc.Origin.X + x, gc.Origin.Y + y, width, height);
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
		
		public void SetPattern (object backend, Pattern p)
		{
			Cairo.Context ctx = ((GtkContext) backend).Context;
			ctx.Pattern = (Cairo.Pattern) WidgetRegistry.GetBackend (p);
		}
		
		public void SetFont (object backend, Font font)
		{
		}
		
		public void DrawTextLayout (object backend, TextLayout layout, double x, double y)
		{
			Pango.Layout pl = (Pango.Layout) WidgetRegistry.GetBackend (layout);
			GtkContext ctx = (GtkContext) backend;
			
			Gdk.GC gc = ctx.Widget.Style.BlackGC;
			ctx.Widget.GdkWindow.DrawLayout (gc, (int)(ctx.Origin.X + x), (int)(ctx.Origin.Y + y), pl);
		}
		
		public void DrawImage (object backend, Image img, double x, double y)
		{
			Gdk.Pixbuf pb = (Gdk.Pixbuf) WidgetRegistry.GetBackend (img);
			GtkContext ctx = (GtkContext) backend;
			Gdk.CairoHelper.SetSourcePixbuf (ctx.Context, pb, x, y);
			ctx.Context.Paint ();
		}
		
		public void Dispose (object backend)
		{
			var ctx = (GtkContext) backend;
			IDisposable d = (IDisposable) ctx.Context;
			d.Dispose ();
			if (ctx.TempSurface != null)
				ctx.TempSurface.Dispose ();
		}
		#endregion
	}
}

