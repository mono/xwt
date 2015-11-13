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
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Xwt.Backends;
using Xwt.Drawing;

using Color = Xwt.Drawing.Color;
using Font = Xwt.Drawing.Font;
using System.Windows.Media;
using SW = System.Windows;
using SWM = System.Windows.Media;
using System.Collections.Generic;

namespace Xwt.WPFBackend
{
	public class WpfContextBackendHandler
		: ContextBackendHandler
	{
		public override double GetScaleFactor (object backend)
		{
			var c = (DrawingContext)backend;
			return c.ScaleFactor;
		}
		public override void Save (object backend)
		{
			var c = (DrawingContext) backend;
			c.Save ();
		}

		public override void Restore (object backend)
		{
			var c = (DrawingContext) backend;
			c.Restore ();
		}

		public override void SetGlobalAlpha (object backend, double alpha)
		{
			var c = (DrawingContext)backend;
			c.Context.PushOpacity (alpha);
			c.NotifyPush ();
		}

		public override void Arc (object backend, double xc, double yc, double radius, double angle1, double angle2)
		{
			Arc (backend, xc, yc, radius, angle1, angle2, false);
		}

		public override void ArcNegative (object backend, double xc, double yc, double radius, double angle1, double angle2)
		{
			Arc (backend, xc, yc, radius, angle1, angle2, true);
		}

		public void Arc (object backend, double xc, double yc, double radius, double angle1, double angle2, bool inverse)
		{
			var c = (DrawingContext)backend;

			if (angle1 == angle2)
				return;

			if (angle1 > angle2)
				angle2 += (Math.Truncate (angle1 / 360) + 1) * 360;

			double nextAngle;

			do {
				nextAngle = angle2 - angle1 < 360 ? angle2 : angle1 + 359;

				var p1 = new SW.Point (xc + radius * Math.Cos (angle1 * Math.PI / 180.0), yc + radius * Math.Sin (angle1 * Math.PI / 180.0));
				var p2 = new SW.Point (xc + radius * Math.Cos (nextAngle * Math.PI / 180.0), yc + radius * Math.Sin (nextAngle * Math.PI / 180.0));

				c.ConnectToLastFigure (p1, true);

				var largeArc = inverse ? nextAngle - angle1 < 180 : nextAngle - angle1 > 180;
				var direction = inverse ? SweepDirection.Counterclockwise : SweepDirection.Clockwise;
				c.Path.Segments.Add (new ArcSegment (p2, new SW.Size (radius, radius), 0, largeArc, direction, true));
				angle1 = nextAngle;
				c.EndPoint = p2;
			}
			while (nextAngle < angle2);
		}

		public override void Clip (object backend)
		{
			var c = (DrawingContext)backend;
			c.Context.PushClip (c.Geometry);
			c.ResetPath ();
			c.NotifyPush ();
		}

		public override void ClipPreserve (object backend)
		{
			var c = (DrawingContext)backend;
			c.Context.PushClip (c.Geometry);
			c.NotifyPush ();
		}

		public override void ClosePath (object backend)
		{
			var c = (DrawingContext)backend;
			if (c.LastFigureStart != c.EndPoint) {
				var p = c.LastFigureStart;
				c.ConnectToLastFigure (c.LastFigureStart, true);
				c.Path.IsClosed = true;
				c.NewFigure (p);
			}
		}

		public override void CurveTo (object backend, double x1, double y1, double x2, double y2, double x3, double y3)
		{
			var c = (DrawingContext)backend;
			c.Path.Segments.Add (new BezierSegment (new SW.Point (x1, y1), new SW.Point (x2, y2), new SW.Point (x3, y3), true));
			c.EndPoint = new SW.Point (x3, y3);
		}

		public override void Fill (object backend)
		{
			var c = (DrawingContext)backend;
			c.Context.DrawGeometry (c.Brush, null, c.Geometry);
			c.ResetPath ();
		}

		public override void FillPreserve (object backend)
		{
			var c = (DrawingContext)backend;
			c.Context.DrawGeometry (c.Brush, null, c.Geometry);
		}

		public override void LineTo (object backend, double x, double y)
		{
			var c = (DrawingContext) backend;
			c.Path.Segments.Add (new LineSegment (new SW.Point (x, y), true) { IsSmoothJoin = true });
			c.EndPoint = new SW.Point (x, y);
		}

		public override void MoveTo (object backend, double x, double y)
		{
			var c = (DrawingContext) backend;

			// Close the current path without a stroke, this will make sure
			// that the are that the path covers is filled if Fill is called.
			if (c.LastFigureStart != c.EndPoint)
				c.ConnectToLastFigure (c.LastFigureStart, false);
			c.NewFigure (new SW.Point (x, y));
		}

		public override void NewPath (object backend)
		{
			var c = (DrawingContext) backend;
			c.ResetPath ();
		}

		public override void Rectangle (object backend, double x, double y, double width, double height)
		{
			MoveTo (backend, x, y);
			var c = (DrawingContext) backend;
			var points = new SW.Point[] { new SW.Point (x + width, y), new SW.Point (x + width, y + height), new SW.Point (x, y + height), new SW.Point (x, y) };
			c.Path.Segments.Add (new PolyLineSegment (points, true));
			c.Path.IsClosed = true;
			c.NewFigure (new SW.Point (x, y));
		}

		public override void RelCurveTo (object backend, double dx1, double dy1, double dx2, double dy2, double dx3, double dy3)
		{
			var c = (DrawingContext)backend;
			var x = c.EndPoint.X;
			var y = c.EndPoint.Y;
			CurveTo (c, x + dx1, y + dy1, x + dx2, y + dy2, x + dx3, y + dy3);
		}

		public override void RelLineTo (object backend, double dx, double dy)
		{
			var c = (DrawingContext)backend;
			var dest = new SW.Point (c.EndPoint.X + dx, c.EndPoint.Y + dy);
			c.Path.Segments.Add (new LineSegment (dest, true) { IsSmoothJoin = true });
			c.EndPoint = dest;
		}

		public override void RelMoveTo (object backend, double dx, double dy)
		{
			var c = (DrawingContext)backend;
			MoveTo (backend, c.EndPoint.X + dx, c.EndPoint.Y + dy);
		}

		public override void Stroke (object backend)
		{
			var c = (DrawingContext) backend;
			c.Context.DrawGeometry (null, c.Pen, c.Geometry);
			c.ResetPath ();
		}

		public override void StrokePreserve (object backend)
		{
			var c = (DrawingContext)backend;
			c.Context.DrawGeometry (null, c.Pen, c.Geometry);
		}

		public override void SetColor (object backend, Color color)
		{
			var c = (DrawingContext) backend;
			c.SetColor (color.ToWpfColor ());
		}

		public override void SetLineWidth (object backend, double width)
		{
			var c = (DrawingContext) backend;
			c.SetThickness (width);
		}

		public override void SetLineDash (object backend, double offset, params double[] pattern)
		{
			var c = (DrawingContext)backend;
			c.SetDash (offset, pattern);
		}

		public override void SetPattern (object backend, object p)
		{
			var c = (DrawingContext) backend;
			var toolkit = ApplicationContext.Toolkit;
			p = toolkit.GetSafeBackend (p);
			if (p is ImagePattern)
				p = ((ImagePattern)p).GetBrush (c.ScaleFactor);
			c.SetPattern ((System.Windows.Media.Brush)p);
		}

		public override void DrawTextLayout (object backend, TextLayout layout, double x, double y)
		{
			var c = (DrawingContext) backend;
			var t = (TextLayoutBackend)ApplicationContext.Toolkit.GetSafeBackend (layout);
			t.SetDefaultForeground (c.ColorBrush);
			c.Context.DrawText (t.FormattedText, new SW.Point (x, y));
		}

		public override void DrawImage (object backend, ImageDescription img, double x, double y)
		{
			var c = (DrawingContext) backend;
			WpfImage bmp = (WpfImage) img.Backend;
			img.Styles = img.Styles.AddRange(c.Styles);
			bmp.Draw (ApplicationContext, c.Context, c.ScaleFactor, x, y, img);
		}

		public override void DrawImage (object backend, ImageDescription img, Rectangle srcRect, Rectangle destRect)
		{
			var c = (DrawingContext) backend;
			WpfImage bmp = (WpfImage)img.Backend;

			img.Styles = img.Styles.AddRange(c.Styles);
			c.Context.PushClip(new RectangleGeometry(destRect.ToWpfRect()));
			c.Context.PushTransform (new TranslateTransform (destRect.X - srcRect.X, destRect.Y - srcRect.Y));
			var sw = destRect.Width / srcRect.Width;
			var sh = destRect.Height / srcRect.Height;
			c.Context.PushTransform (new ScaleTransform (sw, sh));
			bmp.Draw (ApplicationContext, c.Context, c.ScaleFactor, 0, 0, img);

			c.Context.Pop (); // Scale
			c.Context.Pop (); // Translate
			c.Context.Pop (); // Clip
		}

		public override void Rotate (object backend, double angle)
		{
			var c = (DrawingContext)backend;
			c.PushTransform (new RotateTransform (angle));
		}

		public override void Scale (object backend, double scaleX, double scaleY)
		{
			var c = (DrawingContext)backend;
			c.PushTransform (new ScaleTransform (scaleX, scaleY));
		}

		public override void Translate (object backend, double tx, double ty)
		{
			var c = (DrawingContext)backend;
			var t = new TranslateTransform (tx, ty);
			c.PushTransform (t);
		}

		public override void ModifyCTM (object backend, Drawing.Matrix m)
		{
			var c = (DrawingContext)backend;
			MatrixTransform t = new MatrixTransform (m.M11, m.M12, m.M21, m.M22, m.OffsetX, m.OffsetY);
			c.PushTransform (t);
		}

		public override Drawing.Matrix GetCTM (object backend)
		{
			var c = (DrawingContext)backend;
			SWM.Matrix m = c.CurrentTransform;
			return new Drawing.Matrix (m.M11, m.M12, m.M21, m.M22, m.OffsetX, m.OffsetY);
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
			var c = (DrawingContext)backend;
			var other = (DrawingContext)otherBackend;
			c.AppendPath (other);
		}

		public override bool IsPointInFill (object backend, double x, double y)
        {
			var c = (DrawingContext)backend;
			return c.Geometry.FillContains (new SW.Point (x, y));
        }

		public override bool IsPointInStroke (object backend, double x, double y)
        {
			var c = (DrawingContext)backend;
			return c.Geometry.StrokeContains (c.Pen, new SW.Point (x, y));
		}

		public override void SetStyles(object backend, StyleSet styles)
		{
			var c = (DrawingContext)backend;
			c.Styles = styles;
		}

		public override void Dispose (object backend)
		{
		}
	}
}
