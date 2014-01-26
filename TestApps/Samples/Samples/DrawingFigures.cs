// 
// DrawingFigures.cs
//  
// Author:
//       Lytico (http://limada.sourceforge.net)
//       Lluis Sanchez <lluis@xamarin.com>
//       Hywel Thomas <hywel.w.thomas@gmail.com>
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
using Xwt;
using Xwt.Drawing;

namespace Samples
{
	public class DrawingFigures: Drawings
	{
		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			base.OnDraw (ctx, dirtyRect);
			Figures (ctx, 5, 25);
		}
		
		public virtual void Figures (Context ctx, double x, double y)
		{
			Lines (ctx);

			Rectangles (ctx, x, y + 20);
			Curves1 (ctx, x, y + 80);
			Curves2 (ctx, x + 100, y + 80);

			Path (ctx, x + 210, y + 20);
		}

		/// <summary>
		/// Visual test for pixel alignment and odd/even line widths
		/// </summary>
		public void Lines (Context ctx)
		{
			ctx.Save ();

			ctx.SetColor (Colors.Black);

			int nPairs = 4;
			double length = 90;
			double gap = 2;

			// set half-pixel y-coordinate for sharp single-pixel-wide line
			// on first line of Canvas, extending to match line pairs below
			ctx.SetLineWidth (1);
			double x = 0;
			double y = 0.5;
			double end = x + 2*(length - 1) + gap;
			ctx.MoveTo (x, y);
			ctx.LineTo (end, y);
			ctx.Stroke ();

			// draw pairs of lines with odd and even widths,
			// each pair aligned on half-pixel y-coordinates
			y = 4.5;
			for (int w = 1; w <= nPairs; ++w) {
				x = 0;
				ctx.SetLineWidth (w);
				ctx.MoveTo (x, y);
				ctx.RelLineTo (length-1, 0);
				ctx.Stroke ();

				ctx.SetLineWidth (w + 1);
				x += (gap + length - 1);
				ctx.MoveTo (x, y);
				ctx.RelLineTo (length-1, 0);
				ctx.Stroke ();
				y += w * 2 + gap;
			}

			ctx.Restore ();
		}

		public virtual void Rectangles (Context ctx, double x, double y)
		{
			ctx.Save ();
			ctx.Translate (x, y);
			
			// Simple rectangles
			
			ctx.SetLineWidth (1);
			ctx.Rectangle (0, 0, 10, 10);
			ctx.SetColor (Colors.Black);
			ctx.Fill ();
			
			ctx.Rectangle (15, 0, 10, 10);
			ctx.SetColor (Colors.Black);
			ctx.Stroke ();
			
			ctx.SetLineWidth (3);
			ctx.Rectangle (0, 15, 10, 10);
			ctx.SetColor (Colors.Black);
			ctx.Fill ();
			
			ctx.Rectangle (15, 15, 10, 10);
			ctx.SetColor (Colors.Black);
			ctx.Stroke ();
			
			ctx.Restore ();
			
			// Rectangle with hole
			ctx.Save ();
			ctx.Translate (x + 50, y);
			
			ctx.Rectangle (0, 0, 40, 40);
			ctx.MoveTo (35, 35);
			ctx.RelLineTo (0, -20);
			ctx.RelLineTo (-20, 0);
			ctx.RelLineTo (0, 20);
			ctx.ClosePath ();
			ctx.SetColor (Colors.Black);
			ctx.Fill ();
			
			ctx.Restore ();
			
			// Rounded Rectangle with Arcs
			ctx.Save ();
			ctx.Translate (x + 120, y);
			
			var r = 5;
			var l = 0;
			var t = 0;
			var w = 50;
			var h = 30;

			ctx.SetColor (Colors.Black);
			// top left  
			ctx.Arc (l + r, t + r, r, 180, 270);
			// top right 
			ctx.Arc (l + w - r, t + r, r, 270, 0);
			// bottom right  
			ctx.Arc (l + w - r, t + h - r, r, 0, 90);
			// bottom left 
			ctx.Arc (l + r, t + h - r, r, 90, 180);
			
			ctx.ClosePath ();
			ctx.StrokePreserve ();
			ctx.SetColor (Colors.AntiqueWhite);
			ctx.Fill ();
			ctx.Restore ();
		}

		public virtual void Curves1 (Context ctx, double x, double y)
		{
			ctx.Save ();
			ctx.Translate (x, y);
			
			ctx.SetLineWidth (1);
			Action curve1 = () => {
				ctx.MoveTo (0, 30);
				ctx.CurveTo (20, 0, 50, 0, 60, 25);
			};
			// curve2 with lineTo; curve1 is closed
			Action curve2 = () => {
				ctx.LineTo (0, 0);
				ctx.CurveTo (20, 30, 50, 30, 60, 5);
			};
			Action paint = () => {
				curve1 ();
				curve2 ();
				ctx.ClosePath ();
				ctx.SetColor (new Color (0, 0, 0, .5));
				ctx.StrokePreserve ();
				ctx.SetColor (new Color (1, 0, 1, .5));
				ctx.Fill ();
			};
			paint ();

			ctx.Translate (0, 40);
			// curve2 with moveTo; curve1 is open
			curve2 = () => {
				ctx.MoveTo (0, 0);
				ctx.CurveTo (20, 30, 50, 30, 60, 5);
			};
			paint ();
			ctx.Restore ();
			
			//Todo: same stuff with arc
		}

		public virtual void Curves2 (Context ctx, double sx, double sy)
		{
			ctx.Save ();
			ctx.Translate (sx, sy);
			ctx.SetColor (Colors.Black);
			
			double x = 0, y = 40;
			double x1 = y - x, y1 = x1 + y, x2 = x + y, y2 = x, x3 = y1, y3 = y;

			ctx.MoveTo (x, y);
			ctx.CurveTo (x1, y1, x2, y2, x3, y3);

			ctx.SetLineWidth (2.0);
			ctx.Stroke ();

			ctx.SetColor (new Color (1, 0.2, 0.2, 0.6));
			ctx.SetLineWidth (1.0);
			ctx.MoveTo (x, y);
			ctx.LineTo (x1, y1);
			ctx.MoveTo (x2, y2);
			ctx.LineTo (x3, y3);
			ctx.Stroke ();
			
			ctx.Restore ();
		}

		public void Path (Context ctx, double px, double py)
		{
			ctx.Save ();
			ctx.Translate (px, py);

			var path = new DrawingPath ();

			path.MoveTo (0.44, 18);
			path.LineTo (-1, 18);
			path.LineTo (-1, 26);
			path.LineTo (0.44, 26);
			path.LineTo (0, 42);
			path.LineTo (29, 21.98);
			path.LineTo (29, 21.98);
			path.LineTo (0, 2);
			path.LineTo (0.44, 18);

			ctx.AppendPath (path);
			ctx.SetColor (Colors.Black);
			ctx.SetLineWidth (2);
			ctx.Stroke ();

			var path2 = path.CopyPath ();

			path2.LineTo (15, 8);
			path2.ClosePath ();

			ctx.Rotate (180);
			ctx.AppendPath (path2);
			ctx.SetColor (Colors.Red);
			ctx.SetLineDash (0, 5);
			ctx.Stroke ();

			ctx.Restore ();
		}
	}
}

