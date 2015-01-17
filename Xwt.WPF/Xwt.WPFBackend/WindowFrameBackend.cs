// 
// WindowFrameBackend.cs
//  
// Author:
//       Carlos Alberto Cortez <calberto.cortez@gmail.com>
// 
// Copyright (c) 2011 Carlos Alberto Cortez
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
using System.Linq;
using System.Text;
using System.Windows;

using Xwt.Backends;


namespace Xwt.WPFBackend
{
	public class WindowFrameBackend : IWindowFrameBackend
	{
		System.Windows.Window window;
		IWindowFrameEventSink eventSink;
		WindowFrame frontend;
		bool resizable = true;

		public WindowFrameBackend ()
		{
		}

		void IBackend.InitializeBackend (object frontend, ApplicationContext context)
		{
			this.frontend = (WindowFrame) frontend;
			Context = context;
		}

		void IWindowFrameBackend.Initialize (IWindowFrameEventSink eventSink)
		{
			this.eventSink = eventSink;
			Initialize ();
		}

		public ApplicationContext Context { get; private set; }

		public virtual void Initialize ()
		{
		}

		public virtual void Dispose ()
		{	
			Window.Close ();
		}

		public bool Close ()
		{
			closePerformed = true;
			Window.Close ();
			return closePerformed;
		}

		public System.Windows.Window Window {
			get { return window; }
			set { window = value; }
		}

		public virtual bool HasMenu {
			get { return false;  }
		}

		protected WindowFrame Frontend {
			get { return frontend; }
		}

		public IWindowFrameEventSink EventSink
		{
			get { return eventSink; }
		}

		bool IWindowFrameBackend.Decorated {
			get { return window.WindowStyle != WindowStyle.None; }
			set {
				window.WindowStyle = value ? WindowStyle.SingleBorderWindow : WindowStyle.None;
				UpdateResizeMode ();
			}
		}

		bool IWindowFrameBackend.ShowInTaskbar {
			get { return window.ShowInTaskbar; }
			set { window.ShowInTaskbar = value; }
		}

		void IWindowFrameBackend.SetTransientFor (IWindowFrameBackend window)
		{
			this.Window.Owner = ((WindowFrameBackend) window).Window;
		}

		bool IWindowFrameBackend.Resizable {
			get {
				return resizable;
			}
			set {
				if (value != resizable) {
					resizable = value;
					UpdateResizeMode ();
					var bounds = Bounds;
					window.ResizeMode = value ? ResizeMode.CanResize : ResizeMode.NoResize;
					if (window.IsLoaded && bounds != Bounds) {
						// The size of the border of resizable windows is different from fixed windows.
						// If we change the resize mode, the border size will change, and the client
						// area will then change. Here, we restore the client area to the size it
						// had before the mode change.
						Bounds = bounds;
					}
				}
			}
		}

		void UpdateResizeMode ()
		{
			var m = resizable && window.WindowStyle == WindowStyle.SingleBorderWindow ? ResizeMode.CanResize : ResizeMode.NoResize;
			if (m != window.ResizeMode) {
				window.ResizeMode = m;
				OnResizeModeChanged ();
			}
		}

		protected virtual void OnResizeModeChanged ()
		{
		}

		public void SetIcon (ImageDescription imageBackend)
		{
			window.Icon = imageBackend.ToImageSource ();
		}

		string IWindowFrameBackend.Title {
			get { return window.Title; }
			set { window.Title = value; }
		}

		bool IWindowFrameBackend.Visible
		{
			get { return window.Visibility == Visibility.Visible; }
			set {
				if (value)
					window.Show ();
				else
					window.Hide ();
			}
		}

		bool IWindowFrameBackend.Sensitive
		{
			get { return window.IsEnabled; }
			set { window.IsEnabled = value;	}
		}

		public double Opacity
		{
			get { return window.Opacity; }
			set { window.Opacity = value; }
		}

		void IWindowFrameBackend.Present ()
		{
			window.Activate ();
		}
		
		bool IWindowFrameBackend.FullScreen {
			get {
				return window.WindowState == WindowState.Maximized 
					&& window.ResizeMode == ResizeMode.NoResize;
			}
			set {
				if (value) {
					window.WindowState = WindowState.Maximized;
					window.ResizeMode = ResizeMode.NoResize;
				}
				else {
					window.WindowState = WindowState.Normal;
					window.ResizeMode = ResizeMode.CanResize;
				}
			}
		}

		object IWindowFrameBackend.Screen {
			get {
				var sb = Bounds;
				return System.Windows.Forms.Screen.FromRectangle (new System.Drawing.Rectangle ((int)sb.X, (int)sb.Y, (int)sb.Width, (int)sb.Height));
			}
		}

		public void Move (double x, double y)
		{
			var value = ToNonClientRect (new Rectangle (x, y, 1, 1));
			window.Top = value.Top;
			window.Left = value.Left;
			Context.InvokeUserCode (delegate
			{
				eventSink.OnBoundsChanged (Bounds);
			});
		}

		public void SetSize (double width, double height)
		{
			var r = Bounds;
			r.Width = width;
			r.Height = height;
			Bounds = r;
		}

		public virtual Rectangle Bounds {
			get {
				double width = Double.IsNaN (window.Width) ? window.ActualWidth : window.Width;
				double height = Double.IsNaN (window.Height) ? window.ActualHeight : window.Height;
				return ToClientRect (new Rectangle (window.Left, window.Top, width, height));
			}
			set {
				value = ToNonClientRect (value);
				window.Top = value.Top;
				window.Left = value.Left;
				window.Width = value.Width;
				window.Height = value.Height;
				Context.InvokeUserCode (delegate {
					eventSink.OnBoundsChanged (Bounds);
				});
			}
		}

		public virtual void EnableEvent (object eventId)
		{
			if (eventId is WindowFrameEvent) {
				switch ((WindowFrameEvent)eventId) {
					case WindowFrameEvent.BoundsChanged:
						window.LocationChanged += BoundsChangedHandler;
						window.SizeChanged += BoundsChangedHandler;
						break;
					case WindowFrameEvent.Shown:
						window.IsVisibleChanged += ShownHandler;
						break;
					case WindowFrameEvent.Hidden:
						window.IsVisibleChanged += HiddenHandler;
						break;
					case WindowFrameEvent.CloseRequested:
						window.Closing += ClosingHandler;
						break;
					case WindowFrameEvent.Closed:
						window.Closed += ClosedHandler;
						break;
				}
			}
		}

		public virtual void DisableEvent (object eventId)
		{
			if (eventId is WindowFrameEvent) {
				switch ((WindowFrameEvent)eventId) {
					case WindowFrameEvent.BoundsChanged:
						window.LocationChanged -= BoundsChangedHandler;
						window.SizeChanged -= BoundsChangedHandler;
						break;
					case WindowFrameEvent.Shown:
						window.IsVisibleChanged -= ShownHandler;
						break;
					case WindowFrameEvent.Hidden:
						window.IsVisibleChanged -= HiddenHandler;
						break;
					case WindowFrameEvent.CloseRequested:
						window.Closing -= ClosingHandler;
						break;
					case WindowFrameEvent.Closed:
						window.Closing -= ClosedHandler;
						break;
				}
			}
		}

		private void ClosedHandler (object sender, EventArgs e)
		{
			if (!InhibitCloseRequested)
				Context.InvokeUserCode (eventSink.OnClosed);
		}

		void BoundsChangedHandler (object o, EventArgs args)
		{
			Context.InvokeUserCode (delegate () {
				eventSink.OnBoundsChanged (Bounds);
			});
		}

		private void ShownHandler (object sender, DependencyPropertyChangedEventArgs e)
		{
			// delay shown event until window is loaded
			if (!window.IsLoaded) {
				window.Loaded += (sender2, e2) => ShownHandler (sender, e);
				return;
			}
			if((bool)e.NewValue)
			{
				Context.InvokeUserCode (delegate ()
				{
					eventSink.OnShown ();
				});
			}
		}

		private void HiddenHandler (object sender, DependencyPropertyChangedEventArgs e)
		{
			if((bool)e.NewValue == false)
			{
				Context.InvokeUserCode (delegate ()
				{
					eventSink.OnHidden ();
				});
			}
		}

		protected bool InhibitCloseRequested { get; set; }

		bool closePerformed;

		private void ClosingHandler (object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (InhibitCloseRequested)
				return;
			Context.InvokeUserCode (delegate ()
			{
				e.Cancel = !eventSink.OnCloseRequested ();
				closePerformed = !e.Cancel;
			});
		}

		Size GetBorderSize ()
		{
			if (window.ResizeMode == ResizeMode.CanResize)
				return new Size (SystemParameters.ResizeFrameVerticalBorderWidth, SystemParameters.ResizeFrameHorizontalBorderHeight);
			else
				return new Size (SystemParameters.FixedFrameVerticalBorderWidth, SystemParameters.FixedFrameHorizontalBorderHeight);
		}

		protected Rectangle ToNonClientRect (Rectangle rect)
		{
			// WARNING: SystemParameters.ResizeFrameHorizontalBorderHeight is known to return invalid values in some cases, due to
			// a workaround in the Windows API to support legacy applications running on Aero.
			// We can't rely then on ToNonClientRect and ToClientRect to return 100% correct values, so they are not used for calculating
			// the required client area. However, the result of those methods is good enough for calculating the position of the window.

			var size = rect.Size;
			var loc = rect.Location;

			var border = GetBorderSize ();
			size.Height += border.Height * 2;
			size.Width += border.Width * 2;
			loc.X -= border.Width;
			loc.Y -= border.Height;

			if (((IWindowFrameBackend)this).Decorated) {
				size.Height += SystemParameters.WindowCaptionHeight;
				loc.Y -= SystemParameters.WindowCaptionHeight;
			}
			if (HasMenu) {
				size.Height += SystemParameters.MenuBarHeight;
				loc.Y -= SystemParameters.MenuBarHeight;
			}

			return new Rectangle (loc, size);
		}

		protected Rectangle ToClientRect (Rectangle rect)
		{
			var size = rect.Size;
			var loc = rect.Location;

			var border = GetBorderSize ();
			size.Height -= border.Height * 2;
			size.Width -= border.Width * 2;
			loc.X += border.Width;
			loc.Y += border.Height;

			if (((IWindowFrameBackend)this).Decorated) {
                size.Height -= SystemParameters.WindowCaptionHeight;
                loc.Y += SystemParameters.WindowCaptionHeight;
			}
			if (HasMenu) {
				size.Height -= SystemParameters.MenuBarHeight;
				loc.Y += SystemParameters.MenuBarHeight;
			}

			size.Width = Math.Max (0, size.Width);
			size.Height = Math.Max (0, size.Height);

			return new Rectangle (loc, size);
		}
	}
}
