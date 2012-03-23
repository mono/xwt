// 
// DrawingTransforms.cs
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
using Xwt;
using Xwt.Drawing;

namespace Samples
{
	public class Drawings: Canvas
	{
		
		public enum Test
		{
			Figures,
			PatternsAndImages,
			Texts,
			Transforms
		}
		
		Test test = Test.Figures;

		public Drawings (Test test)
		{
			this.test = test;
		}
		
		protected override void OnDraw (Context ctx)
		{
			base.OnDraw (ctx);
			ctx.SetColor(Colors.White);
			ctx.Rectangle(this.Bounds);
			ctx.Fill();
			switch (test) {
			case Test.Figures:
				Figures (ctx, 5, 5);
				break;
			case Test.PatternsAndImages:
				PatternsAndImages (ctx, 5, 5);
				break;
			case Test.Texts:
				Texts (ctx, 5, 5);
				break;
			case Test.Transforms:
				Transforms (ctx, 5, 5);
				break;
			}
			ctx.SetColor(this.BackgroundColor);
		}

		public virtual void Figures (Context ctx, double x, double y)
		{
			Rectangles (ctx, x, y);
			Curves1 (ctx, x, y + 60);
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
			curve2 = () => {
				ctx.MoveTo (0, 0);
				ctx.CurveTo (20, 30, 50, 30, 60, 5);
			};
			paint ();
			ctx.Restore ();
		}

		public virtual void PatternsAndImages (Context ctx, double x, double y)
		{
			ctx.Save ();
			ctx.Translate (x, y);
			
			ctx.SetColor(Colors.Black);
			// Dashed lines
			
			ctx.SetLineDash (15, 10, 10, 5, 5);
			ctx.Rectangle (10, 10, 100, 100);
			ctx.Stroke ();
			ctx.SetLineDash (0);
			
			// Image
			var arcColor = new Color (1, 0, 1);
			ImageBuilder ib = new ImageBuilder (30, 30, ImageFormat.ARGB32);
			ib.Context.Arc (15, 15, 15, 0, 360);
			ib.Context.SetColor (arcColor);
			ib.Context.Fill ();
			ib.Context.SetColor (Colors.DarkKhaki);
			ib.Context.Rectangle (0, 0, 5, 5);
			ib.Context.Fill ();
			var img = ib.ToImage ();
			ctx.DrawImage (img, 0, 0);
			ctx.DrawImage (img, 0, 50, 50, 10);
			
			ctx.Arc (100, 100, 15, 0, 360);
			arcColor.Alpha=0.4;
			ctx.SetColor (arcColor);
			ctx.Fill ();
			
			// ImagePattern
			
			ctx.Save ();
			
			ctx.Translate (x + 130, y);
			ctx.Pattern = new ImagePattern (img);
			ctx.Rectangle (0, 0, 100, 100);
			ctx.Fill ();
			ctx.Restore ();
			
			ctx.Restore ();
		}
		
		public virtual void Texts (Xwt.Drawing.Context ctx, double x, double y)
		{
			ctx.Save ();
            
   			ctx.Translate (x, y);
		
			ctx.SetColor (Colors.Black);

			var text = new TextLayout (ctx);
			text.Font = this.Font.WithSize (10);
			
			text.Text = "Lorem ipsum dolor sit amet,";
			ctx.DrawTextLayout (text, 0, 0);
			var size1 = text.GetSize ();
			
			// proofing width; test should align with text above
			ctx.SetColor (Colors.DarkMagenta);
			text.Text = "consetetur sadipscing elitr, sed diam nonumy";
			text.Width = size1.Width;
			ctx.DrawTextLayout (text, 0, size1.Height+10);
			
			var size2 = text.GetSize ();
			text.Text = string.Format ("Size 1 {0}\r\nSize 2 {1}", size1, size2);
			
			ctx.Save ();
			ctx.SetColor (Colors.Black);

			ctx.Rotate (10);
			// maybe someone knows a formula with angle and textsize to calculyte ty
			var ty = 30; 
			ctx.DrawTextLayout (text, ty, size1.Height+size2.Height+20);

			ctx.Restore ();
			
			// scale example here:
			
			ctx.Restore ();
		}

		public virtual void Transforms (Xwt.Drawing.Context ctx, double x, double y)
		{
			Rotate (ctx, x, y);
			Scale (ctx, x + 100, y);
			
		}
		
		public virtual void Rotate (Xwt.Drawing.Context ctx, double x, double y)
		{
			ctx.Save ();
			ctx.Translate (x + 30, y + 30);
			
			// Rotation
			
			double end = 270;
			
			for (double n = 0; n<=end; n += 5) {
				ctx.Save ();
				ctx.Rotate (n);
				ctx.MoveTo (0, 0);
				ctx.RelLineTo (30, 0);
				double c = n / end;
				ctx.SetColor (new Color (c, c, c));
				ctx.Stroke ();
				ctx.Restore ();
			}
			
			ctx.ResetTransform ();
			ctx.Restore ();
		}
		
		public virtual void Scale (Context ctx, double ax, double ay)
		{
			ctx.Save ();
			ctx.Translate (ax, ay);
			
			ctx.SetLineWidth (1);
			
			var x = 0d;
			var y = 0d;
			var w = 10d;

			for (var i = 1d; i < 3; i += .5) {
				ctx.Save ();
				ctx.Rectangle (x, y, w, w);
//				ctx.Scale (i, i);
				ctx.Stroke ();
				ctx.Restore ();
				ctx.MoveTo (x += w / 2, y += w / 2);
			}

			ctx.Restore ();
		}
	}
	
	public class DrawingFigures:Drawings
	{
		public DrawingFigures ():base(Drawings.Test.Figures)
		{
		}
	}

	public class DrawingPatternsAndImages:Drawings
	{
		public DrawingPatternsAndImages ():base(Drawings.Test.PatternsAndImages)
		{
		}
	}

	public class DrawingTexts:Drawings
	{
		public DrawingTexts ():base(Drawings.Test.Texts)
		{
		}
	}

	public class DrawingTransforms:Drawings
	{
		public DrawingTransforms ():base(Drawings.Test.Transforms)
		{
		}
	}
}

