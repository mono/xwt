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

#if USE_WPF_RENDERING

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
using System.Windows.Media;
using SW = System.Windows;
using SWM = System.Windows.Media;
using System.Collections.Generic;

namespace Xwt.WPFBackend
{
	public class WpfContextBackendHandler
		: ContextBackendHandler
	{
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
			Arc (backend, xc, yc, radius, angle1, angle2, SweepDirection.Clockwise);
		}

		public override void ArcNegative (object backend, double xc, double yc, double radius, double angle1, double angle2)
		{
			Arc (backend, xc, yc, radius, angle1, angle2, SweepDirection.Counterclockwise);
		}

		public void Arc (object backend, double xc, double yc, double radius, double angle1, double angle2, SweepDirection dir)
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

				var largeArc = nextAngle - angle1 > 180;
				c.Path.Segments.Add (new ArcSegment (p2, new SW.Size (radius, radius), 0, largeArc, dir, true));
				angle1 = nextAngle;
				c.EndPoint = p2;
			}
			while (nextAngle < angle2);
		}

		public override void Clip (object backend)
		{
			var c = (DrawingContext)backend;
			c.Context.PushClip (c.GetPathGeometry ());
			c.ResetPath ();
			c.NotifyPush ();
		}

		public override void ClipPreserve (object backend)
		{
			var c = (DrawingContext)backend;
			c.Context.PushClip (c.GetPathGeometry ());
			c.NotifyPush ();
		}

		public override void ClosePath (object backend)
		{
			var c = (DrawingContext)backend;
			if (c.LastFigureStart != c.EndPoint)
				c.ConnectToLastFigure (c.LastFigureStart, true);
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
			c.Context.DrawGeometry (c.Brush, null, c.GetPathGeometry ());
			c.ResetPath ();
		}

		public override void FillPreserve (object backend)
		{
			var c = (DrawingContext)backend;
			c.Context.DrawGeometry (c.Brush, null, c.GetPathGeometry ());
		}

		public override void LineTo (object backend, double x, double y)
		{
			var c = (DrawingContext) backend;
			c.Path.Segments.Add (new LineSegment (new SW.Point (x, y), true));
			c.EndPoint = new SW.Point (x, y);
		}

		public override void MoveTo (object backend, double x, double y)
		{
			var c = (DrawingContext) backend;

			// Close the current path without a stroke, this will make sure
			// that the are that the path covers is filled if Fill is called.
			if (c.LastFigureStart != c.EndPoint)
				c.ConnectToLastFigure (c.LastFigureStart, false);

			c.ConnectToLastFigure (new SW.Point (x, y), false);
			c.EndPoint = new SW.Point (x, y);
		}

		public override void NewPath (object backend)
		{
			var c = (DrawingContext) backend;
			c.ResetPath ();
		}

		public override void Rectangle (object backend, double x, double y, double width, double height)
		{
			var c = (DrawingContext) backend;
			c.LastFigureStart = new SW.Point (x, y);
			var start = new SW.Point (x, y);
			c.ConnectToLastFigure (start, false);
			var points = new SW.Point[] { new SW.Point (x + width, y), new SW.Point (x + width, y + height), new SW.Point (x, y + height), new SW.Point (x, y) };
			c.Path.Segments.Add (new PolyLineSegment (points, true));
			c.EndPoint = start;
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
			c.Path.Segments.Add (new LineSegment (dest, true));
			c.EndPoint = dest;
		}

		public override void RelMoveTo (object backend, double dx, double dy)
		{
			var c = (DrawingContext)backend;
			var dest = new SW.Point (c.EndPoint.X + dx, c.EndPoint.Y + dy);
			c.Path.Segments.Add (new LineSegment (dest, false));
			c.LastFigureStart = c.EndPoint = dest;
		}

		public override void Stroke (object backend)
		{
			var c = (DrawingContext) backend;
			c.Context.DrawGeometry (null, c.Pen, c.GetPathGeometry ());
			c.ResetPath ();
		}

		public override void StrokePreserve (object backend)
		{
			var c = (DrawingContext)backend;
			c.Context.DrawGeometry (null, c.Pen, c.GetPathGeometry ());
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
			c.SetPattern ((System.Windows.Media.Brush)p);
		}

		public override void SetFont (object backend, Font font)
		{
		}

		public override void DrawTextLayout (object backend, TextLayout layout, double x, double y)
		{
			var c = (DrawingContext) backend;
			var t = (TextLayoutBackend)Toolkit.GetBackend (layout);
			t.FormattedText.SetForegroundBrush (c.Brush);
			c.Context.DrawText (t.FormattedText, new SW.Point (x, y));
		}

		public override bool CanDrawImage (object backend, object img)
		{
			return true;
		}

		public override void DrawImage (object backend, object img, double x, double y, double width, double height, double alpha)
		{
			var c = (DrawingContext) backend;
			ImageSource bmp = DataConverter.AsImageSource (img);
			DrawImageCore (c.Context, bmp, x, y, width, height, alpha);
		}

		public override void DrawImage (object backend, object img, Rectangle srcRect, Rectangle destRect, double width, double height, double alpha)
		{
			var c = (DrawingContext) backend;
			ImageSource bmp = DataConverter.AsImageSource (img);
			DrawImageCore (c.Context, bmp, srcRect, destRect, alpha);
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

		public override void TransformPoint (object backend, ref double x, ref double y)
		{
			var c = (DrawingContext)backend;
			var p = new SW.Point (x, y);
			foreach (var t in c.GetCurrentTransforms ())
				p = t.Transform (p);
			x = p.X;
			y = p.Y;
		}

		public override void TransformDistance (object backend, ref double dx, ref double dy)
		{
			var c = (DrawingContext)backend;
			var p = new SW.Point (dx, dy);
			foreach (var t in c.GetCurrentTransforms ().Where (tt => !(tt is TranslateTransform)))
				p = t.Transform (p);
			dx = p.X;
			dy = p.Y;
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
			foreach (var s in other.Path.Segments)
				c.Path.Segments.Add (s);
		}

		public override bool IsPointInFill (object backend, double x, double y)
        {
			var c = (DrawingContext)backend;
			return c.GetPathGeometry ().FillContains (new SW.Point (x, y));
        }

		public override bool IsPointInStroke (object backend, double x, double y)
        {
			var c = (DrawingContext)backend;
			return c.GetPathGeometry ().StrokeContains (c.Pen, new SW.Point (x, y));
		}

		public override void Dispose (object backend)
		{
		}

		internal static void DrawImageCore (SWM.DrawingContext g, ImageSource bmp, double x, double y, double width, double height, double alpha)
		{
			if (alpha < 1)
				g.PushOpacity (alpha);

			g.DrawImage (bmp, new Rect (x, y, width, height));

			if (alpha < 1)
				g.Pop ();
		}

		internal void DrawImageCore (SWM.DrawingContext g, ImageSource bmp, Rectangle srcRect, Rectangle destRect, double alpha)
		{
			if (alpha < 1)
				g.PushOpacity (alpha);

			g.PushClip (new RectangleGeometry (destRect.ToWpfRect ()));

			var size = bmp is BitmapSource ? new Size (((BitmapSource)bmp).PixelWidth, ((BitmapSource)bmp).PixelHeight) : new Size (bmp.Width, bmp.Height);
			var sw = destRect.Width / srcRect.Width;
			var sh = destRect.Height / srcRect.Height;

			g.DrawImage (bmp, new Rect (destRect.X - srcRect.X * sw, destRect.Y - srcRect.Y * sh, size.Width * sw, size.Height * sh));

			g.Pop (); // Clip

			if (alpha < 1)
				g.Pop ();
		}
	}
}

#endif