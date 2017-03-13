// 
// ScrollAdjustmentBackend.cs
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

using AppKit;
using Xwt.Backends;

namespace Xwt.Mac
{
	class ScrollAdjustmentBackend: IScrollAdjustmentBackend, IScrollControlBackend
	{
		bool vertical;
		NSScrollView scrollView;
		IScrollAdjustmentEventSink eventSink;
		IScrollControlEventSink controlEventSink;
		ApplicationContext context;

		public ScrollAdjustmentBackend (NSScrollView scrollView, bool vertical)
		{
			this.vertical = vertical;
			this.scrollView = scrollView;
		}

		#region IBackend implementation
		public void InitializeBackend (object frontend, ApplicationContext context)
		{
			this.context = context;
		}

		public void EnableEvent (object eventId)
		{
		}

		public void DisableEvent (object eventId)
		{
		}
		#endregion
		
		public void NotifyValueChanged ()
		{
			context.InvokeUserCode (delegate {
				if (eventSink != null)
					eventSink.OnValueChanged ();
				if (controlEventSink != null)
					controlEventSink.OnValueChanged ();
			});
		}

		#region IScrollAdjustmentBackend implementation
		public void Initialize (IScrollAdjustmentEventSink eventSink)
		{
			this.eventSink = eventSink;
		}

		public void Initialize (IScrollControlEventSink eventSink)
		{
			this.controlEventSink = eventSink;
		}

		CustomClipView ClipView {
			get { return ((CustomClipView)scrollView.ContentView); }
		}

		public double Value {
			get {
				if (vertical)
					return ClipView.CurrentY;
				else
					return ClipView.CurrentX;
			}
			set {
				if (vertical)
					ClipView.CurrentY = value;
				else
					ClipView.CurrentX = value;
			}
		}

		public void SetRange (double lowerValue, double upperValue, double pageSize, double pageIncrement, double stepIncrement, double value)
		{
			LowerValue = lowerValue;
			UpperValue = upperValue;
			PageSize = pageSize;
			PageIncrement = pageIncrement;
			StepIncrement = stepIncrement;

			if (vertical)
				scrollView.VerticalLineScroll = (float)stepIncrement;
			else
				scrollView.HorizontalLineScroll = (float)stepIncrement;

			ClipView.UpdateDocumentSize ();
			if (Value != value)
				Value = value;
		}

		public double LowerValue { get; private set; }

		public double UpperValue { get; private set; }

		public double PageIncrement { get; private set; }

		public double PageSize { get; private set; }

		public double StepIncrement { get; private set; }

		#endregion
	}
}

