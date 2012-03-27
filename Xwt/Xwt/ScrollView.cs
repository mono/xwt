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
	public class ScrollView: Widget
	{
		Widget child;
		EventHandler visibleRectChanged;
		
		protected new class EventSink: Widget.EventSink, IScrollViewEventSink
		{
			public void OnVisibleRectChanged ()
			{
				((ScrollView)Parent).OnVisibleRectChanged (EventArgs.Empty);
			}
			
			public override Size GetDefaultNaturalSize ()
			{
				return Xwt.Engine.DefaultNaturalSizes.ScrollView;
			}
		}
		
		protected override Widget.EventSink CreateEventSink ()
		{
			return new EventSink ();
		}
		
		new IScrollViewBackend Backend {
			get { return (IScrollViewBackend)base.Backend; }
		}
		
		public ScrollView ()
		{
		}
		
		public ScrollView (Widget child)
		{
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
			base.OnReallocate ();
			if (child != null && !child.SupportsCustomScrolling) {
				var ws = (IWidgetSurface) child;
				if (ws.SizeRequestMode == SizeRequestMode.HeightForWidth) {
					var w = ws.GetPreferredWidth ();
					var h = ws.GetPreferredHeightForWidth (w.NaturalSize);
					Backend.SetChildSize (new Size (w.NaturalSize, h.NaturalSize));
				} else {
					var h = ws.GetPreferredHeight ();
					var w = ws.GetPreferredWidthForHeight (h.NaturalSize);
					Backend.SetChildSize (new Size (w.NaturalSize, h.NaturalSize));
				}
			}
		}
		
		public ScrollPolicy VerticalScrollPolicy {
			get { return Backend.VerticalScrollPolicy; }
			set { Backend.VerticalScrollPolicy = value; OnPreferredSizeChanged (); }
		}
		
		public ScrollPolicy HorizontalScrollPolicy {
			get { return Backend.HorizontalScrollPolicy; }
			set { Backend.HorizontalScrollPolicy = value; OnPreferredSizeChanged (); }
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
				OnBeforeEventAdd (ScrollViewEvent.VisibleRectChanged, visibleRectChanged);
				visibleRectChanged += value;
			}
			remove {
				visibleRectChanged -= value;
				OnAfterEventRemove (ScrollViewEvent.VisibleRectChanged, visibleRectChanged);
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

