//
// PopoverMacBackend.cs
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

using Xwt.Backends;

using Gtk;
using MonoMac.AppKit;

namespace Xwt.GtkBackend.Mac
{
	public class PopoverMacBackend : IPopoverBackend
	{
		NSPopover popover;
		//Xwt.Popover.Position orientation;
		public event EventHandler Closed;

		/*class OffscreenWindow : Gtk.Window
		{
			public OffscreenWindow () : base (WindowType.Toplevel)
			{
			}

			[GLib.ConnectBefore]
			protected override void OnRealized ()
			{
				Gdk.WindowAttr attributes;
				Gdk.WindowAttributesType attributes_mask;
				GtkMacInterop.SetRealized (this, true);

				attributes.X = Allocation.X;
				attributes.Y= Allocation.Y;
				attributes.Width = Allocation.Width;
				attributes.Height = Allocation.Height;
				attributes.WindowType = (Gdk.WindowType)(((int)Gdk.WindowType.Foreign) + 1);
				attributes.EventMask = (int)(Events | Gdk.EventMask.ExposureMask);
				attributes.Visual = Visual;
				attributes.Colormap = Colormap;
				attributes.Wclass = Gdk.WindowClass.InputOutput;

				attributes_mask = Gdk.WindowAttributesType.X | Gdk.WindowAttributesType.Y | Gdk.WindowAttributesType.Visual | Gdk.WindowAttributesType.Colormap;

				GdkWindow = new Gdk.Window (ParentWindow, attributes, (int)attributes_mask);

				if (Child != null)
					Child.ParentWindow = GdkWindow;
				Style.Attach (GdkWindow);
				Style.SetBackground (GdkWindow, Gtk.StateType.Normal);
			}

			/*protected override void OnSizeAllocated (Gdk.Rectangle allocation)
			{
				Allocation = allocation;
				int borderWidth = (int)BorderWidth;
				if (IsRealized)
					GdkWindow.MoveResize (allocation.X, allocation.Y, allocation.Width, allocation.Height);
				if (Child != null && Child.Visible) {
					Gdk.Rectangle childAlloc = new Gdk.Rectangle ();
					childAlloc.X = borderWidth;
					childAlloc.Y = borderWidth;
					childAlloc.Width = allocation.Width - 2 * borderWidth;
					childAlloc.Height = allocation.Height - 2 * borderWidth;
					Child.SizeAllocate (childAlloc);
				}
				QueueDraw ();
			}

			protected override void OnSizeRequested (ref Requisition requisition)
			{
				int border_width;
				int default_width, default_height;

				border_width = (int)BorderWidth;

				requisition.Width = border_width * 2;
				requisition.Height = border_width * 2;

				if (Child != null && Child.Visible) {
					Gtk.Requisition childReq = Child.SizeRequest ();
					requisition.Width += childReq.Width;
					requisition.Height += childReq.Height;
				}

				GetDefaultSize (out default_width, out default_height);
				if (default_width > 0)
					requisition.Width = default_width;

				if (default_height > 0)
					requisition.Height = default_height;
			}

			void InternalResize ()
			{
				Gdk.Rectangle allocation = new Gdk.Rectangle (0, 0, 0, 0);
				Gtk.Requisition requisition = SizeRequest ();

				allocation.Width  = requisition.Width;
				allocation.Height = requisition.Height;
				SizeAllocate (allocation);
			}

			new void MoveFocus (Gtk.DirectionType dir)
			{
				ChildFocus (dir);
				if (FocusChild != null)
					OnSetFocus (this);
			}

			protected override void OnShown ()
			{
				SetFlag (Gtk.WidgetFlags.Visible);
				bool needResize = !IsRealized;

				if (needResize)
					InternalResize ();
				Map ();
				if (Focus == null)
					MoveFocus (Gtk.DirectionType.TabForward);
			}

			protected override void OnHidden ()
			{
				ClearFlag (Gtk.WidgetFlags.Visible);
				Unmap ();
			}

			protected override void OnResizeChecked ()
			{
				if (Visible)
					InternalResize ();
			}/
		}

		class GtkProxyViewController : NSViewController
		{
			Gtk.Widget innerWidget;
			// We use a container event box to make sure we have a valid parent GdkWindow
			Gtk.Window container;
			NSView view;

			public GtkProxyViewController (global::Gtk.Widget innerWidget) : base (null, null)
			{
				this.innerWidget = innerWidget;
			}

			public override void LoadView ()
			{
				//container = new OffscreenWindow ();
				container = new Gtk.Window (WindowType.Toplevel);
				container.Add (innerWidget);
				container.Realize ();
				innerWidget.ShowAll ();
				/container.Decorated = false;
				container.SkipTaskbarHint = true;
				container.SkipPagerHint = true;
				container.TypeHint = Gdk.WindowTypeHint.Utility;/
				//container.ShowAll ();
				/*container.Hide ();
				innerContainer.ShowAll ();/
				if (container.GdkWindow == null)
					throw new InvalidOperationException ("Fail");
				innerWidget.SetFlag (WidgetFlags.Visible);
				NSView innerView = GtkMacInterop.GetNSViewFromGdkWindow (container.GdkWindow);
				if (innerView == null)
					throw new InvalidOperationException ("Fail NSView");
				innerView.NeedsDisplay = true;
				innerView.Hidden = false;
				//MonoMac.ObjCRuntime.Messaging.void_objc_msgSend (innerView.Handle, MonoMac.ObjCRuntime.Selector.GetHandle ("retain"));
				//innerView.RemoveFromSuperview ();
				View = new NSView ();
				//View.SetBoundsSize (innerView.Bounds.Size);
				View.AddSubview (innerView);
				innerWidget.ShowAll ();
				//MonoMac.ObjCRuntime.Messaging.void_objc_msgSend (innerView.Handle, MonoMac.ObjCRuntime.Selector.GetHandle ("release"));
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
		}*/

		public Xwt.Engine.WidgetRegistry PreferredRegistry {
			get {
				return GtkMacEngine.MacWidgetRegistry;
			}
		}

		class GtkViewController : NSViewController
		{
			Func<Xwt.Widget> childCreator;
			NSView view;
			Xwt.Widget child;

			public GtkViewController (Func<Xwt.Widget> childCreator) : base (null, null)
			{
				this.childCreator = childCreator;
			}

			public override void LoadView ()
			{
				Xwt.Engine.WidgetRegistry.RunAsIfDefault (GtkMacEngine.MacWidgetRegistry, () => {
					child = childCreator ();
					view = ((IWidgetBackend)GtkMacEngine.MacWidgetRegistry.GetBackend (child)).NativeWidget as NSView;
				});
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
			//this.orientation = orientation;
			var controller = new GtkViewController (childSource);
			popover = new NSPopover ();
			popover.Behavior = NSPopoverBehavior.Transient;
			popover.ContentViewController = controller;
			var gtkWidget = (Gtk.Widget)((WidgetBackend)GtkEngine.Registry.GetBackend (referenceWidget)).NativeWidget;
			var gdkWindow = gtkWidget.GdkWindow;
			if (gdkWindow == null)
				throw new InvalidOperationException ("The provided reference Gtk.Widget must have a valid GdkWindow associated");
			var bounds = referenceWidget.ScreenBounds;
			popover.Show (new System.Drawing.RectangleF ((float)bounds.X, (float)bounds.Y, (float)bounds.Width, (float)bounds.Height),
			              GtkMacInterop.GetNSViewFromGdkWindow (gdkWindow),
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