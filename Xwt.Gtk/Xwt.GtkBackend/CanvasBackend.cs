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
			#if XWT_GTK3
			base.AppPaintable = true;
			#else
			WidgetFlags |= Gtk.WidgetFlags.AppPaintable;
			#endif
			VisibleWindow = false;
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

		#if XWT_GTK3
		protected override Gtk.SizeRequestMode OnGetRequestMode ()
		{
			// always in fixed mode, since we have fixed width-height relation
			return (Gtk.SizeRequestMode)2;
		}

		protected override void OnGetPreferredHeight (out int minimum_height, out int natural_height)
		{
			minimum_height = natural_height = (this.HeightRequest > 0 ? this.HeightRequest : 0);
			minimum_height = natural_height = (int)(Backend.Frontend.MinHeight > 0 ? Backend.Frontend.MinHeight : 0);
			foreach (var cr in children.Where (c => c.Key.Visible)) {
				minimum_height = (int)Math.Max(minimum_height, cr.Value.Y + cr.Value.Height);
				natural_height = (int)Math.Max(natural_height, cr.Value.Y + cr.Value.Height);
			}
		}

		protected override void OnGetPreferredWidth (out int minimum_width, out int natural_width)
		{
			minimum_width = natural_width = (this.WidthRequest > 0 ? this.WidthRequest : 0);
			minimum_width = natural_width = (int)(Backend.Frontend.MinWidth > 0 ? Backend.Frontend.MinWidth : 0);
			foreach (var cr in children.Where (c => c.Key.Visible)) {
				minimum_width = (int)Math.Max(minimum_width, cr.Value.X + cr.Value.Height);
				natural_width = (int)Math.Max(natural_width, cr.Value.X + cr.Value.Height);
			}
		}
		#else
		protected override void OnSizeRequested (ref Gtk.Requisition requisition)
		{
			base.OnSizeRequested (ref requisition);
			foreach (var cr in children.ToArray ())
				cr.Key.SizeRequest ();
		}
		#endif
		
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
			var dx = VisibleWindow ? 0 : allocation.X;
			var dy = VisibleWindow ? 0 : allocation.Y;
			foreach (var cr in children) {
				var r = cr.Value;
				var w = (int) Math.Max (r.Width, 0);
				var h = (int) Math.Max (r.Height, 0);
				cr.Key.SizeAllocate (new Gdk.Rectangle (dx + (int)r.X, dy + (int)r.Y, w, h));
			}
		}
		
		protected override void ForAll (bool includeInternals, Gtk.Callback callback)
		{
			base.ForAll (includeInternals, callback);
			foreach (var c in children.Keys.ToArray ())
				callback (c);
		}

		#if XWT_GTK3
		protected override bool OnDrawn (Cairo.Context cr)
		#else
		protected override bool OnExposeEvent (Gdk.EventExpose evnt)
		#endif
		{
			Backend.ApplicationContext.InvokeUserCode (delegate {
				using (var context = CreateContext ()) {
					#if XWT_GTK3
					var a = Allocation;
					#else
					var a = evnt.Area;
					#endif
					EventSink.OnDraw (context, new Rectangle (a.X, a.Y, a.Width, a.Height));
				}
			});
			#if XWT_GTK3
			return base.OnDrawn (cr);
			#else
			return base.OnExposeEvent (evnt);
			#endif
		}
		
		public CairoContextBackend CreateContext ()
		{
			CairoContextBackend ctx = new CairoContextBackend (Util.GetScaleFactor (this));
			if (!IsRealized) {
				Cairo.Surface sf = new Cairo.ImageSurface (Cairo.Format.ARGB32, 1, 1);
				Cairo.Context c = new Cairo.Context (sf);
				ctx.Context = c;
				ctx.TempSurface = sf;
			} else {
				ctx.Context = Gdk.CairoHelper.Create (GdkWindow);
			}
			if (!VisibleWindow) {
				ctx.Context.Translate (Allocation.X, Allocation.Y);
				// Set ContextBackend Origin
				ctx.Origin.X = Allocation.X;
				ctx.Origin.Y = Allocation.Y;
			}
			return ctx;
		}
	}
}

