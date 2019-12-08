// 
// WidgetSpacing.cs
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
using System.ComponentModel;
using System.Collections.Generic;
using Xwt.Backends;

using Xwt.Drawing;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Xwt
{
	/// <summary>
	/// Spacing/Margin around a widget.
	/// </summary>
	[TypeConverter (typeof(WidgetSpacingValueConverter))]
	public struct WidgetSpacing : IEquatable<WidgetSpacing>
	{
		public static WidgetSpacing Zero = new WidgetSpacing ();

		static public implicit operator WidgetSpacing (double value)
		{
			return new WidgetSpacing (value, value, value, value);
		}

		public WidgetSpacing (double left = 0, double top = 0, double right = 0, double bottom = 0): this ()
		{
			Left = left;
			Top = top;
			Bottom = bottom;
			Right = right;
		}

		/// <summary>
		/// Gets the space on the left side of a widget.
		/// </summary>
		/// <value>The spance on the left side.</value>
		public double Left { get; internal set; }

		/// <summary>
		/// Gets the space on the bottom side of a widget.
		/// </summary>
		/// <value>The spance on the bottom side.</value>
		public double Bottom { get; internal set; }
	
		/// <summary>
		/// Gets the space on the right side of a widget.
		/// </summary>
		/// <value>The spance on the right side.</value>
		public double Right { get; internal set; }
	
		/// <summary>
		/// Gets the space on the top side of a widget.
		/// </summary>
		/// <value>The spance on the top side.</value>
		public double Top { get; internal set; }

		/// <summary>
		/// Gets the horizontal spacing (left + right) of a widget.
		/// </summary>
		/// <value>The horizontal spacing.</value>
		public double HorizontalSpacing {
			get { return Left + Right; }
		}
		
		/// <summary>
		/// Gets the vertical spacing (top + bottom) of a widget.
		/// </summary>
		/// <value>The vertical spacing.</value>
		public double VerticalSpacing {
			get { return Top + Bottom; }
		}

		public bool IsZero {
			get {
				return ((Left == 0) && (Right == 0) && (Top == 0) && (Bottom == 0));
			}
		}

		/// <summary>
		/// Get the spacing of a widget for the specified orientation.
		/// </summary>
		/// <returns>The spacing for an orientation.</returns>
		/// <param name="orientation">The orientation.</param>
		public double GetSpacingForOrientation (Orientation orientation)
		{
			if (orientation == Orientation.Vertical)
				return Top + Bottom;
			else
				return Left + Right;
		}

		// Equality
		public override bool Equals (object o)
		{
			if (!(o is WidgetSpacing))
				return false;

			return (this == (WidgetSpacing)o);
		}

		public bool Equals (WidgetSpacing other)
		{
			return this == other;
		}

		public override int GetHashCode ()
		{
			unchecked {
				var hash = Left.GetHashCode ();
				hash = (hash * 397) ^ Right.GetHashCode ();
				hash = (hash * 397) ^ Top.GetHashCode ();
				hash = (hash * 397) ^ Bottom.GetHashCode ();
				return hash;
			}
		}

		public static bool operator == (WidgetSpacing s1, WidgetSpacing s2)
		{
			return ((s1.Left == s2.Left) && (s1.Right == s2.Right) && (s1.Top == s2.Top) && (s1.Bottom == s2.Bottom));
		}

		public static bool operator != (WidgetSpacing s1, WidgetSpacing s2)
		{
			return !(s1 == s2);
		}
	}

	
	class WidgetSpacingValueConverter: TypeConverter
	{
		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(string);
		}
		
		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(string);
		}
	}
}
