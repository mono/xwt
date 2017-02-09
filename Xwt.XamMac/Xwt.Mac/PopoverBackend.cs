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
		public ApplicationContext ApplicationContext { get; set; }
		public IPopoverEventSink EventSink { get; set; }
		internal bool EnableCloseEvent { get; private set; }
		NSPopover popover;

		class FactoryViewController : NSViewController, INSPopoverDelegate
		{
			public Widget Child { get; private set; }
			public PopoverBackend Backend { get; private set; }
			public CGColor BackgroundColor { get; set; }
			public ViewBackend ChildBackend { get; private set; }
			public NSView NativeChild { get { return ChildBackend?.Widget; } }

			public FactoryViewController (PopoverBackend backend, Widget child) : base (null, null)
			{
				Child = child;
				ChildBackend = Toolkit.GetBackend (Child) as ViewBackend;
				Backend = backend;
			}

			public override void LoadView ()
			{
				View = new NSView ();
				View.AddSubview (NativeChild);

				if (View.Layer == null)
					View.WantsLayer = true;
				if (BackgroundColor != null)
					View.Layer.BackgroundColor = BackgroundColor;

				WidgetSpacing padding = 0;
				View.AddConstraints (new NSLayoutConstraint [] {
					NSLayoutConstraint.Create (NativeChild, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1, (nfloat)padding.Left),
					NSLayoutConstraint.Create (NativeChild, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1, -(nfloat)padding.Right),
					NSLayoutConstraint.Create (NativeChild, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1, (nfloat)padding.Top),
					NSLayoutConstraint.Create (NativeChild, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1, -(nfloat)padding.Bottom),
				});
			}

			[Export ("popoverWillShow:")]
			public void WillShow (NSNotification notification)
			{
				ChildBackend.SetAutosizeMode (true);
				Child.Surface.Reallocate ();
				WidgetSpacing padding = 0;
				NativeChild.SetFrameOrigin (new CGPoint ((nfloat)padding.Left, (nfloat)padding.Top));
			}

			[Export ("popoverDidClose:")]
			public virtual void DidClose (NSNotification notification)
			{
				NativeChild.SetFrameOrigin (new CGPoint (0, 0));
				ChildBackend.SetAutosizeMode (false);

				if (Backend?.EnableCloseEvent == true)
					Backend.ApplicationContext.InvokeUserCode (Backend.EventSink.OnClosed);
			}
		}

		public Color BackgroundColor { get; set; }

		public void Initialize (IPopoverEventSink sink)
		{
			EventSink = sink;
		}

		public void InitializeBackend (object frontend, ApplicationContext context)
		{
			ApplicationContext = context;
		}

		public void EnableEvent (object eventId)
		{
			if (eventId is PopoverEvent) {
				switch ((PopoverEvent)eventId) {
				case PopoverEvent.Closed:
					EnableCloseEvent = true;
					break;
				}
			}
		}

		public void DisableEvent (object eventId)
		{
			if (eventId is PopoverEvent) {
				switch ((PopoverEvent)eventId) {
				case PopoverEvent.Closed:
					EnableCloseEvent = false;
					break;
				}
			}
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

			popover = new NSPopover {
				Behavior = NSPopoverBehavior.Transient
			};
			var controller = new FactoryViewController (this, child) { BackgroundColor = BackgroundColor.ToCGColor () };
			popover.ContentViewController = controller;
			popover.WeakDelegate = controller;

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
				ContentViewController = new FactoryViewController (null, child)
			};
		}

		public static NSPopover MakePopover (Widget child, Color backgroundColor)
		{
			return new NSPopover {
				Behavior = NSPopoverBehavior.Transient,
				ContentViewController = new FactoryViewController (null, child) { BackgroundColor = backgroundColor.ToCGColor () }
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

