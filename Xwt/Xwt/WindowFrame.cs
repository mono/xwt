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
using Xwt.Engine;

namespace Xwt
{
	public class WindowFrame: XwtComponent
	{
		EventHandler boundsChanged;
		Rectangle bounds;
		EventSink eventSink;
		bool pendingReallocation;
		
		protected class EventSink: IWindowFrameEventSink
		{
			internal protected WindowFrame Parent { get; set; }
			
			public void OnBoundsChanged (Rectangle bounds)
			{
				Parent.OnBoundsChanged (new BoundsChangedEventArgs () { Bounds = bounds });
			}
		}
		
		public WindowFrame ()
		{
			eventSink = CreateEventSink ();
			eventSink.Parent = this;
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
			if (disposing && BackendCreated)
				Backend.Dispose ();
		}
		
		new IWindowFrameBackend Backend {
			get { return (IWindowFrameBackend) base.Backend; } 
		}
		
		protected override void OnBackendCreated ()
		{
			base.OnBackendCreated ();
			Backend.Initialize (eventSink);
			bounds = Backend.Bounds;
			Backend.EnableEvent (WindowFrameEvent.BoundsChanged);
		}
		
		protected virtual EventSink CreateEventSink ()
		{
			return new EventSink ();
		}
		
		protected EventSink WindowEventSink {
			get { return eventSink; }
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
				SetSize (value.Width, value.Height);
				BackendBounds = value;
			}
		}

		public double X {
			get { return BackendBounds.X; }
			set { BackendBounds = new Xwt.Rectangle (value, Y, Width, Height); }
		}
		
		public double Y {
			get { return BackendBounds.Y; }
			set { BackendBounds = new Xwt.Rectangle (X, value, Width, Height); }
		}
		
		public double Width {
			get { return BackendBounds.Width; }
			set {
				if (value < 0)
					value = 0;
				SetSize (value, -1);
				BackendBounds = new Rectangle (X, Y, value, Height);
			}
		}
		
		public double Height {
			get { return BackendBounds.Height; }
			set {
				if (value < 0)
					value = 0;
				SetSize (-1, value);
				BackendBounds = new Rectangle (X, Y, Width, value);
			}
		}
		
		public Size Size {
			get { return BackendBounds.Size; }
			set {
				if (value.Width < 0)
					value.Width = 0;
				if (value.Height < 0)
					value.Height = 0;
				SetSize (value.Width, value.Height);
				BackendBounds = new Rectangle (Location, value);
			}
		}
		
		public Point Location {
			get { return BackendBounds.Location; }
			set { BackendBounds = new Rectangle (value.X, value.Y, Width, Height); }
		}
		
		public string Title {
			get { return Backend.Title; }
			set { Backend.Title = value; }
		}
		
		public bool Decorated {
			get { return Backend.Decorated; }
			set { Backend.Decorated = value; }
		}
		
		public bool ShowInTaskbar {
			get { return Backend.ShowInTaskbar; }
			set { Backend.ShowInTaskbar = value; }
		}
		
		public bool Visible {
			get { return Backend.Visible; }
			set { Backend.Visible = value; }
		}
		
		public void Show ()
		{
			Visible = true;
		}
		
		public void Hide ()
		{
			Visible = false;
		}

		internal virtual void SetSize (double width, double height)
		{
			BackendBounds = new Rectangle (X, Y, width != -1 ? width : Width, height != -1 ? height : Height);
		}

		internal virtual Rectangle BackendBounds {
			get { LoadBackend ();  return bounds; }
			set { bounds = Backend.Bounds = value; }
		}
		
		protected virtual void OnBoundsChanged (BoundsChangedEventArgs a)
		{
			if (bounds != a.Bounds) {
				bounds = a.Bounds;
				Reallocate ();
				if (boundsChanged != null)
					boundsChanged (this, a);
			}
		}
		
		internal void Reallocate ()
		{
			if (!pendingReallocation) {
				pendingReallocation = true;
				Toolkit.QueueExitAction (delegate {
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
	}
	
	public class BoundsChangedEventArgs: EventArgs
	{
		public Rectangle Bounds { get; set; }
	}
}

