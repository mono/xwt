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
	class CustomCellRenderer: CellViewBackend, ICanvasCellViewBackend
	{
		Rectangle cellArea;
		Rectangle backgroundArea;
		bool isSelected;
		bool hasFocus;
		bool isDrawing;

		public override void Initialize (ICellViewFrontend cellView, ICellRendererTarget rendererTarget, object target)
		{
			base.Initialize (cellView, rendererTarget, target);
			CellRenderer = new CanvasRenderer {
				Parent = this,
				CellView = (ICanvasCellViewFrontend) cellView 
			};
		}

		internal void StartDrawing (Rectangle ba, Rectangle ca, Gtk.CellRendererState flags)
		{
			this.cellArea = ca;
			this.backgroundArea = ba;
			isSelected = (flags & Gtk.CellRendererState.Selected) != 0;
			hasFocus = (flags & Gtk.CellRendererState.Focused) != 0;
			isDrawing = true;
		}

		internal void EndDrawing ()
		{
			isDrawing = false;
		}

		public override Rectangle CellBounds {
			get {
				if (isDrawing)
					return cellArea;
				return base.CellBounds;
			}
		}

		public override Rectangle BackgroundBounds {
			get {
				if (isDrawing)
					return backgroundArea;
				return base.BackgroundBounds;
			}
		}

		public override bool Selected {
			get {
				if (isDrawing)
					return isSelected;
				return base.Selected;
			}
		}

		public override bool HasFocus {
			get {
				if (isDrawing)
					return hasFocus;
				return base.HasFocus;
			}
		}
	}

	class CanvasRenderer: GtkCellRendererCustom
	{
		public ICanvasCellViewFrontend CellView;
		public CustomCellRenderer Parent;

		protected override void OnRender (Cairo.Context cr, Gtk.Widget widget, Gdk.Rectangle background_area, Gdk.Rectangle cell_area, CellRendererState flags)
		{
			Parent.StartDrawing (new Rectangle (background_area.X, background_area.Y, background_area.Width, background_area.Height), new Rectangle (cell_area.X, cell_area.Y, cell_area.Width, cell_area.Height), flags);
			CellView.ApplicationContext.InvokeUserCode (delegate {
				CairoContextBackend ctx = new CairoContextBackend (Util.GetScaleFactor (widget));
				ctx.Context = cr;
				using (ctx) {
					CellView.Draw (ctx, new Rectangle (cell_area.X, cell_area.Y, cell_area.Width, cell_area.Height));
				}
			});
			Parent.EndDrawing ();
		}

		protected override void OnGetSize (Gtk.Widget widget, ref Gdk.Rectangle cell_area, out int x_offset, out int y_offset, out int width, out int height)
		{
			Size size = new Size ();
			CellView.ApplicationContext.InvokeUserCode (delegate {
				size = CellView.GetRequiredSize ();
			});
			width = (int) size.Width;
			height = (int) size.Height;
			x_offset = y_offset = 0;
		}
	}
}

