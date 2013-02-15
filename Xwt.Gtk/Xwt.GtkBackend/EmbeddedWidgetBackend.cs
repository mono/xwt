//
// EmbeddedWidgetBackend.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc.
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
using System.Reflection;
using System.Drawing;
using Xwt.Backends;

namespace Xwt.GtkBackend
{
	public class EmbeddedWidgetBackend: WidgetBackend, IEmbeddedWidgetBackend
	{

		static Type nsViewType;
		static Type NSViewType {
			get {
				if (nsViewType == null)
					nsViewType = Type.GetType ("MonoMac.AppKit.NSView, MonoMac", false);
				return nsViewType;
			}
		}

		static PropertyInfo nsViewFrame;
		static PropertyInfo NSViewFrame {
			get {
				if (nsViewFrame == null)
					nsViewFrame = NSViewType.GetProperty ("Frame");
				return nsViewFrame;
			}
		}

		static PropertyInfo nsViewHidden;
		static PropertyInfo NSViewHidden {
			get {
				if (nsViewHidden == null)
					nsViewHidden = NSViewType.GetProperty ("Hidden");
				return nsViewHidden;
			}
		}

		object nativeWidget;

		public EmbeddedWidgetBackend ()
		{
		}

		public void SetContent (object nativeWidget)
		{
			if (nativeWidget is Gtk.Widget) {
				Widget = (Gtk.Widget)nativeWidget;
				return;
			}

			// Check if it is an NSView
			if (NSViewType != null && NSViewType.IsInstanceOfType (nativeWidget)) {
				this.nativeWidget = nativeWidget;

				Gtk.Widget wrapper = GtkMacInterop.NSViewToGtkWidget (nativeWidget);
				wrapper.Show ();

				Widget = new Gtk.EventBox {
					VisibleWindow = false,
					Child = wrapper
				};
				Widget.ParentSet += HandleParentSet;
				Widget.VisibilityNotifyEvent += HandleVisibilityNotifyEvent;
				Widget.Show ();
				return;
			}
		}

		void HandleVisibilityNotifyEvent (object o, Gtk.VisibilityNotifyEventArgs args)
		{
			NSViewHidden.SetValue (nativeWidget, args.Event.State == Gdk.VisibilityState.FullyObscured, null);
		}

		double hvalue, vvalue;
		void HandleParentSet (object o, Gtk.ParentSetArgs args)
		{
			// workaround a bug when GtkNSView is inside a ScrolledWindow

			var w = args.PreviousParent;
			while (w != null) {
				w.ParentSet -= HandleParentSet;
				w = w.Parent;
			}

			w = ((Gtk.Widget)o).Parent;
			while (w != null) {
				w.ParentSet += HandleParentSet;

				var viewport = w as Gtk.Viewport;
				if (viewport != null) {
					viewport.ScrollAdjustmentsSet += (s, evt) => {
						TrackScrollAdjustment (evt.Hadjustment, true);
						TrackScrollAdjustment (evt.Vadjustment, false);
					};
					TrackScrollAdjustment (viewport.Hadjustment, true);
					TrackScrollAdjustment (viewport.Vadjustment, false);
				}
				w = w.Parent;
			}
		}

		void TrackScrollAdjustment (Gtk.Adjustment adj, bool isHorizontal)
		{
			if (adj == null)
				return;
			if (isHorizontal)
				hvalue = adj.Value;
			else
				vvalue = adj.Value;

			adj.ValueChanged += delegate {
				var frame = (RectangleF) NSViewFrame.GetValue (nativeWidget, null);
				var newValue = adj.Value;
				if (isHorizontal) {
					frame.X -= (float)(newValue - hvalue);
					hvalue = newValue;
				} else {
					frame.Y -= (float)(newValue - vvalue);
					vvalue = newValue;
				}
				NSViewFrame.SetValue (nativeWidget, frame, null);
			};
		}
	}
}

