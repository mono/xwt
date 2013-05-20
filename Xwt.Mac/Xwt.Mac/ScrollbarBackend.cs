//
// ScrollbarBackend.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc.
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
using MonoMac.AppKit;
using Xwt.Backends;

namespace Xwt.Mac
{
	public class ScrollbarBackend: ViewBackend<NSScroller,IWidgetEventSink>, IScrollbarBackend
	{
		Orientation orientation;

		public ScrollbarBackend ()
		{
		}

		public void Initialize (Orientation dir)
		{
			orientation = dir;
			ViewObject = new CustomScroller ();
		}

		public IScrollAdjustmentBackend CreateAdjustment ()
		{
			return (CustomScroller) Widget;
		}

		protected override Size GetNaturalSize ()
		{
			if (orientation == Orientation.Vertical)
				return new Size (20, 20);
			else
				return new Size (20, 20);
		}
	}

	class CustomScroller: NSScroller, IScrollAdjustmentBackend, IViewObject
	{
		IScrollAdjustmentEventSink eventSink;

		NSView IViewObject.View {
			get {
				return this;
			}
		}

		ViewBackend IViewObject.Backend { get; set; }

		public void Initialize (IScrollAdjustmentEventSink eventSink)
		{
			this.eventSink = eventSink;
			KnobStyle = NSScrollerKnobStyle.Default;
			ScrollerStyle = NSScrollerStyle.Legacy;
			ControlSize = NSControlSize.Regular;
			Enabled = true;
		}

		void HandleActivated (object sender, EventArgs e)
		{
			value = lowerValue + (DoubleValue * (upperValue - lowerValue - pageSize));
			switch (HitPart) {
			case NSScrollerPart.DecrementPage:
				value -= pageSize;
				if (value < lowerValue)
					value = lowerValue;
				UpdateValue ();
				break;
			case NSScrollerPart.IncrementPage:
				value += pageSize;
				if (value + pageSize > upperValue)
					value = upperValue - pageSize;
				if (value < lowerValue)
					value = lowerValue;
				UpdateValue ();
				break;
			case NSScrollerPart.DecrementLine:
				value -= stepIncrement;
				if (value < lowerValue)
					value = lowerValue;
				UpdateValue ();
				break;
			case NSScrollerPart.IncrementLine:
				value += stepIncrement;
				if (value + pageSize > upperValue)
					value = upperValue - pageSize;
				if (value < lowerValue)
					value = lowerValue;
				UpdateValue ();
				break;
			}
			eventSink.OnValueChanged ();
		}

		double value;
		public double Value {
			get {
				return value;
			}
			set {
				this.value = value;
				UpdateValue ();
			}
		}

		double lowerValue;
		public double LowerValue {
			get {
				return lowerValue;
			}
			set {
				lowerValue = value;
				UpdateValue ();
				UpdateKnobProportion ();
			}
		}

		double upperValue;
		public double UpperValue {
			get {
				return upperValue;
			}
			set {
				upperValue = value;
				UpdateValue ();
				UpdateKnobProportion ();
			}
		}

		double pageIncrement;
		public double PageIncrement {
			get {
				return pageIncrement;
			}
			set {
				pageIncrement = value;
			}
		}

		double stepIncrement;
		public double StepIncrement {
			get {
				return stepIncrement;
			}
			set {
				stepIncrement = value;
			}
		}

		double pageSize;
		public double PageSize {
			get {
				return pageSize;
			}
			set {
				pageSize = value;
				UpdateValue ();
				UpdateKnobProportion ();
			}
		}

		public void InitializeBackend (object frontend, ApplicationContext context)
		{
		}

		public void EnableEvent (object eventId)
		{
			if (((ScrollAdjustmentEvent)eventId) == ScrollAdjustmentEvent.ValueChanged)
				Activated += HandleActivated;
		}

		public void DisableEvent (object eventId)
		{
			if (((ScrollAdjustmentEvent)eventId) == ScrollAdjustmentEvent.ValueChanged)
				Activated -= HandleActivated;
		}

		void UpdateValue ()
		{
			if (lowerValue >= upperValue) {
				DoubleValue = 0.5;
				return;
			}

			var val = value;
			if (val < lowerValue)
				val = lowerValue;
			if (val + pageSize > upperValue) {
				val = upperValue - pageSize;
				if (val < lowerValue) {
					DoubleValue = 0.5;
					return;
				}
			}
			DoubleValue = (val - lowerValue) / (upperValue - lowerValue - pageSize);
		}

		void UpdateKnobProportion ()
		{
			if (pageSize == 0 || lowerValue >= upperValue || pageSize >= (upperValue - lowerValue))
				KnobProportion = 1;
			else {
				KnobProportion = (float)(pageSize / (upperValue - lowerValue));
			}
		}
	}
}

