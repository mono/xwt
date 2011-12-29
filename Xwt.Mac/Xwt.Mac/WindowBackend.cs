// 
// WindowBackend.cs
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
using MonoMac.AppKit;
using System.Drawing;
using MonoMac.ObjCRuntime;
using Xwt.Engine;

namespace Xwt.Mac
{
	public class WindowBackend: NSWindow, IWindowBackend
	{
		WindowBackendController controller;
		IWindowFrameEventSink eventSink;
		Window frontend;
		IMacViewBackend child;
		
		public WindowBackend (IntPtr ptr): base (ptr)
		{
		}
		
		public WindowBackend ()
		{
			this.controller = new WindowBackendController ();
			controller.Window = this;
			StyleMask |= NSWindowStyle.Resizable;
			ContentView.AutoresizesSubviews = true;
			Center ();
		}

		public virtual void Initialize (object frontend)
		{
			this.frontend = (Window) frontend;
		}
		
		public void Initialize (IWindowFrameEventSink eventSink)
		{
			this.eventSink = eventSink;
		}
		
		public object NativeWidget {
			get {
				return this;
			}
		}
		
		internal void InternalShow ()
		{
			MakeKeyAndOrderFront (MacEngine.App);
		}

		public bool Visible {
			get {
				return !ContentView.Hidden;
			}
			set {
				if (value)
					MacEngine.App.ShowWindow (this);
				ContentView.Hidden = !value;
			}
		}

		public bool Sensitive {
			get {
				return true;
			}
			set {
			}
		}
		
		public virtual bool CanGetFocus {
			get { return true; }
			set { }
		}
		
		public virtual bool HasFocus {
			get { return false; }
		}
		
		public void SetFocus ()
		{
		}
		
		#region IWindowBackend implementation
		void IBackend.EnableEvent (object ev)
		{
			if ((ev is WindowFrameEvent) && ((WindowFrameEvent)ev) == WindowFrameEvent.BoundsChanged)
				DidResize += HandleDidResize;
		}

		void IBackend.DisableEvent (object ev)
		{
			if ((ev is WindowFrameEvent) && ((WindowFrameEvent)ev) == WindowFrameEvent.BoundsChanged)
				DidResize -= HandleDidResize;
		}

		void HandleDidResize (object sender, EventArgs e)
		{
			Toolkit.Invoke (delegate {
				eventSink.OnBoundsChanged (((IWindowBackend)this).Bounds);
			});
		}

		void IWindowBackend.SetChild (IWidgetBackend child)
		{
			if (this.child != null) {
				this.child.View.RemoveFromSuperview ();
			}
			this.child = (IMacViewBackend) child;
			if (child != null) {
				ContentView.AddSubview (this.child.View);
				SetPadding (frontend.Padding.Left, frontend.Padding.Top, frontend.Padding.Right, frontend.Padding.Bottom);
				this.child.View.AutoresizingMask = NSViewResizingMask.HeightSizable | NSViewResizingMask.WidthSizable;
			}
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
			}
		}
		
		bool IWindowFrameBackend.ShowInTaskbar {
			get {
				return false;
			}
			set {
			}
		}
		
		public void SetPadding (double left, double top, double right, double bottom)
		{
			if (child != null) {
				var frame = ContentView.Frame;
				frame.X += (float) left;
				frame.Width -= (float) (left + right);
				frame.Y += (float) top;
				frame.Height -= (float) (top + bottom);
				child.View.Frame = frame;
			}
		}
		
		Rectangle IWindowFrameBackend.Bounds {
			get {
				var r = ContentRectFor (Frame);
				return new Rectangle ((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height);
			}
			set {
				var r = FrameRectFor (new System.Drawing.RectangleF ((float)value.X, (float)value.Y, (float)value.Width, (float)value.Height));
				SetFrame (r, true);
			}
		}
		
		public void SetMainMenu (IMenuBackend menu)
		{
			NSMenu m = (NSMenu) menu;
			NSApplication.SharedApplication.Menu = m;
//			base.Menu = m;
		}
		
		#endregion

		static Selector closeSel = new Selector ("close");
		
		void IWindowFrameBackend.Dispose (bool disposing)
		{
			if (disposing)
				Messaging.void_objc_msgSend (this.Handle, closeSel.Handle);
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
	}
	
	public partial class WindowBackendController : MonoMac.AppKit.NSWindowController
	{
		public WindowBackendController ()
		{
		}
	}
}

