// 
// ScrollView.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2011 Xamarin Inc
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

namespace Xwt
{
	[BackendType (typeof(IScrollViewBackend))]
	public class ScrollView: Widget, IScrollableWidget
	{
		Widget child;
		EventHandler visibleRectChanged;
		
		protected new class WidgetBackendHost: Widget.WidgetBackendHost, IScrollViewEventSink
		{
			public void OnVisibleRectChanged ()
			{
				((ScrollView)Parent).OnVisibleRectChanged (EventArgs.Empty);
			}
			
			public override Size GetDefaultNaturalSize ()
			{
				return Xwt.Backends.DefaultNaturalSizes.ScrollView;
			}
		}
		
		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}
		
		IScrollViewBackend Backend {
			get { return (IScrollViewBackend)BackendHost.Backend; }
		}
		
		public ScrollView ()
		{
			HorizontalScrollPolicy = ScrollPolicy.Automatic;
			VerticalScrollPolicy = ScrollPolicy.Automatic;
		}
		
		public ScrollView (Widget child): this ()
		{
			VerifyConstructorCall (this);
			Content = child;
		}
		
		public new Widget Content {
			get { return child; }
			set {
				if (child != null)
					UnregisterChild (child);
				child = value;
				if (child != null)
					RegisterChild (child);
				Backend.SetChild ((IWidgetBackend)GetBackend (child));
				OnPreferredSizeChanged ();
			}
		}
		
		protected override void OnReallocate ()
		{
			if (child != null && !child.SupportsCustomScrolling)
				Backend.SetChildSize (child.Surface.GetPreferredSize (SizeConstraint.Unconstrained, SizeConstraint.Unconstrained));
			base.OnReallocate ();
		}
		
		public ScrollPolicy VerticalScrollPolicy {
			get { return Backend.VerticalScrollPolicy; }
			set { Backend.VerticalScrollPolicy = value; OnPreferredSizeChanged (); }
		}
		
		public ScrollPolicy HorizontalScrollPolicy {
			get { return Backend.HorizontalScrollPolicy; }
			set { Backend.HorizontalScrollPolicy = value; OnPreferredSizeChanged (); }
		}

		ScrollControl verticalScrollAdjustment;
		public ScrollControl VerticalScrollControl {
			get {
				if (verticalScrollAdjustment == null)
					verticalScrollAdjustment = new ScrollControl (Backend.CreateVerticalScrollControl ());
				return verticalScrollAdjustment;
			}
		}

		ScrollControl horizontalScrollAdjustment;
		public ScrollControl HorizontalScrollControl {
			get {
				if (horizontalScrollAdjustment == null)
					horizontalScrollAdjustment = new ScrollControl (Backend.CreateHorizontalScrollControl ());
				return horizontalScrollAdjustment;
			}
		}

		public Rectangle VisibleRect {
			get { return Backend.VisibleRect; }
		}
		
		public bool BorderVisible {
			get { return Backend.BorderVisible; }
			set { Backend.BorderVisible = value; }
		}
		
		public event EventHandler VisibleRectChanged {
			add {
				BackendHost.OnBeforeEventAdd (ScrollViewEvent.VisibleRectChanged, visibleRectChanged);
				visibleRectChanged += value;
			}
			remove {
				visibleRectChanged -= value;
				BackendHost.OnAfterEventRemove (ScrollViewEvent.VisibleRectChanged, visibleRectChanged);
			}
		}
		
		protected virtual void OnVisibleRectChanged (EventArgs e)
		{
			if (visibleRectChanged != null)
				visibleRectChanged (this, e);
		}
	}
	
	public enum ScrollPolicy
	{
		Always,
		Automatic,
		Never
	}
}

