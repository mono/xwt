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


namespace Xwt.Drawing
{
	public sealed class Context: Path
	{
		ContextBackendHandler handler;
		Pattern pattern;
		Font font;
		double globalAlpha = 1;
		
		internal Context (object backend): base (backend)
		{
			handler = ToolkitEngine.ContextBackendHandler;
		}

		internal Context (object backend): base (backend, ToolkitEngine.ContextBackendHandler)
		{
			handler = ToolkitEngine.ContextBackendHandler;
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
		
		public void Fill ()
		{
			handler.Fill (Backend);
		}
		
		public void FillPreserve ()
		{
			handler.FillPreserve (Backend);
		}

		public void NewPath ()
		{
			handler.NewPath (Backend);
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

		public bool IsPointInStroke (Point p)
		{
			return IsPointInStroke (p.X, p.Y);
		}
		
		/// <summary>
		/// Tests whether the given point is inside the area that would be affected if Stroke were called on this Context.
		/// </summary>
		/// <returns>
		/// <c>true</c> if the specified point would be in the stroke; otherwise, <c>false</c>.
		/// </returns>
		/// <param name='x'>
		/// The x coordinate.
		/// </param>
		/// <param name='y'>
		/// The y coordinate.
		/// </param>
		public bool IsPointInStroke (double x, double y)
		{
			return handler.IsPointInStroke (Backend, x, y);
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

