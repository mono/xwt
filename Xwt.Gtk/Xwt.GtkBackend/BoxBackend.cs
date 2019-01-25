// 
// BoxBackend.cs
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
using System.Linq;
using Xwt.Backends;
using Xwt;
using System.Collections.Generic;

namespace Xwt.GtkBackend
{
	partial class BoxBackend: WidgetBackend, IBoxBackend
	{
		public BoxBackend ()
		{
			Widget = new CustomContainer () { Backend = this };
			Widget.Show ();
		}
		
		new CustomContainer Widget {
			get { return (CustomContainer)base.Widget; }
			set { base.Widget = value; }
		}
		
		public void Add (IWidgetBackend widget)
		{
			WidgetBackend wb = (WidgetBackend) widget;
			Widget.Add (wb.Frontend, GetWidget (widget));
		}

		public void Remove (IWidgetBackend widget)
		{
			Widget.Remove (GetWidget (widget));
		}
		
		public void SetAllocation (IWidgetBackend[] widgets, Rectangle[] rects)
		{
			bool changed = false;
			for (int n=0; n<widgets.Length; n++) {
				var w = GetWidget (widgets[n]);
				if (Widget.SetAllocation (w, rects[n]))
					changed = true;
			}
			if (changed)
				Widget.QueueResizeIfRequired ();
		}
	}
	
	partial class CustomContainer: Gtk.Container, IGtkContainer
	{
		public BoxBackend Backend;
		public bool IsReallocating;
		Dictionary<Gtk.Widget, WidgetData> children = new Dictionary<Gtk.Widget, WidgetData> ();
		
		struct WidgetData
		{
			public Rectangle Rect;
			public Widget Widget;
		}
		
		public CustomContainer ()
		{
			this.FixContainerLeak ();
			this.SetHasWindow (false);
		}
		
		public void ReplaceChild (Gtk.Widget oldWidget, Gtk.Widget newWidget)
		{
			WidgetData r = children [oldWidget];
			Remove (oldWidget);
			Add (newWidget);
			children [newWidget] = r;
		}

		void UpdateFocusChain (Orientation orientation)
		{
			var focusChain = children.Keys.ToArray();
			Array.Sort (focusChain, (x, y) => {
				int left, right;
				if (orientation == Orientation.Horizontal) {
					left = (int)children[x].Rect.X;
					right = (int)children[y].Rect.X;
				} else {
					left = (int)children[x].Rect.Y;
					right = (int)children[y].Rect.Y;
				}
				return left - right;
			});
			FocusChain = focusChain;
		}

		public bool SetAllocation (Gtk.Widget w, Rectangle rect)
		{
			WidgetData r;
			children.TryGetValue (w, out r);
			if (r.Rect != rect) {
				r.Rect = rect;
				children [w] = r;
				UpdateFocusChain (Backend.Frontend is HBox ? Orientation.Horizontal : Orientation.Vertical);
				return true;
			} else
				return false;
		}
		
		public void Add (Widget w, Gtk.Widget gw)
		{
			children.Add (gw, new WidgetData () { Widget = w, Rect = new Rectangle (0,0,0,0) });
			Add (gw);
		}
		
		protected override void OnAdded (Gtk.Widget widget)
		{
			widget.Parent = this;
		}
		
		protected override void OnRemoved (Gtk.Widget widget)
		{
			children.Remove (widget);
			widget.Unparent ();
			QueueResize ();
		}

		protected void OnReallocate ()
		{
			((IWidgetSurface)Backend.Frontend).Reallocate ();
		}

		protected Gtk.Requisition OnGetRequisition (SizeConstraint widthConstraint, SizeConstraint heightConstraint)
		{
			var size = Backend.Frontend.Surface.GetPreferredSize (widthConstraint, heightConstraint, true);
			return size.ToGtkRequisition ();
		}

		protected override void OnSizeAllocated (Gdk.Rectangle allocation)
		{
			base.OnSizeAllocated (allocation);
			try {
				IsReallocating = true;
				OnReallocate ();
			} finally {
				IsReallocating = false;
			}
			foreach (var cr in children.ToArray()) {
				var r = cr.Value.Rect;
				cr.Key.SizeAllocate (new Gdk.Rectangle (allocation.X + (int)r.X, allocation.Y + (int)r.Y, (int)r.Width, (int)r.Height));
			}
		}
		
		protected override void ForAll (bool includeInternals, Gtk.Callback callback)
		{
			base.ForAll (includeInternals, callback);
			foreach (var c in children.Keys.ToArray ())
				callback (c);
		}
	}
}

