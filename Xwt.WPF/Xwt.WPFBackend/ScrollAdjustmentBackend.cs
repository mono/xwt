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
		: Backend, IScrollAdjustmentBackend
	{
		double scrollValue;
		double lowerValue;
		double upperValue;
		double pageIncrement;
		double stepIncrement;
		double pageSize;

		public CustomScrollViewPort TargetViewport { get; set; }

		public void Initialize (IScrollAdjustmentEventSink eventSink)
		{
			EventSink = eventSink;
		}

		public void SetOffset (double offset)
		{
			// The offset is relative to 0, it has to be converted to the lower/upper value range
			scrollValue = LowerValue + offset;
			Xwt.Engine.ApplicationContext.InvokeUserCode (EventSink.OnValueChanged);
		}

		public double Value
		{
			get { return scrollValue; }
			set {
				// Provide the value to the viewport, which will update
				// the ScrollView. The viewport expects an offset starting at 0.
				TargetViewport.SetOffset (this, value - LowerValue);
			}
		}

		public double LowerValue
		{
			get { return lowerValue; }
			set { lowerValue = value; InvalidateExtent (); }
		}

		public double UpperValue
		{
			get { return upperValue; }
			set { upperValue = value; InvalidateExtent (); }
		}

		public double PageIncrement
		{
			get { return pageIncrement; }
			set { pageIncrement = value; InvalidateScrollInfo (); }
		}

		public double StepIncrement
		{
			get { return stepIncrement; }
			set { stepIncrement = value; InvalidateScrollInfo (); }
		}

		public double PageSize
		{
			get { return pageSize; }
			set { pageSize = value; InvalidateExtent (); }
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

		internal IScrollAdjustmentEventSink EventSink
		{
			get;
			private set;
		}
	}
}