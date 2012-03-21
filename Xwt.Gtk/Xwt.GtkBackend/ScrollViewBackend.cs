// 
// ScrollViewBackend.cs
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
using Xwt.Engine;

namespace Xwt.GtkBackend
{
	public class ScrollViewBackend: WidgetBackend, IScrollViewBackend
	{
		bool showBorder = true;
		
		public ScrollViewBackend ()
		{
			Widget = new Gtk.ScrolledWindow ();
			Widget.Show ();
		}
		
		protected new Gtk.ScrolledWindow Widget {
			get { return (Gtk.ScrolledWindow)base.Widget; }
			set { base.Widget = value; }
		}
		
		protected new IScrollViewEventSink EventSink {
			get { return (IScrollViewEventSink)base.EventSink; }
		}

		public void SetChild (IWidgetBackend child)
		{
			if (Widget.Child != null) {
				if (Widget.Child is Gtk.Bin) {
					Gtk.Bin vp = (Gtk.Bin) Widget.Child;
					vp.Remove (vp.Child);
				}
				Widget.Remove (Widget.Child);
			}
			
			if (child != null) {
				
				var w = GetWidget (child);
				
				WidgetBackend wb = (WidgetBackend) child;
				
				if (wb.EventSink.SupportsCustomScrolling ()) {
					CustomViewPort vp = new CustomViewPort (wb.EventSink);
					vp.Show ();
					vp.Add (w);
					Widget.Child = vp;
				}
				else if (w is Gtk.Viewport)
					Widget.Child = w;
				else {
					Gtk.Viewport vp = new Gtk.Viewport ();
					vp.Show ();
					vp.Add (w);
					Widget.Child = vp;
				}
			}
			
			UpdateBorder ();
		}
		
		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is ScrollViewEvent) {
				if (((ScrollViewEvent)eventId) == ScrollViewEvent.VisibleRectChanged) {
					Widget.Hadjustment.ValueChanged += HandleValueChanged;
					Widget.Vadjustment.ValueChanged += HandleValueChanged;
				}
			}
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is ScrollViewEvent) {
				if (((ScrollViewEvent)eventId) == ScrollViewEvent.VisibleRectChanged) {
					Widget.Hadjustment.ValueChanged -= HandleValueChanged;
					Widget.Vadjustment.ValueChanged -= HandleValueChanged;
				}
			}
		}
		
		[GLib.ConnectBefore]
		void HandleValueChanged (object sender, EventArgs e)
		{
			Toolkit.Invoke (delegate {
				EventSink.OnVisibleRectChanged ();
			});
		}
		
		public Rectangle VisibleRect {
			get {
				double x = Widget.Hadjustment.Value;
				double y = Widget.Vadjustment.Value;
				return new Rectangle (x, y, Widget.Hadjustment.PageSize, Widget.Vadjustment.PageSize);
			}
		}
		
		public bool BorderVisible {
			get {
				return Widget.ShadowType == Gtk.ShadowType.In;
			}
			set {
				showBorder = value;
				UpdateBorder ();
			}
		}
		
		void UpdateBorder ()
		{
			var shadowType = showBorder ? Gtk.ShadowType.In : Gtk.ShadowType.None;
			if (Widget.Child is Gtk.Viewport)
				((Gtk.Viewport)Widget.Child).ShadowType = shadowType;
			else
				Widget.ShadowType = shadowType;
		}
		
		public ScrollPolicy VerticalScrollPolicy {
			get {
				return ConvertPolicy (Widget.VscrollbarPolicy);
			}
			set {
				Widget.VscrollbarPolicy = ConvertPolicy (value);
			}
		}
		
		public ScrollPolicy HorizontalScrollPolicy {
			get {
				return ConvertPolicy (Widget.HscrollbarPolicy);
			}
			set {
				Widget.HscrollbarPolicy = ConvertPolicy (value);
			}
		}
		
		ScrollPolicy ConvertPolicy (Gtk.PolicyType p)
		{
			switch (p) {
			case Gtk.PolicyType.Always:
				return ScrollPolicy.Always;
			case Gtk.PolicyType.Automatic:
				return ScrollPolicy.Automatic;
			case Gtk.PolicyType.Never:
				return ScrollPolicy.Never;
			}
			throw new InvalidOperationException ("Invalid policy value:" + p);
		}
		
		Gtk.PolicyType ConvertPolicy (ScrollPolicy p)
		{
			switch (p) {
			case ScrollPolicy.Always:
				return Gtk.PolicyType.Always;
			case ScrollPolicy.Automatic:
				return Gtk.PolicyType.Automatic;
			case ScrollPolicy.Never:
				return Gtk.PolicyType.Never;
			}
			throw new InvalidOperationException ("Invalid policy value:" + p);
		}
	}
	
	class CustomViewPort: Gtk.Bin
	{
		Gtk.Widget child;
		IWidgetEventSink eventSink;
		
		public CustomViewPort (IWidgetEventSink eventSink)
		{
			this.eventSink = eventSink;
		}
		
		protected override void OnAdded (Gtk.Widget widget)
		{
			base.OnAdded (widget);
			child = widget;
		}

		protected override void OnSizeRequested (ref Gtk.Requisition requisition)
		{
			if (child != null) {
				requisition = child.SizeRequest ();
			} else {
				requisition.Width = 0;
				requisition.Height = 0;
			}
		}

		protected override void OnSizeAllocated (Gdk.Rectangle allocation)
		{
			base.OnSizeAllocated (allocation);
			if (child != null)
				child.SizeAllocate (allocation);
		}

		protected override void OnSetScrollAdjustments (Gtk.Adjustment hadj, Gtk.Adjustment vadj)
		{
			var hsa = new ScrollAdjustmentBackend (hadj);
			var vsa = new ScrollAdjustmentBackend (vadj);
			
			eventSink.SetScrollAdjustments (hsa, vsa);
		}
	}
}

