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
			Widget.Frontend = Frontend;
			Widget.EventSink = EventSink;
			Widget.Events |= Gdk.EventMask.ButtonPressMask | Gdk.EventMask.ButtonReleaseMask | Gdk.EventMask.PointerMotionMask;
			Widget.ButtonPressEvent += HandleWidgetButtonPressEvent;
			Widget.ButtonReleaseEvent += HandleWidgetButtonReleaseEvent;
			Widget.MotionNotifyEvent += HandleWidgetMotionNotifyEvent;
			Widget.SizeAllocated += HandleWidgetSizeAllocated;
			Widget.SizeRequested += HandleSizeRequested;
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
		
		public void OnPreferredSizeChanged ()
		{
			Widget.QueueResize ();
		}

		void HandleSizeRequested (object o, Gtk.SizeRequestedArgs args)
		{
			IWidgetSurface ws = (IWidgetSurface)Frontend;
			Gtk.Requisition req = args.Requisition;
			int w = (int)ws.GetPreferredWidth ().MinSize;
			int h = (int)ws.GetPreferredHeight ().MinSize;
			if (req.Width < w)
				req.Width = w;
			if (req.Height < h)
				req.Height = h;
			args.Requisition = req;
		}
		
		void HandleWidgetMotionNotifyEvent (object o, Gtk.MotionNotifyEventArgs args)
		{
			var a = new MouseMovedEventArgs ();
			a.X = args.Event.X;
			a.Y = args.Event.Y;
			EventSink.OnMouseMoved (a);
		}

		void HandleWidgetButtonReleaseEvent (object o, Gtk.ButtonReleaseEventArgs args)
		{
			var a = new ButtonEventArgs ();
			a.X = args.Event.X;
			a.Y = args.Event.Y;
			a.Button = (int) args.Event.Button;
			EventSink.OnButtonReleased (a);
		}

		void HandleWidgetButtonPressEvent (object o, Gtk.ButtonPressEventArgs args)
		{
			var a = new ButtonEventArgs ();
			a.X = args.Event.X;
			a.Y = args.Event.Y;
			a.Button = (int) args.Event.Button;
			if (args.Event.Type == Gdk.EventType.TwoButtonPress)
				a.MultiplePress = 2;
			else if (args.Event.Type == Gdk.EventType.ThreeButtonPress)
				a.MultiplePress = 3;
			else
				a.MultiplePress = 1;
			EventSink.OnButtonPressed (a);
		}

		public Rectangle Bounds {
			get {
				return new Rectangle (0, 0, Widget.Allocation.Width, Widget.Allocation.Height);
			}
		}

		void HandleWidgetSizeAllocated (object o, Gtk.SizeAllocatedArgs args)
		{
			EventSink.OnBoundsChanged ();
		}
		
		public void AddChild (IWidgetBackend widget)
		{
			var w = ((IGtkWidgetBackend)widget).Widget;
			Widget.Add (w);
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
		public Widget Frontend;
		public ICanvasEventSink EventSink;
		Dictionary<Gtk.Widget, Rectangle> children = new Dictionary<Gtk.Widget, Rectangle> ();
		
		public CustomCanvas (IntPtr p): base (p)
		{
		}
		
		public CustomCanvas ()
		{
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
			IWidgetSurface ws = Frontend;
			int w = (int)ws.GetPreferredWidth ().MinSize;
			int h = (int)ws.GetPreferredHeight ().MinSize;
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
				((IWidgetSurface)Frontend).Reallocate ();
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
			EventSink.OnDraw (null);
			return base.OnExposeEvent (evnt);
		}
	}
}

