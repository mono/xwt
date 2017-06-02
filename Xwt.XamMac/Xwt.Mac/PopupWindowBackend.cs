//
// PopupWindowBackend.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//       Andres G. Aragoneses <andres.aragoneses@7digital.com>
//       Konrad M. Kruczynski <kkruczynski@antmicro.com>
//       Vsevolod Kukol <sevoku@microsoft.com>
// 
// Copyright (c) 2011 Xamarin Inc
// Copyright (c) 2012 7Digital Media Ltd
// Copyright (c) 2016 Antmicro Ltd
// Copyright (c) 2017 (c) Microsoft Corporation
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
using AppKit;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using Xwt.Backends;
using Xwt.Drawing;

// TODO: Merge back with WindowBackend and reuse code

namespace Xwt.Mac
{
	public class PopupWindowBackend : NSPanel, IPopupWindowBackend, IUtilityWindowBackend
	{
		WindowBackendController controller;
		IWindowFrameEventSink eventSink;
		Window frontend;
		ViewBackend child;
		NSView childView;
		IWindowFrameBackend transientParent;
		bool sensitive = true;
		bool isPopup = false;
		PopupWindow.PopupType windowType;

		public PopupWindowBackend ()
		{
			AutorecalculatesKeyViewLoop = true;

			ContentView.AutoresizesSubviews = true;
			ContentView.Hidden = true;

			// TODO: do it only if mouse move events are enabled in a widget
			AcceptsMouseMovedEvents = true;

			WillClose += delegate {
				OnClosed ();
			};

		}

		public override bool CanBecomeKeyWindow {
			get {
				if (!isPopup)
					return true;
				return (windowType != PopupWindow.PopupType.Tooltip);
			}
		}

		object IWindowFrameBackend.Window {
			get { return this; }
		}

		public IntPtr NativeHandle {
			get { return Handle; }
		}

		public IWindowFrameEventSink EventSink {
			get { return (IWindowFrameEventSink)eventSink; }
		}

		public virtual void InitializeBackend (object frontend, ApplicationContext context)
		{
			this.ApplicationContext = context;
			this.frontend = (Window) frontend;
		}

		bool backendInitiaized;
		
		public void Initialize (IWindowFrameEventSink eventSink)
		{
			if (!backendInitiaized) {
				this.eventSink = eventSink;
				this.isPopup = false;
				UpdateWindowStyle();
				backendInitiaized = true;
			}
		}

		public void Initialize (IWindowFrameEventSink eventSink, PopupWindow.PopupType windowType)
		{
			if (!backendInitiaized) {
				this.eventSink = eventSink;
				this.isPopup = true;
				this.windowType = windowType;
				UpdateWindowStyle();
				backendInitiaized = true;
			}
		}

		void UpdateWindowStyle ()
		{
			StyleMask |= NSWindowStyle.Closable | NSWindowStyle.Miniaturizable | NSWindowStyle.Utility;

			if (!isPopup) {
				StyleMask |= NSWindowStyle.Resizable;
				TitleVisibility = NSWindowTitleVisibility.Visible;
				Level = NSWindowLevel.Floating;
				FloatingPanel = true;
			} else {
				StyleMask |= NSWindowStyle.FullSizeContentView;
				MovableByWindowBackground = true;
				TitlebarAppearsTransparent = true;
				TitleVisibility = NSWindowTitleVisibility.Hidden;
				this.StandardWindowButton (NSWindowButton.CloseButton).Hidden = true;
				this.StandardWindowButton (NSWindowButton.MiniaturizeButton).Hidden = true;
				this.StandardWindowButton (NSWindowButton.ZoomButton).Hidden = true;

				if (windowType == PopupWindow.PopupType.Tooltip)
					// NSWindowLevel.ScreenSaver overlaps menus, this allows showing tooltips above menus
					Level = NSWindowLevel.ScreenSaver;
				else
					Level = NSWindowLevel.PopUpMenu;
			}
		}

		public override void MouseExited (NSEvent theEvent)
		{
			if (windowType == PopupWindow.PopupType.Tooltip)
				Visible = false;
			base.MouseExited (theEvent);
		}

		public ApplicationContext ApplicationContext {
			get;
			private set;
		}

		public object NativeWidget {
			get {
				return this;
			}
		}

		public string Name { get; set; }

		public void Present ()
		{
			Visible = true;
			MakeKeyAndOrderFront (MacEngine.App);
		}

		public bool Visible {
			get {
				return IsVisible;
			}
			set {
				ContentView.Hidden = !value; // handle shown/hidden events
				IsVisible = value;

				if (transientParent != null) {
					var win = transientParent as NSWindow ?? ApplicationContext.Toolkit.GetNativeWindow (transientParent) as NSWindow;
					if (win != null) {
						if (value)
							win.AddChildWindow (this, NSWindowOrderingMode.Above);
						else
							win.RemoveChildWindow (this);
					}
				}
			}
		}

		public double Opacity {
			get { return AlphaValue; }
			set { AlphaValue = (float)value; }
		}

		Color? backgroundColor;
		public new Color BackgroundColor {
			get {
				if (backgroundColor.HasValue)
					return backgroundColor.Value;
				return base.BackgroundColor.ToXwtColor ();
			}
			set {
				backgroundColor = value;
				base.BackgroundColor = value.ToNSColor ();
				IsOpaque = false;
			}
		}

		public bool Sensitive {
			get {
				return sensitive;
			}
			set {
				sensitive = value;
				if (child != null)
					child.UpdateSensitiveStatus (child.Widget, sensitive);
			}
		}

		public bool HasFocus {
			get {
				return IsKeyWindow;
			}
		}

		public bool FullScreen {
			get {
				if (MacSystemInformation.OsVersion < MacSystemInformation.Lion)
					return false;

				return (StyleMask & NSWindowStyle.FullScreenWindow) != 0;

			}
			set {
				if (MacSystemInformation.OsVersion < MacSystemInformation.Lion)
					return;

				if (value != ((StyleMask & NSWindowStyle.FullScreenWindow) != 0))
					ToggleFullScreen (null);
			}
		}

		object IWindowFrameBackend.Screen {
			get {
				return Screen;
			}
		}

		#region IWindowBackend implementation
		void IBackend.EnableEvent (object eventId)
		{
			if (eventId is WindowFrameEvent) {
				var @event = (WindowFrameEvent)eventId;
				switch (@event) {
				case WindowFrameEvent.BoundsChanged:
					DidResize += HandleDidResize;
					DidMove += HandleDidResize;
					break;
				case WindowFrameEvent.Hidden:
					EnableVisibilityEvent (@event);
					this.WillClose += OnWillClose;
					break;
				case WindowFrameEvent.Shown:
					EnableVisibilityEvent (@event);
					break;
				case WindowFrameEvent.CloseRequested:
					WindowShouldClose = OnShouldClose;
					break;
				}
			}
		}
		
		void OnWillClose (object sender, EventArgs args) {
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
			if (!disposing)
				ApplicationContext.InvokeUserCode (eventSink.OnClosed);
		}

		bool closePerformed;

		bool IWindowFrameBackend.Close ()
		{
			closePerformed = true;
			if ((StyleMask & NSWindowStyle.Titled) != 0 && (StyleMask & NSWindowStyle.Closable) != 0)
				PerformClose(this);
			else
				Close ();
			return closePerformed;
		}
		
		bool VisibilityEventsEnabled ()
		{
			return eventsEnabled != WindowFrameEvent.BoundsChanged;
		}
		WindowFrameEvent eventsEnabled = WindowFrameEvent.BoundsChanged;

		NSString HiddenProperty {
			get { return new NSString ("hidden"); }
		}
		
		void EnableVisibilityEvent (WindowFrameEvent ev)
		{
			if (!VisibilityEventsEnabled ()) {
				ContentView.AddObserver (this, HiddenProperty, NSKeyValueObservingOptions.New, IntPtr.Zero);
			}
			if (!eventsEnabled.HasFlag (ev)) {
				eventsEnabled |= ev;
			}
		}

		public override void ObserveValue (NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
		{
			if (keyPath.ToString () == HiddenProperty.ToString () && ofObject.Equals (ContentView)) {
				if (ContentView.Hidden) {
					if (eventsEnabled.HasFlag (WindowFrameEvent.Hidden)) {
						OnHidden ();
					}
				} else {
					if (eventsEnabled.HasFlag (WindowFrameEvent.Shown)) {
						OnShown ();
					}
				}
			}
		}

		void OnHidden () {
			ApplicationContext.InvokeUserCode (delegate ()
			{
				eventSink.OnHidden ();
			});
		}

		void OnShown () {
			ApplicationContext.InvokeUserCode (delegate ()
			{
				eventSink.OnShown ();
			});
		}

		void DisableVisibilityEvent (WindowFrameEvent ev)
		{
			if (eventsEnabled.HasFlag (ev)) {
				eventsEnabled ^= ev;
				if (!VisibilityEventsEnabled ()) {
					ContentView.RemoveObserver (this, HiddenProperty);
				}
			}
		}

		void IBackend.DisableEvent (object eventId)
		{
			if (eventId is WindowFrameEvent) {
				var @event = (WindowFrameEvent)eventId;
				switch (@event) {
					case WindowFrameEvent.BoundsChanged:
						DidResize -= HandleDidResize;
						DidMove -= HandleDidResize;
						break;
					case WindowFrameEvent.Hidden:
						this.WillClose -= OnWillClose;
						DisableVisibilityEvent (@event);
						break;
					case WindowFrameEvent.Shown:
						DisableVisibilityEvent (@event);
						break;
				}
			}
		}

		void HandleDidResize (object sender, EventArgs e)
		{
			OnBoundsChanged ();
		}

		protected virtual void OnBoundsChanged ()
		{
			LayoutWindow ();
			ApplicationContext.InvokeUserCode (delegate {
				eventSink.OnBoundsChanged (((IWindowBackend)this).Bounds);
			});
		}

		void IWindowBackend.SetChild (IWidgetBackend child)
		{
			if (this.child != null) {
				ViewBackend.RemoveChildPlacement (this.child.Widget);
				this.child.Widget.RemoveFromSuperview ();
				childView = null;
			}
			this.child = (ViewBackend) child;
			if (child != null) {
				childView = ViewBackend.GetWidgetWithPlacement (child);
				ContentView.AddSubview (childView);
				LayoutWindow ();
				childView.AutoresizingMask = NSViewResizingMask.HeightSizable | NSViewResizingMask.WidthSizable;
			}
		}
		
		public virtual void UpdateChildPlacement (IWidgetBackend childBackend)
		{
			var w = ViewBackend.SetChildPlacement (childBackend);
			LayoutWindow ();
			w.AutoresizingMask = NSViewResizingMask.HeightSizable | NSViewResizingMask.WidthSizable;
		}

		bool IWindowFrameBackend.Decorated {
			get {
				return (StyleMask & NSWindowStyle.Titled) != 0;
			}
			set {
				if (value)
					StyleMask |= NSWindowStyle.Titled;
				else
					StyleMask &= ~(NSWindowStyle.Titled | NSWindowStyle.Borderless);
				HasShadow = value;
			}
		}
		
		bool IWindowFrameBackend.ShowInTaskbar {
			get {
				return false;
			}
			set {
			}
		}

		void IWindowFrameBackend.SetTransientFor (IWindowFrameBackend window)
		{
			if (transientParent == window)
				return;

			transientParent = window;

			if (!isPopup)
				Level = window == null ? NSWindowLevel.Floating : NSWindowLevel.ModalPanel;

			if (ParentWindow != null)
				ParentWindow.RemoveChildWindow (this);

			// add to parent now, only if already visible
			if (window != null && IsVisible) {
				var win = window as NSWindow ?? ApplicationContext.Toolkit.GetNativeWindow (window) as NSWindow;
				if (win != null)
					win.AddChildWindow (this, NSWindowOrderingMode.Above);
			}
		}

		bool IWindowFrameBackend.Resizable {
			get {
				return (StyleMask & NSWindowStyle.Resizable) != 0;
			}
			set {
				if (value)
					StyleMask |= NSWindowStyle.Resizable;
				else
					StyleMask &= ~NSWindowStyle.Resizable;
			}
		}
		
		public void SetPadding (double left, double top, double right, double bottom)
		{
			LayoutWindow ();
		}

		void IWindowFrameBackend.Move (double x, double y)
		{
			var r = MacDesktopBackend.FromDesktopRect (new Rectangle (x, y, Frame.Width, Frame.Height));
			r = FrameRectFor (r);
			SetFrameOrigin (r.Location);
		}
		
		void IWindowFrameBackend.SetSize (double width, double height)
		{
			var cr = ContentRectFor (Frame);
			if (width == -1)
				width = cr.Width;
			if (height == -1)
				height = cr.Height;
			var r = FrameRectFor (new CGRect ((nfloat)cr.X, (nfloat)cr.Y, (nfloat)width, (nfloat)height));
			SetFrame (r, true);
			LayoutWindow ();
		}
		
		Rectangle IWindowFrameBackend.Bounds {
			get {
				var b = ContentRectFor (Frame);
				var r = MacDesktopBackend.ToDesktopRect (b);
				return new Rectangle ((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height);
			}
			set {
				var r = MacDesktopBackend.FromDesktopRect (value);
				var fr = FrameRectFor (r);
				SetFrame (fr, true);
			}
		}
		
		public void SetMainMenu (IMenuBackend menu)
		{
			var m = (MenuBackend) menu;
			m.SetMainMenuMode ();
			NSApplication.SharedApplication.Menu = m;
			
//			base.Menu = m;
		}
		
		#endregion

		static Selector closeSel = new Selector ("close");
		#if MONOMAC
		static Selector retainSel = new Selector("retain");
		#endif

		bool disposing, disposed;

		protected override void Dispose(bool disposing)
		{
			if (!disposed && disposing)
			{
				this.disposing = true;
				try
				{
					if (VisibilityEventsEnabled() && ContentView != null)
						ContentView.RemoveObserver(this, HiddenProperty);
					
					// HACK: Xamarin.Mac/MonoMac limitation: no direct way to release a window manually
					// A NSWindow instance will be removed from NSApplication.SharedApplication.Windows
					// only if it is being closed with ReleasedWhenClosed set to true but not on Dispose
					// and there is no managed way to tell Cocoa to release the window manually (and to
					// remove it from the active window list).
					// see also: https://bugzilla.xamarin.com/show_bug.cgi?id=45298
					// WORKAROUND:
					// bump native reference count by calling DangerousRetain()
					// base.Dispose will now unref the window correctly without crashing
					DangerousRetain();
					// tell Cocoa to release the window on Close
					ReleasedWhenClosed = true;
					// Close the window (Cocoa will do its job even if the window is already closed)
					Messaging.void_objc_msgSend (this.Handle, closeSel.Handle);
				} finally {
					this.disposing = false;
					this.disposed = true;
				}
			}
			if (controller != null) {
				controller.Dispose ();
				controller = null;
			}
			base.Dispose (disposing);
		}
		
		public void DragStart (TransferDataSource data, DragDropAction dragAction, object dragImage, double xhot, double yhot)
		{
			throw new NotImplementedException ();
		}
		
		public void SetDragSource (string[] types, DragDropAction dragAction)
		{
		}
		
		public void SetDragTarget (string[] types, DragDropAction dragAction)
		{
		}
		
		public virtual void SetMinSize (Size s)
		{
			var b = ((IWindowBackend)this).Bounds;
			if (b.Size.Width < s.Width)
				b.Width = s.Width;
			if (b.Size.Height < s.Height)
				b.Height = s.Height;

			if (b != ((IWindowBackend)this).Bounds)
				((IWindowBackend)this).Bounds = b;

			var r = FrameRectFor (new CGRect (0, 0, (nfloat)s.Width, (nfloat)s.Height));
			MinSize = r.Size;
		}

		public void SetIcon (ImageDescription icon)
		{
		}
		
		public virtual void GetMetrics (out Size minSize, out Size decorationSize)
		{
			minSize = decorationSize = Size.Zero;
		}

		public virtual void LayoutWindow ()
		{
			LayoutContent (ContentView.Frame);
		}
		
		public void LayoutContent (CGRect frame)
		{
			if (child != null) {
				frame.X += (nfloat) frontend.Padding.Left;
				frame.Width -= (nfloat) (frontend.Padding.HorizontalSpacing);
				frame.Y += (nfloat) frontend.Padding.Top;
				frame.Height -= (nfloat) (frontend.Padding.VerticalSpacing);
				childView.Frame = frame;
			}
		}
	}
}

