//
// ScrollControlBackend.cs
//
// Author:
//       lluis <>
//
// Copyright (c) 2014 lluis
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
using System.Windows.Controls;

namespace Xwt.WPFBackend
{
	public class ScrollControlBackend: Backend, IScrollControlBackend
	{
		double scrollValue;
		bool isVertical;
		ScrollViewer targetScrollViewer;

		public ScrollControlBackend()
		{
		}

		public ScrollControlBackend (ScrollViewer s, bool isVertical)
		{
			targetScrollViewer = s;
			this.isVertical = isVertical;
			scrollValue = Value;
			targetScrollViewer.ScrollChanged += HandleScrollChanged;
		}

		void HandleScrollChanged (object sender, ScrollChangedEventArgs e)
		{
			if (Value != scrollValue) {
				scrollValue = Value;
				Context.InvokeUserCode (EventSink.OnValueChanged);
			}
		}

		public void Initialize(IScrollControlEventSink eventSink)
		{
			EventSink = eventSink;
		}

		public double Value
		{
			get { return isVertical ? targetScrollViewer.VerticalOffset : targetScrollViewer.HorizontalOffset; }
			set {
				if (isVertical)
					targetScrollViewer.ScrollToVerticalOffset(value);
				else
					targetScrollViewer.ScrollToHorizontalOffset(value);
			}
		}

		public double LowerValue
		{
			get { return 0; }
		}

		public double UpperValue
		{
			get { return isVertical ? targetScrollViewer.ScrollableHeight : targetScrollViewer.ScrollableWidth; }
		}

		public double PageIncrement
		{
			get { return PageSize; }
		}

		public double StepIncrement
		{
			get { return 1; }
		}

		public double PageSize
		{
			get { return isVertical ? targetScrollViewer.ViewportWidth : targetScrollViewer.ViewportHeight; }
		}

		internal IScrollControlEventSink EventSink
		{
			get;
			private set;
		}
	}
}

