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
		protected override void OnDraw (Context ctx)
		{
			base.OnDraw (ctx);
			Transforms (ctx, 5, 5);
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
			ctx.SetLineWidth (3);
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
	}
}

