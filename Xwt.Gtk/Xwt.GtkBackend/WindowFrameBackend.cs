// 
// WindowFrameBa.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//       Konrad M. Kruczynski <kkruczynski@antmicro.com>
// 
// Copyright (c) 2011 Xamarin Inc
// Copyright (c) 2016 Antmicro Ltd
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
using System.Linq;

namespace Xwt.GtkBackend
{
	public class WindowFrameBackend: IWindowFrameBackend
	{
		Gtk.Window window;
		IntPtr nativeHandle = IntPtr.Zero;
		IWindowFrameEventSink eventSink;
		WindowFrame frontend;
		Size requestedSize;

		public WindowFrameBackend ()
		{
		}
		
		public Gtk.Window Window {
			get { return window; }
			set {
				if (window != null)
					window.Realized -= HandleRealized;
				window = value;
				window.Realized += HandleRealized;
			}
		}

		object IWindowFrameBackend.Window {
			get { return Window; }
		}

		public IntPtr NativeHandle {
			get {
				if (nativeHandle == IntPtr.Zero)
					nativeHandle = GtkWorkarounds.GetGtkWindowNativeHandle (Window);
				return nativeHandle;
			}
		}

		void HandleRealized (object sender, EventArgs e)
		{
			if (opacity != 1d)
				window.GdkWindow.Opacity = opacity;
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

			#if !XWT_GTK3
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
			#endif
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

		public virtual void SetSize (double width, double height)
		{
			Window.SetDefaultSize ((int)width, (int)height);
			if (width == -1)
				width = Bounds.Width;
			if (height == -1)
				height = Bounds.Height;
			requestedSize = new Size (width, height);
			Window.Resize ((int)width, (int)height);
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

		public Size RequestedSize {
			get { return requestedSize; }
		}

		string IWindowFrameBackend.Name {
			get { return Window.Name; }
			set { Window.Name = value; }
		}

		bool IWindowFrameBackend.Visible {
			get {
				return window.Visible;
			}
			set {
				window.Visible = value;
			}
		}

		bool IWindowFrameBackend.Sensitive {
			get {
				return window.Sensitive;
			}
			set {
				window.Sensitive = value;
			}
		}

		double opacity = 1d;
		double IWindowFrameBackend.Opacity {
			get {
				return opacity;
			}
			set {
				opacity = value;
				if (Window.GdkWindow != null)
					Window.GdkWindow.Opacity = value;
			}
		}

		bool IWindowFrameBackend.HasFocus {
			get {
				return Window.HasToplevelFocus;
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
			Window.TransientFor = ApplicationContext.Toolkit.GetNativeWindow (window) as Gtk.Window;
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

		object IWindowFrameBackend.Screen {
			get {
				return Window.Screen.GetMonitorAtWindow (Window.GdkWindow);
			}
		}

		public void SetIcon(ImageDescription icon)
		{
			Window.IconList = ((GtkImage)icon.Backend).Frames.Select (f => f.Pixbuf).ToArray ();
		}
		#endregion

		public virtual void EnableEvent (object ev)
		{
			if (ev is WindowFrameEvent) {
				switch ((WindowFrameEvent)ev) {
				case WindowFrameEvent.BoundsChanged:
					Window.AddEvents ((int)Gdk.EventMask.StructureMask);
					Window.ConfigureEvent += HandleConfigureEvent; break;
				case WindowFrameEvent.Closed:
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
			args.RetVal = !PerformClose (true);
		}

		internal bool PerformClose (bool userClose)
		{
			bool close = false;
			ApplicationContext.InvokeUserCode(delegate {
				close = EventSink.OnCloseRequested ();
			});
			if (close) {
				if (!userClose)
					Window.Hide ();
				ApplicationContext.InvokeUserCode(EventSink.OnClosed);
			}
			return close;
		}

		public void Present ()
		{
			if (Platform.IsMac)
				GtkWorkarounds.GrabDesktopFocus ();
			Window.Present ();
		}

		public virtual bool Close ()
		{
			return PerformClose (false);
		}

		public virtual void GetMetrics (out Size minSize, out Size decorationSize)
		{
			minSize = decorationSize = Size.Zero;
		}
	}
}

