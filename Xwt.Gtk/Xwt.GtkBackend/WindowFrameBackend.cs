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
using Xwt.Drawing;

namespace Xwt.GtkBackend
{
	public class WindowFrameBackend: IWindowFrameBackend
	{
		Gtk.Window window;
		IWindowFrameEventSink eventSink;
		WindowFrame frontend;
		Size requestedSize;

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
		
		void IBackend.InitializeBackend (object frontend)
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
			Window.Child.SizeRequested += delegate(object o, Gtk.SizeRequestedArgs args) {
				if (!Window.Resizable) {
					var r = args.Requisition;
					if (args.Requisition.Width < (int) requestedSize.Width)
						r = new Gtk.Requisition () { Width = (int) requestedSize.Width, Height = args.Requisition.Height};
					if (args.Requisition.Height < (int) requestedSize.Height)
						r = new Gtk.Requisition () { Width = args.Requisition.Width, Height = (int) requestedSize.Height};
					args.Requisition = r;
				}
			};
		}
		
		public virtual void Initialize ()
		{
		}
		
		public virtual void Dispose ()
		{
			Window.Destroy ();
		}
		
		public IWindowFrameEventSink EventSink {
			get { return eventSink; }
		}

		public void Move (double x, double y)
		{
			Window.Move ((int)x, (int)y);
			Toolkit.Invoke (delegate {
				EventSink.OnBoundsChanged (Bounds);
			});
		}

		public void Resize (double width, double height)
		{
			requestedSize = new Size (width, height);
			Window.Resize ((int)width, (int)height);
			Window.SetDefaultSize ((int)width, (int)height);
			Toolkit.Invoke (delegate {
				EventSink.OnBoundsChanged (Bounds);
			});
		}

		public Rectangle Bounds {
			get {
				int w, h, x, y;
				Window.GetPosition (out x, out y);
				Window.GetSize (out w, out h);
				return new Rectangle (x, y, w, h);
			}
			set {
				requestedSize = value.Size;
				Window.Move ((int)value.X, (int)value.Y);
				Window.Resize ((int)value.Width, (int)value.Height);
				Window.SetDefaultSize ((int)value.Width, (int)value.Height);
//				Window.SetSizeRequest ((int)value.Width, (int)value.Height);
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

		void IWindowFrameBackend.SetTransientFor (IWindowFrameBackend window)
		{
			Window.TransientFor = ((WindowFrameBackend)window).Window;
		}

		public bool Resizable {
			get {
				return Window.Resizable;
			}
			set {
				Window.Resizable = value;
			}
		}

		public void SetIcon(object backendImage)
		{
			Window.Icon = backendImage as Gdk.Pixbuf;
		}
		#endregion

		public virtual void EnableEvent (object ev)
		{
			if (ev is WindowFrameEvent) {
				switch ((WindowFrameEvent)ev) {
				case WindowFrameEvent.BoundsChanged:
					Window.SizeAllocated += HandleWidgetSizeAllocated; break;
				case WindowFrameEvent.CloseRequested:
					Window.DeleteEvent += HandleCloseRequested; break;
				}
			}
		}

		public virtual void DisableEvent (object ev)
		{
			if (ev is WindowFrameEvent) {
				switch ((WindowFrameEvent)ev) {
				case WindowFrameEvent.BoundsChanged:
					Window.SizeAllocated -= HandleWidgetSizeAllocated; break;
				case WindowFrameEvent.CloseRequested:
					Window.DeleteEvent -= HandleCloseRequested; break;
				}
			}
		}

		void HandleWidgetSizeAllocated (object o, Gtk.SizeAllocatedArgs args)
		{
			Toolkit.Invoke (delegate {
				EventSink.OnBoundsChanged (Bounds);
			});
		}

		void HandleCloseRequested (object o, Gtk.DeleteEventArgs args)
		{
			Toolkit.Invoke(delegate {
				args.RetVal = EventSink.OnCloseRequested ();
			});
		}

		public void Present ()
		{
			if (Platform.IsMac)
				GtkWorkarounds.GrabDesktopFocus ();
			Window.Present ();
		}

		public virtual Size ImplicitMinSize {
			get { return new Size (0,0); }
		}
	}
}

