// 
// BoxBackend.cs
//  
// Author:
//       Carlos Alberto Cortez <calberto.cortez@gmail.com>
// 
// Copyright (c) 2012 Carlos Alberto Cortez
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SW = System.Windows;
using SWC = System.Windows.Controls;

using Xwt.Backends;
using Xwt.Drawing;

namespace Xwt.WPFBackend
{
	public class BoxBackend : WidgetBackend
	{
		public BoxBackend ()
		{
			Widget = new CustomPanel ();
		}

		new CustomPanel Widget {
			get { return (CustomPanel)base.Widget; }
			set { base.Widget = value; }
		}

		public void Add (IWidgetBackend widget)
		{
			Widget.Children.Add (GetFrameworkElement (widget));
		}

		public void Remove (IWidgetBackend widget)
		{
			Widget.Children.Remove (GetFrameworkElement (widget));
		}

		public void SetAllocation (IWidgetBackend [] widget, Rectangle [] rect)
		{
			for (int i = 0; i < widget.Length; i++) {
				var e = GetFrameworkElement (widget [i]);
				e.Arrange (DataConverter.ToWpfRect (rect [i]));
			}
		}
	}

	public class CustomPanel : SWC.Canvas
	{
	}
}
