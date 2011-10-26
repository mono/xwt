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
	class BoxBackend<T, S>: WidgetBackend<T, S>, IBoxBackend where T:CustomContainer where S:IWidgetEventSink
	{
		public BoxBackend ()
		{
			Widget = (T) new CustomContainer ();
			Widget.Show ();
		}
		
		public override void Initialize ()
		{
			Widget.Frontend = Frontend;
		}

		public void Add (IWidgetBackend widget)
		{
			Widget.Add (GetWidget (widget));
		}

		public void Remove (IWidgetBackend widget)
		{
			Widget.Remove (GetWidget (widget));
		}
		
		public void SetAllocation (IWidgetBackend widget, Rectangle rect)
		{
			var w = GetWidget (widget);
			Widget.SetAllocation (w, rect);
		}
	}
	
	class CustomContainer: Gtk.Container
	{
		public Widget Frontend;
		Dictionary<Gtk.Widget, Rectangle> children = new Dictionary<Gtk.Widget, Rectangle> ();
		
		public CustomContainer (IntPtr p): base (p)
		{
		}
		
		public CustomContainer ()
		{
			WidgetFlags |= Gtk.WidgetFlags.NoWindow;
		}
		
		public void SetAllocation (Gtk.Widget w, Rectangle rect)
		{
			children [w] = rect;
		}
		
		protected override void OnAdded (Gtk.Widget widget)
		{
			children.Add (widget, new Rectangle (0,0,0,0));
			widget.Parent = this;
			widget.QueueResize ();
		}
		
		protected override void OnRemoved (Gtk.Widget widget)
		{
			children.Remove (widget);
			widget.Unparent ();
		}
		
		protected override void OnSizeRequested (ref Gtk.Requisition requisition)
		{
			requisition.Height = requisition.Width = 0;
			foreach (var cr in children) {
				cr.Key.SizeRequest ();
				if (cr.Value.Right + 1 > requisition.Width)
					requisition.Width = (int) cr.Value.Right + 1;
				if (cr.Value.Bottom + 1 > requisition.Height)
					requisition.Height = (int) cr.Value.Bottom + 1;
			}
		}
		
		protected override void OnSizeAllocated (Gdk.Rectangle allocation)
		{
			base.OnSizeAllocated (allocation);
			((IWidgetSurface)Frontend).Reallocate ();
			foreach (var cr in children) {
				var r = cr.Value;
				cr.Key.SizeAllocate (new Gdk.Rectangle (allocation.X + (int)r.X, allocation.Y + (int)r.Y, (int)r.Width, (int)r.Height));
			}
		}
		
		protected override void ForAll (bool include_internals, Gtk.Callback callback)
		{
			base.ForAll (include_internals, callback);
			foreach (var c in children.Keys.ToArray ())
				callback (c);
		}
	}
}

