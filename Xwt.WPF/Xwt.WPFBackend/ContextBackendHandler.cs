// 
// ContextBackendHandler.cs
//  
// Author:
//       Eric Maupin <ermau@xamarin.com>
//       Hywel Thomas <hywel.w.thomas@gmail.com>
//       Lytico (http://limada.sourceforge.net)
//       Luís Reis <luiscubal@gmail.com>
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

using Color = Xwt.Drawing.Color;
using Font = Xwt.Drawing.Font;

namespace Xwt.WPFBackend
{
	public class WpfContextBackendHandler
		: ContextBackendHandler
	{
		public override void Save (object backend)
		{
			var c = (DrawingContext) backend;
			c.Save();
		}

		public override void Restore (object backend)
		{
			var c = (DrawingContext) backend;
			c.Restore();
		}

		public override void SetGlobalAlpha (object backend, double alpha)
		{
			// TODO
		}

		public override void Arc (object backend, double xc, double yc, double radius, double angle1, double angle2)
		{
			// ensure sweep angle is always positive
			if (angle2 < angle1)
				angle2 = 360 + angle2;
			ArcInternal (backend, xc, yc, radius, angle1, angle2);
		}

		public override void ArcNegative (object backend, double xc, double yc, double radius, double angle1, double angle2)
		{
			// ensure sweep angle is always negative
			if (angle1 < angle2)
				angle1 = 360 + angle1;
			ArcInternal (backend, xc, yc, radius, angle1, angle2);
		}

		void ArcInternal (object backend, double xc, double yc, double radius, double angle1, double angle2)
		{
			var c = (DrawingContext)backend;
			c.Path.AddArc ((float)(xc - radius), (float)(yc - radius), (float)radius * 2, (float)radius * 2, (float)angle1,
			               (float)(angle2 - angle1));

			var current = c.Path.GetLastPoint ();
			c.CurrentX = current.X;
			c.CurrentY = current.Y;
		}

		public override void Clip (object backend)
		{
			var c = (DrawingContext) backend;
			c.Graphics.SetClip (c.Path);
			c.Path.Reset ();
		}

		public override void ClipPreserve (object backend)
		{
			var c = (DrawingContext) backend;
			c.Graphics.SetClip (c.Path);
		}

		public override void ClosePath (object backend)
		{
			var c = (DrawingContext) backend;
			c.Path.CloseFigure();
		}

		public override void CurveTo (object backend, double x1, double y1, double x2, double y2, double x3, double y3)
		{
			var c = (DrawingContext)backend;
			c.Path.AddBezier (c.CurrentX, c.CurrentY,
					(float)x1, (float)y1,
					(float)x2, (float)y2,
					(float)x3, (float)y3);
			c.CurrentX = (float)x3;
			c.CurrentY = (float)y3;
		}

		public override void Fill (object backend)
		{
			var c = (DrawingContext) backend;
			c.Graphics.FillPath (c.Brush, c.Path);
			c.Path.Reset();
			c.CurrentX = 0;
			c.CurrentY = 0;
		}

		public override void FillPreserve (object backend)
		{
			var c = (DrawingContext) backend;
			c.Graphics.FillPath (c.Brush, c.Path);
		}

		public override void LineTo (object backend, double x, double y)
		{
			var c = (DrawingContext) backend;

			c.Path.AddLine (c.CurrentX, c.CurrentY, (float) x, (float) y);
			c.CurrentX = (float) x;
			c.CurrentY = (float) y;
		}

		public override void MoveTo (object backend, double x, double y)
		{
			var c = (DrawingContext) backend;
			if (c.CurrentX != x || c.CurrentY != y) {
				c.Path.StartFigure ();
				c.CurrentX = (float)x;
				c.CurrentY = (float)y;
			}
		}

		public override void NewPath (object backend)
		{
			var c = (DrawingContext) backend;
			c.Path.Reset();
			c.CurrentX = 0;
			c.CurrentY = 0;
		}

		public override void Rectangle (object backend, double x, double y, double width, double height)
		{
			var c = (DrawingContext) backend;
			if (c.CurrentX != x || c.CurrentY != y)
				c.Path.StartFigure ();
			c.Path.AddRectangle (new RectangleF ((float)x, (float)y, (float)width, (float)height));
			c.CurrentX = (float)x;
			c.CurrentY = (float)y;
		}

		public override void RelCurveTo (object backend, double dx1, double dy1, double dx2, double dy2, double dx3, double dy3)
		{
			var c = (DrawingContext)backend;
			c.Path.AddBezier (c.CurrentX, c.CurrentY,
					(float)(c.CurrentX + dx1), (float)(c.CurrentY + dy1),
					(float)(c.CurrentX + dx2), (float)(c.CurrentY + dy2),
					(float)(c.CurrentX + dx3), (float)(c.CurrentY + dy3));
			c.CurrentX = (float)(c.CurrentX + dx3);
			c.CurrentY = (float)(c.CurrentX + dy3);
		}

		public override void RelLineTo (object backend, double dx, double dy)
		{
			var c = (DrawingContext) backend;
			
			float x = c.CurrentX;
			float y = c.CurrentY;
			c.CurrentX += (float)dx;
			c.CurrentY += (float)dy;

			c.Path.AddLine (x, y, c.CurrentX, c.CurrentY);
		}

		public override void RelMoveTo (object backend, double dx, double dy)
		{
			var c = (DrawingContext) backend;
			c.Path.StartFigure ();
			c.CurrentX += (float)dx;
			c.CurrentY += (float)dy;
		}

		public override void Stroke (object backend)
		{
			var c = (DrawingContext) backend;
			c.Graphics.DrawPath (c.Pen, c.Path);
			c.Path.Reset();
			c.CurrentX = 0;
			c.CurrentY = 0;
		}

		public override void StrokePreserve (object backend)
		{
			var c = (DrawingContext) backend;
			c.Graphics.DrawPath (c.Pen, c.Path);
		}

		public override void SetColor (object backend, Color color)
		{
			var c = (DrawingContext) backend;

			var dc = color.ToDrawingColor ();
			c.SetColor (dc);
		}

		public override void SetLineWidth (object backend, double width)
		{
			var c = (DrawingContext) backend;
			c.SetWidth ((float) width);
		}

		public override void SetLineDash (object backend, double offset, params double[] pattern)
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

		public override void SetPattern (object backend, object p)
		{
			var c = (DrawingContext) backend;

			if (p is LinearGradient) {
				c.Brush = ((LinearGradient) p).CreateBrush ();
			} else if (p is RadialGradient) {
				c.Brush = ((RadialGradient) p).CreateBrush ();
			} else if (p is Brush)
				c.Brush = (Brush) p;
		}

		public override void SetFont (object backend, Font font)
		{
			var c = (DrawingContext) backend;
			c.Font.Dispose();
			c.Font = font.ToDrawingFont ();
		}

		public override void DrawTextLayout (object backend, TextLayout layout, double x, double y)
		{
			var c = (DrawingContext)backend;
			Size measure = layout.GetSize ();
			float h = layout.Height > 0 ? (float)layout.Height : (float)measure.Height;
			System.Drawing.StringFormat stringFormat = TextLayoutContext.StringFormat;
			StringTrimming trimming = layout.Trimming.ToDrawingStringTrimming ();

			if (layout.Height > 0 && stringFormat.Trimming != trimming) {
				stringFormat = (System.Drawing.StringFormat)stringFormat.Clone ();
				stringFormat.Trimming = trimming;
			}

			c.Graphics.DrawString (layout.Text, layout.Font.ToDrawingFont (), c.Brush,
			                       new RectangleF ((float)x, (float)y, (float)measure.Width, h),
			                       stringFormat);
		}

		public override void DrawImage (object backend, object img, double x, double y, double alpha)
		{
			var c = (DrawingContext) backend;

			Bitmap bmp = DataConverter.AsBitmap (img);
			DrawImageCore (c.Graphics, bmp, (float) x, (float) y, bmp.Width, bmp.Height, (float)alpha);
		}

		public override void DrawImage (object backend, object img, double x, double y, double width, double height, double alpha)
		{
			var c = (DrawingContext) backend;

			Bitmap bmp = DataConverter.AsBitmap (img);
			DrawImageCore (c.Graphics, bmp, (float) x, (float) y, (float) width, (float) height, (float) alpha);
		}

		public override void DrawImage (object backend, object img, Rectangle srcRect, Rectangle destRect, double alpha)
		{
			var c = (DrawingContext) backend;

			Bitmap bmp = DataConverter.AsBitmap (img);

			DrawImageCore (c.Graphics, bmp, srcRect, destRect, (float) alpha);
		}

		public override void Rotate (object backend, double angle)
		{
			var c = (DrawingContext)backend;
			c.Graphics.RotateTransform ((float)angle);
		}

		public override void Scale (object backend, double scaleX, double scaleY)
		{
			var c = (DrawingContext)backend;
			c.Graphics.ScaleTransform ((float)scaleX, (float)scaleY);
		}

		public override void Translate (object backend, double tx, double ty)
		{
			var c = (DrawingContext)backend;
			c.Graphics.TranslateTransform ((float)tx, (float)ty);
		}

		public override void TransformPoint (object backend, ref double x, ref double y)
		{
			var m = ((DrawingContext)backend).Graphics.Transform;
			PointF p = new PointF ((float)x, (float)y);
			PointF[] pts = new PointF[] { p };
			m.TransformPoints (pts);
			x = pts[0].X;
			y = pts[0].Y;
		}

		public override void TransformDistance (object backend, ref double dx, ref double dy)
		{
			var m = ((DrawingContext)backend).Graphics.Transform;
			PointF p = new PointF ((float)dx, (float)dy);
			PointF[] pts = new PointF[] {p};
			m.TransformVectors (pts);
			dx = pts[0].X;
			dy = pts[0].Y;
		}

		public override void TransformPoints (object backend, Point[] points)
		{
			var m = ((DrawingContext)backend).Graphics.Transform;
			PointF[] pts = new PointF[points.Length];
			for (int i = 0; i < points.Length; ++i) {
				pts[i].X = (float)points[i].X;
				pts[i].Y = (float)points[i].Y;
			}
			m.TransformPoints (pts);
			for (int i = 0; i < points.Length; ++i) {
				points[i].X = pts[i].X;
				points[i].Y = pts[i].Y;
			}
		}

		public override void TransformDistances (object backend, Distance[] vectors)
		{
			var m = ((DrawingContext)backend).Graphics.Transform;
			PointF[] pts = new PointF[vectors.Length];
			for (int i = 0; i < vectors.Length; ++i) {
				pts[i].X = (float)vectors[i].Dx;
				pts[i].Y = (float)vectors[i].Dy;
			}
			m.TransformVectors (pts);
			for (int i = 0; i < vectors.Length; ++i) {
				vectors[i].Dx = pts[i].X;
				vectors[i].Dy = pts[i].Y;
			}
		}

		public override object CreatePath ()
        {
            return new DrawingContext ();
        }

		public override object CopyPath (object backend)
		{
			return new DrawingContext ((DrawingContext)backend);
		}

		public override void AppendPath (object backend, object otherBackend)
        {
            var dest = (DrawingContext)backend;
            var src = (DrawingContext)otherBackend;

            dest.Path.AddPath (src.Path, false);
            dest.CurrentX = src.CurrentX;
            dest.CurrentY = src.CurrentY;
        }

		public override bool IsPointInFill (object backend, double x, double y)
        {
            return ((DrawingContext)backend).Path.IsVisible ((float)x, (float)y);
        }

		public override bool IsPointInStroke (object backend, double x, double y)
        {
            var c = (DrawingContext)backend;
            return c.Path.IsOutlineVisible ((float)x, (float)y, c.Pen);
        }

		public override void Dispose (object backend)
		{
			((DrawingContext)backend).Dispose();
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

		internal void DrawImageCore (Graphics g, Bitmap bmp, Rectangle srcRect, Rectangle destRect, float alpha)
		{
			if (alpha < 1)
			{
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
				points[0] = new PointF ((float) destRect.X, (float) destRect.Y);
				points[1] = new PointF ((float) (destRect.X + destRect.Width), (float) destRect.Y);
				points[2] = new PointF ((float) destRect.X, (float) (destRect.Y + destRect.Height));

				g.DrawImage (bmp, points, srcRect.ToSDRectF (), GraphicsUnit.Pixel, attr);
			}
			else
				g.DrawImage (bmp, destRect.ToSDRectF (), srcRect.ToSDRectF (), GraphicsUnit.Pixel);
		}
	}
}
