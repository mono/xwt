// 
// DrawingTransforms.cs
//  
// Authors:
//       Lluis Sanchez <lluis@xamarin.com>
//       Lytico (http://limada.sourceforge.net)
//       Hywel Thomas <hywel.w.thomas@gmail.com>
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
using Xwt;
using Xwt.Drawing;

namespace Samples
{
	public class Drawings: Canvas
	{
		public Drawings ()
		{
			this.BackgroundColor = Colors.White;
		}
	}
	
	public class DrawingTransforms: Canvas
	{
		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			base.OnDraw (ctx, dirtyRect);
			Transforms (ctx, 5, 5);
		}

		public virtual void Transforms (Xwt.Drawing.Context ctx, double x, double y)
		{
			Rotate (ctx, x, y);
			Scale (ctx, x + 140, y);
			Reflect (ctx, x+20, y + 100);
		}
		
		public virtual void Rotate (Xwt.Drawing.Context ctx, double x, double y)
		{
			// draws a line along the x-axis from (0,0) to (r,0) with a constant translation and an increasing
			// rotational component. This composite transform is then applied to a vertical line, with inverse
			// color, and an additional x-offset, to form a mirror image figure for easy visual comparison.
			// These transformed points must be drawn with the identity CTM, hence the Restore() each time.

			ctx.Save ();	// save caller's context (assumed to be the Identity CTM)
			ctx.SetLineWidth (3);	// should align exactly if drawn with half-pixel coordinates

			// Vector length (pixels) and rotation limit (degrees)
			double r = 30;
			double end = 270;
			
			for (double n = 0; n<=end; n += 5) {
				ctx.Save ();	// save context and identity CTM for each line

				// Set up translation to centre point of first figure, ensuring pixel alignment
				ctx.Translate (x + 30.5, y + 30.5);
				ctx.Rotate (n);
				ctx.MoveTo (0, 0);
				ctx.RelLineTo (r, 0);
				double c = n / end;
				ctx.SetColor (new Color (c, c, c));
				ctx.Stroke ();	// stroke first figure with composite Translation and Rotation CTM

				// Generate mirror image figure as a visual test of TransformPoints
				Point p0 = new Point (0,0);
				Point p1 = new Point (0, -r);
				Point[] p = new Point[] {p0, p1};
				ctx.TransformPoints (p);	// using composite transformation

				ctx.Restore ();	// restore identity CTM 
				ctx.Save ();	// save again (to restore after additional Translation)

				ctx.Translate (2 * r + 1, 0);	// extra x-offset to clear first figure
				ctx.MoveTo (p[0]);
				ctx.LineTo (p[1]);
				c = 1-c;
				ctx.SetColor (new Color (c, c, c));
				ctx.Stroke();		// stroke transformed points with offset in CTM

				ctx.Restore ();		// restore identity CTM for next line
			}
			ctx.Restore ();	// restore caller's context
		}
		
		public virtual void Scale (Context ctx, double ax, double ay)
		{
			ctx.Save ();
			ctx.Translate (ax, ay);
			ctx.SetColor (Colors.Black);
			ctx.SetLineWidth (1);
			
			var x = 0d;
			var y = 0d;
			var w = 10d;
			var inc = .1d;
			for (var i = inc; i < 3.5d; i +=inc) {
				ctx.Save ();
				ctx.Scale (i, i);
				ctx.Rectangle (x, y, w, w);
				ctx.SetColor (Colors.Yellow.WithAlpha (1 / i));
				ctx.FillPreserve ();
				ctx.SetColor (Colors.Red.WithAlpha (1 / i));
				ctx.Stroke ();
				ctx.MoveTo (x += w * inc, y += w * inc / 3);
				ctx.Restore ();
				
			}

			ctx.Restore ();
		}

		/// <summary>
		/// Illustrates the use of matrix transforms to skew and reflect text
		/// </summary>
		public void Reflect (Context ctx, double x, double y)
		{
			ctx.Save ();
			ctx.SetLineWidth (1);

			TextLayout layout = new TextLayout ();
			layout.Text = "Reflected and Skewed Text";
			layout.Font = Font.WithSize (16);
			Size size = layout.GetSize ();
			Rectangle r = new Rectangle (Point.Zero, size);

			// Draw text with no transformations at (x+0.5, y+0.5)
			ctx.Translate (x+0.5, y+0.5);	// final move to specified location
			ctx.SetColor (Colors.Blue);
			ctx.DrawTextLayout (layout, 0, 0);

			// Use Matrix transforms to reflect Y-values and skew X-values by -0.5*Y
			// Note that transforms are prepended, so are actioned in reverse order
			// This is the same order that Backend Context transforms are applied

			Matrix m = new Matrix ();				// Identity matrix
			Matrix s = new Matrix (1.0, 0.0,		// new skew matrix
			                      -0.5, 1.0,
			                       0.0, 0.0);

			m.Translate (0, size.Height);			// Shift text back to place
			m.Prepend (s);							// Skew X-values
			m.Scale (1, -1);						// Reflect text Y-values
			m.Translate (0, -size.Height);			// Shift text base to (0,0)

			ctx.ModifyCTM (m);		// NB ctx.Translate (x+0.5, y+0.5) still active

			ctx.SetColor (Colors.DarkGray);
			ctx.Rectangle (r);
			ctx.Fill ();
			ctx.SetColor (Colors.LightBlue);
			ctx.DrawTextLayout (layout, 0, 0);

			ctx.Restore ();
		}

	}
}

