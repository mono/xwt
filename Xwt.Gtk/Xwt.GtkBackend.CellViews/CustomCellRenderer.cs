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
using Xwt.Backends;
using Xwt.GtkBackend;

namespace Xwt.GtkBackend
{
	class CustomCellRenderer: Gtk.CellRenderer, ICellDataSource
	{
		TreeModel treeModel;
		TreeIter iter;
		ICanvasCellViewFrontend cellView;

		public CustomCellRenderer (ICanvasCellViewFrontend cellView)
		{
			this.cellView = cellView;
		}

		#region ICellDataSource implementation

		public void LoadData (TreeViewBackend treeBackend, TreeModel treeModel, TreeIter iter)
		{
			this.treeModel = treeModel;
			this.iter = iter;
			cellView.Initialize (this);
			Visible = cellView.Visible;
		}

		object ICellDataSource.GetValue (IDataField field)
		{
			return CellUtil.GetModelValue (treeModel, iter, field.Index);
		}

		#endregion

		protected override void Render (Gdk.Drawable window, Gtk.Widget widget, Gdk.Rectangle background_area, Gdk.Rectangle cell_area, Gdk.Rectangle expose_area, Gtk.CellRendererState flags)
		{
			cellView.ApplicationContext.InvokeUserCode (delegate {
				CairoContextBackend ctx = new CairoContextBackend (Util.GetScaleFactor (widget));
				ctx.Context = Gdk.CairoHelper.Create (window);
				using (ctx) {
					cellView.Draw (ctx, new Rectangle (cell_area.X, cell_area.Y, cell_area.Width, cell_area.Height));
				}
			});
		}

		public override void GetSize (Gtk.Widget widget, ref Gdk.Rectangle cell_area, out int x_offset, out int y_offset, out int width, out int height)
		{
			Size size = new Size ();
			cellView.ApplicationContext.InvokeUserCode (delegate {
				size = cellView.GetRequiredSize ();
			});
			width = (int) size.Width;
			height = (int) size.Height;
			x_offset = y_offset = 0;
		}
	}
}

