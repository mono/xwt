// 
// Distance.cs
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
using System.ComponentModel;
using System.Globalization;

namespace Xwt
{
	public struct Distance
	{
		double dx, dy;
		
		public static readonly Distance Zero = new Distance (0d, 0d);
		
		public Distance (double dx, double dy)
		{
			this.dx = dx;
			this.dy = dy;
		}
		
		public bool IsZero {
			get {
				return ((dx == 0) && (dy == 0));
			}
		}
		
		[DefaultValue (0d)]
		public double Dx {
			get {
				return dx;
			}
			set {
				dx = value;
			}
		}
		
		[DefaultValue (0d)]
		public double Dy {
			get {
				return dy;
			}
			set {
				dy = value;
			}
		}
		
		public static Distance operator + (Distance d1, Distance d2)
		{
			return new Distance (d1.dx + d2.dx, d1.dy + d2.dy);
		}
		
		public static Distance operator - (Distance d1, Distance d2)
		{
			return new Distance (d1.dx - d2.dx, d1.dy - d2.dy);
		}
		
		public static bool operator == (Distance d1, Distance d2)
		{
			return (d1.dx == d2.dx) && (d1.dy == d2.dy);
		}
		
		public static bool operator != (Distance d1, Distance d2)
		{
			return (d1.dx != d2.dx) || (d1.dy != d2.dy);
		}
		
		public static explicit operator Point (Distance distance) 
		{
			return new Point (distance.dx, distance.dx);
		}
		
		public static explicit operator Size (Distance distance) 
		{
			return new Size (distance.dx, distance.dx);
		}
		
		public override bool Equals (object ob)
		{
			return (ob is Distance) && this == (Distance)ob;
		}
		
		public override int GetHashCode ()
		{
			unchecked {
				return (dx.GetHashCode () * 397) ^ dy.GetHashCode ();
			}
		}
		
		public override string ToString ()
		{
			return String.Format ("{{Dx={0} Dy={1}}}", dx.ToString (CultureInfo.InvariantCulture), dy.ToString (CultureInfo.InvariantCulture));
		}
	}
}

