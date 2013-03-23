// 
// DrawingPatternsAndImages.cs
//  
// Author:
//       Lytico (http://limada.sourceforge.net)
//       Lluis Sanchez <lluis@xamarin.com>
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
	public class DrawingPatternsAndImages: Drawings
	{
		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			base.OnDraw (ctx, dirtyRect);
			PatternsAndImages (ctx, 5, 5);
		}
		
		public void PatternsAndImages (Context ctx, double x, double y)
		{
			ctx.Save ();
			ctx.Translate (x, y);
			
			ctx.SetColor (Colors.Black);
			// Dashed lines

			ctx.SetLineWidth (2);
			ctx.SetLineDash (15, 10, 10, 5, 5);
			ctx.Rectangle (10, 10, 100, 100);
			ctx.Stroke ();
			ctx.SetLineDash (0);
			
			// Image
			var arcColor = new Color (1, 0, 1);
			ImageBuilder ib = new ImageBuilder (30, 30);
			ib.Context.Arc (15, 15, 15, 0, 360);
			ib.Context.SetColor (arcColor);
			ib.Context.Fill ();
			ib.Context.SetColor (Colors.DarkKhaki);
			ib.Context.Rectangle (0, 0, 5, 5);
			ib.Context.Fill ();
			var img = ib.ToVectorImage ();
			ctx.DrawImage (img, 0, 0);
			ctx.DrawImage (img, 0, 50, 50, 10);
			
			ctx.Arc (100, 100, 15, 0, 360);
			arcColor.Alpha = 0.4;
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
			
			// Setting pixels
			
			ctx.SetLineWidth (1);
			for (int i=0; i<50;i++) {
				for (var j=0; j<50;j++) {
					Color c = Color.FromHsl (0.5, (double)i / 50d, (double)j / 50d);
					ctx.Rectangle (i, j, 1, 1);
					ctx.SetColor (c);
					ctx.Fill ();
				}
			}
		}	
	}
}

