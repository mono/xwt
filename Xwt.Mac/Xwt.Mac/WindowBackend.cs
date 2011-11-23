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

namespace Xwt.Mac
{
	public class WindowBackend: NSWindow, IWindowBackend
	{
		WindowBackendController controller;
		IWindowEventSink eventSink;
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
		
		public object NativeWidget {
			get {
				return this;
			}
		}
		
		public void ShowAll ()
		{
			MacEngine.App.ShowWindow (this);
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
		void IWidgetBackend.Initialize (IWidgetEventSink eventSink)
		{
			this.eventSink = (IWindowEventSink) eventSink;
		}
		
		Point IWidgetBackend.ConvertToScreenCoordinates (Point widgetCoordinates)
		{
			var lo = ConvertBaseToScreen (new PointF ((float)widgetCoordinates.X, (float)widgetCoordinates.Y));
			return new Point (lo.X, lo.Y);
		}

		void IBackend.EnableEvent (object ev)
		{
			if ((ev is WindowEvent) && ((WindowEvent)ev) == WindowEvent.BoundsChanged)
				DidResize += HandleDidResize;
		}

		void IBackend.DisableEvent (object ev)
		{
			if ((ev is WindowEvent) && ((WindowEvent)ev) == WindowEvent.BoundsChanged)
				DidResize -= HandleDidResize;
		}

		void HandleDidResize (object sender, EventArgs e)
		{
			eventSink.OnBoundsChanged (((IWindowBackend)this).Bounds);
		}

		void IWindowBackend.SetChild (IWidgetBackend child)
		{
			if (this.child != null) {
				this.child.View.RemoveFromSuperview ();
			}
			this.child = (IMacViewBackend) child;
			if (child != null) {
				ContentView.AddSubview (this.child.View);
				UpdateLayout ();
				this.child.View.AutoresizingMask = NSViewResizingMask.HeightSizable | NSViewResizingMask.WidthSizable;
			}
		}
		
		bool IWindowBackend.Decorated {
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
		
		bool IWindowBackend.ShowInTaskbar {
			get {
				return false;
			}
			set {
			}
		}
		
		public virtual void UpdateLayout ()
		{
			if (child != null) {
				var frame = ContentView.Frame;
				frame.X += frontend.Margin.Left;
				frame.Width -= frontend.Margin.HorizontalSpacing;
				frame.Y += frontend.Margin.Top;
				frame.Height -= frontend.Margin.VerticalSpacing;
				child.View.Frame = frame;
			}
		}
		
		Rectangle IWindowBackend.Bounds {
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

		#region IWidgetBackend implementation

		Size IWidgetBackend.Size {
			get { return ((IWindowBackend)this).Bounds.Size; }
		}
		
		WidgetSize IWidgetBackend.GetPreferredWidth ()
		{
			int w = (int)Frame.Width + frontend.Margin.HorizontalSpacing;
			return new WidgetSize (w, w);
		}

		WidgetSize IWidgetBackend.GetPreferredHeightForWidth (double width)
		{
			int h = (int) Frame.Height + frontend.Margin.VerticalSpacing;
			return new WidgetSize (h, h);
		}

		WidgetSize IWidgetBackend.GetPreferredHeight ()
		{
			int h = (int) Frame.Height + frontend.Margin.VerticalSpacing;
			return new WidgetSize (h, h);
		}

		WidgetSize IWidgetBackend.GetPreferredWidthForHeight (double height)
		{
			int w = (int)Frame.Width + frontend.Margin.HorizontalSpacing;
			return new WidgetSize (w, w);
		}

		bool IWidgetBackend.Visible {
			get {
				return IsVisible;
			}
			set {
				if (value)
					MakeKeyAndOrderFront (controller);
				else
					OrderOut (controller);
			}
		}

		bool IWidgetBackend.Sensitive {
			get {
				return true;
			}
			set {
			}
		}
		#endregion

		static Selector closeSel = new Selector ("close");
		
		void IDisposable.Dispose ()
		{
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

