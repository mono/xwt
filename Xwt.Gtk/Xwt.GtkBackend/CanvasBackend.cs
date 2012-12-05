// 
// CanvasBackend.cs
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
using System.Collections.Generic;
using Xwt.Backends;
using System.Linq;

using Xwt.CairoBackend;

namespace Xwt.GtkBackend
{
	public class CanvasBackend: WidgetBackend, ICanvasBackend
	{
		public CanvasBackend ()
		{
		}
		
		public override void Initialize ()
		{
			Widget = new CustomCanvas ();
			Widget.Backend = this;
			Widget.EventSink = EventSink;
			Widget.Events |= Gdk.EventMask.ButtonPressMask | Gdk.EventMask.ButtonReleaseMask | Gdk.EventMask.PointerMotionMask;
			Widget.Show ();
		}

		new CustomCanvas Widget {
			get { return (CustomCanvas)base.Widget; }
			set { base.Widget = value; }
		}
		
		protected new ICanvasEventSink EventSink {
			get { return (ICanvasEventSink)base.EventSink; }
		}

		public override void UpdateLayout ()
		{
			base.UpdateLayout ();

			// This is required to make sure that subsequent QueueDraws invalidate the
			// whole new area, not the area that covers the old size of the widget.
			Widget.QueueResize ();
		}

		public void QueueDraw ()
		{
			Widget.QueueDraw ();
		}
		
		public void QueueDraw (Rectangle rect)
		{
			Widget.QueueDrawArea ((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
		}
		
		public Rectangle Bounds {
			get {
				return new Rectangle (0, 0, Widget.Allocation.Width, Widget.Allocation.Height);
			}
		}

		public void AddChild (IWidgetBackend widget, Rectangle bounds)
		{
			var w = ((IGtkWidgetBackend)widget).Widget;
			Widget.Add (w);
			Widget.SetAllocation (w, bounds);
		}
		
		public void RemoveChild (IWidgetBackend widget)
		{
			var w = ((IGtkWidgetBackend)widget).Widget;
			Widget.Remove (w);
		}
		
		public void SetChildBounds (IWidgetBackend widget, Rectangle bounds)
		{
			var w = ((IGtkWidgetBackend)widget).Widget;
			Widget.SetAllocation (w, bounds);
		}
	}
	
	class CustomCanvas: Gtk.EventBox
	{
		public CanvasBackend Backend;
		public ICanvasEventSink EventSink;
		Dictionary<Gtk.Widget, Rectangle> children = new Dictionary<Gtk.Widget, Rectangle> ();
		
		public CustomCanvas ()
		{
			GtkWorkarounds.FixContainerLeak (this);

			WidgetFlags |= Gtk.WidgetFlags.AppPaintable;
		}
		
		public void SetAllocation (Gtk.Widget w, Rectangle rect)
		{
			children [w] = rect;
			QueueResize ();
		}
		
		protected override void OnAdded (Gtk.Widget widget)
		{
			children.Add (widget, new Rectangle (0,0,0,0));
			widget.Parent = this;
			QueueResize ();
		}
		
		protected override void OnRemoved (Gtk.Widget widget)
		{
			children.Remove (widget);
			widget.Unparent ();
		}
		
		protected override void OnSizeRequested (ref Gtk.Requisition requisition)
		{
			base.OnSizeRequested (ref requisition);
			IWidgetSurface ws = Backend.Frontend.Surface;
			int h, w;
			if (ws.SizeRequestMode == SizeRequestMode.HeightForWidth) {
				w = (int)ws.GetPreferredWidth ().MinSize;
				h = (int)ws.GetPreferredHeightForWidth (w).MinSize;
			} else {
				h = (int)ws.GetPreferredHeight ().MinSize;
				w = (int)ws.GetPreferredWidthForHeight(h).MinSize;
			}
			if (requisition.Width < w)
				requisition.Width = w;
			if (requisition.Height < h)
				requisition.Height = h;
			foreach (var cr in children)
				cr.Key.SizeRequest ();
		}
		
		protected override void OnUnrealized ()
		{
			base.OnUnrealized ();
			lastAllocation = new Gdk.Rectangle ();
		}
		
		Gdk.Rectangle lastAllocation;
		
		protected override void OnSizeAllocated (Gdk.Rectangle allocation)
		{
			base.OnSizeAllocated (allocation);
			if (!lastAllocation.Equals (allocation))
				((IWidgetSurface)Backend.Frontend).Reallocate ();
			lastAllocation = allocation;
			foreach (var cr in children) {
				var r = cr.Value;
				cr.Key.SizeAllocate (new Gdk.Rectangle ((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height));
			}
		}
		
		protected override void ForAll (bool includeInternals, Gtk.Callback callback)
		{
			base.ForAll (includeInternals, callback);
			foreach (var c in children.Keys.ToArray ())
				callback (c);
		}
		
		protected override bool OnExposeEvent (Gdk.EventExpose evnt)
		{
			Backend.ApplicationContext.InvokeUserCode (delegate {
				var a = evnt.Area;
				EventSink.OnDraw (CreateContext (), new Rectangle (a.X, a.Y, a.Width, a.Height));
			});
			return base.OnExposeEvent (evnt);
		}
		
		public object CreateContext ()
		{
			CairoContextBackend ctx = new CairoContextBackend ();
			if (!IsRealized) {
				Cairo.Surface sf = new Cairo.ImageSurface (Cairo.Format.ARGB32, 1, 1);
				Cairo.Context c = new Cairo.Context (sf);
				ctx.Context = c;
				ctx.TempSurface = sf;
			} else {
				ctx.Context = Gdk.CairoHelper.Create (GdkWindow);
			}
			return ctx;
		}
	}
}

