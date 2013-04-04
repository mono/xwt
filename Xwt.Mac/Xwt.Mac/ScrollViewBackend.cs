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
		IWidgetBackend child;
		ScrollPolicy verticalScrollPolicy;
		ScrollPolicy horizontalScrollPolicy;

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
			this.child = child;
			ViewBackend backend = (ViewBackend) child;
			if (backend.EventSink.SupportsCustomScrolling ()) {
				var vs = new ScrollAdjustmentBackend (Widget, true);
				var hs = new ScrollAdjustmentBackend (Widget, false);
				CustomClipView clipView = new CustomClipView (hs, vs);
				Widget.ContentView = clipView;
				var dummy = new DummyClipView ();
				dummy.AddSubview (backend.Widget);
				backend.Widget.Frame = new System.Drawing.RectangleF (0, 0, clipView.Frame.Width, clipView.Frame.Height);
				clipView.DocumentView = dummy;
				backend.EventSink.SetScrollAdjustments (hs, vs);
			}
			else {
				Widget.DocumentView = backend.Widget;
				UpdateChildSize ();
			}
		}
		
		public ScrollPolicy VerticalScrollPolicy {
			get {
				return verticalScrollPolicy;
			}
			set {
				verticalScrollPolicy = value;
			}
		}

		public ScrollPolicy HorizontalScrollPolicy {
			get {
				return horizontalScrollPolicy;
			}
			set {
				horizontalScrollPolicy = value;
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

		void UpdateChildSize ()
		{
			if (child == null)
				return;

			if (Widget.ContentView is CustomClipView) {
			} else {
				NSView view = (NSView)Widget.DocumentView;
				ViewBackend c = (ViewBackend)child;
				var pw = c.Frontend.Surface.GetPreferredWidth ();
				var w = Math.Max (pw.NaturalSize, Widget.ContentView.Frame.Width);
				var ph = c.Frontend.Surface.GetPreferredHeightForWidth (w);
				var h = Math.Max (ph.NaturalSize, Widget.ContentView.Frame.Height);
				view.Frame = new System.Drawing.RectangleF (view.Frame.X, view.Frame.Y, (float)w, (float)h);
			}
		}
		
		public void SetChildSize (Size s)
		{
			UpdateChildSize ();
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

	class DummyClipView: NSView
	{
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

		public float CurrentX { get; set; }
		public float CurrentY { get; set; }

		public CustomClipView (ScrollAdjustmentBackend hScroll, ScrollAdjustmentBackend vScroll)
		{
			this.hScroll = hScroll;
			this.vScroll = vScroll;
			CopiesOnScroll = false;
		}

		public override bool IsFlipped {
			get {
				return true;
			}
		}

		public override void SetFrameSize (System.Drawing.SizeF newSize)
		{
			base.SetFrameSize (newSize);
			var v = DocumentView.Subviews [0];
			v.Frame = new System.Drawing.RectangleF (v.Frame.X, v.Frame.Y, newSize.Width, newSize.Height);
		}
		
		public override void ScrollToPoint (System.Drawing.PointF newOrigin)
		{
			base.ScrollToPoint (newOrigin);
			var v = DocumentView.Subviews [0];

			CurrentX = newOrigin.X >= 0 ? newOrigin.X : 0;
			CurrentY = newOrigin.Y >= 0 ? newOrigin.Y : 0;
			if (CurrentX + v.Frame.Width > DocumentView.Frame.Width)
				CurrentX = DocumentView.Frame.Width - v.Frame.Width;
			if (CurrentY + v.Frame.Height > DocumentView.Frame.Height)
				CurrentY = DocumentView.Frame.Height - v.Frame.Height;

			v.Frame = new System.Drawing.RectangleF (CurrentX, CurrentY, v.Frame.Width, v.Frame.Height);

			hScroll.NotifyValueChanged ();
			vScroll.NotifyValueChanged ();
		}

		public void UpdateDocumentSize ()
		{
			DocumentView.Frame = new System.Drawing.RectangleF (0, 0, (float)(hScroll.UpperValue - hScroll.LowerValue), (float)(vScroll.UpperValue - vScroll.LowerValue));
		}
	}
}

