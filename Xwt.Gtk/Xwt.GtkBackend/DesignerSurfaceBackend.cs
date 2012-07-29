// 
// DesignerSurfaceBackend.cs
//  
// Author:
//       lluis <${AuthorEmail}>
// 
// Copyright (c) 2011 lluis
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
using Xwt.Engine;

namespace Xwt.GtkBackend
{
	public class DesignerSurfaceBackend: WidgetBackend, IDesignerSurfaceBackend
	{
		Gtk.EventBox box;
		
		public DesignerSurfaceBackend ()
		{
			box = new DesignerBox ();
			box.Show ();
			Widget = box;
		}
		
		public void Load (Widget w)
		{
			var wb = (IGtkWidgetBackend) GtkEngine.Registry.GetBackend (w);
			box.Add (wb.Widget);
		}
	}
	
	class DesignerBox: Gtk.EventBox
	{
		Gtk.EventBox surface;
		
		public DesignerBox ()
		{
			surface = new Gtk.EventBox ();
			surface.ShowAll ();
			surface.VisibleWindow = false;
			surface.Parent = this;
		}
		
		protected override void OnSizeAllocated (Gdk.Rectangle allocation)
		{
			base.OnSizeAllocated (allocation);
			surface.Allocation = new Gdk.Rectangle (0,0, allocation.Width, allocation.Height);
		}
		
		protected override void OnSizeRequested (ref Gtk.Requisition requisition)
		{
			base.OnSizeRequested (ref requisition);
			surface.SizeRequest ();
		}
		
		protected override void ForAll (bool include_internals, Gtk.Callback callback)
		{
			base.ForAll (include_internals, callback);
			callback (surface);
		}
	}
		
}

