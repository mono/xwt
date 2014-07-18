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
using Xwt.Motion;

namespace Xwt
{
	[BackendType (typeof(IWindowFrameBackend))]
	public class WindowFrame: XwtComponent, IAnimatable
	{
		EventHandler boundsChanged;
		EventHandler shown;
		EventHandler hidden;
		CloseRequestedHandler closeRequested;
		EventHandler closed;

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

			public virtual void OnClosed ()
			{
				Parent.OnClosed ();
			}
		}

		static WindowFrame ()
		{
			MapEvent (WindowFrameEvent.Shown, typeof(WindowFrame), "OnShown");
			MapEvent (WindowFrameEvent.Hidden, typeof(WindowFrame), "OnHidden");
			MapEvent (WindowFrameEvent.CloseRequested, typeof(WindowFrame), "OnCloseRequested");
			MapEvent (WindowFrameEvent.Closed, typeof(WindowFrame), "OnClosed");
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
				if (Visible)
					AdjustSize ();
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
				if (Visible)
					AdjustSize ();
			}
		}
		
		public double Height {
			get { return BackendBounds.Height; }
			set {
				if (value < 0)
					value = 0;
				SetBackendSize (-1, value);
				if (Visible)
					AdjustSize ();
			}
		}

		/// <summary>
		/// Size of the window, not including the decorations
		/// </summary>
		/// <value>The size.</value>
		public Size Size {
			get { return BackendBounds.Size; }
			set {
				if (value.Width < 0)
					value.Width = 0;
				if (value.Height < 0)
					value.Height = 0;
				SetBackendSize (value.Width, value.Height);
				if (Visible)
					AdjustSize ();
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
			set { icon = value; Backend.SetIcon (icon != null ? icon.GetImageDescription (BackendHost.ToolkitEngine) : ImageDescription.Null); }
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

		[DefaultValue (true)]
		public bool Sensitive {
			get { return Backend.Sensitive; }
			set { Backend.Sensitive = value; }
		}

		public double Opacity {
			get { return Backend.Opacity; }
			set { Backend.Opacity = value; }
		}
		
		/// <summary>
		/// Gets or sets a value indicating whether this window is in full screen mode
		/// </summary>
		/// <value><c>true</c> if the window is in full screen mode; otherwise, <c>false</c>.</value>
		public bool FullScreen {
			get { return Backend.FullScreen; }
			set { Backend.FullScreen = value; }
		}

		/// <summary>
		/// Gets the screen on which most of the area of this window is placed
		/// </summary>
		/// <value>The screen.</value>
		public Screen Screen {
			get {
				if (!Visible)
					throw new InvalidOperationException ("The window is not visible");
				return Desktop.GetScreen (Backend.Screen);
			}
		}

		public void Show ()
		{
			if (!Visible) {
				AdjustSize ();
				Visible = true;
			}
		}
		
		internal virtual void AdjustSize ()
		{
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

		/// <summary>
		/// Closes the window
		/// </summary>
		/// <remarks>>
		/// Closes the window like if the user clicked on the close window button.
		/// The CloseRequested event is fired and subscribers can cancel the closing,
		/// so there is no guarantee that the window will actually close.
		/// This method doesn't dispose the window. The Dispose method has to be called.
		/// </remarks>
		public bool Close ()
		{
			return Backend.Close ();
		}

		/// <summary>
		/// Called to check if the window can be closed
		/// </summary>
		/// <returns><c>true</c> if the window can be closed, <c>false</c> otherwise</returns>
		protected virtual bool OnCloseRequested ()
		{
			if (closeRequested == null)
				return true;
			var eventArgs = new CloseRequestedEventArgs();
			closeRequested (this, eventArgs);
			return eventArgs.AllowClose;
		}

		/// <summary>
		/// Called when the window has been closed by the user, or by a call to Close
		/// </summary>
		/// <remarks>
		/// This method is not called when the window is disposed, only when explicitly closed (either by code or by the user)
		/// </remarks>
		protected virtual void OnClosed ()
		{
			if (closed != null)
				closed (this, EventArgs.Empty);
		}

		internal virtual void SetBackendSize (double width, double height)
		{
			Backend.SetSize (width, height);
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
		
		void IAnimatable.BatchBegin ()
		{
		}

		void IAnimatable.BatchCommit ()
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

		/// <summary>
		/// Raised when the window has been closed by the user, or by a call to Close
		/// </summary>
		/// <remarks>
		/// This event is not raised when the window is disposed, only when explicitly closed (either by code or by the user)
		/// </remarks>
		public event EventHandler Closed {
			add {
				BackendHost.OnBeforeEventAdd (WindowFrameEvent.Closed, closed);
				closed += value;
			}
			remove {
				closed -= value;
				BackendHost.OnAfterEventRemove (WindowFrameEvent.Closed, closed);
			}
		}
	}
	
	public class BoundsChangedEventArgs: EventArgs
	{
		public Rectangle Bounds { get; set; }
	}
}

