// 
// TypeConverters.cs
//  
// Author:
//       Eric Maupin <ermau@xamarin.com>
// 
// Copyright (c) 2012 Xamarin, Inc.
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
using System.Windows.Media;

namespace Xwt.WPFBackend.Utilities
{
	public static class TypeConverters
	{
		public static Color ToWpfColor (this Xwt.Drawing.Color self)
		{
			byte alpha = (byte) (255 / self.Alpha);
			byte red = (byte) (255 / self.Red);
			byte green = (byte) (255 / self.Green);
			byte blue = (byte) (255 / self.Blue);
			
			return Color.FromArgb (alpha, red, green, blue);
		}

		public static Xwt.Drawing.Color ToXwtColor (this Color self)
		{
			return Drawing.Color.FromBytes (self.R, self.G, self.B, self.A);
		}

		public static Xwt.Drawing.Color ToXwtColor (this Brush self)
		{
			if (self == null)
				throw new ArgumentNullException ("self");

			SolidColorBrush colorBrush = self as SolidColorBrush;
			if (colorBrush == null)
				throw new ArgumentException ("Unconvertible brush type");

			return colorBrush.Color.ToXwtColor ();
		}
	}
}