//
// WindowFrameBackend.cs
//
// Author:
//       Vsevolod Kukol <sevoku@microsoft.com>
//
// Copyright (c) 2019 (c) Microsoft Corporation
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
using System.Drawing;
using System.Linq;
using AppKit;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using Xwt.Backends;

namespace Xwt.Mac
{
	public class WindowFrameBackend : IMacWindowBackend
	{
		IWindowFrameEventSink eventSink;
		WindowFrame frontend;
		bool hasExternalDelegate;

		public WindowFrameBackend ()
		{
		}

		public WindowFrameBackend (NSWindow window)
		{
			Window = window;
			// don't replace existing delegates
			hasExternalDelegate = Window.Delegate != null || Window.WeakDelegate != null;
		}

		public NSWindow Window { get; set; }

		object IWindowFrameBackend.Window {
			get { return Window; }
		}

		public IntPtr NativeHandle {
			get { return Window?.Handle ?? IntPtr.Zero; }
		}

		public IWindowFrameEventSink EventSink {
			get { return (IWindowFrameEventSink)eventSink; }
		}

		public virtual void InitializeBackend (object frontend, ApplicationContext context)
		{
			ApplicationContext = context;
			this.frontend = (WindowFrame)frontend;
		}

		public void Initialize (IWindowFrameEventSink eventSink)
		{
			this.eventSink = eventSink;
		}

		public ApplicationContext ApplicationContext {
			get;
			private set;
		}

		public string Name { get; set; }

		public string Title {
			get { return Window.Title; }
			set { Window.Title = value; }
		}

		void IMacWindowBackend.InternalShow ()
		{
			InternalShow ();
		}

		internal void InternalShow ()
		{
			Window.MakeKeyAndOrderFront (MacEngine.App);
			var parentWindow = Window.ParentWindow;

			if (parentWindow != null && Visible)
			{
				//if there is any child window we remove it
				if (!parentWindow.ChildWindows.Contains (Window))
					parentWindow.AddChildWindow (Window, NSWindowOrderingMode.Above);
			}

			//we center in any case
			Util.CenterWindow(Window, parentWindow);
		}

		public void Present ()
		{
			InternalShow ();
		}

		public bool Visible {
			get {
				return Window.IsVisible;
			}
			set {
				if (value)
					MacEngine.App.ShowWindow (this);
				Window.IsVisible = value;
			}
		}

		public double Opacity {
			get { return Window.AlphaValue; }
			set { Window.AlphaValue = (float)value; }
		}

		public bool Sensitive { // TODO
			get;
			set;
		}

		public bool HasFocus {
			get {
				return Window.IsKeyWindow;
			}
		}

		public bool FullScreen {
			get {
				if (MacSystemInformation.OsVersion < MacSystemInformation.Lion)
					return false;

				return (Window.StyleMask & NSWindowStyle.FullScreenWindow) != 0;

			}
			set {
				if (MacSystemInformation.OsVersion < MacSystemInformation.Lion)
					return;

				if (value != ((Window.StyleMask & NSWindowStyle.FullScreenWindow) != 0))
					Window.ToggleFullScreen (null);
			}
		}

		object IWindowFrameBackend.Screen {
			get {
				return Window.Screen;
			}
		}

		WindowFrameEvent eventsEnabled;
		NSObject didResizeObserver, didMoveObserver, willCloseObserver;

		void IBackend.EnableEvent (object eventId)
		{
			if (eventId is WindowFrameEvent) {
				var @event = (WindowFrameEvent)eventId;
				switch (@event) {
				case WindowFrameEvent.BoundsChanged:
					didResizeObserver = NSNotificationCenter.DefaultCenter.AddObserver (NSWindow.ResizedNotification, HandleDidResize, Window);
					didMoveObserver = NSNotificationCenter.DefaultCenter.AddObserver (NSWindow.MovedNotification, HandleDidResize, Window);
					break;
				case WindowFrameEvent.Hidden:
					EnableVisibilityEvent (@event);
					willCloseObserver = NSNotificationCenter.DefaultCenter.AddObserver (NSWindow.WillCloseNotification, OnWillClose, Window);
					break;
				case WindowFrameEvent.Shown:
					EnableVisibilityEvent (@event);
					break;
				case WindowFrameEvent.CloseRequested:
					// NOTE: this works only if the wrapped window has no delegate
					if (!hasExternalDelegate) // don't replace existing delegates
						Window.WindowShouldClose = OnShouldClose;
					else
						return; // skip eventEnabled update to avoid touching WindowShouldClose when disabling events
					break;
				}
				eventsEnabled |= @event;
			}
		}

		void OnWillClose (NSNotification note)
		{
			OnHidden ();
		}

		bool OnShouldClose (NSObject ob)
		{
			return closePerformed = RequestClose ();
		}

		internal bool RequestClose ()
		{
			bool res = true;
			ApplicationContext.InvokeUserCode (() => res = eventSink.OnCloseRequested ());
			return res;
		}

		protected virtual void OnClosed ()
		{
			ApplicationContext.InvokeUserCode (eventSink.OnClosed);
		}

		bool closePerformed;

		bool IWindowFrameBackend.Close ()
		{
			closePerformed = true;
			if ((Window.StyleMask & NSWindowStyle.Titled) != 0 && (Window.StyleMask & NSWindowStyle.Closable) != 0)
				Window.PerformClose (Window);
			else
				Window.Close ();
			if (Window.ParentWindow != null)
				Window.ParentWindow.RemoveChildWindow (Window);
			return closePerformed;
		}

		bool VisibilityEventsEnabled ()
		{
			return eventsEnabled.HasFlag (WindowFrameEvent.Hidden) || eventsEnabled.HasFlag (WindowFrameEvent.Shown);
		}

		NSString HiddenProperty {
			get { return new NSString ("hidden"); }
		}

		void EnableVisibilityEvent (WindowFrameEvent ev)
		{
			if (!VisibilityEventsEnabled ()) {
				// TODO
			}
		}

		void HandleContentViewVisiblityChanged ()
		{
			if (Window.ContentView.Hidden) {
				if (eventsEnabled.HasFlag (WindowFrameEvent.Hidden)) {
					OnHidden ();
				}
			} else {
				if (eventsEnabled.HasFlag (WindowFrameEvent.Shown)) {
					OnShown ();
				}
			}
		}

		void DisableVisibilityEvent ()
		{
			if (!VisibilityEventsEnabled ()) {
				// TODO
			}
		}

		void OnHidden ()
		{
			ApplicationContext.InvokeUserCode (eventSink.OnHidden);
		}

		void OnShown ()
		{
			ApplicationContext.InvokeUserCode (eventSink.OnShown);
		}

		void IBackend.DisableEvent (object eventId)
		{
			if (eventId is WindowFrameEvent) {
				var @event = (WindowFrameEvent)eventId;
				eventsEnabled &= ~@event;
				switch (@event) {
				case WindowFrameEvent.BoundsChanged:
					NSNotificationCenter.DefaultCenter.RemoveObservers (new [] { didResizeObserver, didMoveObserver });
					break;
				case WindowFrameEvent.Hidden:
					NSNotificationCenter.DefaultCenter.RemoveObserver (willCloseObserver);
					DisableVisibilityEvent ();
					break;
				case WindowFrameEvent.Shown:
					DisableVisibilityEvent ();
					break;
				}
			}
		}

		void HandleDidResize (NSNotification note)
		{
			OnBoundsChanged ();
		}

		protected virtual void OnBoundsChanged ()
		{
			ApplicationContext.InvokeUserCode (delegate {
				eventSink.OnBoundsChanged (((IWindowBackend)this).Bounds);
			});
		}

		bool IWindowFrameBackend.Decorated {
			get {
				return (Window.StyleMask & NSWindowStyle.Titled) != 0;
			}
			set {
				if (value)
					Window.StyleMask |= NSWindowStyle.Titled;
				else
					Window.StyleMask &= ~(NSWindowStyle.Titled | NSWindowStyle.Borderless);
			}
		}

		bool IWindowFrameBackend.ShowInTaskbar {
			get {
				return false;
			}
			set {
			}
		}

		void IWindowFrameBackend.SetTransientFor (IWindowFrameBackend parent)
		{
			//TODO: why this?
			if (!((IWindowFrameBackend)this).ShowInTaskbar)
				Window.StyleMask &= ~NSWindowStyle.Miniaturizable;

			//we try to get the native object from the parameter if not we fallback into the real parent
			NSWindow nParent = (ApplicationContext.Toolkit.GetNativeWindow(parent) as NSWindow) ?? Window.ParentWindow;
			if (nParent != Window.ParentWindow) {
				Window.ParentWindow = nParent;
			}
		}

		bool IWindowFrameBackend.Resizable {
			get {
				return (Window.StyleMask & NSWindowStyle.Resizable) != 0;
			}
			set {
				if (value)
					Window.StyleMask |= NSWindowStyle.Resizable;
				else
					Window.StyleMask &= ~NSWindowStyle.Resizable;
			}
		}

		void IWindowFrameBackend.Move (double x, double y)
		{
			var r = Window.FrameRectFor (new CGRect ((nfloat)x, (nfloat)y, Window.Frame.Width, Window.Frame.Height));
			Window.SetFrame (r, true);
		}

		void IWindowFrameBackend.SetSize (double width, double height)
		{
			var cr = Window.ContentRectFor (Window.Frame);
			if (width <= -1)
				width = cr.Width;
			if (height <= -1)
				height = cr.Height;
			var r = Window.FrameRectFor (new CGRect (cr.X, cr.Y, (nfloat)width, (nfloat)height));
			Window.SetFrame (r, true);
		}

		Rectangle IWindowFrameBackend.Bounds {
			get {
				var b = Window.ContentRectFor (Window.Frame);
				var r = MacDesktopBackend.ToDesktopRect (b);
				return new Rectangle ((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height);
			}
			set {
				var r = MacDesktopBackend.FromDesktopRect (value);
				var fr = Window.FrameRectFor (r);
				Window.SetFrame (fr, true);
			}
		}

		bool disposed;

		public void Dispose ()
		{
			if (!disposed && Window != null) {
				if (eventsEnabled.HasFlag (WindowFrameEvent.BoundsChanged)) {
					NSNotificationCenter.DefaultCenter.RemoveObservers (new [] { didResizeObserver, didMoveObserver });
				}
				if (eventsEnabled.HasFlag (WindowFrameEvent.Hidden)) {
					DisableVisibilityEvent ();
					NSNotificationCenter.DefaultCenter.RemoveObserver (willCloseObserver);
				}
				if (eventsEnabled.HasFlag (WindowFrameEvent.Shown)) {
					DisableVisibilityEvent ();
				}
				if (eventsEnabled.HasFlag (WindowFrameEvent.CloseRequested)) {
					Window.WindowShouldClose = null;
				}
			}
			Window = null;
			disposed = true;
		}

		public void SetIcon (ImageDescription icon)
		{
		}
	}

	interface IMacWindowBackend : IWindowFrameBackend
	{
		void InternalShow ();
	}
}
