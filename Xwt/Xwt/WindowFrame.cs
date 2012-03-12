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
				LoadBackend();
				return bounds;
			}
			set {
				Backend.Bounds = value;
			}
		}
		
		public double X {
			get { return ScreenBounds.X; }
			set { ScreenBounds = new Xwt.Rectangle (value, Y, Width, Height); }
		}
		
		public double Y {
			get { return ScreenBounds.Y; }
			set { ScreenBounds = new Xwt.Rectangle (X, value, Width, Height); }
		}
		
		public double Width {
			get { return ScreenBounds.Width; }
			set { ScreenBounds = new Xwt.Rectangle (X, Y, value, Height); }
		}
		
		public double Height {
			get { return ScreenBounds.Height; }
			set { ScreenBounds = new Xwt.Rectangle (X, Y, Width, value); }
		}
		
		public Size Size {
			get { return ScreenBounds.Size; }
			set { ScreenBounds = new Rectangle (X, Y, value.Width, value.Height); }
		}
		
		public Point Location {
			get { return ScreenBounds.Location; }
			set { ScreenBounds = new Rectangle (value.X, value.Y, Width, Height); }
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

