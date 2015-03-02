// 
// ScrollControlBackend.cs
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

#if MONOMAC
using nint = System.Int32;
using nfloat = System.Single;
using CGRect = System.Drawing.RectangleF;
using CGPoint = System.Drawing.PointF;
using CGSize = System.Drawing.SizeF;
using MonoMac.AppKit;
#else
using AppKit;
using CoreGraphics;
#endif

namespace Xwt.Mac
{
	class ScrollControlBackend: IScrollControlBackend
	{
		bool vertical;
		NSScrollView scrollView;
		IScrollControlEventSink eventSink;
		ApplicationContext appContext;
		double lastValue;

		public ScrollControlBackend (ApplicationContext appContext, NSScrollView scrollView, bool vertical)
		{
			this.vertical = vertical;
			this.scrollView = scrollView;
			this.appContext = appContext;
			lastValue = Value;
		}

		public void NotifyValueChanged ()
		{
			if (lastValue != Value) {
				lastValue = Value;
				appContext.InvokeUserCode (delegate {
					eventSink.OnValueChanged ();
				});
			}
		}

		#region IBackend implementation
		public void InitializeBackend (object frontend, ApplicationContext context)
		{
		}

		public void EnableEvent (object eventId)
		{
		}

		public void DisableEvent (object eventId)
		{
		}
		#endregion

		#region IScrollAdjustmentBackend implementation
		public void Initialize (IScrollControlEventSink eventSink)
		{
			this.eventSink = eventSink;
		}

		public double Value {
			get {
				if (vertical)
					return scrollView.DocumentVisibleRect.Y;
				else
					return scrollView.DocumentVisibleRect.X;
			}
			set {
				if (vertical)
					scrollView.ContentView.ScrollToPoint (new CGPoint (scrollView.DocumentVisibleRect.X, (nfloat)value));
				else
					scrollView.ContentView.ScrollToPoint (new CGPoint ((nfloat)value, scrollView.DocumentVisibleRect.Y));
				scrollView.ReflectScrolledClipView (scrollView.ContentView);
			}
		}

		public double LowerValue {
			get {
				return 0;
			}
		}

		public double UpperValue {
			get { return vertical ? scrollView.ContentSize.Height : scrollView.ContentSize.Width; }
		}

		public double PageIncrement {
			get {
				return vertical ? scrollView.VerticalPageScroll : scrollView.HorizontalPageScroll;
			}
		}

		public double PageSize {
			get {
				return vertical ? scrollView.DocumentVisibleRect.Height : scrollView.DocumentVisibleRect.Width;
			}
		}

		public double StepIncrement {
			get {
				return vertical ? scrollView.VerticalLineScroll : scrollView.HorizontalLineScroll;
			}
		}
		#endregion
	}
}

