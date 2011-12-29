// 
// WindowFrameBa.cs
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
	public class WindowFrameBackend: IWindowFrameBackend
	{
		Gtk.Window window;
		IWindowFrameEventSink eventSink;
		WindowFrame frontend;
		
		public WindowFrameBackend ()
		{
		}
		
		public Gtk.Window Window {
			get { return window; }
			set { window = value; }
		}
		
		protected WindowFrame Frontend {
			get { return frontend; }
		}
		
		void IBackend.Initialize (object frontend)
		{
			this.frontend = (WindowFrame) frontend;
		}

		public virtual void ReplaceChild (Gtk.Widget oldWidget, Gtk.Widget newWidget)
		{
			throw new NotSupportedException ();
		}
		
		#region IWindowFrameBackend implementation
		void IWindowFrameBackend.Initialize (IWindowFrameEventSink eventSink)
		{
			this.eventSink = eventSink;
			Initialize ();
		}
		
		public virtual void Initialize ()
		{
		}
		
		public virtual void Dispose (bool disposing)
		{
			if (disposing)
				Window.Destroy ();
		}
		
		public IWindowFrameEventSink EventSink {
			get { return eventSink; }
		}

		public Rectangle Bounds {
			get {
				int w, h, x, y;
				Window.GetPosition (out x, out y);
				Window.GetSize (out w, out h);
				return new Rectangle (x, y, w, h);
			}
			set {
				Window.Move ((int)value.X, (int)value.Y);
				Window.Resize ((int)value.Width, (int)value.Height);
				Window.SetDefaultSize ((int)value.Width, (int)value.Height);
				Toolkit.Invoke (delegate {
					EventSink.OnBoundsChanged (Bounds);
				});
			}
		}

		bool IWindowFrameBackend.Visible {
			get {
				return window.Visible;
			}
			set {
				window.Visible = value;
			}
		}

		string IWindowFrameBackend.Title {
			get { return Window.Title; }
			set { Window.Title = value; }
		}

		bool IWindowFrameBackend.Decorated {
			get {
				return Window.Decorated;
			}
			set {
				Window.Decorated = value;
			}
		}

		bool IWindowFrameBackend.ShowInTaskbar {
			get {
				return !Window.SkipTaskbarHint;
			}
			set {
				Window.SkipTaskbarHint = !value;
			}
		}
		
		#endregion

		public virtual void EnableEvent (object ev)
		{
			if (ev is WindowFrameEvent) {
				switch ((WindowFrameEvent)ev) {
				case WindowFrameEvent.BoundsChanged:
					Window.SizeAllocated += HandleWidgetSizeAllocated; break;
				}
			}
		}

		public virtual void DisableEvent (object ev)
		{
			if (ev is WindowFrameEvent) {
				switch ((WindowFrameEvent)ev) {
				case WindowFrameEvent.BoundsChanged:
					Window.SizeAllocated -= HandleWidgetSizeAllocated; break;
				}
			}
		}

		void HandleWidgetSizeAllocated (object o, Gtk.SizeAllocatedArgs args)
		{
			Toolkit.Invoke (delegate {
				EventSink.OnBoundsChanged (new Rectangle (args.Allocation.X, args.Allocation.Y, args.Allocation.Width, args.Allocation.Height));
			});
		}
	}
}

