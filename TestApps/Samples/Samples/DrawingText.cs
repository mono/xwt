// 
// DrawingText.cs
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
	public class DrawingText: Drawings
	{
		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			base.OnDraw (ctx, dirtyRect);
			Texts (ctx, 5, 5);
		}
		
		public virtual void Texts (Xwt.Drawing.Context ctx, double x, double y)
		{
			ctx.Save ();
            
			ctx.Translate (x, y);
		
			ctx.SetColor (Colors.Black);
			
			var col1 = new Rectangle ();
			var col2 = new Rectangle ();
			
			var text = new TextLayout ();
			text.Font = this.Font.WithSize (24);
			Console.WriteLine (text.Font.Size);
			
			// first text
			text.Text = "Lorem ipsum dolor sit amet,";
			var size1 = text.GetSize ();
			col1.Width = size1.Width;
			col1.Height += size1.Height + 10;
			ctx.DrawTextLayout (text, 0, 0);
			
			
			// proofing width; test should align with text above
			ctx.SetColor (Colors.DarkMagenta);
			text.Text = "consetetur sadipscing elitr, sed diam nonumy";
			text.Width = col1.Width;
			var size2 = text.GetSize ();
			
			ctx.DrawTextLayout (text, 0, col1.Bottom);
			col1.Height += size2.Height + 10;
			
			ctx.SetColor (Colors.Black);
			
			// proofing scale, on second col
			ctx.Save ();
			ctx.SetColor (Colors.Red);
			col2.Left = col1.Right + 10;
			
			text.Text = "eirmod tempor invidunt ut.";
			
			var scale = 1.2;
			text.Width = text.Width / scale;
			var size3 = text.GetSize ();
			col2.Height = size3.Height * scale;
			col2.Width = size3.Width * scale + 5;
			ctx.Scale (scale, scale);
			ctx.DrawTextLayout (text, col2.Left / scale, col2.Top / scale);
			ctx.Restore ();
			
			// proofing heigth, on second col
			ctx.Save ();
			ctx.SetColor (Colors.DarkCyan);
			text.Text = "Praesent ac lacus nec dolor pulvinar feugiat a id elit.";
			var size4 = text.GetSize ();
			text.Height = size4.Height / 2;
			text.Trimming=TextTrimming.WordElipsis;
			ctx.DrawTextLayout (text, col2.Left, col2.Bottom + 5);
			
			ctx.SetLineWidth (1);
			ctx.SetColor (Colors.Blue);
			ctx.Rectangle (new Rectangle (col2.Left, col2.Bottom + 5, text.Width, text.Height));
			ctx.Stroke();
			ctx.Restore ();
			
			// drawing col line
			ctx.SetLineWidth (1);
			
			ctx.SetColor (Colors.Black.WithAlpha (.5));
			ctx.MoveTo (col1.Right + 5, col1.Top);
			ctx.LineTo (col1.Right + 5, col1.Bottom);
			ctx.Stroke ();
			ctx.MoveTo (col2.Right + 5, col2.Top);
			ctx.LineTo (col2.Right + 5, col2.Bottom);
			ctx.Stroke ();
			ctx.SetColor (Colors.Black);
			
			// proofing rotate, and printing size to see the values
			ctx.Save ();
			
			text.Font = this.Font.WithSize (10);
			text.Text = string.Format ("Size 1 {0}\r\nSize 2 {1}\r\nSize 3 {2} Scale {3}", 
			                           size1, size2, size3, scale);
			text.Width = -1; // this clears textsize
			text.Height = -1;
			ctx.Rotate (5);
			// maybe someone knows a formula with angle and textsize to calculyte ty
			var ty = 30; 
			ctx.DrawTextLayout (text, ty, col1.Bottom + 10);

			ctx.Restore ();
			
			// scale example here:
			
			ctx.Restore ();

			TextLayout tl0 = new TextLayout (this);

			tl0.Font = this.Font.WithSize (10);
			tl0.Text = "This text contains attributes.";
			tl0.SetUnderline ( 0, "This".Length);
			tl0.SetForeground (new Color (0, 1.0, 1.0), "This ".Length, "text".Length);
			tl0.SetBackground (new Color (0, 0, 0), "This ".Length, "text".Length);
			tl0.SetFontWeight (FontWeight.Bold, "This text ".Length, "contains".Length);
			tl0.SetFontStyle (FontStyle.Italic, "This text ".Length, "contains".Length);
			tl0.SetStrikethrough ("This text contains ".Length, "attributes".Length);

			ctx.SetColor(Colors.DarkGreen);
			ctx.DrawTextLayout (tl0, col2.Left, col2.Bottom + 100);

			
			// Text boces

			x = 10;
			y = 180;
			
			// Without wrapping
			
			TextLayout tl = new TextLayout (this);
			tl.Text = "Stright text";
			DrawText (ctx, tl, ref x, ref y);

			// With wrapping
			
			tl = new TextLayout (this);
			tl.Text = "The quick brown fox jumps over the lazy dog";
			tl.Width = 100;
			DrawText (ctx, tl, ref x, ref y);

			// With blank lines
			
			tl = new TextLayout (this);
			tl.Text = "\nEmpty line above\nLine break above\n\nEmpty line above\n\n\nTwo empty lines above\nEmpty line below\n";
			tl.Width = 200;
			DrawText (ctx, tl, ref x, ref y);

			// With wrapping and center alignment

			tl = new TextLayout (this);
			tl.Text = "This text should be centered";
			tl.Width = 100;
			tl.TextAlignment = Alignment.Center;
			DrawText (ctx, tl, ref x, ref y);

			// With wrapping and right alignment

			tl = new TextLayout (this);
			tl.Text = "This text should be right aligned";
			tl.Width = 50;
			tl.TextAlignment = Alignment.End;
			DrawText (ctx, tl, ref x, ref y);
		}	
		
		void DrawText (Context ctx, TextLayout tl, ref double x, ref double y)
		{
			var dx = 0d;
			var s = tl.GetSize ();
			switch (tl.TextAlignment) {
			case Alignment.Center:
				dx = Math.Round ((tl.Width - s.Width) / 2);
				break;
			case Alignment.End:
				dx = tl.Width - s.Width;
				break;
			}
			var rect = new Rectangle (x + dx, y, s.Width, s.Height).Inflate (0.5, 0.5);
			ctx.SetLineWidth (1);
			ctx.SetColor (Colors.Blue);
			ctx.Rectangle (rect);
			ctx.Stroke ();
			ctx.SetColor (Colors.Black);
			ctx.DrawTextLayout (tl, x, y);
			
			y += s.Height + 20;
			if (y > 400) {
				y = 180;
				x += 150;
			}
		}
	}
}

