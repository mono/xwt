﻿// 
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

		void IBackend.Initialize (object frontend)
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

		public virtual void Dispose (bool disposing)
		{
			if (disposing)
				Window.Close ();
		}

		public System.Windows.Window Window {
			get { return window; }
			set { window = value; }
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

		string IWindowFrameBackend.Title {
			get { return window.Title; }
			set { window.Title = value; }
		}

		bool IWindowFrameBackend.Visible
		{
			get { return window.Visibility == Visibility.Visible; }
			set { window.Visibility = value ? Visibility.Visible : Visibility.Hidden; }
		}

		public Rectangle Bounds {
			get {
				return new Rectangle (window.Left, window.Top, window.Width, window.Height);
			}
			set {
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
				}
			}
		}

		void BoundsChangedHandler (object o, EventArgs args)
		{
			Toolkit.Invoke (delegate () {
				eventSink.OnBoundsChanged (Bounds);
			});
		}
	}
}
