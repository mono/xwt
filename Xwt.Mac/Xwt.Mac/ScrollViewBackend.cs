// 
// ScrollViewBackend.cs
//  
// Author:
//       Lluis Sanchez Gual <lluis@xamarin.com>
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
	public class ScrollViewBackend: ViewBackend<NSScrollView,IScrollViewEventSink>, IScrollViewBackend
	{
		public override void Initialize ()
		{
			ViewObject = new CustomScrollView ();
			Widget.HasHorizontalScroller = true;
			Widget.HasVerticalScroller = true;
			Widget.AutoresizesSubviews = true;
		}
		
		protected override Size GetNaturalSize ()
		{
			return EventSink.GetDefaultNaturalSize ();
		}
		
		public void SetChild (IWidgetBackend child)
		{
			ViewBackend backend = (ViewBackend) child;
			if (backend.EventSink.SupportsCustomScrolling ()) {
				var vs = new ScrollAdjustmentBackend (Widget, true);
				var hs = new ScrollAdjustmentBackend (Widget, false);
				CustomClipView clipView = new CustomClipView (hs, vs);
				Widget.ContentView = clipView;
				clipView.DocumentView = backend.Widget;
				backend.EventSink.SetScrollAdjustments (hs, vs);
				backend.Widget.Frame = new System.Drawing.RectangleF (0, 0, 500,500);
			}
			else {
				Widget.DocumentView = backend.Widget;
				backend.Widget.Frame = Widget.ContentView.DocumentRect;
			}
		}
		
		public ScrollPolicy VerticalScrollPolicy {
			get {
				return ScrollPolicy.Automatic;
			}
			set {
			}
		}

		public ScrollPolicy HorizontalScrollPolicy {
			get {
				return ScrollPolicy.Automatic;
			}
			set {
			}
		}
		
		public Rectangle VisibleRect {
			get {
				return Rectangle.Zero;
			}
		}
		
		public bool BorderVisible {
			get {
				return false;
			}
			set {
			}
		}
		
		public void SetChildSize (Size s)
		{
			NSView view = (NSView) Widget.DocumentView;
			var w = Math.Max (s.Width, Widget.ContentView.Frame.Width);
			var h = Math.Max (s.Height, Widget.ContentView.Frame.Height);
			view.Frame = new System.Drawing.RectangleF (view.Frame.X, view.Frame.Y, (float)w, (float)h);
		}
	}
	
	class CustomScrollView: NSScrollView, IViewObject
	{
		public NSView View {
			get {
				return this;
			}
		}

		public ViewBackend Backend { get; set; }
		
		public override bool IsFlipped {
			get {
				return true;
			}
		}
	}
	
	class CustomClipView: NSClipView
	{
		ScrollAdjustmentBackend hScroll;
		ScrollAdjustmentBackend vScroll;
		System.Drawing.RectangleF visibleRect;
		
		public CustomClipView (ScrollAdjustmentBackend hScroll, ScrollAdjustmentBackend vScroll)
		{
			this.hScroll = hScroll;
			this.vScroll = vScroll;
			CopiesOnScroll = false;
			
		}
		
		public override void ScrollToPoint (System.Drawing.PointF newOrigin)
		{
			visibleRect = new System.Drawing.RectangleF (newOrigin.X, newOrigin.Y, 100, 100);
			hScroll.NotifyValueChanged ();
			vScroll.NotifyValueChanged ();
		}
		
		public override System.Drawing.RectangleF DocumentVisibleRect ()
		{
			return visibleRect;
		}
	}
}

