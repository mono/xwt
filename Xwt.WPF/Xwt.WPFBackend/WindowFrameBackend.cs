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
using Xwt.Engine;

namespace Xwt.WPFBackend
{
	public class WindowFrameBackend : IWindowFrameBackend
	{
		System.Windows.Window window;
		IWindowFrameEventSink eventSink;
		WindowFrame frontend;

		public WindowFrameBackend ()
		{
		}

		void IBackend.InitializeBackend (object frontend)
		{
			this.frontend = (WindowFrame) frontend;
		}

		void IWindowFrameBackend.Initialize (IWindowFrameEventSink eventSink)
		{
			this.eventSink = eventSink;
			Initialize ();
		}

		public virtual void Initialize ()
		{
		}

		public virtual void Dispose ()
		{	
			Window.Close ();
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
			set { window.WindowStyle = value ? WindowStyle.SingleBorderWindow : WindowStyle.None; }
		}

		bool IWindowFrameBackend.ShowInTaskbar {
			get { return window.ShowInTaskbar; }
			set { window.ShowInTaskbar = value; }
		}

		void IWindowFrameBackend.SetTransientFor (IWindowFrameBackend window)
		{
			this.Window.Owner = ((WindowFrameBackend) window).Window;
		}

		public void SetIcon (object imageBackend)
		{
			window.Icon = DataConverter.AsImageSource (imageBackend);
		}

		string IWindowFrameBackend.Title {
			get { return window.Title; }
			set { window.Title = value; }
		}

		bool IWindowFrameBackend.Visible
		{
			get { return window.Visibility == Visibility.Visible; }
			set { window.Visibility = value ? Visibility.Visible : Visibility.Hidden; }
		}

		void IWindowFrameBackend.Present ()
		{
			window.Activate ();
		}

		public void Move (double x, double y)
		{
			var value = ToNonClientRect (new Rectangle (x, y, 1, 1));
			window.Top = value.Top;
			window.Left = value.Left;
			Toolkit.Invoke (delegate
			{
				eventSink.OnBoundsChanged (Bounds);
			});
		}

		public void Resize (double width, double height)
		{
			var value = ToNonClientRect (new Rectangle (0, 0, width, height));
			window.Width = value.Width;
			window.Height = value.Height;
			Toolkit.Invoke (delegate
			{
				eventSink.OnBoundsChanged (Bounds);
			});
		}

		public Rectangle Bounds {
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
				Toolkit.Invoke (delegate {
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
				}
			}
		}

		void BoundsChangedHandler (object o, EventArgs args)
		{
			Toolkit.Invoke (delegate () {
				eventSink.OnBoundsChanged (Bounds);
			});
		}

		private void ShownHandler (object sender, DependencyPropertyChangedEventArgs e)
		{
			if((bool)e.NewValue)
			{
				Toolkit.Invoke (delegate ()
				{
					eventSink.OnShown ();
				});
			}
		}

		private void HiddenHandler (object sender, DependencyPropertyChangedEventArgs e)
		{
			if((bool)e.NewValue == false)
			{
				Toolkit.Invoke (delegate ()
				{
					eventSink.OnHidden ();
				});
			}
		}

		private void ClosingHandler (object sender, System.ComponentModel.CancelEventArgs e)
		{
			Toolkit.Invoke (delegate ()
			{
				e.Cancel = eventSink.OnCloseRequested ();
			});
		}

		protected Rectangle ToNonClientRect (Rectangle rect)
		{
			var size = rect.Size;
			var loc = rect.Location;

			size.Height += SystemParameters.ResizeFrameHorizontalBorderHeight * 2;
			size.Width += SystemParameters.ResizeFrameVerticalBorderWidth * 2;
			loc.X -= SystemParameters.ResizeFrameVerticalBorderWidth;
			loc.Y -= SystemParameters.ResizeFrameHorizontalBorderHeight;

			if (((IWindowFrameBackend)this).Decorated) {
				size.Height += SystemParameters.WindowCaptionHeight;
				loc.Y -= SystemParameters.CaptionHeight;
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

			size.Height -= SystemParameters.ResizeFrameHorizontalBorderHeight * 2;
			size.Width -= SystemParameters.ResizeFrameVerticalBorderWidth * 2;
			loc.X += SystemParameters.ResizeFrameVerticalBorderWidth;
			loc.Y += SystemParameters.ResizeFrameHorizontalBorderHeight;

			if (((IWindowFrameBackend)this).Decorated) {
				size.Height -= SystemParameters.CaptionHeight;
				loc.Y += SystemParameters.CaptionHeight;
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
