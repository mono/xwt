// 
// ContextBackendHandler.cs
//  
// Author:
//       Eric Maupin <ermau@xamarin.com>
//       Hywel Thomas <hywel.w.thomas@gmail.com>
//       Lytico (http://limada.sourceforge.net)
//
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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Xwt.Backends;
using Xwt.Drawing;
using Xwt.Engine;
using Color = Xwt.Drawing.Color;
using Font = Xwt.Drawing.Font;

namespace Xwt.WPFBackend
{
	public class ContextBackendHandler
		: Backend, IContextBackendHandler
	{
		public void Save (object backend)
		{
			var c = (DrawingContext) backend;
			c.Save();
		}

		public void Restore (object backend)
		{
			var c = (DrawingContext) backend;
			c.Restore();
		}

		public void SetGlobalAlpha (object backend, double alpha)
		{
			// TODO
		}

		public void Arc (object backend, double xc, double yc, double radius, double angle1, double angle2)
		{
			var c = (DrawingContext)backend;
			if (angle1 > 0 && angle2 == 0)
				angle2 = 360;
			c.Path.AddArc ((float)(xc - radius), (float)(yc - radius), (float)radius * 2, (float)radius * 2, (float)angle1,
			               (float)(angle2 - angle1));

			var current = c.Path.GetLastPoint ();
			c.CurrentX = current.X;
			c.CurrentY = current.Y;
		}

		public void Clip (object backend)
		{
			var c = (DrawingContext) backend;
		}

		public void ClipPreserve (object backend)
		{
			var c = (DrawingContext) backend;
		}

		public void ResetClip (object backend)
		{
			var c = (DrawingContext) backend;
		}

		public void ClosePath (object backend)
		{
			var c = (DrawingContext) backend;
			c.Path.CloseFigure();
		}

		public void CurveTo (object backend, double x1, double y1, double x2, double y2, double x3, double y3)
		{
			var c = (DrawingContext)backend;
			c.Path.AddBezier (c.CurrentX, c.CurrentY,
					(float)x1, (float)y1,
					(float)x2, (float)y2,
					(float)x3, (float)y3);
			c.CurrentX = (float)x3;
			c.CurrentY = (float)y3;
		}

		public void Fill (object backend)
		{
			var c = (DrawingContext) backend;
			c.Graphics.FillPath (c.Brush, c.Path);
			c.Path.Reset();
		}

		public void FillPreserve (object backend)
		{
			var c = (DrawingContext) backend;
			c.Graphics.FillPath (c.Brush, c.Path);
		}

		public void LineTo (object backend, double x, double y)
		{
			var c = (DrawingContext) backend;

			c.Path.AddLine (c.CurrentX, c.CurrentY, (float) x, (float) y);
			c.CurrentX = (float) x;
			c.CurrentY = (float) y;
		}

		public void MoveTo (object backend, double x, double y)
		{
			var c = (DrawingContext) backend;
			if (c.CurrentX != x || c.CurrentY != y) {
				c.Path.StartFigure ();
				c.CurrentX = (float)x;
				c.CurrentY = (float)y;
			}
		}

		public void NewPath (object backend)
		{
			var c = (DrawingContext) backend;
			c.Path.Reset();
		}

		public void Rectangle (object backend, double x, double y, double width, double height)
		{
			var c = (DrawingContext) backend;
			if (c.CurrentX != x || c.CurrentY != y)
				c.Path.StartFigure ();
			c.Path.AddRectangle (new RectangleF ((float)x, (float)y, (float)width, (float)height));
			c.CurrentX = (float)x;
			c.CurrentY = (float)y;
		}

		public void RelCurveTo (object backend, double dx1, double dy1, double dx2, double dy2, double dx3, double dy3)
		{
			var c = (DrawingContext)backend;
			c.Path.AddBezier (c.CurrentX, c.CurrentY,
					(float)(c.CurrentX + dx1), (float)(c.CurrentY + dy1),
					(float)(c.CurrentX + dx2), (float)(c.CurrentY + dy2),
					(float)(c.CurrentX + dx3), (float)(c.CurrentY + dy3));
			c.CurrentX = (float)(c.CurrentX + dx3);
			c.CurrentY = (float)(c.CurrentX + dy3);
		}

		public void RelLineTo (object backend, double dx, double dy)
		{
			var c = (DrawingContext) backend;
			
			float x = c.CurrentX;
			float y = c.CurrentY;
			c.CurrentX += (float)dx;
			c.CurrentY += (float)dy;

			c.Path.AddLine (x, y, c.CurrentX, c.CurrentY);
		}

		public void RelMoveTo (object backend, double dx, double dy)
		{
			var c = (DrawingContext) backend;
			c.Path.StartFigure ();
			c.CurrentX += (float)dx;
			c.CurrentY += (float)dy;
		}

		public void Stroke (object backend)
		{
			var c = (DrawingContext) backend;
			c.Graphics.DrawPath (c.Pen, c.Path);
			c.Path.Reset();
		}

		public void StrokePreserve (object backend)
		{
			var c = (DrawingContext) backend;
			c.Graphics.DrawPath (c.Pen, c.Path);
		}

		public void SetColor (object backend, Color color)
		{
			var c = (DrawingContext) backend;
			c.SetColor (color.ToDrawingColor ());
		}

		public void SetLineWidth (object backend, double width)
		{
			var c = (DrawingContext) backend;
			c.Pen.Width = (float) width;
		}

		public void SetLineDash (object backend, double offset, params double[] pattern)
		{
			var c = (DrawingContext) backend;

			if (pattern.Length != 0) {
				c.Pen.DashOffset = (float) (offset / c.Pen.Width);
				float[] fp = new float [pattern.Length];
				for (int i = 0; i < fp.Length; ++i)
					fp [i] = (float) (pattern [i] / c.Pen.Width);

				c.Pen.DashStyle = DashStyle.Custom;
				c.Pen.DashPattern = fp;
			} else
				c.Pen.DashStyle = DashStyle.Solid;
		}

		public void SetPattern (object backend, object p)
		{
			var c = (DrawingContext) backend;

			var lg = p as LinearGradient;
			if (lg != null) {
				if (lg.ColorStops.Count == 0)
					throw new ArgumentException ();

				var stops = lg.ColorStops.OrderBy (t => t.Item1).ToArray ();
				var first = stops[0];
				var last = stops[stops.Length - 1];

				var brush = new LinearGradientBrush (lg.Start, lg.End, first.Item2.ToDrawingColor (),
														last.Item2.ToDrawingColor ());

				//brush.InterpolationColors = new ColorBlend (stops.Length);
				//var blend = brush.InterpolationColors;
				//for (int i = 0; i < stops.Length; ++i) {
				//    var s = stops [i];

				//    blend.Positions [i] = (float)s.Item1;
				//    blend.Colors [i] = s.Item2.ToDrawingColor ();
				//}

				c.Brush = brush;
			}
			else if (p is Brush)
				c.Brush = (Brush)p;
		}

		public void SetFont (object backend, Font font)
		{
			var c = (DrawingContext) backend;
			c.Font = font.ToDrawingFont ();
		}

		public void DrawTextLayout (object backend, TextLayout layout, double x, double y)
		{
			var c = (DrawingContext) backend;
			var sfont = layout.Font.ToDrawingFont ();
			var measure = layout.GetSize ();
			
			c.Graphics.DrawString (layout.Text, layout.Font.ToDrawingFont (), c.Brush,
			                       new RectangleF ((float)x, (float)y, (float)measure.Width, (float)measure.Height));
		}

		public void DrawImage (object backend, object img, double x, double y, double alpha)
		{
			var c = (DrawingContext) backend;

			Bitmap bmp = DataConverter.AsBitmap (img);
			DrawImageCore (c.Graphics, bmp, (float) x, (float) y, bmp.Width, bmp.Height, (float)alpha);
		}

		public void DrawImage (object backend, object img, double x, double y, double width, double height, double alpha)
		{
			var c = (DrawingContext) backend;

			Bitmap bmp = DataConverter.AsBitmap (img);
			DrawImageCore (c.Graphics, bmp, (float) x, (float) y, (float) width, (float) height, (float) alpha);
		}

		public void ResetTransform (object backend)
		{
			var c = (DrawingContext)backend;
			c.Graphics.ResetTransform();
		}

		public void Rotate (object backend, double angle)
		{
			var c = (DrawingContext)backend;
			c.Graphics.RotateTransform((float)angle);
		}

		public void Scale (object backend, double scaleX, double scaleY)
		{
			var c = (DrawingContext)backend;
			c.Graphics.ScaleTransform ((float)scaleX, (float)scaleY);
		}
		
		public void Translate (object backend, double tx, double ty)
		{
			var c = (DrawingContext)backend;
			c.Graphics.TranslateTransform ((float)tx, (float)ty);
		}

		public void Dispose (object backend)
		{
		}

		internal static void DrawImageCore (Graphics g, Bitmap bmp, float x, float y, float width, float height, float alpha)
		{
			if (bmp == null)
				throw new ArgumentException();

			if (alpha < 1) {
				var attr = new ImageAttributes ();

				float[][] matrixItems = new[] {
					new float[] { 1, 0, 0, 0, 0 },
					new float[] { 0, 1, 0, 0, 0 },
					new float[] { 0, 0, 1, 0, 0 },
					new float[] { 0, 0, 0, alpha, 0 },
					new float[] { 0, 0, 0, 0, 1 },
				};

				attr.SetColorMatrix (new ColorMatrix (matrixItems), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

				PointF[] points = new PointF[3];
				points [0] = new PointF (x, y);
				points [1] = new PointF (x + width, y);
				points [2] = new PointF (x, y + height);

				g.DrawImage (bmp, points, new RectangleF (0, 0, bmp.Width, bmp.Height), GraphicsUnit.Pixel, attr);
			}
			else
				g.DrawImage (bmp, x, y, width, height);
		}
	}
}
