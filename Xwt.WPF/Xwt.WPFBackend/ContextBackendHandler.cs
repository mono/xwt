// 
// ContextBackendHandler.cs
//  
// Author:
//       Eric Maupin <ermau@xamarin.com>
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
using System.Linq;
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

		public void Arc (object backend, double xc, double yc, double radius, double angle1, double angle2)
		{
			var c = (DrawingContext) backend;
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
			var c = (DrawingContext) backend;
			c.Path.AddCurve (new []
			{
				new PointF ((float)x1, (float)y1),
				new PointF ((float)x2, (float)y2), 
				new PointF ((float)x3, (float)y3), 
			});
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
			c.CurrentX = (float) x;
			c.CurrentY = (float) y;
		}

		public void NewPath (object backend)
		{
			var c = (DrawingContext) backend;
			c.Path.Reset();
		}

		public void Rectangle (object backend, double x, double y, double width, double height)
		{
			var c = (DrawingContext) backend;
			c.Path.AddRectangle (new RectangleF ((float) x, (float) y, (float) width, (float) height));
		}

		public void RelCurveTo (object backend, double dx1, double dy1, double dx2, double dy2, double dx3, double dy3)
		{
		}

		public void RelLineTo (object backend, double dx, double dy)
		{
		}

		public void RelMoveTo (object backend, double dx, double dy)
		{
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
			c.Pen.DashOffset = (float) offset;

			float[] fp = new float[pattern.Length];
			for (int i = 0; i < fp.Length; ++i)
				fp [i] = (float)pattern[i];

			c.Pen.DashPattern = fp;
		}

		public void SetPattern (object backend, object p)
		{
			var c = (DrawingContext) backend;

			var lg = p as LinearGradient;
			if (lg != null) {
				if (lg.ColorStops.Count == 0)
					throw new ArgumentException();

				var stops = lg.ColorStops.OrderBy (t => t.Item1).ToArray ();
				var first = stops [0];
				var last = stops [stops.Length - 1];

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
		}

		public void SetFont (object backend, Font font)
		{
			var c = (DrawingContext) backend;
			c.Font = font.ToDrawingFont ();
		}

		public void DrawTextLayout (object backend, TextLayout layout, double x, double y)
		{
		}

		public void DrawImage (object backend, object img, double x, double y, double alpha)
		{
		}

		public void DrawImage (object backend, object img, double x, double y, double width, double height, double alpha)
		{
		}

		public void ResetTransform (object backend)
		{
		}

		public void Rotate (object backend, double angle)
		{
			var g = (Graphics) backend;
			g.RotateTransform ((float)angle);
		}

		public void Translate (object backend, double tx, double ty)
		{
		}

		public void Dispose (object backend)
		{
		}
	}
}
