//
// PopoverBackend.cs
//
// Author:
//       Jérémie Laval <jeremie.laval@xamarin.com>
//
// Copyright (c) 2012 Xamarin, Inc.
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

using Xwt;
using Xwt.Backends;

using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;
using Xwt.Engine;

namespace Xwt.Mac
{
	public class PopoverBackend : IPopoverBackend
	{
		NSPopover popover;
		public event EventHandler Closed;

		class FactoryViewController : NSViewController
		{
			Xwt.Widget child;
			NSView view;

			public FactoryViewController (Xwt.Widget child) : base (null, null)
			{
				this.child = child;
			}

			// Called when created from unmanaged code
			public FactoryViewController (IntPtr handle) : base (handle)
			{
			}
			
			// Called when created directly from a XIB file
			[Export ("initWithCoder:")]
			public FactoryViewController (NSCoder coder) : base (coder)
			{
			}
			
			public override void LoadView ()
			{
				var backend = (IMacViewBackend)Xwt.Engine.ToolkitEngine.GetBackend (child);
				view = ((IWidgetBackend)backend).NativeWidget as NSView;
				ForceChildLayout ();
				backend.SetAutosizeMode (true);
				// FIXME: unset when the popover is closed
			}
			
			void ForceChildLayout ()
			{
				((IWidgetSurface)child).Reallocate ();
			}
			
			public override NSView View {
				get {
					if (view == null)
						LoadView ();
					return view;
				}
				set {
					if (value == null)
						return;
					view = value;
				}
			}
		}

		IPopoverEventSink sink;
		
		public void Initialize (IPopoverEventSink sink)
		{
			this.sink = sink;
		}

		public void InitializeBackend (object frontend, ApplicationContext context)
		{
		}

		public void EnableEvent (object eventId)
		{
		}

		public void DisableEvent (object eventId)
		{
		}

		public void Show (Xwt.Popover.Position orientation, Xwt.Widget referenceWidget, Xwt.Rectangle positionRect, Xwt.Widget child)
		{
			var controller = new FactoryViewController (child);
			popover = new NSPopover ();
			popover.Behavior = NSPopoverBehavior.Transient;
			popover.ContentViewController = controller;
			IMacViewBackend backend = (IMacViewBackend)ToolkitEngine.GetBackend (referenceWidget);
			var reference = backend.View;
			popover.Show (System.Drawing.RectangleF.Empty,
			              reference,
			              ToRectEdge (orientation));
		}

		public void Hide ()
		{
			popover.Close ();
		}
		
		NSRectEdge ToRectEdge (Xwt.Popover.Position pos)
		{
			switch (pos) {
			case Popover.Position.Top:
				return NSRectEdge.MaxYEdge;
			case Popover.Position.Bottom:
			default:
				return NSRectEdge.MinYEdge;
			}
		}
		
		public void Dispose ()
		{
			popover.Close ();
		}
	}
}

