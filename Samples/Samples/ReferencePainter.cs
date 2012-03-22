// 
// DrawingTransforms.cs
//  
// Authors:
//       Lluis Sanchez <lluis@xamarin.com>
//       Lytico (http://limada.sourceforge.net)
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
using Xwt.Drawing;

namespace Samples
{
	public class ReferencePainter
	{
		public virtual void Rectangles (Xwt.Drawing.Context ctx)
		{
			// Simple rectangles
			
			ctx.SetLineWidth (1);
			ctx.Rectangle (100, 5, 10, 10);
			ctx.SetColor (Color.Black);
			ctx.Fill ();
			
			ctx.Rectangle (115, 5, 10, 10);
			ctx.SetColor (Color.Black);
			ctx.Stroke ();
			
			//
			
			ctx.SetLineWidth (3);
			ctx.Rectangle (100, 20, 10, 10);
			ctx.SetColor (Color.Black);
			ctx.Fill ();
			
			ctx.Rectangle (115, 20, 10, 10);
			ctx.SetColor (Color.Black);
			ctx.Stroke ();
			
			// Rectangle with hole
			
			ctx.Rectangle (10, 100, 40, 40);
			ctx.MoveTo (45, 135);
			ctx.RelLineTo (0, -20);
			ctx.RelLineTo (-20, 0);
			ctx.RelLineTo (0, 20);
			ctx.ClosePath ();
			ctx.SetColor (Color.Black);
			ctx.Fill ();
		}
		
		public virtual void LineDash (Xwt.Drawing.Context ctx)
		{
			// Dashed lines
			
			ctx.SetLineDash (15, 10, 10, 5, 5);
			ctx.Rectangle (100, 100, 100, 100);
			ctx.Stroke ();
			ctx.SetLineDash (0);
		}
		
		public virtual void Imaging (Xwt.Drawing.Context ctx)
		{
			
			ImageBuilder ib = new ImageBuilder (30, 30, ImageFormat.ARGB32);
			ib.Context.Arc (15, 15, 15, 0, 360);
			ib.Context.SetColor (new Color (1, 0, 1));
			ib.Context.Rectangle (0, 0, 5, 5);
			ib.Context.Fill ();
			var img = ib.ToImage ();
			ctx.DrawImage (img, 90, 90);
			ctx.DrawImage (img, 90, 140, 50, 10);
			
			ctx.Arc (190, 190, 15, 0, 360);
			ctx.SetColor (new Color (1, 0, 1, 0.4));
			ctx.Fill ();
			
			ImagePattern (img, ctx);
		}
		
		public virtual void ImagePattern (Image img, Xwt.Drawing.Context ctx)
		{
			ctx.Save ();
			ctx.Translate (90, 220);
			if (img != null)
				ctx.Pattern = new ImagePattern (img);
			
			ctx.Rectangle (0, 0, 100, 70);
			ctx.Fill ();
			ctx.Restore ();
		}
		
		public virtual void Rotation (Xwt.Drawing.Context ctx)
		{
			
			ctx.Translate (30, 30);
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
		}
		
		public virtual void Curves (Xwt.Drawing.Context ctx)
		{
			ctx.Save ();
			ctx.SetLineWidth (1);
			ctx.Translate (10, 220);
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
		
		public virtual void Text (Xwt.Drawing.Context ctx)
		{
			ctx.Save ();
            
			ctx.SetColor (Color.Black);

			ctx.Translate (10, 300);

			ctx.Save ();

			var text = new TextLayout (ctx);

			text.Text = this.GetType ().Name;
			text.Font = ctx.Font.WithSize (10);
			ctx.DrawTextLayout (text, 0, 0);
			var size = text.GetSize ();

			text.Width = size.Width;
			text.Text = string.Format ("Size {0}", size);
			ctx.Translate (size.Width + 10, 0);
			ctx.Rotate (10);

			ctx.DrawTextLayout (text, 0, 0);

			ctx.Restore ();
			size = text.GetSize ();
			text.Text = string.Format ("Size 2 {0}", size);
			ctx.Translate (0, size.Height);
			ctx.DrawTextLayout (text, 0, 0);

			ctx.Restore ();
		}

		public virtual void All (Context ctx)
		{
			Rectangles (ctx);

			LineDash (ctx);

			Imaging (ctx);

			Rotation (ctx);

			Curves (ctx);

			Text (ctx);
		}
	}
}