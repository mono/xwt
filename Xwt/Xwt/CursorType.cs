// 
// CursorType.cs
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
using System.Windows.Markup;
using System.Collections.Generic;

namespace Xwt
{
	[TypeConverter (typeof(CursorTypeValueConverter))]
	[ValueSerializer (typeof(CursorTypeValueSerializer))]
	public class CursorType
	{
		string id;
		static Dictionary<string,CursorType> cursors = new Dictionary<string, CursorType> ();

		internal CursorType (string id)
		{
			// Maybe some day we'll support creating custom cursors
			this.id = id;
			cursors [id] = this;
		}
		
		public static readonly CursorType Arrow = new CursorType ("Arrow");
		public static readonly CursorType IBeam = new CursorType ("IBeam");
		public static readonly CursorType Crosshair = new CursorType ("Crosshair");
		public static readonly CursorType ResizeLeft = new CursorType ("ResizeLeft");
		public static readonly CursorType ResizeRight = new CursorType ("ResizeRight");
		public static readonly CursorType ResizeLeftRight = new CursorType ("ResizeLeftRight");
		public static readonly CursorType ResizeUp = new CursorType ("ResizeUp");
		public static readonly CursorType ResizeDown = new CursorType ("ResizeDown");
		public static readonly CursorType ResizeUpDown = new CursorType ("ResizeUpDown");
		public static readonly CursorType ResizeNE = new CursorType("ResizeNE");
		public static readonly CursorType ResizeNW = new CursorType("ResizeNW");
		public static readonly CursorType ResizeSE = new CursorType("ResizeSE");
		public static readonly CursorType ResizeSW = new CursorType("ResizeSW");
		public static readonly CursorType Hand = new CursorType ("Hand");
		public static readonly CursorType Hand2 = new CursorType("Hand2");
		public static readonly CursorType Move = new CursorType ("Move");
		public static readonly CursorType Wait = new CursorType ("Watch");
		public static readonly CursorType Help = new CursorType ("Help");
		public static readonly CursorType Invisible = new CursorType ("Invisible");
		public static readonly CursorType DragCopy = new CursorType("DragCopy");
		public static readonly CursorType NotAllowed = new CursorType("NotAllowed");

		
		class CursorTypeValueConverter: TypeConverter
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
		
		class CursorTypeValueSerializer: ValueSerializer
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
				CursorType s = (CursorType) value;
				return s.id;
			}
			
			public override object ConvertFromString (string value, IValueSerializerContext context)
			{
				CursorType ct;
				cursors.TryGetValue (value, out ct);
				return ct;
			}
		}

		public override string ToString() => id;
	}
}

