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

namespace Xwt.Mac
{
	public class PopoverBackend : IPopoverBackend
	{
		NSPopover popover;
		public event EventHandler Closed;

		class FactoryViewController : NSViewController
		{
			Func<Xwt.Widget> childCreator;
			NSView view;
			Xwt.Widget child;
			
			public FactoryViewController (Func<Xwt.Widget> childCreator) : base (null, null)
			{
				this.childCreator = childCreator;
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
				child = childCreator ();
				view = ((IWidgetBackend)MacEngine.Registry.GetBackend (child)).NativeWidget as NSView;
				ForceChildLayout ();
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
		
		public void Run (Xwt.WindowFrame parent, Xwt.Popover.Position orientation, Func<Xwt.Widget> childSource, Xwt.Widget referenceWidget)
		{
			var controller = new FactoryViewController (childSource);
			popover = new NSPopover ();
			popover.Behavior = NSPopoverBehavior.Transient;
			popover.ContentViewController = controller;
			var reference = ((IMacViewBackend)MacEngine.Registry.GetBackend (referenceWidget)).View;
			popover.Show (System.Drawing.RectangleF.Empty,
			              reference,
			              ToRectEdge (orientation));
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

