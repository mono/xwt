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
using Gtk;
using Xwt.Backends;

namespace Xwt.GtkBackend
{
	public static class CellUtil
	{
		class CellDataSource: ICellDataSource
		{
			TreeIter iter;
			TreeModel treeModel;

			public CellDataSource (TreeIter iter, TreeModel treeModel)
			{
				this.iter = iter;
				this.treeModel = treeModel;
			}

			public object GetValue (IDataField field)
			{
				return treeModel.GetValue (iter, field.Index);
			}
		}

		public static Gtk.CellRenderer CreateCellRenderer (ApplicationContext actx, ICellRendererTarget col, object target, ICellViewFrontend view, Gtk.TreeModel model)
		{
			if (view is ITextCellViewFrontend) {
				var cr = new CustomCellRendererText ((ITextCellViewFrontend)view);
				col.PackStart (target, cr, false);
				col.SetCellDataFunc (target, cr, (cell_layout, cell, treeModel, iter) => cr.LoadData (treeModel, iter));
				return cr;
			}
			else if (view is ICheckBoxCellViewFrontend) {
				CustomCellRendererToggle cr = new CustomCellRendererToggle ((ICheckBoxCellViewFrontend)view);
				col.PackStart (target, cr, false);
				col.SetCellDataFunc (target, cr, (cellLayout, cell, treeModel, iter) => cr.LoadData (treeModel, iter));
				return cr;
			}
			else if (view is IImageCellViewFrontend) {
				CustomCellRendererImage cr = new CustomCellRendererImage (actx, (IImageCellViewFrontend)view);
				col.PackStart (target, cr, false);
				col.SetCellDataFunc (target, cr, (cellLayout, cell, treeModel, iter) => cr.LoadData (treeModel, iter));
				return cr;
			}
			else if (view is ICanvasCellViewFrontend) {
				var cr = new CustomCellRenderer ((ICanvasCellViewFrontend) view);
				col.PackStart (target, cr, false);
				col.SetCellDataFunc (target, cr, (cellLayout, cell, treeModel, iter) => cr.LoadData (treeModel, iter));
				return cr;
			}
			throw new NotSupportedException ("Unknown cell view type: " + view.GetType ());
		}
		
		public static Gtk.Widget CreateCellRenderer (ApplicationContext actx, ICollection<CellView> views)
		{
			if (views.Count == 1) {
				Gtk.HBox box = new Gtk.HBox ();
				foreach (var v in views)
					box.PackStart (CreateCellRenderer (actx, v), false, false, 0);
				box.ShowAll ();
				return box;
			}
			else
				return CreateCellRenderer (actx, views.First ());
		}
		
		public static Gtk.Widget CreateCellRenderer (ApplicationContext actx, CellView view)
		{
			if (view is TextCellView) {
				Gtk.Label lab = new Gtk.Label ();
				lab.Xalign = 0;
//				lab.Text = ((TextCellView)view).TextField;
				return lab;
			}
			throw new NotImplementedException ();
		}

		public static void SetModelValue (Gtk.TreeModel store, Gtk.TreeIter it, int column, Type type, object value)
		{
			if (type == typeof(ObjectWrapper) && value != null)
				store.SetValue (it, column, new ObjectWrapper (value));
			else if (value is string)
				store.SetValue (it, column, (string)value);
			else
				store.SetValue (it, column, value ?? DBNull.Value);
		}

		public static object GetModelValue (Gtk.TreeModel store, Gtk.TreeIter it, int column)
		{
			object val = store.GetValue (it, column);
			if (val is DBNull)
				return null;
			else if (val is ObjectWrapper)
				return ((ObjectWrapper)val).Object;
			else
				return val;
		}
	}
	
	public interface ICellRendererTarget
	{
		void PackStart (object target, Gtk.CellRenderer cr, bool expand);
		void PackEnd (object target, Gtk.CellRenderer cr, bool expand);
		void AddAttribute (object target, Gtk.CellRenderer cr, string field, int column);
		void SetCellDataFunc (object target, Gtk.CellRenderer cr, Gtk.CellLayoutDataFunc dataFunc);
	}

}

