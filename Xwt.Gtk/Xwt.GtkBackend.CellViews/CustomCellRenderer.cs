//
// CustomCellRenderer.cs
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
using Gtk;
using Xwt.CairoBackend;

namespace Xwt.GtkBackend
{
	class CustomCellRenderer: Gtk.CellRenderer, ICellDataSource
	{
		TreeModel treeModel;
		TreeIter iter;
		CanvasCellView cellView;

		public CustomCellRenderer (CanvasCellView cellView)
		{
			this.cellView = cellView;
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

		protected override void Render (Gdk.Drawable window, Gtk.Widget widget, Gdk.Rectangle background_area, Gdk.Rectangle cell_area, Gdk.Rectangle expose_area, Gtk.CellRendererState flags)
		{
			ICanvasCellRenderer cell = cellView;
			cell.ApplicationContext.InvokeUserCode (delegate {
				CairoContextBackend ctx = new CairoContextBackend (Util.GetScaleFactor (widget));
				ctx.Context = Gdk.CairoHelper.Create (window);
				using (ctx) {
					cell.Draw (ctx, new Rectangle (cell_area.X, cell_area.Y, cell_area.Width, cell_area.Height));
				}
			});
		}

		public override void GetSize (Gtk.Widget widget, ref Gdk.Rectangle cell_area, out int x_offset, out int y_offset, out int width, out int height)
		{
			ICanvasCellRenderer cell = cellView;
			Size size = new Size ();
			cell.ApplicationContext.InvokeUserCode (delegate {
				size = cell.GetRequiredSize ();
			});
			width = (int) size.Width;
			height = (int) size.Height;
			x_offset = y_offset = 0;
		}
	}
}

