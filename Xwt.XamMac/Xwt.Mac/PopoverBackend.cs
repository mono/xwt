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
		public Popover Frontend { get; private set; }
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
				View = new ContainerView (this);
				View.AddSubview (NativeChild);

				WidgetSpacing padding = 0;
				if (Backend != null)
					padding = Backend.Frontend.Padding;
				View.AddConstraints (new NSLayoutConstraint [] {
					NSLayoutConstraint.Create (NativeChild, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1, (nfloat)padding.Left),
					NSLayoutConstraint.Create (NativeChild, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1, -(nfloat)padding.Right),
					NSLayoutConstraint.Create (NativeChild, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1, (nfloat)padding.Top),
					NSLayoutConstraint.Create (NativeChild, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1, -(nfloat)padding.Bottom),
				});
			}

			class ContainerView : NSView
			{
				FactoryViewController controller;
				public ContainerView (FactoryViewController controller)
				{
					this.controller = controller;
				}
				
				#if !MONOMAC
				public override bool AllowsVibrancy {
					get {
						// disable vibrancy for custom background
						return controller.BackgroundColor == null ? base.AllowsVibrancy : false;
					}
				}
				#endif
			}

			[Export ("popoverWillShow:")]
			public void WillShow (NSNotification notification)
			{
				ChildBackend.SetAutosizeMode (true);
				Child.Surface.Reallocate ();
				if (BackgroundColor != null) {
					if (View.Window.ContentView.Superview.Layer == null)
						View.Window.ContentView.Superview.WantsLayer = true;
					View.Window.ContentView.Superview.Layer.BackgroundColor = BackgroundColor;
				}
				WidgetSpacing padding = 0;
				if (Backend != null)
					padding = Backend.Frontend.Padding;
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

		CGColor backgroundColor;

		public Color BackgroundColor {
			get {
				return (backgroundColor ?? NSColor.WindowBackground.CGColor).ToXwtColor ();
			}
			set {
				backgroundColor = value.ToCGColor ();
			}
		}

		public void Initialize (IPopoverEventSink sink)
		{
			EventSink = sink;
		}

		public void InitializeBackend (object frontend, ApplicationContext context)
		{
			ApplicationContext = context;
			Frontend = frontend as Popover;
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
			var refBackend = Toolkit.GetBackend (referenceWidget) as IWidgetBackend;
			NSView refView = (refBackend as ViewBackend)?.Widget;

			if (refView == null) {
				if (referenceWidget.Surface.ToolkitEngine.Type == ToolkitType.Gtk) {
					try {
						refView = GtkQuartz.GetView (refBackend.NativeWidget);
						var rLocation = refView.ConvertRectToView (refView.Frame, null).Location.ToXwtPoint ();
						if (referenceWidget.WindowBounds.Location != rLocation) {
							positionRect.X += referenceWidget.WindowBounds.Location.X - rLocation.X;
							positionRect.Y += referenceWidget.WindowBounds.Location.Y - rLocation.Y;
						}
					} catch (Exception ex) {
						throw new ArgumentException ("Widget belongs to an unsupported Toolkit", nameof (referenceWidget), ex);
					}
				} else if (referenceWidget.Surface.ToolkitEngine != ApplicationContext.Toolkit)
					throw new ArgumentException ("Widget belongs to an unsupported Toolkit", nameof (referenceWidget));
			}

			// If the rect is empty, the coordinates of the rect will be ignored.
			// Set the width and height, for the positioning to function correctly.
			if (Math.Abs (positionRect.Width) < double.Epsilon)
				positionRect.Width = referenceWidget.Size.Width;
			if (Math.Abs (positionRect.Height) < double.Epsilon)
				positionRect.Height = referenceWidget.Size.Height;

			DestroyPopover ();

			popover = new NSAppearanceCustomizationPopover {
				Behavior = NSPopoverBehavior.Transient
			};
			var controller = new FactoryViewController (this, child) { BackgroundColor = backgroundColor };
			popover.ContentViewController = controller;
			popover.WeakDelegate = controller;

			#if !MONOMAC
			// if the reference has a custom appearance, use it for the popover
			if (popover is INSAppearanceCustomization && refView.EffectiveAppearance.Name != NSAppearance.NameAqua)
				((INSAppearanceCustomization)popover).SetAppearance (refView.EffectiveAppearance);
			#endif

			popover.Show (positionRect.ToCGRect (),
				      refView,
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
			var controller = new FactoryViewController (null, child);
			return new NSPopover {
				Behavior = NSPopoverBehavior.Transient,
				ContentViewController = controller,
				WeakDelegate = controller
			};
		}

		public static NSPopover MakePopover (Widget child, Color backgroundColor)
		{

			var controller = new FactoryViewController (null, child) { BackgroundColor = backgroundColor.ToCGColor () };
			return new NSPopover {
				Behavior = NSPopoverBehavior.Transient,
				ContentViewController = controller,
				WeakDelegate = controller
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

		#if MONOMAC
		public class NSAppearanceCustomizationPopover : NSPopover
		#else
		public class NSAppearanceCustomizationPopover : NSPopover, INSAppearanceCustomization
		#endif
		{
		}
	}
}

