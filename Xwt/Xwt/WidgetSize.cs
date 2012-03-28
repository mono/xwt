// 
// WidgetSize.cs
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

namespace Xwt
{
	public struct WidgetSize
	{
		public WidgetSize (double minAndNatural): this ()
		{
			MinSize = minAndNatural;
			NaturalSize = minAndNatural;
		}
		
		public WidgetSize (double min, double natural): this ()
		{
			MinSize = min;
			NaturalSize = natural;
		}
		
		public double MinSize { get; set; }
		public double NaturalSize { get; set; }
		
		public override bool Equals (object obj)
		{
			if (!(obj is WidgetSize))
				return false;
			WidgetSize s = (WidgetSize) obj;
			return s.MinSize == MinSize && s.NaturalSize == NaturalSize;
		}
		
		public override int GetHashCode ()
		{
			return MinSize.GetHashCode () ^ NaturalSize.GetHashCode ();
		}
		
		public static WidgetSize operator + (WidgetSize sz1, WidgetSize sz2)
		{
			return new WidgetSize (sz1.MinSize + sz2.MinSize, sz1.NaturalSize + sz2.NaturalSize);
		}
		
		public static bool operator == (WidgetSize sz_a, WidgetSize sz_b)
		{
			return ((sz_a.MinSize == sz_b.MinSize) && (sz_a.NaturalSize == sz_b.NaturalSize));
		}
		
		public static bool operator != (WidgetSize sz_a, WidgetSize sz_b)
		{
			return ((sz_a.MinSize != sz_b.MinSize) || (sz_a.NaturalSize != sz_b.NaturalSize));
		}
		
		public static WidgetSize operator - (WidgetSize sz1, WidgetSize sz2)
		{
			return new WidgetSize (sz1.MinSize - sz2.MinSize, sz1.NaturalSize - sz2.NaturalSize);
		}
		
		public static WidgetSize operator + (WidgetSize sz1, double sz2)
		{
			return new WidgetSize (sz1.MinSize + sz2, sz1.NaturalSize + sz2);
		}
		
		public static WidgetSize operator - (WidgetSize sz1, double sz2)
		{
			return new WidgetSize (sz1.MinSize - sz2, sz1.NaturalSize - sz2);
		}

		public WidgetSize UnionWith (WidgetSize s2)
		{
			return new WidgetSize (Math.Max (MinSize, s2.MinSize), Math.Max (NaturalSize, s2.NaturalSize));
		}
		
		public override string ToString ()
		{
			return string.Format ("[WidgetSize: MinSize={0}, NaturalSize={1}]", MinSize, NaturalSize);
		}
	}
}

