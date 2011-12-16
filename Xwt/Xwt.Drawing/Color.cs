// 
// Color.cs
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

namespace Xwt.Drawing
{
	public struct Color
	{
		double r, g, b, a;
		HslColor hsl;
		
		public static Color Black = new Color (0, 0, 0);
		public static Color White = new Color (1, 1, 1);
		
		public double Red {
			get { return r; }
			set { r = Normalize (value); hsl = null; }
		}
		
		public double Green {
			get { return g; }
			set { g = Normalize (value); hsl = null; }
		}
		
		public double Blue {
			get { return b; }
			set { b = Normalize (value); hsl = null; }
		}
		
		public double Alpha {
			get { return a; }
			set { a = Normalize (value); }
		}
		
		public double Hue {
			get {
				return Hsl.H;
			}
			set {
				Hsl = new HslColor (Normalize (value), Hsl.S, Hsl.L);
			}
		}
		
		public double Saturation {
			get {
				return Hsl.S;
			}
			set {
				Hsl = new HslColor (Hsl.H, Normalize (value), Hsl.L);
			}
		}
		
		public double Light {
			get {
				return Hsl.L;
			}
			set {
				Hsl = new HslColor (Hsl.H, Hsl.S, Normalize (value));
			}
		}
		
		double Normalize (double v)
		{
			if (v < 0) return 0;
			if (v > 1) return 1;
			return v;
		}
		
		public double Brightness {
			get {
				return System.Math.Sqrt (Red * .241 + Green * .691 + Blue * .068);
			}
		}
		
		HslColor Hsl {
			get {
				if (hsl == null)
					hsl = (HslColor)this;
				return hsl;
			}
			set {
				hsl = value;
				Color c = (Color)value;
				r = c.r;
				b = c.b;
				g = c.g;
			}
		}
		
		public Color (double red, double green, double blue): this ()
		{
			Red = red;
			Green = green;
			Blue = blue;
			Alpha = 1f;
		}
		
		public Color (double red, double green, double blue, double alpha): this ()
		{
			Red = red;
			Green = green;
			Blue = blue;
			Alpha = alpha;
		}
		
		public Color WithAlpha (double alpha)
		{
			Color c = this;
			c.Alpha = alpha;
			return c;
		}
		
		public Color WithIncreasedLight (double lightIncrement)
		{
			Color c = this;
			c.Light += lightIncrement;
			return c;
		}
		
		public Color WithIncreasedContrast (Color referenceColor, double amount)
		{
			Color c = this;
			if (referenceColor.Brightness > Brightness)
				c.Light -= amount;
			else
				c.Light += amount;
			return c;
		}
			
		public Color BlendWith (Color target, double amount)
		{
			if (amount < 0 || amount > 1)
				throw new ArgumentException ("Blend amount must be between 0 and 1");
			return new Color (BlendValue (r, target.r, amount), BlendValue (g, target.g, amount), BlendValue (b, target.b, amount));
		}
		
		double BlendValue (double s, double t, double amount)
		{
			if (t > s)
				return s + (t - s) * amount;
			else
				return t + (s - t) * amount;
		}
	
		public static Color FromBytes (byte red, byte green, byte blue)
		{
			return FromBytes (red, green, blue, 255);
		}
		
		public static Color FromBytes (byte red, byte green, byte blue, byte alpha)
		{
			return new Color {
				Red = ((double)red) / 255.0,
				Green = ((double)green) / 255.0,
				Blue = ((double)blue) / 255.0,
				Alpha = ((double)alpha) / 255.0
			};
		}
		
		public static Color FromHsl (double h, double s, double l)
		{
			return FromHsl (h, s, l, 1);
		}
		
		public static Color FromHsl (double h, double s, double l, double alpha)
		{
			HslColor hsl = new HslColor (h, s, l);
			Color c = (Color)hsl;
			c.Alpha = alpha;
			c.hsl = hsl;
			return c;
		}
		
		public static Color FromName (string name)
		{
			uint val;
			if (!TryParseColourFromHex (name, out val))
				return Color.Black;
			return Color.FromBytes ((byte)(val >> 24), (byte)((val >> 16) & 0xff), (byte)((val >> 8) & 0xff), (byte)(val & 0xff));
		}
		

		static bool TryParseColourFromHex (string str, out uint val)
		{
			val = 0;
			
			if (str.Length > 9)
				return false;
			
			if (!uint.TryParse (str.Substring (1), System.Globalization.NumberStyles.HexNumber, null, out val))
				return false;
			
			val = val << (9 - str.Length * 4);
			
			if (str.Length <= 7)
				val |= 0xff;
			
			return true;
		}
	}
}

