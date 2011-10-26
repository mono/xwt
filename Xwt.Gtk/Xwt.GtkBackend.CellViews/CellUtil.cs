// 
// CellUtil.cs
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
using System.Collections.Generic;
using System.Linq;

namespace Xwt.GtkBackend
{
	public static class CellUtil
	{
		public static Gtk.CellRenderer CreateCellRenderer (Gtk.TreeViewColumn col, CellView view)
		{
			if (view is TextCellView) {
				Gtk.CellRendererText cr = new Gtk.CellRendererText ();
				col.PackStart (cr, false);
				col.AddAttribute (cr, "text", ((TextCellView)view).TextField.Index);
				return cr;
			}
			else if (view is CheckBoxCellView) {
				Gtk.CellRendererToggle cr = new Gtk.CellRendererToggle ();
				col.PackStart (cr, false);
				col.AddAttribute (cr, "active", ((CheckBoxCellView)view).ActiveField.Index);
				return cr;
			}
			else if (view is ImageCellView) {
				Gtk.CellRendererPixbuf cr = new Gtk.CellRendererPixbuf ();
				col.PackStart (cr, false);
				col.AddAttribute (cr, "pixbuf", ((ImageCellView)view).ImageField.Index);
				return cr;
			}
			throw new NotSupportedException ("Unknown cell view type: " + view.GetType ());
		}
		
		public static Gtk.Widget CreateCellRenderer (ICollection<CellView> views)
		{
			if (views.Count == 1) {
				Gtk.HBox box = new Gtk.HBox ();
				foreach (var v in views)
					box.PackStart (CreateCellRenderer (v), false, false, 0);
				box.ShowAll ();
				return box;
			}
			else
				return CreateCellRenderer (views.First ());
		}
		
		public static Gtk.Widget CreateCellRenderer (CellView view)
		{
			if (view is TextCellView) {
				Gtk.Label lab = new Gtk.Label ();
				lab.Xalign = 0;
//				lab.Text = ((TextCellView)view).TextField;
				return lab;
			}
			throw new NotImplementedException ();
		}
	}
}

