//
// PopoverBackend.cs
//
// Author:
//       Jérémie Laval <jeremie.laval@xamarin.com>
//       Vsevolod Kukol <sevoku@microsoft.com>
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
using AppKit;
using CoreGraphics;
using Foundation;
using Xwt.Backends;
using Xwt.Drawing;


namespace Xwt.Mac
{
	public class PopoverBackend : IPopoverBackend
	{
		NSPopover popover;

		class FactoryViewController : NSViewController
		{
			readonly Widget child;
			public CGColor BackgroundColor { get; set; }

			public FactoryViewController (Widget child) : base (null, null)
			{
				this.child = child;
			}

			public override void LoadView ()
			{
				var backend = (ViewBackend)Toolkit.GetBackend (child);
				View = backend.Widget;

				if (View.Layer == null)
					View.WantsLayer = true;
				if (BackgroundColor != null)
					View.Layer.BackgroundColor = BackgroundColor;
				backend.SetAutosizeMode (true);
				ForceChildLayout ();
				// FIXME: unset when the popover is closed
			}

			void ForceChildLayout ()
			{
				((IWidgetSurface)child).Reallocate ();
			}
		}

		public Color BackgroundColor { get; set; }

		public void Initialize (IPopoverEventSink sink)
		{
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

		public void Show (Popover.Position orientation, Widget referenceWidget, Rectangle positionRect, Widget child)
		{
			ViewBackend backend = (ViewBackend)Toolkit.GetBackend (referenceWidget);
			var reference = backend.Widget;

			// If the position rect is empty, the coordinates of the rect will be ignored.
			// Width and Height of the rect must be > Epsilon, for the positioning to function correctly.
			if (Math.Abs (positionRect.Width) < double.Epsilon)
				positionRect.Width = 1;
			if (Math.Abs (positionRect.Height) < double.Epsilon)
				positionRect.Height = 1;

			DestroyPopover ();
			popover = MakePopover (child, BackgroundColor);
			popover.Show (positionRect.ToCGRect (),
				      reference,
				      ToRectEdge (orientation));
		}

		public void Hide ()
		{
			DestroyPopover ();
		}

		public void Dispose ()
		{
			DestroyPopover ();
		}

		void DestroyPopover ()
		{
			if (popover != null) {
				popover.Close ();
				popover.Dispose ();
				popover = null;
			}
		}

		public static NSPopover MakePopover (Widget child)
		{
			return new NSPopover {
				Behavior = NSPopoverBehavior.Transient,
				ContentViewController = new FactoryViewController (child)
			};
		}

		public static NSPopover MakePopover (Widget child, Color backgroundColor)
		{
			return new NSPopover {
				Behavior = NSPopoverBehavior.Transient,
				ContentViewController = new FactoryViewController (child) { BackgroundColor = backgroundColor.ToCGColor () }
			};
		}

		public static NSRectEdge ToRectEdge (Popover.Position pos)
		{
			switch (pos) {
			case Popover.Position.Top:
				return NSRectEdge.MaxYEdge;
			default:
				return NSRectEdge.MinYEdge;
			}
		}
	}
}

