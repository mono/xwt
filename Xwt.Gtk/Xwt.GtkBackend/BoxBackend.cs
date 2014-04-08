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
	class BoxBackend: WidgetBackend, IBoxBackend
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
			#if XWT_GTK3
			// since we have no SizeRequest event, we must always queue up for resize
			if (changed)
			#else
			if (changed && !Widget.IsReallocating)
			#endif
				Widget.QueueResize ();
		}
	}
	
	class CustomContainer: Gtk.Container, IGtkContainer
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
			GtkWorkarounds.FixContainerLeak (this);
			#if XWT_GTK3
			HasWindow = false;
			#else
			WidgetFlags |= Gtk.WidgetFlags.NoWindow;
			#endif
		}
		
		public void ReplaceChild (Gtk.Widget oldWidget, Gtk.Widget newWidget)
		{
			WidgetData r = children [oldWidget];
			Remove (oldWidget);
			Add (newWidget);
			children [newWidget] = r;
		}
		
		public bool SetAllocation (Gtk.Widget w, Rectangle rect)
		{
			WidgetData r;
			children.TryGetValue (w, out r);
			if (r.Rect != rect) {
				r.Rect = rect;
				children [w] = r;
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

		#if XWT_GTK3
		protected override void OnUnrealized ()
		{
			// force reallocation on next realization, since allocation may be lost
			IsReallocating = false;
			base.OnUnrealized ();
		}

		protected override void OnRealized ()
		{
			// force reallocation, if unrealized previously
			if (!IsReallocating) {
				try {
					((IWidgetSurface)Backend.Frontend).Reallocate ();
				} catch {
					IsReallocating = false;
				}
			}
			base.OnRealized ();
		}

		protected override Gtk.SizeRequestMode OnGetRequestMode ()
		{
			// dirty fix: unwrapped labels report fixed sizes, forcing parents to fixed mode
			//            -> report always width_for_height, since we don't support angles
			return Gtk.SizeRequestMode.WidthForHeight;
		}

		protected override void OnGetPreferredHeight (out int minimum_height, out int natural_height)
		{
			// containers need initial width in heigt_for_width mode
			// dirty fix: do not constrain width on first allocation 
			var force_width = SizeConstraint.Unconstrained;
			if (IsReallocating)
				force_width = SizeConstraint.WithSize (Allocation.Width);
			var size = Backend.Frontend.Surface.GetPreferredSize (force_width, SizeConstraint.Unconstrained, true);
			minimum_height = natural_height = (int)size.Height;
		}

		protected override void OnGetPreferredWidth (out int minimum_width, out int natural_width)
		{
			// containers need initial height in width_for_height mode
			// dirty fix: do not constrain height on first allocation
			var force_height = SizeConstraint.Unconstrained;
			if (IsReallocating)
				force_height = SizeConstraint.WithSize (Allocation.Width);
			var size = Backend.Frontend.Surface.GetPreferredSize (SizeConstraint.Unconstrained, force_height, true);
			minimum_width = natural_width = (int)size.Height;
		}

		protected override void OnGetPreferredHeightForWidth (int width, out int minimum_height, out int natural_height)
		{
			var size = Backend.Frontend.Surface.GetPreferredSize (SizeConstraint.WithSize (width),
			                                                      SizeConstraint.Unconstrained,
			                                                      true);
			minimum_height = natural_height = (int)size.Height;
		}

		protected override void OnGetPreferredWidthForHeight (int height, out int minimum_width, out int natural_width)
		{
			var size = Backend.Frontend.Surface.GetPreferredSize (SizeConstraint.Unconstrained,
			                                                      SizeConstraint.WithSize (height),
			                                                      true);
			minimum_width = natural_width = (int)size.Width;
		}
		#endif

		protected override void OnSizeAllocated (Gdk.Rectangle allocation)
		{
			base.OnSizeAllocated (allocation);
			try {
				IsReallocating = true;
				((IWidgetSurface)Backend.Frontend).Reallocate ();
			} catch {
				IsReallocating = false;
			}
			foreach (var cr in children) {
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

