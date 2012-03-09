// 
// Context.cs
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

namespace Xwt.Drawing
{
	public sealed class Context: XwtObject, IDisposable
	{
		static IContextBackendHandler handler;
		
		Pattern pattern;
		Font font;
		
		static Context ()
		{
			handler = WidgetRegistry.CreateSharedBackend<IContextBackendHandler> (typeof(Context));
		}
		
		protected override IBackendHandler BackendHandler {
			get {
				return handler;
			}
		}
		
		internal Context (object backend): base (backend)
		{
		}
		
		public void Save ()
		{
			handler.Save (Backend);
		}
		
		public void Restore ()
		{
			handler.Restore (Backend);
		}
		
		public void SetColor (Color color)
		{
			handler.SetColor (Backend, color);
		}
		
		public void Arc (double xc, double yc, double radius, double angle1, double angle2)
		{
			handler.Arc (Backend, xc, yc, radius, angle1, angle2);
		}
		
		public void Clip()
		{
			handler.Clip (Backend);
		}
		
		public void ClipPreserve()
		{
			handler.ClipPreserve (Backend);
		}
		
		public void ResetClip()
		{
			handler.ResetClip (Backend);
		}
		
		public void ClosePath()
		{
			handler.ClosePath (Backend);
		}
		
		public void CurveTo (Point p1, Point p2, Point p3)
		{
			CurveTo (p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y);
		}
		
		public void CurveTo (double x1, double y1, double x2, double y2, double x3, double y3)
		{
			handler.CurveTo (Backend, x1, y1, x2, y2, x3, y3);
		}
		
		public void Fill ()
		{
			handler.Fill (Backend);
		}
		
		public void FillPreserve ()
		{
			handler.FillPreserve (Backend);
		}
		
		public void LineTo (Point p)
		{
			LineTo (p.X, p.Y);
		}
		
		public void LineTo (double x, double y)
		{
			handler.LineTo (Backend, x, y);
		}
		
		public void MoveTo (Point p)
		{
			MoveTo (p.X, p.Y);
		}
		
		public void MoveTo (double x, double y)
		{
			handler.MoveTo (Backend, x, y);
		}
		
		public void NewPath ()
		{
			handler.NewPath (Backend);
		}
		
		public void Rectangle (Rectangle rectangle)
		{
			Rectangle (rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
		}
		
		public void Rectangle (Point p, double width, double height)
		{
			Rectangle (p.X, p.Y, width, height);
		}
		
		public void Rectangle (double x, double y, double width, double height)
		{
			handler.Rectangle (Backend, x, y, width, height);
		}
		
		public void RelCurveTo (Distance d1, Distance d2, Distance d3)
		{
			RelCurveTo (d1.Dx, d1.Dy, d2.Dx, d2.Dy, d3.Dx, d3.Dy);
		}
		
		public void RelCurveTo (double dx1, double dy1, double dx2, double dy2, double dx3, double dy3)
		{
			handler.RelCurveTo (Backend, dx1, dy1, dx2, dy2, dx3, dy3);
		}
		
		public void RelLineTo (Distance d)
		{
			RelLineTo (d.Dx, d.Dy);
		}
		
		public void RelLineTo (double dx, double dy)
		{
			handler.RelLineTo (Backend, dx, dy);
		}
		
		public void RelMoveTo (Distance d)
		{
			RelMoveTo (d.Dx, d.Dy);
		}
		
		public void RelMoveTo (double dx, double dy)
		{
			handler.RelMoveTo (Backend, dx, dy);
		}
		
		public void Stroke ()
		{
			handler.Stroke (Backend);
		}
		
		public void StrokePreserve ()
		{
			handler.StrokePreserve (Backend);
		}
		
		public void SetLineWidth (double width)
		{
			handler.SetLineWidth (Backend, width);
		}
		
		public void DrawTextLayout (TextLayout layout, Point location)
		{
			handler.DrawTextLayout (Backend, layout, location.X, location.Y);
		}
		
		public void DrawTextLayout (TextLayout layout, double x, double y)
		{
			handler.DrawTextLayout (Backend, layout, x, y);
		}
		
		public void DrawImage (Image img, Point location)
		{
			handler.DrawImage (Backend, GetBackend (img), location.X, location.Y, 1);
		}
		
		public void DrawImage (Image img, double x, double y)
		{
			handler.DrawImage (Backend, GetBackend (img), x, y, 1);
		}
		
		public void DrawImage (Image img, Point location, double alpha)
		{
			handler.DrawImage (Backend, GetBackend (img), location.X, location.Y, alpha);
		}
		
		public void DrawImage (Image img, double x, double y, double alpha)
		{
			handler.DrawImage (Backend, GetBackend (img), x, y, alpha);
		}
		
		public void DrawImage (Image img, Rectangle rect)
		{
			handler.DrawImage (Backend, GetBackend (img), rect.X, rect.Y, rect.Width, rect.Height, 1);
		}
		
		public void DrawImage (Image img, double x, double y, double width, double height)
		{
			handler.DrawImage (Backend, GetBackend (img), x, y, width, height, 1);
		}
		
		public void DrawImage (Image img, Rectangle rect, double alpha)
		{
			handler.DrawImage (Backend, GetBackend (img), rect.X, rect.Y, rect.Width, rect.Height, alpha);
		}

        /// <summary>
        /// Resets the Current Trasnformation Matrix (CTM) to the Identity Matrix
        /// </summary>
		public void ResetTransform ()
		{
			handler.ResetTransform (Backend);
		}
		
		
		/// <summary>
		/// Applies a rotation transformation
		/// </summary>
		/// <param name='angle'>
		/// Angle in degrees
		/// </param>
		/// <remarks>
		/// Modifies the current transformation matrix (CTM) by rotating the user-space axes by angle degrees.
		/// The rotation of the axes takes places after any existing transformation of user space.
		/// The rotation direction for positive angles is from the positive X axis toward the positive Y axis.
		/// </remarks>
		public void Rotate (double angle)
		{
			handler.Rotate (Backend, angle);
		}
		
		public void Translate (double tx, double ty)
		{
			handler.Translate (Backend, tx, ty);
		}
		
		public void Translate (Point p)
		{
			handler.Translate (Backend, p.X, p.Y);
		}
		
		public void Dispose ()
		{
			handler.Dispose (Backend);
		}
		
		public Pattern Pattern {
			get { return pattern; }
			set { pattern = value; handler.SetPattern (Backend, GetBackend (value)); }
		}
		
		public Font Font {
			get { return font; }
			set { font = value; handler.SetFont (Backend, value); }
		}
		
		public void SetLineDash (double offset, params double[] pattern)
		{
			handler.SetLineDash (Backend, offset, pattern);
		}
	}
}

