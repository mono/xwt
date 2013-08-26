// 
// DrawingPath.cs
//  
// Author:
//       Alex Corrado <corrado@xamarin.com>
// 
// Copyright (c) 2012 Xamarin Inc
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

namespace Xwt.Drawing
{
	public class DrawingPath: XwtObject, IDisposable
	{
		DrawingPathBackendHandler handler;

		public DrawingPath ()
		{
			handler = Toolkit.CurrentEngine.VectorImageRecorderContextHandler;
			Backend = handler.CreatePath ();
			Init ();
		}

		internal DrawingPath (object backend, Toolkit toolkit, DrawingPathBackendHandler h): base (backend, toolkit)
		{
			handler = h;
			Init ();
		}

		void Init ()
		{
			if (handler.DisposeHandleOnUiThread)
				ResourceManager.RegisterResource (Backend, handler.Dispose);
			else
				GC.SuppressFinalize (this);
		}
		
		public void Dispose ()
		{
			if (handler.DisposeHandleOnUiThread) {
				GC.SuppressFinalize (this);
				ResourceManager.FreeResource (Backend);
			}
			else
				handler.Dispose (Backend);
		}

		~DrawingPath ()
		{
			ResourceManager.FreeResource (Backend);
		}

		/// <summary>
		/// Copies the current drawing path.
		/// </summary>
		/// <returns>A new DrawingPath instance that is a copy of the current drawing path.</returns>
		public DrawingPath CopyPath ()
		{
			return new DrawingPath (handler.CopyPath (Backend), ToolkitEngine, handler);
		}

		/// <summary>
		/// Adds a circular arc of the given radius to the current path.
		/// The arc is centered at (xc, yc), 
		/// begins at angle1 and proceeds in the direction 
		/// of increasing angles to end at angle2. 
		/// If angle2 is less than angle1,
		/// it will be progressively increased by 1 degree until it is greater than angle1.
		/// If there is a current point, an initial line segment will be added to the path 
		/// to connect the current point to the beginning of the arc. 
		/// If this initial line is undesired, 
		/// it can be avoided by calling NewPath() before calling Arc().
		/// </summary>
		/// <param name='xc'>
		/// Xc.
		/// </param>
		/// <param name='yc'>
		/// Yc.
		/// </param>
		/// <param name='radius'>
		/// Radius.
		/// </param>
		/// <param name='angle1'>
		/// Angle1 in degrees
		/// </param>
		/// <param name='angle2'>
		/// Angle2 in degrees
		/// </param>
		public void Arc (double xc, double yc, double radius, double angle1, double angle2)
		{
			if (radius <= 0)
				throw new ArgumentException ("Radius must be greater than zero");
			handler.Arc (Backend, xc, yc, radius, angle1, angle2);
		}

		/// <summary>
		/// Adds a circular arc of the given radius to the current path.
		/// The arc is centered at (xc, yc), 
		/// begins at angle1 and proceeds in the direction 
		/// of decreasing angles to end at angle2. 
		/// If angle2 is greater than angle1 it will be progressively decreased
		/// by 1 degree until it is less than angle1.
		/// If there is a current point, an initial line segment will be added to the path 
		/// to connect the current point to the beginning of the arc. 
		/// If this initial line is undesired, 
		/// it can be avoided by calling NewPath() before calling ArcNegative().
		/// </summary>
		/// <param name='xc'>
		/// Xc.
		/// </param>
		/// <param name='yc'>
		/// Yc.
		/// </param>
		/// <param name='radius'>
		/// Radius.
		/// </param>
		/// <param name='angle1'>
		/// Angle1 in degrees
		/// </param>
		/// <param name='angle2'>
		/// Angle2 in degrees
		/// </param>
		public void ArcNegative (double xc, double yc, double radius, double angle1, double angle2)
		{
			handler.ArcNegative (Backend, xc, yc, radius, angle1, angle2);
		}

		public void ClosePath ()
		{
			handler.ClosePath (Backend);
		}

		public void CurveTo (Point p1, Point p2, Point p3)
		{
			CurveTo (p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y);
		}

		/// <summary>
		/// Adds a cubic Bezier spline to the path from the current point to position (x3, y3) in user-space coordinates, 
		/// using (x1, y1) and (x2, y2) as the control points. 
		/// </summary>
		/// <param name='x1'>
		/// X1.
		/// </param>
		/// <param name='y1'>
		/// Y1.
		/// </param>
		/// <param name='x2'>
		/// X2.
		/// </param>
		/// <param name='y2'>
		/// Y2.
		/// </param>
		/// <param name='x3'>
		/// X3.
		/// </param>
		/// <param name='y3'>
		/// Y3.
		/// </param>
		public void CurveTo (double x1, double y1, double x2, double y2, double x3, double y3)
		{
			handler.CurveTo (Backend, x1, y1, x2, y2, x3, y3);
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

		/// <summary>
		/// If the current subpath is not empty, begin a new subpath.
		/// After this call the current point will be (x, y).
		/// </summary>
		/// <param name='x'>
		/// X.
		/// </param>
		/// <param name='y'>
		/// Y.
		/// </param>
		public void MoveTo (double x, double y)
		{
			handler.MoveTo (Backend, x, y);
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

		public void RoundRectangle (Rectangle rectangle, double radius)
		{
			RoundRectangle (rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, radius);
		}

		public void RoundRectangle (Point p, double width, double height, double radius)
		{
			RoundRectangle (p.X, p.Y, width, height, radius);
		}

		public void RoundRectangle (double x, double y, double width, double height, double radius)
		{
			if (radius > width - radius)
				radius = width / 2;
			if (radius > height - radius)
				radius = height / 2;

			// top-left corner
			MoveTo (x + radius, y);

			// top edge
			LineTo (x + width - radius, y);

			// top-right corner
			if (radius > 0)
				Arc (x + width - radius, y + radius, radius, -90, 0);

			// right edge
			LineTo (x + width, y + height - radius);

			// bottom-right corner
			if (radius > 0)
				Arc (x + width - radius, y + height - radius, radius, 0, 90);

			// bottom edge
			LineTo (x + radius, y + height);

			// bottom-left corner
			if (radius > 0)
				Arc (x + radius, y + height - radius, radius, 90, 180);

			// left edge
			LineTo (x, y + radius);

			// top-left corner
			if (radius > 0)
				Arc (x + radius, y + radius, radius, 180, 270);
		}

		public void RelCurveTo (Distance d1, Distance d2, Distance d3)
		{
			RelCurveTo (d1.Dx, d1.Dy, d2.Dx, d2.Dy, d3.Dx, d3.Dy);
		}

		/// <summary>
		/// Relative-coordinate version of curve_to().
		/// All offsets are relative to the current point. 
		/// Adds a cubic Bezier spline to the path from the current point to a point offset 
		/// from the current point by (dx3, dy3), using points offset by (dx1, dy1) and (dx2, dy2) 
		/// as the control points. After this call the current point will be offset by (dx3, dy3).
		/// Given a current point of (x, y), RelCurveTo(dx1, dy1, dx2, dy2, dx3, dy3)
		/// is logically equivalent to CurveTo(x + dx1, y + dy1, x + dx2, y + dy2, x + dx3, y + dy3).
		/// </summary>
		/// <param name='dx1'>
		/// Dx1.
		/// </param>
		/// <param name='dy1'>
		/// Dy1.
		/// </param>
		/// <param name='dx2'>
		/// Dx2.
		/// </param>
		/// <param name='dy2'>
		/// Dy2.
		/// </param>
		/// <param name='dx3'>
		/// Dx3.
		/// </param>
		/// <param name='dy3'>
		/// Dy3.
		/// </param>
		public void RelCurveTo (double dx1, double dy1, double dx2, double dy2, double dx3, double dy3)
		{
			handler.RelCurveTo (Backend, dx1, dy1, dx2, dy2, dx3, dy3);
		}

		public void RelLineTo (Distance d)
		{
			RelLineTo (d.Dx, d.Dy);
		}

		/// <summary>
		/// Adds a line to the path from the current point to a point that 
		/// is offset from the current point by (dx, dy) in user space. 
		/// After this call the current point will be offset by (dx, dy).
		/// Given a current point of (x, y), 
		/// RelLineTo(dx, dy) is logically equivalent to LineTo(x + dx, y + dy).
		/// </summary>
		/// <param name='dx'>
		/// Dx.
		/// </param>
		/// <param name='dy'>
		/// Dy.
		/// </param>
		public void RelLineTo (double dx, double dy)
		{
			handler.RelLineTo (Backend, dx, dy);
		}

		/// <summary>
		/// If the current subpath is not empty, begin a new subpath.
		/// After this call the current point will offset by (x, y).
		/// Given a current point of (x, y), 
		/// RelMoveTo(dx, dy) is logically equivalent to MoveTo(x + dx, y + dy).
		/// </summary>
		/// <param name='d'>
		/// D.
		/// </param>
		public void RelMoveTo (Distance d)
		{
			RelMoveTo (d.Dx, d.Dy);
		}

		public void RelMoveTo (double dx, double dy)
		{
			handler.RelMoveTo (Backend, dx, dy);
		}

		/// <summary>
		/// Appends the given path onto the current path.
		/// </summary>
		/// <param name='p'>
		/// The path to append.
		/// </param>
		public void AppendPath (DrawingPath p)
		{
			if (p is Context)
				throw new NotSupportedException ("Can't directly append a Context object to a path");
			if (!(handler is VectorImageRecorderContextHandler) && (p.Backend is VectorBackend)) {
				var c = (VectorBackend)p.Backend;
				ToolkitEngine.VectorImageRecorderContextHandler.Draw (handler, Backend, c.ToVectorImageData ());
			} else {
				handler.AppendPath (Backend, p.Backend);
			}
		}

		public bool IsPointInFill (Point p)
		{
			return IsPointInFill (p.X, p.Y);
		}

		/// <summary>
		/// Tests whether the given point is inside the area that would be affected if the current path were filled.
		/// </summary>
		/// <returns>
		/// <c>true</c> if the specified point would be in the fill; otherwise, <c>false</c>.
		/// </returns>
		/// <param name='x'>
		/// The x coordinate.
		/// </param>
		/// <param name='y'>
		/// The y coordinate.
		/// </param>
		public bool IsPointInFill (double x, double y)
		{
			return handler.IsPointInFill (Backend, x, y);
		}
	}
}

