// 
// Colors.cs
//  
// Author:
//       Lytico 
// 
// Copyright (c) 2012 Lytico (http://limada.sourceforge.net)
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
using System.Drawing.Drawing2D;
using Xwt.Backends;
using Xwt.Engine;

namespace Xwt.Gdi.Backend
{
	public class ContextBackendHandler:IContextBackendHandler
	{
  

		public object CreateContext (Widget w)
		{
			var b = (IGdiGraphicsBackend)WidgetRegistry.GetBackend (w);

			var ctx = new GdiContext ();
			if (b.Graphics != null) {
				ctx.Graphics = b.Graphics;
			} else {
				//ctx.Graphics = System.Drawing.BufferedGraphicsManager.Current.Allocate();
			}
			return ctx;
		}

		public void Save (object backend)
		{
			var gc = (GdiContext)backend;
			gc.State = gc.Graphics.Save ();
		}

		public void Restore (object backend)
		{
			var gc = (GdiContext)backend;
			gc.Graphics.Restore (gc.State);
		}

		// http://cairographics.org/documentation/cairomm/reference/classCairo_1_1Context.html

		// mono-libgdiplus\src\graphics-cairo.c

		/// <summary>
		/// Adds a circular arc of the given radius to the current path.
		/// The arc is centered at (xc, yc), 
		/// begins at angle1 and proceeds in the direction of increasing angles to end at angle2. 
		/// If angle2 is less than angle1 
		/// it will be progressively increased by 2*M_PI until it is greater than angle1.
		/// If there is a current point, an initial line segment will be added to the path 
		/// to connect the current point to the beginning of the arc. 
		/// If this initial line is undesired, 
		/// it can be avoided by calling begin_new_sub_path() before calling arc().
		/// </summary>
		/// <param name="backend"></param>
		/// <param name="xc"></param>
		/// <param name="yc"></param>
		/// <param name="radius"></param>
		/// <param name="angle1"></param>
		/// <param name="angle2"></param>
		public void Arc (object backend, double xc, double yc, double radius, double angle1, double angle2)
		{
			var gc = (GdiContext)backend;
			//?? look in mono-libgdiplus 
			gc.Path.AddArc (
                	(float)xc, (float)yc, 
                	(float)radius, (float)radius, 
                	(float)angle1, (float)angle2);
		}

		public void Clip (object backend)
		{
			var gc = (GdiContext)backend;
			gc.Graphics.DrawPath (gc.Pen, gc.Path);
			gc.Path.Dispose ();
			gc.Path = new GraphicsPath ();
		}

		public void ClipPreserve (object backend)
		{
			throw new NotImplementedException ();
		}

		public void ResetClip (object backend)
		{
			throw new System.NotImplementedException ();
		}

		public void ClosePath (object backend)
		{
			var gc = (GdiContext)backend;
			gc.Path.CloseFigure ();
		}

		public void CurveTo (object backend, double x1, double y1, double x2, double y2, double x3, double y3)
		{
			var gc = (GdiContext)backend;

			gc.Path.AddBezier (gc.Current,
                		new PointF ((float)x1, (float)y1),
                		new PointF ((float)x2, (float)y2),
                		new PointF ((float)x3, (float)y3));

		}

		public void Fill (object backend)
		{
			var gc = (GdiContext)backend;
			gc.Graphics.FillPath (gc.Brush, gc.Path);
			gc.Path.Dispose ();
			gc.Path = null;
		}

		public void FillPreserve (object backend)
		{
			var gc = (GdiContext)backend;
			gc.Graphics.FillPath (gc.Brush, gc.Path);
		}

		public void LineTo (object backend, double x, double y)
		{
			var gc = (GdiContext)backend;

			gc.Path.AddLine (gc.Current, new PointF ((float)x, (float)y));
		}

		/// <summary>
		/// If the current subpath is not empty, begin a new subpath.
		/// After this call the current point will be (x, y).
		/// </summary>
		/// <param name="backend"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void MoveTo (object backend, double x, double y)
		{
			var gc = (GdiContext)backend;
			gc.Current = new PointF ((float)x, (float)y);
		}

		public void NewPath (object backend)
		{
			var gc = (GdiContext)backend;
			gc.Path = new GraphicsPath ();
		}

		public void Rectangle (object backend, double x, double y, double width, double height)
		{
			var gc = (GdiContext)backend;
			gc.Path.AddRectangle (new RectangleF ((float)x, (float)y, (float)width, (float)height));
		}

		public void RelCurveTo (object backend, double dx1, double dy1, double dx2, double dy2, double dx3, double dy3)
		{
			throw new System.NotImplementedException ();
			var gc = (GdiContext)backend;
           
		}

		public void RelLineTo (object backend, double dx, double dy)
		{
			throw new System.NotImplementedException ();
			var gc = (GdiContext)backend;
		}

		public void RelMoveTo (object backend, double dx, double dy)
		{
			throw new System.NotImplementedException ();
			var gc = (GdiContext)backend;
		}

		public void Stroke (object backend)
		{
			var gc = (GdiContext)backend;
			gc.Graphics.DrawPath (gc.Pen, gc.Path);
			gc.Path.Dispose ();
			gc.Path = null;

		}

		public void StrokePreserve (object backend)
		{
			var gc = (GdiContext)backend;
			gc.Graphics.DrawPath (gc.Pen, gc.Path);
		}

		public void SetColor (object backend, Xwt.Drawing.Color color)
		{
			var gc = (GdiContext)backend;
			gc.Color = GdiConverter.ToGdi (color);
		}

		public void SetLineWidth (object backend, double width)
		{
			var gc = (GdiContext)backend;
			gc.LineWidth = width;
		}

		public void SetLineDash (object backend, double offset, params double[] pattern)
		{
			var gc = (GdiContext)backend;
			gc.LineDash = pattern;
		}

		public void SetPattern (object backend, object p)
		{
			var gc = (GdiContext)backend;
			gc.Pattern = p;
		}

		public void SetFont (object backend, Xwt.Drawing.Font font)
		{
			var gc = (GdiContext)backend;
			gc.Font = font;
		}

		public void DrawTextLayout (object backend, Xwt.Drawing.TextLayout layout, double x, double y)
		{
			throw new System.NotImplementedException ();
		}

		public void DrawImage (object backend, object img, double x, double y, double alpha)
		{
			throw new System.NotImplementedException ();
		}

		public void DrawImage (object backend, object img, double x, double y, double width, double height, double alpha)
		{
			throw new System.NotImplementedException ();
		}

		public void Rotate (object backend, double angle)
		{
			var gc = (GdiContext)backend;
			var m = new Matrix ();
			m.Rotate ((float)angle);
			gc.Path.Transform (m);
		}

		public void Translate (object backend, double tx, double ty)
		{
			var gc = (GdiContext)backend;
			var m = new Matrix ();
			m.Translate ((float)tx, (float)ty);
			gc.Path.Transform (m);
		}

		public void ResetTransform (object backend)
		{
			throw new NotImplementedException ();
		}

		public void Dispose (object backend)
		{
			var gc = (GdiContext)backend;
			gc.Dispose ();
		}

       

       

       
	}
}