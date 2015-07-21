// 
// Size.cs
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
using System.Windows.Markup;
using System.Globalization;

namespace Xwt {
	
	[TypeConverter (typeof(SizeValueConverter))]
	[ValueSerializer (typeof(SizeValueSerializer))]
	[Serializable]
	public struct Size
	{		
		double width, height;

		public static readonly Size Zero = new Size (0d, 0d);

		public Size (double width, double height)
		{
			this.width = width;
			this.height = height;
		}

		public bool IsZero {
			get {
				return ((width == 0) && (height == 0));
			}
		}
		
		[DefaultValue (0d)]
		public double Width {
			get {
				return width;
			}
			set {
				width = value;
			}
		}

		[DefaultValue (0d)]
		public double Height {
			get {
				return height;
			}
			set {
				height = value;
			}
		}

		public static Size operator + (Size s1, Size s2)
		{
			return new Size (s1.width + s2.width, s1.height + s2.height);
		}
		
		public static Size operator - (Size s1, Size s2)
		{
			return new Size (s1.width - s2.width, s1.height - s2.height);
		}
		
		public static bool operator == (Size s1, Size s2)
		{
			return (s1.width == s2.width) && (s1.height == s2.height);
		}
		
		public static bool operator != (Size s1, Size s2)
		{
			return (s1.width != s2.width) || (s1.height != s2.height);
		}

		public static Size operator * (Size size, double factor)
		{
			return new Size (size.width * factor, size.height * factor);
		}

		public static Size operator / (Size size, double factor)
		{
			return new Size (size.width / factor, size.height / factor);
		}
		
		public static explicit operator Point (Size size) 
		{
			return new Point (size.Width, size.Height);
		}
		
		public static explicit operator Distance (Size size) 
		{
			return new Distance (size.Width, size.Height);
		}
		
		public override bool Equals (object ob)
		{
			return (ob is Size) && this == (Size)ob;
		}

		public override int GetHashCode ()
		{
			unchecked {
				return (width.GetHashCode () * 397) ^ height.GetHashCode ();
			}
		}

		public override string ToString ()
		{
			return String.Format ("{{Width={0} Height={1}}}", width.ToString (CultureInfo.InvariantCulture), height.ToString (CultureInfo.InvariantCulture));
		}
	}
	
	class SizeValueConverter: TypeConverter
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
	
	class SizeValueSerializer: ValueSerializer
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
			Size s = (Size) value;
			return s.Width.ToString () + "," + s.Height.ToString ();
		}
		
		public override object ConvertFromString (string value, IValueSerializerContext context)
		{
			int i = value.IndexOf (',');
			if (i == -1)
				return Size.Zero;
			double w, h;
			if (!double.TryParse (value.Substring (0, i), NumberStyles.Any, CultureInfo.InvariantCulture, out w))
				return Size.Zero;
			if (!double.TryParse (value.Substring (i+1), NumberStyles.Any, CultureInfo.InvariantCulture, out h))
				return Size.Zero;
			return new Size (w, h);
		}
	}
}
