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
using System.Windows.Controls;

namespace Xwt.WPFBackend
{
	internal class ScrollAdjustmentBackend
		: Backend, IScrollAdjustmentBackend, IScrollControlBackend
	{
		double scrollValue;
		double lowerValue;
		double upperValue;
		double pageIncrement;
		double stepIncrement;
		double pageSize;
        bool isVertical;
		IScrollAdjustmentEventSink eventSink;
		IScrollControlEventSink controlEventSink;

		public CustomScrollViewPort TargetViewport { get; set; }

        public ScrollViewer TargetScrollViewer { get; set; }

        public ScrollAdjustmentBackend()
        {
        }

        public ScrollAdjustmentBackend (ScrollViewer s, bool isVertical)
        {
            TargetScrollViewer = s;
            this.isVertical = isVertical;
            scrollValue = 0;
            lowerValue = 0;
        }

        public void Initialize(IScrollAdjustmentEventSink eventSink)
		{
			this.eventSink = eventSink;
		}

		public void Initialize (IScrollControlEventSink eventSink)
		{
			controlEventSink = eventSink;
		}

		public void SetOffset (double offset)
		{
			// The offset is relative to 0, it has to be converted to the lower/upper value range
			scrollValue = LowerValue + offset;
			Context.InvokeUserCode (delegate {
				if (eventSink != null)
					eventSink.OnValueChanged ();
				if (controlEventSink != null)
					controlEventSink.OnValueChanged ();
			});
		}

		public double Value
		{
			get { return scrollValue; }
			set {
                scrollValue = value;

				// Provide the value to the viewport, which will update
				// the ScrollView. The viewport expects an offset starting at 0.
                if (TargetViewport != null)
                    TargetViewport.SetOffset(this, value - LowerValue);

                if (upperValue == lowerValue)
                    return;

                var off = (value - lowerValue) / (upperValue - lowerValue);

				if (TargetScrollViewer != null)	{
					if (isVertical)
						TargetScrollViewer.ScrollToVerticalOffset(TargetScrollViewer.ExtentHeight * off);
					else
						TargetScrollViewer.ScrollToHorizontalOffset(TargetScrollViewer.ExtentWidth * off);
				}
			}
		}

		public void SetRange (double lowerValue, double upperValue, double pageSize, double pageIncrement, double stepIncrement, double value)
		{
			this.lowerValue = lowerValue;
			this.upperValue = upperValue;
			this.pageSize = pageSize;
			InvalidateExtent ();

			this.pageIncrement = pageIncrement;
			this.stepIncrement = stepIncrement;
			InvalidateScrollInfo ();

			Value = value;
		}

		public double LowerValue
		{
			get { return lowerValue; }
		}

		public double UpperValue
		{
			get { return upperValue; }
		}

		public double PageIncrement
		{
			get { return pageIncrement; }
		}

		public double StepIncrement
		{
			get { return stepIncrement; }
		}

		public double PageSize
		{
			get { return pageSize; }
		}

		void InvalidateScrollInfo ()
		{
			if (TargetViewport != null && TargetViewport.ScrollOwner != null)
				TargetViewport.ScrollOwner.InvalidateScrollInfo ();
		}

		void InvalidateExtent ()
		{
			if (TargetViewport != null)
				TargetViewport.UpdateCustomExtent ();
		}
	}
}