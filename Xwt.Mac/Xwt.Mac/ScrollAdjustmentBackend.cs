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
using System;
using Xwt.Backends;
using MonoMac.AppKit;

namespace Xwt.Mac
{
	class ScrollAdjustmentBackend: IScrollAdjustmentBackend
	{
		bool vertical;
		NSScrollView scrollView;
		IScrollAdjustmentEventSink eventSink;
		double lower;
		double upper;
		
		public ScrollAdjustmentBackend (NSScrollView scrollView, bool vertical)
		{
			this.vertical = vertical;
			this.scrollView = scrollView;
		}

		#region IBackend implementation
		public void Initialize (object frontend)
		{
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
			eventSink.OnValueChanged ();
		}

		#region IScrollAdjustmentBackend implementation
		public void Initialize (IScrollAdjustmentEventSink eventSink)
		{
			this.eventSink = eventSink;
		}

		public double Value {
			get {
				if (vertical)
					return scrollView.DocumentVisibleRect.Top;
				else
					return scrollView.DocumentVisibleRect.Left;
			}
			set {
			}
		}

		public double LowerValue {
			get {
				return lower;
			}
			set {
				lower = value;
			}
		}

		public double UpperValue {
			get {
				return upper;
			}
			set {
				upper = value;
			}
		}

		public double PageIncrement {
			get {
				return 10;
			}
			set {
			}
		}

		public double StepIncrement {
			get {
				if (vertical)
					return scrollView.VerticalLineScroll;
				else
					return scrollView.HorizontalLineScroll;
			}
			set {
				if (vertical)
					scrollView.VerticalLineScroll = (float)value;
				else
					scrollView.HorizontalLineScroll = (float)value;
			}
		}

		public double PageSize {
			get {
				if (vertical)
					return scrollView.DocumentVisibleRect.Height;
				else
					return scrollView.DocumentVisibleRect.Width;
			}
			set {
			}
		}
		#endregion
	}
}

