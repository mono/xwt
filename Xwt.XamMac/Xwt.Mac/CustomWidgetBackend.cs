// 
// CustomWidgetBackend.cs
//  
// Author:
//       Alex Corrado <corrado@xamarin.com>
// 
// Copyright (c) 2013 Xamarin Inc
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
	public class CustomWidgetBackend: ViewBackend<NSView,IWidgetEventSink>, ICustomWidgetBackend
	{
		ViewBackend childBackend;

		public CustomWidgetBackend ()
		{
		}

		public override void Initialize ()
		{
			ViewObject = new CustomWidgetView (EventSink, ApplicationContext);
		}

		public void SetContent (IWidgetBackend widget)
		{
			if (childBackend != null) {
				childBackend.Widget.RemoveFromSuperview ();
				RemoveChildPlacement (childBackend.Widget);
				childBackend.Widget.AutoresizingMask = NSViewResizingMask.NotSizable;
			}
			if (widget == null)
				return;

			var view = Widget;
			childBackend = (ViewBackend)widget;
			var childView = GetWidgetWithPlacement (childBackend);
			childView.Frame = view.Bounds;
			childView.AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable;
			view.AddSubview (childView);
			view.SetNeedsDisplayInRect (view.Bounds);
		}

		protected override Size GetNaturalSize ()
		{
			if (childBackend != null)
				return childBackend.Frontend.Surface.GetPreferredSize (includeMargin: true);
			else
				return base.GetNaturalSize ();
		}

		public override bool CanGetFocus {
			get {
				return base.CanGetFocus;
			}
			set {
				((CustomWidgetView)ViewObject).CanGetFocus = value;
				base.CanGetFocus = value;
			}
		}
	}

	class CustomWidgetView: WidgetView, INSAccessibleEventSource
	{
		public CustomWidgetView (IWidgetEventSink eventSink, ApplicationContext context) : base (eventSink, context)
		{
		}
		bool canGetFocus;

		public bool CanGetFocus {
			get {
				return canGetFocus;
			}

			set {
				canGetFocus = value;
			}
		}

		public override bool BecomeFirstResponder ()
		{
			var res = base.BecomeFirstResponder ();
			base.AccessibilityFocused = res;
			return res;
		}

		public override bool AcceptsFirstResponder ()
		{
			return CanGetFocus;
		}

		public Func<bool> PerformAccessiblePressDelegate { get; set; }

		public override void SetFrameSize (CGSize newSize)
		{
			base.SetFrameSize (newSize);
			if (Subviews.Length == 0)
				return;
			Subviews [0].SetFrameSize (newSize);
			Backend.Frontend.Surface.Reallocate ();
		}

		public override bool RespondsToSelector (ObjCRuntime.Selector sel)
		{
			return base.RespondsToSelector (sel);
		}

		public override bool AccessibilityPerformPress ()
		{
			if (PerformAccessiblePressDelegate != null) {
				if (PerformAccessiblePressDelegate ())
					return true;
			}
			return base.AccessibilityPerformPress ();
		}
	}
}

