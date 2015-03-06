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
using Xwt.Drawing;

#if MONOMAC
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using MonoMac.ObjCRuntime;
#else
using Foundation;
using AppKit;
using ObjCRuntime;
using CoreGraphics;
#endif


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
			public CGColor BackgroundColor { get; set; }

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
				var backend = (ViewBackend)Toolkit.GetBackend (child);
				view = ((ViewBackend)backend).NativeWidget as NSView;

				if (view.Layer == null)
					view.WantsLayer = true;
				if (BackgroundColor != null)
					view.Layer.BackgroundColor = BackgroundColor;
				backend.SetAutosizeMode (true);
				ForceChildLayout ();
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

		public Color BackgroundColor { get; set; }

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
			popover = MakePopover (child, BackgroundColor);
			ViewBackend backend = (ViewBackend)Toolkit.GetBackend (referenceWidget);
			var reference = backend.Widget;

			// If the position rect is empty, the coordinates of the rect will be ignored.
			// Width and Height of the rect must be > Epsilon, for the positioning to function correctly.
			if (Math.Abs (positionRect.Width) < double.Epsilon)
				positionRect.Width = 1;
			if (Math.Abs (positionRect.Height) < double.Epsilon)
				positionRect.Height = 1;

			popover.Show (positionRect.ToCGRect (),
			              reference,
			              ToRectEdge (orientation));
		}

		public void Hide ()
		{
			popover.Close ();
		}

		public void Dispose ()
		{
			popover.Close ();
		}

		public static NSPopover MakePopover (Xwt.Widget child)
		{
			return new NSPopover {
				Behavior = NSPopoverBehavior.Transient,
				ContentViewController = new FactoryViewController (child)
			};
		}

		public static NSPopover MakePopover (Xwt.Widget child, Color backgroundColor)
		{
			return new NSPopover {
				Behavior = NSPopoverBehavior.Transient,
				ContentViewController = new FactoryViewController (child) { BackgroundColor = backgroundColor.ToCGColor () }
			};
		}
		
		public static NSRectEdge ToRectEdge (Xwt.Popover.Position pos)
		{
			switch (pos) {
			case Popover.Position.Top:
				return NSRectEdge.MaxYEdge;
			case Popover.Position.Bottom:
			default:
				return NSRectEdge.MinYEdge;
			}
		}
	}
}

