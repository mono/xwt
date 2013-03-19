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
			Widget = new CustomCanvas (this);
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
			if (Widget.SetAllocation (w, bounds) && !Widget.IsReallocating)
			    Widget.QueueResize ();
		}
	}
	
	class CustomCanvas: CustomContainer
	{
		public ICanvasEventSink EventSink;

		public CustomCanvas (WidgetBackend backend): base (backend)
		{
			GtkWorkarounds.FixContainerLeak (this);

			WidgetFlags |= Gtk.WidgetFlags.AppPaintable;
		}

		protected override bool OnExposeEvent (Gdk.EventExpose evnt)
		{
			Backend.ApplicationContext.InvokeUserCode (delegate {
				using (var context = CreateContext ()) {
					var a = evnt.Area;
					EventSink.OnDraw (context, new Rectangle (a.X, a.Y, a.Width, a.Height));
				}
			});
			return base.OnExposeEvent (evnt);
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
				ctx.Context.Translate (Allocation.X, Allocation.Y);
				ctx.Context.Rectangle (0, 0, Allocation.Width, Allocation.Height);
				ctx.Context.Clip ();
			}
			return ctx;
		}
	}
}

