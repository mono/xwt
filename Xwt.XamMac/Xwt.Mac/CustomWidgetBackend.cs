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
using Xwt.Backends;

#if MONOMAC
using nint = System.Int32;
using nfloat = System.Single;
using CGSize = System.Drawing.SizeF;
using MonoMac.AppKit;
#else
using AppKit;
using CoreGraphics;
#endif

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
	}

	class CustomWidgetView: WidgetView
	{
		public CustomWidgetView (IWidgetEventSink eventSink, ApplicationContext context) : base (eventSink, context)
		{
		}

		public override void SetFrameSize (CGSize newSize)
		{
			base.SetFrameSize (newSize);
			if (Subviews.Length == 0)
				return;
			Subviews [0].SetFrameSize (newSize);
			Backend.Frontend.Surface.Reallocate ();
		}
	}
}

