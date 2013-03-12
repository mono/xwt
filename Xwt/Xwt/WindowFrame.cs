// 
// WindowFrame.cs
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

// 
// WindowFrame.cs
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

using System.ComponentModel;
using Xwt.Drawing;

namespace Xwt
{
	[BackendType (typeof(IWindowFrameBackend))]
	public class WindowFrame: XwtComponent
	{
		EventHandler boundsChanged;
		EventHandler shown;
		EventHandler hidden;
		CloseRequestedHandler closeRequested;

		Point location;
		Size size;
		bool pendingReallocation;
        Image icon;
		WindowFrame transientFor;
		
		protected class WindowBackendHost: BackendHost<WindowFrame,IWindowFrameBackend>, IWindowFrameEventSink
		{
			protected override void OnBackendCreated ()
			{
				Backend.Initialize (this);
				base.OnBackendCreated ();
				Parent.location = Backend.Bounds.Location;
				Parent.size = Backend.Bounds.Size;
				Backend.EnableEvent (WindowFrameEvent.BoundsChanged);
			}
			
			public void OnBoundsChanged (Rectangle bounds)
			{
				Parent.OnBoundsChanged (new BoundsChangedEventArgs () { Bounds = bounds });
			}

			public virtual void OnShown ()
			{
				Parent.OnShown ();
			}

			public virtual void OnHidden ()
			{
				Parent.OnHidden ();
			}

			public virtual bool OnCloseRequested ()
			{
				return Parent.OnCloseRequested ();
			}
		}

		static WindowFrame ()
		{
			MapEvent (WindowFrameEvent.Shown, typeof(WindowFrame), "OnShown");
			MapEvent (WindowFrameEvent.Hidden, typeof(WindowFrame), "OnHidden");
			MapEvent (WindowFrameEvent.CloseRequested, typeof(WindowFrame), "OnCloseRequested");
		}

		public WindowFrame ()
		{
			if (!(base.BackendHost is WindowBackendHost))
				throw new InvalidOperationException ("CreateBackendHost for WindowFrame did not return a WindowBackendHost instance");
		}
		
		public WindowFrame (string title): this ()
		{
			Backend.Title = title;
		}
		
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			
			// Don't dispose the backend if this object is being finalized
			// The backend has to handle the finalizing on its own
			if (disposing && BackendHost.BackendCreated)
				Backend.Dispose ();
		}
		
		IWindowFrameBackend Backend {
			get { return (IWindowFrameBackend) BackendHost.Backend; } 
		}
		
		protected override BackendHost CreateBackendHost ()
		{
			return new WindowBackendHost ();
		}
		
		protected new WindowBackendHost BackendHost {
			get { return (WindowBackendHost) base.BackendHost; }
		}
		
		public Rectangle ScreenBounds {
			get {
				return BackendBounds;
			}
			set {
				if (value.Width < 0)
					value.Width = 0;
				if (value.Height < 0)
					value.Height = 0;
				BackendBounds = value;
			}
		}

		public double X {
			get { return BackendBounds.X; }
			set { SetBackendLocation (value, Y); }
		}
		
		public double Y {
			get { return BackendBounds.Y; }
			set { SetBackendLocation (X, value); }
		}
		
		public double Width {
			get { return BackendBounds.Width; }
			set {
				if (value < 0)
					value = 0;
				SetBackendSize (value, -1);
			}
		}
		
		public double Height {
			get { return BackendBounds.Height; }
			set {
				if (value < 0)
					value = 0;
				SetBackendSize (-1, value);
			}
		}
		
		public Size Size {
			get { return BackendBounds.Size; }
			set {
				if (value.Width < 0)
					value.Width = 0;
				if (value.Height < 0)
					value.Height = 0;
				SetBackendSize (value.Width, value.Height);
			}
		}
		
		public Point Location {
			get { return BackendBounds.Location; }
			set { SetBackendLocation (value.X, value.Y); }
		}
		
		public string Title {
			get { return Backend.Title; }
			set { Backend.Title = value; }
		}

        public Image Icon {
            get { return icon; }
            set { Backend.SetIcon((value as IFrontend).Backend); }
        }
		
		public bool Decorated {
			get { return Backend.Decorated; }
			set { Backend.Decorated = value; }
		}
		
		public bool ShowInTaskbar {
			get { return Backend.ShowInTaskbar; }
			set { Backend.ShowInTaskbar = value; }
		}

		public WindowFrame TransientFor {
			get { return transientFor; }
			set {
				transientFor = value;
				Backend.SetTransientFor ((IWindowFrameBackend)(value as IFrontend).Backend);
			}
		}

		public bool Resizable {
			get { return Backend.Resizable; }
			set { Backend.Resizable = value; }
		}
		
		public bool Visible {
			get { return Backend.Visible; }
			set { Backend.Visible = value; }
		}
		
		/// <summary>
		/// Gets or sets a value indicating whether this window is in full screen mode
		/// </summary>
		/// <value><c>true</c> if the window is in full screen mode; otherwise, <c>false</c>.</value>
		public bool FullScreen {
			get { return Backend.FullScreen; }
			set { Backend.FullScreen = value; }
		}

		public void Show ()
		{
			Visible = true;
		}
		
		/// <summary>
		/// Presents a window to the user. This may mean raising the window in the stacking order,
		/// deiconifying it, moving it to the current desktop, and/or giving it the keyboard focus
		/// </summary>
		public void Present ()
		{
			Backend.Present ();
		}

		protected virtual void OnShown ()
		{
			if(shown != null)
				shown (this, EventArgs.Empty);
		}
		
		public void Hide ()
		{
			Visible = false;
		}

		protected virtual void OnHidden ()
		{
			if (hidden != null)
				hidden (this, EventArgs.Empty);
		}

		protected virtual bool OnCloseRequested ()
		{
			if (closeRequested == null)
				return false;
			var eventArgs = new CloseRequestedEventArgs();
			closeRequested (this, eventArgs);
			return eventArgs.Handled;
		}

		internal virtual void SetBackendSize (double width, double height)
		{
			size = new Size (width != -1 ? width : Width, height != -1 ? height : Height);
			Backend.Resize (size.Width, size.Height);
		}

		internal virtual void SetBackendLocation (double x, double y)
		{
			location = new Point (x, y);
			Backend.Move (x, y);
		}

		internal virtual Rectangle BackendBounds {
			get {
				BackendHost.EnsureBackendLoaded ();
				return new Rectangle (location, size);
			}
			set {
				size = value.Size;
				location = value.Location;
				Backend.Bounds = value;
			}
		}
		
		protected virtual void OnBoundsChanged (BoundsChangedEventArgs a)
		{
			var bounds = new Rectangle (location, size);
			if (bounds != a.Bounds) {
				size = a.Bounds.Size;
				location = a.Bounds.Location;
				Reallocate ();
				if (boundsChanged != null)
					boundsChanged (this, a);
			}
		}
		
		internal void Reallocate ()
		{
			if (!pendingReallocation) {
				pendingReallocation = true;
				BackendHost.ToolkitEngine.QueueExitAction (delegate {
					pendingReallocation = false;
					OnReallocate ();
				});
			}
		}
		
		protected virtual void OnReallocate ()
		{
		}
		
		public event EventHandler BoundsChanged {
			add {
				boundsChanged += value;
			}
			remove {
				boundsChanged -= value;
			}
		}

		public event EventHandler Shown {
			add {
				BackendHost.OnBeforeEventAdd (WindowFrameEvent.Shown, shown);
				shown += value;
			}
			remove {
				shown -= value;
				BackendHost.OnAfterEventRemove (WindowFrameEvent.Shown, shown);
			}
		}

		public event EventHandler Hidden {
			add {
				BackendHost.OnBeforeEventAdd (WindowFrameEvent.Hidden, hidden);
				hidden += value;
			}
			remove {
				hidden -= value;
				BackendHost.OnAfterEventRemove (WindowFrameEvent.Hidden, hidden);
			}
		}

		public event CloseRequestedHandler CloseRequested {
			add {
				BackendHost.OnBeforeEventAdd (WindowFrameEvent.CloseRequested, closeRequested);
				closeRequested += value;
			}
			remove {
				closeRequested -= value;
				BackendHost.OnAfterEventRemove (WindowFrameEvent.CloseRequested, closeRequested);
			}
		}
	}
	
	public class BoundsChangedEventArgs: EventArgs
	{
		public Rectangle Bounds { get; set; }
	}
}

