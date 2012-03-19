// 
// ScrollViewBackend.cs
//  
// Author:
//	   Eric Maupin <ermau@xamarin.com>
// 
// Copyright (c) 2012 Xamarin, Inc.
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
using Xwt.Backends;
using Xwt.WPFBackend;
using System.Windows.Controls;

namespace Xwt.WPFBackend
{
	public class ScrollViewBackend
		: WidgetBackend, IScrollViewBackend
	{
		public ScrollViewBackend()
		{
			ScrollViewer = new ScrollViewer();
		}

		public void SetChild (IWidgetBackend child)
		{
			var widget = (WidgetBackend) child;
			if (widget.EventSink.SupportsCustomScrolling()) {
				var vbackend = new ScrollAdjustmentBackend ();
				var hbackend = new ScrollAdjustmentBackend ();
				widget.EventSink.SetScrollAdjustments (hbackend, vbackend);
				var vp = new CustomScrollViewPort (widget.NativeWidget, vbackend, hbackend);
				ScrollViewer.Content = vp;
				vp.ScrollOwner = ScrollViewer;
			} else
				ScrollViewer.Content = child.NativeWidget;
		}

		public ScrollPolicy VerticalScrollPolicy
		{
			get { return GetScrollPolicy (ScrollViewer.VerticalScrollBarVisibility); }
			set { ScrollViewer.VerticalScrollBarVisibility = GetScrollVisibility (value); }
		}

		public ScrollPolicy HorizontalScrollPolicy
		{
			get { return GetScrollPolicy (ScrollViewer.HorizontalScrollBarVisibility); }
			set { ScrollViewer.HorizontalScrollBarVisibility = GetScrollVisibility (value); }
		}

		private bool borderVisible = true;
		public bool BorderVisible
		{
			get { return this.borderVisible; }
			set
			{
				if (this.borderVisible == value)
					return;

				if (value)
					ScrollViewer.ClearValue (Control.BorderBrushProperty);
				else
					ScrollViewer.BorderBrush = null;

				this.borderVisible = value;
			}
		}

		public Rectangle VisibleRect
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		protected ScrollViewer ScrollViewer
		{
			get { return (ScrollViewer) Widget; }
			set { Widget = value; }
		}

		private ScrollBarVisibility GetScrollVisibility (ScrollPolicy policy)
		{
			switch (policy) {
				case ScrollPolicy.Always:
					return ScrollBarVisibility.Visible;
				case ScrollPolicy.Automatic:
					return ScrollBarVisibility.Auto;
				case ScrollPolicy.Never:
					return ScrollBarVisibility.Hidden;

				default:
					throw new NotSupportedException();
			}
		}

		private ScrollPolicy GetScrollPolicy (ScrollBarVisibility visibility)
		{
			switch (visibility) {
				case ScrollBarVisibility.Auto:
					return ScrollPolicy.Automatic;
				case ScrollBarVisibility.Visible:
					return ScrollPolicy.Always;
				case ScrollBarVisibility.Hidden:
					return ScrollPolicy.Never;

				default:
					throw new NotSupportedException();
			}
		}
	}
}
