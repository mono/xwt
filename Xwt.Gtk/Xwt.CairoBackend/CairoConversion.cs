// 
// CairoConversion.cs
//  
// Author:
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
using Xwt.Drawing;

namespace Xwt.CairoBackend
{
	public static class CairoConversion
	{
		public static Cairo.Color ToCairoColor (this Color col)
		{
			return new Cairo.Color (col.Red, col.Green, col.Blue, col.Alpha);
		}
		
		public static Cairo.Color ToCairoColor (this Gdk.Color color)
		{
			return new Cairo.Color(
				(double)(color.Red >> 8) / 255.0,
				(double)(color.Green >> 8) / 255.0,
				(double)(color.Blue >> 8) / 255.0);
		}

		#if XWT_GTK3
		public static Cairo.Color ToCairoColor (this Gdk.RGBA color)
		{
			return new Cairo.Color (color.Red, color.Green, color.Blue, color.Alpha);
		}
		#endif

		public static void SelectFont (this Cairo.Context ctx, Font font)
		{
			Cairo.FontSlant slant;
			switch (font.Style) {
			case FontStyle.Oblique: slant = Cairo.FontSlant.Oblique; break;
			case FontStyle.Italic: slant = Cairo.FontSlant.Italic; break;
			default: slant = Cairo.FontSlant.Normal; break;
			}
			
			Cairo.FontWeight w = font.Weight >= FontWeight.Bold ? Cairo.FontWeight.Bold : Cairo.FontWeight.Normal;
			
			ctx.SelectFontFace (font.Family, slant, w);
		}

		public static void RoundedRectangle(this Cairo.Context cr, double x, double y, double w, double h, double r)
		{
			if(r < 0.0001) {
				cr.Rectangle(x, y, w, h);
				return;
			}

			cr.MoveTo(x + r, y);
			cr.Arc(x + w - r, y + r, r, Math.PI * 1.5, Math.PI * 2);
			cr.Arc(x + w - r, y + h - r, r, 0, Math.PI * 0.5);
			cr.Arc(x + r, y + h - r, r, Math.PI * 0.5, Math.PI);
			cr.Arc(x + r, y + r, r, Math.PI, Math.PI * 1.5);
		}
	}
}

