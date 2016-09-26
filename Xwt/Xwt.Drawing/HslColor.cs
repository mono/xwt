// 
// HslColor.cs
//  
// Author:
//       Mike Krüger <mkrueger@novell.com>
// 
// Copyright (c) 2009 Xamarin, Inc (http://www.xamarin.com)
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

namespace Xwt.Drawing
{
	class HslColor
	{
		public double H {
			get;
			set;
		}
		
		public double S {
			get;
			set;
		}
		
		public double L {
			get;
			set;
		}
		
		public HslColor ()
		{
		}
		
		public HslColor (double h, double s, double l)
		{
			H = h;
			S = s;
			L = l;
		}
		
		public static implicit operator Color (HslColor hsl)
		{
			if (hsl.L > 1) hsl.L = 1;
			if (hsl.L < 0) hsl.L = 0;
			if (hsl.H > 1) hsl.H = 1;
			if (hsl.H < 0) hsl.H = 0;
			if (hsl.S > 1) hsl.S = 1;
			if (hsl.S < 0) hsl.S = 0;
			
			double r = 0, g = 0, b = 0;
			
			if (hsl.L == 0)
				return new Color (0f, 0f, 0f);
			
			if (hsl.S == 0) {
				r = g = b = hsl.L;
			} else {
				double temp2 = hsl.L <= 0.5 ? hsl.L * (1.0 + hsl.S) : hsl.L + hsl.S -(hsl.L * hsl.S);
				double temp1 = 2.0 * hsl.L - temp2;

				r = ConvertVector(temp2, temp1, hsl.H + 1.0 / 3.0);
				g = ConvertVector(temp2, temp1, hsl.H);
				b = ConvertVector(temp2, temp1, hsl.H - 1.0 / 3.0);
			}
			return new Color (r, g, b);
		}

		static double ConvertVector(double temp2, double temp1, double x)
		{
			if (x < 0)
				x += 1.0;
			if (x > 1)
				x -= 1.0;
			if (6.0 * x < 1.0)
				return temp1 + (temp2 - temp1) * x * 6.0;
			if (2.0 * x < 1.0)
				return temp2;
			if (3.0 * x < 2.0)
				return (temp1 + (temp2 - temp1) * ((2.0 / 3.0) - x) * 6.0);
			return temp1;
		}

		public static implicit operator HslColor (Color color)
		{
			return new HslColor (color);
		}
		
		public HslColor (Color color)
		{
			double r = color.Red;
			double g = color.Green;
			double b = color.Blue;

			double v = System.Math.Max (r, g);
			v = System.Math.Max (v, b);

			double m = System.Math.Min (r, g);
			m = System.Math.Min (m, b);
			
			this.L = (m + v) / 2.0;
			if (this.L <= 0.0)
				return;
			double vm = v - m;
			this.S = vm;
			
			if (this.S > 0.0) {
				this.S /= (this.L <= 0.5) ? (v + m) : (2.0 - v - m);
			} else {
				return;
			}
			
			double r2 = (v - r) / vm;
			double g2 = (v - g) / vm;
			double b2 = (v - b) / vm;
			
			if (r == v) {
				this.H = (g == m ? 5.0 + b2 : 1.0 - g2);
			} else if (g == v) {
				this.H = (b == m ? 1.0 + r2 : 3.0 - b2);
			} else {
				this.H = (r == m ? 3.0 + g2 : 5.0 - r2);
			}
			this.H /= 6.0;
		}
		
		public static double Brightness (Color c)
		{
			double r = c.Red / (double)ushort.MaxValue;
			double g = c.Green / (double)ushort.MaxValue;
			double b = c.Blue / (double)ushort.MaxValue;
			return System.Math.Sqrt (r * .241 + g * .691 + b * .068);
		}
		
		public override string ToString ()
		{
			return string.Format ("[HslColor: H={0}, S={1}, L={2}]", H, S, L);
		}
	}
}
