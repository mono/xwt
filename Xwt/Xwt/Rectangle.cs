// 
// Rectangle.cs
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
using System.Collections;
using System.Globalization;

namespace Xwt
{
	public struct Rectangle
	{
		public double X { get; set; }
		public double Y { get; set; }
		public double Width { get; set; }
		public double Height { get; set; }

		public static Rectangle Zero = new Rectangle ();
		
		public override string ToString ()
		{
			return String.Format ("{0},{1}/{2}x{3}", X.ToString (CultureInfo.InvariantCulture), Y.ToString (CultureInfo.InvariantCulture), Width.ToString (CultureInfo.InvariantCulture), Height.ToString (CultureInfo.InvariantCulture));
		}
		
		// constructors
		public Rectangle (double x, double y, double width, double height): this ()
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}
		
		public Rectangle (Point loc, Size sz) : this (loc.X, loc.Y, sz.Width, sz.Height) {}
		
		public static Rectangle FromLTRB (double left, double top, double right, double bottom)
		{
			return new Rectangle (left, top, right - left + 1, bottom - top + 1);
		}
		
		// Equality
		public override bool Equals (object o)
		{
			if (!(o is Rectangle))
				return false;
		
			return (this == (Rectangle) o);
		}
		
		public override int GetHashCode ()
		{
			return ((int)Height + (int)Width) ^ (int)X + (int)Y;
		}
		
		public static bool operator == (Rectangle r1, Rectangle r2)
		{
			return ((r1.Location == r2.Location) && (r1.Size == r2.Size));
		}
		
		public static bool operator != (Rectangle r1, Rectangle r2)
		{
			return !(r1 == r2);
		}
		
		// Hit Testing / Intersection / Union
		public bool Contains (Rectangle rect)
		{
			return (rect == Intersect (this, rect));
		}
		
		public bool Contains (Point pt)
		{
			return Contains (pt.X, pt.Y);
		}
		
		public bool Contains (double x, double y)
		{
			return ((x >= Left) && (x <= Right) && 
				(y >= Top) && (y <= Bottom));
		}
		
		public bool IntersectsWith (Rectangle r)
		{
			return !((Left > r.Right) || (Right < r.Left) ||
			    (Top > r.Bottom) || (Bottom < r.Top));
		}
		
		public Rectangle Union (Rectangle r)
		{
			return Union (this, r);
		}
		
		Rectangle Union (Rectangle r1, Rectangle r2)
		{
			return FromLTRB (Math.Min (r1.Left, r2.Left),
					 Math.Min (r1.Top, r2.Top),
					 Math.Max (r1.Right, r2.Right),
					 Math.Max (r1.Bottom, r2.Bottom));
		}
		
		public Rectangle Intersect (Rectangle r)
		{
			return Intersect (this, r);
		}
		
		Rectangle Intersect (Rectangle r1, Rectangle r2)
		{
			if (!r1.IntersectsWith (r2))
				return new Rectangle ();
			return FromLTRB (Math.Max (r1.Left, r2.Left),
					 Math.Max (r1.Top, r2.Top),
					 Math.Min (r1.Right, r2.Right),
					 Math.Min (r1.Bottom, r2.Bottom));
		}
		
		// Position/Size
		public double Top {
			get { return Y; }
			set { Y = value; }
		}
		public double Bottom {
			get { return Y + Height - 1; }
			set { Height = value - Y + 1; }
		}
		public double Right {
			get { return X + Width - 1; }
			set { Width = value - X + 1; }
		}
		public double Left {
			get { return X; }
			set { X = value; }
		}
		
		public bool IsEmpty {
			get { return (Width == 0) || (Height == 0); }
		}
		
		public Size Size {
			get {
				return new Size (Width, Height);
			}
			set {
				Width = value.Width;
				Height = value.Height;
			}
		}
		
		public Point Location {
			get {
				return new Point (X, Y);
			}
			set {
				X = value.X;
				Y = value.Y;
			}
		}
		
		// Inflate and Offset
		public Rectangle Inflate (Size sz)
		{
			return Inflate (sz.Width, sz.Height);
		}
		
		public Rectangle Inflate (double width, double height)
		{
			Rectangle r = this;
			r.X -= width;
			r.Y -= height;
			r.Width += width * 2;
			r.Height += height * 2;
			return r;
		}
		
		public Rectangle Offset (double dx, double dy)
		{
			Rectangle r = this;
			r.X += dx;
			r.Y += dy;
			return r;
		}
		
		public Rectangle Offset (Point dr)
		{
			return Offset (dr.X, dr.Y);
		}
	}
}
