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
		double globalAlpha = 1;
		
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
		
		/// <summary>
		/// Makes a copy of the current state of the Context and saves it on an internal stack of saved states.
		/// When Restore() is called, it will be restored to the saved state. 
		/// Multiple calls to Save() and Restore() can be nested; 
		/// each call to Restore() restores the state from the matching paired save().
		/// </summary>
		public void Save ()
		{
			handler.Save (Backend);
		}
		
		public void Restore ()
		{
			handler.Restore (Backend);
		}
		
		public double GlobalAlpha {
			get { return globalAlpha; }
			set {
				globalAlpha = value;
				handler.SetGlobalAlpha (Backend, globalAlpha);
			}
		}
		
		public void SetColor (Color color)
		{
			handler.SetColor (Backend, color);
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
			handler.Arc (Backend, xc, yc, radius, angle1, angle2);
		}
		
		/// <summary>
		/// Establishes a new clip region by intersecting the current clip region with the current Path 
		/// as it would be filled by fill() and according to the current fill rule.
		/// After clip(), the current path will be cleared from the Context.
		/// The current clip region affects all drawing operations by effectively masking out any changes to the surface 
		/// that are outside the current clip region.
		/// Calling clip() can only make the clip region smaller, never larger. 
		/// But the current clip is part of the graphics state, 
		/// so a temporary restriction of the clip region can be achieved by calling clip() within a save()/restore() pair. 
		/// The only other means of increasing the size of the clip region is reset_clip().
		/// </summary>
		public void Clip ()
		{
			handler.Clip (Backend);
		}
		
		public void ClipPreserve ()
		{
			handler.ClipPreserve (Backend);
		}
		
		public void ResetClip ()
		{
			handler.ResetClip (Backend);
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

		public void DrawImage (Image img, Rectangle srcRect, Rectangle destRect)
		{
			handler.DrawImage (Backend, GetBackend (img), srcRect, destRect, 1);
		}

		public void DrawImage (Image img, Rectangle srcRect, Rectangle destRect, double alpha)
		{
			handler.DrawImage (Backend, GetBackend (img), srcRect, destRect, alpha);
		}

		/// <summary>
		/// Resets the current trasnformation matrix (CTM) to the Identity Matrix
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
		/// </summary>
		public void Rotate (double angle)
		{
			handler.Rotate (Backend, angle);
		}
		
		public void Scale (double scaleX, double scaleY)
		{
			handler.Scale (Backend, scaleX, scaleY);
		}
		
		public void Translate (double tx, double ty)
		{
			handler.Translate (Backend, tx, ty);
		}
		
		public void Translate (Point p)
		{
			handler.Translate (Backend, p.X, p.Y);
		}
		
		/// <summary>
		/// Transforms the point (x, y) by the current transformation matrix (CTM)
		/// </summary>
		public void TransformPoint (ref double x, ref double y)
		{
			handler.TransformPoint (Backend, ref x, ref y);
		}

		/// <summary>
		/// Transforms the distance (dx, dy) by the scale and rotation elements (only) of the CTM
		/// </summary>
		public void TransformDistance (ref double dx, ref double dy)
		{
			handler.TransformDistance (Backend, ref dx, ref dy);
		}

		/// <summary>
		/// Transforms the array of points by the current transformation matrix (CTM)
		/// </summary>
		public void TransformPoints (Point[] points)
		{
			handler.TransformPoints (Backend, points);
		}

		/// <summary>
		/// Transforms the array of distances by the scale and rotation elements (only) of the CTM
		/// </summary>
		public void TransformDistances (Distance[] vectors)
		{
			handler.TransformDistances (Backend, vectors);
		}

		public void Dispose ()
		{
			handler.Dispose (Backend);
		}
		
		public Pattern Pattern {
			get { return pattern; }
			set {
				pattern = value;
				handler.SetPattern (Backend, GetBackend (value));
			}
		}
		
		public Font Font {
			get { return font; }
			set {
				font = value;
				handler.SetFont (Backend, value);
			}
		}
		
		/// <summary>
		/// Sets the dash pattern to be used by stroke().
		/// A dash pattern is specified by dashes, an array of positive values. 
		/// Each value provides the user-space length of altenate "on" and "off" portions of the stroke. 
		/// The offset specifies an offset into the pattern at which the stroke begins.
		/// If dashes is empty dashing is disabled. If the size of dashes is 1, 
		/// a symmetric pattern is assumed with alternating on and off portions of the size specified by the single value in dashes.
		/// It is invalid for any value in dashes to be negative, or for all values to be 0. 
		/// If this is the case, an exception will be thrown
		/// </summary>
		/// <param name='offset'>
		/// Offset.
		/// </param>
		/// <param name='pattern'>
		/// Pattern.
		/// </param>
		public void SetLineDash (double offset, params double[] pattern)
		{
			handler.SetLineDash (Backend, offset, pattern);
		}
	}
}

