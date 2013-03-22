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
		public static Gtk.CellRenderer CreateCellRenderer (ApplicationContext actx, ICellRendererTarget col, object target, CellView view, Gtk.TreeModel model)
		{
			if (view is TextCellView) {
				Gtk.CellRendererText cr = new Gtk.CellRendererText ();
				if (((TextCellView)view).Editable) {
					cr.Editable = true;
					cr.Edited += (o, args) => {
						Gtk.TreeIter iter;
						if (model.GetIterFromString (out iter, args.Path))
							model.SetValue (iter, ((TextCellView)view).TextField.Index, args.NewText);
					};
				}
				col.PackStart (target, cr, false);
				col.AddAttribute (target, cr, "text", ((TextCellView)view).TextField.Index);
				return cr;
			}
			else if (view is CheckBoxCellView) {
				Gtk.CellRendererToggle cr = new Gtk.CellRendererToggle ();
				col.PackStart (target, cr, false);
				col.AddAttribute (target, cr, "active", ((CheckBoxCellView)view).ActiveField.Index);
				return cr;
			}
			else if (view is ImageCellView) {
				CellRendererImage cr = new CellRendererImage (actx);
				col.PackStart (target, cr, false);
				col.AddAttribute (target, cr, "image", ((ImageCellView)view).ImageField.Index);
				return cr;
			}
			else if (view is CanvasCellView) {
				var canvas = (CanvasCellView) view;
				var cr = new CustomCellRenderer (canvas);
				col.PackStart (target, cr, false);
				col.SetCellDataFunc (target, cr, delegate (CellLayout cell_layout, CellRenderer cell, TreeModel tree_model, TreeIter iter) {
					cr.LoadData (cell_layout, cell, tree_model, iter);
					((CanvasCellView) view).Initialize (cr);
				});
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
	}
	
	public interface ICellRendererTarget
	{
		void PackStart (object target, Gtk.CellRenderer cr, bool expand);
		void PackEnd (object target, Gtk.CellRenderer cr, bool expand);
		void AddAttribute (object target, Gtk.CellRenderer cr, string field, int column);
		void SetCellDataFunc (object target, Gtk.CellRenderer cr, Gtk.CellLayoutDataFunc dataFunc);
	}

	public class CellRendererImage: Gtk.CellRenderer, ICellDataSource
	{
		TreeModel treeModel;
		TreeIter iter;
		ImageDescription image;
		ApplicationContext actx;

		public CellRendererImage (ApplicationContext actx)
		{
			this.actx = actx;
		}
		
		#region ICellDataSource implementation
		
		public void LoadData (CellLayout cell_layout, CellRenderer cell, TreeModel treeModel, TreeIter iter)
		{
			this.treeModel = treeModel;
			this.iter = iter;
		}
		
		object ICellDataSource.GetValue (IDataField field)
		{
			return treeModel.GetValue (iter, field.Index);
		}
		
		#endregion
		
		[GLib.Property ("image")]
		public ImageDescription Image {
			get { return image; }
			set { image = value; }
		}

		protected override void Render (Gdk.Drawable window, Gtk.Widget widget, Gdk.Rectangle background_area, Gdk.Rectangle cell_area, Gdk.Rectangle expose_area, Gtk.CellRendererState flags)
		{
			if (image.IsNull)
				return;

			var ctx = Gdk.CairoHelper.Create (window);
			using (ctx) {
				var pix = ((GtkImage)image.Backend);
				pix.Draw (actx, ctx, cell_area.X, cell_area.Y, image);
			}
		}
		
		public override void GetSize (Gtk.Widget widget, ref Gdk.Rectangle cell_area, out int x_offset, out int y_offset, out int width, out int height)
		{
			if (image.IsNull) {
				width = height = 0;
			} else {
				width = (int) image.Size.Width;
				height = (int) image.Size.Height;
			}
			x_offset = y_offset = 0;
		}
	}
}

