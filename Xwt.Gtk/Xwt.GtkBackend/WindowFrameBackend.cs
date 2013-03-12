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
		
		public ApplicationContext ApplicationContext {
			get;
			private set;
		}

		void IBackend.InitializeBackend (object frontend, ApplicationContext context)
		{
			this.frontend = (WindowFrame) frontend;
			ApplicationContext = context;
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
			Window.SizeRequested += delegate(object o, Gtk.SizeRequestedArgs args) {
				if (!Window.Resizable) {
					int w = args.Requisition.Width, h = args.Requisition.Height;
					if (w < (int) requestedSize.Width)
						w = (int) requestedSize.Width;
					if (h < (int) requestedSize.Height)
						h = (int) requestedSize.Height;
					args.Requisition = new Gtk.Requisition () { Width = w, Height = h };
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
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnBoundsChanged (Bounds);
			});
		}

		public void Resize (double width, double height)
		{
			requestedSize = new Size (width, height);
			Window.Resize ((int)width, (int)height);
			Window.SetDefaultSize ((int)width, (int)height);
		}

		public Rectangle Bounds {
			get {
				int w, h, x, y;
				if (Window.GdkWindow != null) {
					Window.GdkWindow.GetOrigin (out x, out y);
					Window.GdkWindow.GetSize (out w, out h);
				} else {
					Window.GetPosition (out x, out y);
					Window.GetSize (out w, out h);
				}
				return new Rectangle (x, y, w, h);
			}
			set {
				requestedSize = value.Size;
				Window.Move ((int)value.X, (int)value.Y);
				Window.Resize ((int)value.Width, (int)value.Height);
				Window.SetDefaultSize ((int)value.Width, (int)value.Height);
				ApplicationContext.InvokeUserCode (delegate {
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

		bool fullScreen;
		bool IWindowFrameBackend.FullScreen {
			get {
				return fullScreen;
			}
			set {
				if (value != fullScreen) {
					fullScreen = value;
					if (fullScreen)
						Window.Fullscreen ();
					else
						Window.Unfullscreen ();
				}
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
					Window.AddEvents ((int)Gdk.EventMask.StructureMask);
					Window.ConfigureEvent += HandleConfigureEvent; break;
				case WindowFrameEvent.CloseRequested:
					Window.DeleteEvent += HandleCloseRequested; break;
				case WindowFrameEvent.Shown:
					Window.Shown += HandleShown; break;
				case WindowFrameEvent.Hidden:
					Window.Hidden += HandleHidden; break;
				}
			}
		}

		public virtual void DisableEvent (object ev)
		{
			if (ev is WindowFrameEvent) {
				switch ((WindowFrameEvent)ev) {
				case WindowFrameEvent.BoundsChanged:
					Window.ConfigureEvent -= HandleConfigureEvent; break;
				case WindowFrameEvent.CloseRequested:
					Window.DeleteEvent -= HandleCloseRequested; break;
				case WindowFrameEvent.Shown:
					Window.Shown -= HandleShown; break;
				case WindowFrameEvent.Hidden:
					Window.Hidden -= HandleHidden; break;
				}
			}
		}
		
		void HandleHidden (object sender, EventArgs e)
		{
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnHidden ();
			});
		}

		void HandleShown (object sender, EventArgs e)
		{
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnShown ();
			});
		}

		[GLib.ConnectBefore]
		void HandleConfigureEvent (object o, Gtk.ConfigureEventArgs args)
		{
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnBoundsChanged (Bounds);
			});
		}

		void HandleCloseRequested (object o, Gtk.DeleteEventArgs args)
		{
			ApplicationContext.InvokeUserCode(delegate {
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

