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
using System.Xaml;
using System.Linq;
using System.Windows.Markup;
using System.Text;
using System.Globalization;

namespace Xwt
{
	/// <summary>
	/// Spacing/Margin around a widget.
	/// </summary>
	[TypeConverter (typeof(WidgetSpacingValueConverter))]
	[ValueSerializer (typeof(WidgetSpacingValueSerializer))]
	public struct WidgetSpacing
	{
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
	
	class WidgetSpacingValueSerializer: ValueSerializer
	{
		public override bool CanConvertFromString (string value, IValueSerializerContext context)
		{
			return true;
		}
		
		public override bool CanConvertToString (object value, IValueSerializerContext context)
		{
			return true;
		}
		
		public override string ConvertToString (object value, IValueSerializerContext context)
		{
			WidgetSpacing s = (WidgetSpacing) value;
			if (s.Left == s.Right && s.Right == s.Top && s.Top == s.Bottom)
				return s.Left.ToString (CultureInfo.InvariantCulture);
			if (s.Bottom != 0)
				return s.Left.ToString (CultureInfo.InvariantCulture) + " " + s.Top.ToString (CultureInfo.InvariantCulture) + " " + s.Right.ToString (CultureInfo.InvariantCulture) + " " + s.Bottom.ToString (CultureInfo.InvariantCulture);
			if (s.Right != 0)
				return s.Left.ToString (CultureInfo.InvariantCulture) + " " + s.Top.ToString (CultureInfo.InvariantCulture) + " " + s.Right.ToString (CultureInfo.InvariantCulture);
			return s.Left.ToString (CultureInfo.InvariantCulture) + " " + s.Top.ToString (CultureInfo.InvariantCulture);
		}
		
		public override object ConvertFromString (string value, IValueSerializerContext context)
		{
			WidgetSpacing c = new WidgetSpacing ();
			string[] values = value.Split (new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (values.Length == 0)
				return c;

			double v;
			if (double.TryParse (values [0], NumberStyles.Any, CultureInfo.InvariantCulture, out v))
				c.Left = v;

			if (value.Length == 1) {
				c.Top = c.Right = c.Bottom = v;
				return c;
			}

			if (value.Length >= 2 && double.TryParse (values [1], NumberStyles.Any, CultureInfo.InvariantCulture, out v))
				c.Top = v;
			if (value.Length >= 3 && double.TryParse (values [2], NumberStyles.Any, CultureInfo.InvariantCulture, out v))
				c.Right = v;
			if (value.Length >= 4 && double.TryParse (values [3], NumberStyles.Any, CultureInfo.InvariantCulture, out v))
				c.Bottom = v;
			return c;
		}
	}
}
