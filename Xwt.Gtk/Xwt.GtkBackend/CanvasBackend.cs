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
using Xwt.Backends;

namespace Xwt.GtkBackend
{
	public class CanvasBackend<T,S>: WidgetBackend<T, S>, ICanvasBackend where T:Gtk.DrawingArea where S:ICanvasEventSink
	{
		public CanvasBackend ()
		{
		}
		
		public override void Initialize ()
		{
			Widget = (T) new Gtk.DrawingArea ();
			Widget.Events |= Gdk.EventMask.ButtonPressMask | Gdk.EventMask.ButtonReleaseMask | Gdk.EventMask.PointerMotionMask;
			Widget.ExposeEvent += HandleWidgetExposeEvent;
			Widget.ButtonPressEvent += HandleWidgetButtonPressEvent;
			Widget.ButtonReleaseEvent += HandleWidgetButtonReleaseEvent;
			Widget.MotionNotifyEvent += HandleWidgetMotionNotifyEvent;
			Widget.SizeAllocated += HandleWidgetSizeAllocated;
			Widget.Show ();
		}
		
		public void QueueDraw ()
		{
			Widget.QueueDraw ();
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
			EventSink.OnButtonPressed (a);
		}

		void HandleWidgetExposeEvent (object o, Gtk.ExposeEventArgs args)
		{
			EventSink.OnDraw (null);
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
	}
}

