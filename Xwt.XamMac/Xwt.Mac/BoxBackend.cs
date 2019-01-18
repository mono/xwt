// 
// BoxBackend.cs
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
using AppKit;
using CoreGraphics;
using Xwt.Backends;

namespace Xwt.Mac
{
	public class BoxBackend: ViewBackend<NSView,IWidgetEventSink>, IBoxBackend
	{
		public BoxBackend ()
		{
		}

		public override void Initialize ()
		{
			ViewObject = new WidgetView (EventSink, ApplicationContext);
			CanGetFocus = false;
		}

		public void Add (IWidgetBackend widget)
		{
			ViewBackend b = (ViewBackend) widget;
			Widget.AddSubview (b.Widget);
		}

		public void Remove (IWidgetBackend widget)
		{
			var w = GetWidget (widget);
			if (w.Superview != Widget)
				throw new InvalidOperationException ("Widget is not a child of this container");
			w.RemoveFromSuperview ();
		}
		
		public void SetAllocation (IWidgetBackend[] widgets, Rectangle[] rects)
		{
			for (int n=0; n<widgets.Length; n++) {
				var w = GetWidget (widgets[n]);
				var r = rects[n];
				w.Frame = new CGRect ((nfloat)r.Left, (nfloat)r.Top, (nfloat)r.Width, (nfloat)r.Height);
				w.NeedsDisplay = true;
			}
		}
	}
}

