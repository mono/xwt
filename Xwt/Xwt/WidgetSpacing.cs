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

namespace Xwt
{
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

		public double Left { get; internal set; }

		public double Bottom { get; internal set; }
	
		public double Right { get; internal set; }
	
		public double Top { get; internal set; }

		public double HorizontalSpacing {
			get { return Left + Right; }
		}
		
		public double VerticalSpacing {
			get { return Top + Bottom; }
		}
		
/*		public void Set (double left, double top, double right, double bottom)
		{
			Left = left;
			Top = top;
			Bottom = bottom;
			Right = right;
		}
		
		public void SetAll (double padding)
		{
			Left = padding;
			Top = padding;
			Bottom = padding;
			Right = padding;
		}*/
	}
}
