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
using Xwt.Engine;
using Xwt.Drawing;
using System.Reflection;
using System.Xaml;
using System.Linq;

namespace Xwt
{
	public class WidgetSpacing
	{
		ISpacingListener parent;
		double top, left, right, bottom;
		
		internal WidgetSpacing (ISpacingListener parent)
		{
			this.parent = parent;
		}
		
		void NotifyChanged ()
		{
			parent.OnSpacingChanged (this);
		}
		
		public double Left {
			get { return left; }
			set { left = value; NotifyChanged (); }
		}
		
		public double Bottom {
			get {
				return this.bottom;
			}
			set {
				bottom = value; NotifyChanged ();
			}
		}
	
		public double Right {
			get {
				return this.right;
			}
			set {
				right = value; NotifyChanged ();
			}
		}
	
		public double Top {
			get {
				return this.top;
			}
			set {
				top = value; NotifyChanged ();
			}
		}
		
		public double HorizontalSpacing {
			get { return left + right; }
		}
		
		public double VerticalSpacing {
			get { return top + bottom; }
		}
		
		public void Set (double left, double top, double right, double bottom)
		{
			this.left = left;
			this.top = top;
			this.bottom = bottom;
			this.right = right;
			NotifyChanged ();
		}
		
		public void SetAll (double padding)
		{
			this.left = padding;
			this.top = padding;
			this.bottom = padding;
			this.right = padding;
			NotifyChanged ();
		}
	}
	
	public interface ISpacingListener
	{
		void OnSpacingChanged (WidgetSpacing source);
	}
}
