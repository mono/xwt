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
		bool isPrelit;
		TreeIter lastIter;
		bool shown;

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
			isSelected = (flags & CellRendererState.Selected) != 0;
			hasFocus = (flags & CellRendererState.Focused) != 0;
			isPrelit = (flags & CellRendererState.Prelit) != 0;

			// Gtk will rerender the cell on every status change, hence we can expect the values
			// set here to be valid until the geometry or any other status of the cell and tree changes.
			// Setting shown=true ensures that those values are always reused instead if queried
			// from the parent tree after the cell has been rendered.
			shown = true;
		}

		protected override void OnLoadData ()
		{
			if (!CurrentIter.Equals (lastIter)) {
				// if the current iter has changed all cached values from the last draw are invalid
				shown = false;
				lastIter = CurrentIter;
			}
			base.OnLoadData ();
		}

		internal void EndDrawing ()
		{
		}

		public override Rectangle CellBounds {
			get {
				if (shown)
					return cellArea;
				return base.CellBounds;
			}
		}

		public override Rectangle BackgroundBounds {
			get {
				if (shown)
					return backgroundArea;
				return base.BackgroundBounds;
			}
		}

		public override bool Selected {
			get {
				if (shown)
					return isSelected;
				return base.Selected;
			}
		}

		public override bool HasFocus {
			get {
				if (shown)
					return hasFocus;
				return base.HasFocus;
			}
		}

		public bool IsHighlighted {
			get {
				if (shown)
					return isPrelit;
				return false;
			}
		}
	}

	class CanvasRenderer: GtkCellRendererCustom
	{
		public ICanvasCellViewFrontend CellView;
		public CustomCellRenderer Parent;

		protected override void OnRender (Cairo.Context cr, Gtk.Widget widget, Gdk.Rectangle background_area, Gdk.Rectangle cell_area, CellRendererState flags)
		{
			int wx, wy, dx = 0, dy = 0;
			var tree = widget as Gtk.TreeView;
			// Tree coordinates must be converted to widget coordinates,
			// otherwise custom cell bounds will have an offset to the parent widget
			if (tree != null) {
				tree.ConvertBinWindowToWidgetCoords (cell_area.X, cell_area.Y, out wx, out wy);
				dx = wx - cell_area.X;
				dy = wy - cell_area.Y;
				cell_area.X += dx;
				background_area.X += dx;
				cell_area.Y += dy;
				background_area.Y += dy;
			}

			Parent.StartDrawing (new Rectangle (background_area.X, background_area.Y, background_area.Width, background_area.Height), new Rectangle (cell_area.X, cell_area.Y, cell_area.Width, cell_area.Height), flags);
			CellView.ApplicationContext.InvokeUserCode (delegate {
				CairoContextBackend ctx = new CairoContextBackend (Util.GetScaleFactor (widget));
				ctx.Context = cr;
				ctx.Context.Translate(-dx, -dy);
				using (ctx) {
					CellView.Draw (ctx, new Rectangle (cell_area.X, cell_area.Y, cell_area.Width, cell_area.Height));
				}
			});
			Parent.EndDrawing ();
		}

		protected override void OnGetSize (Gtk.Widget widget, ref Gdk.Rectangle cell_area, out int x_offset, out int y_offset, out int width, out int height)
		{
			Size size = new Size ();
			var widthConstraint = cell_area.Width > 0 ? SizeConstraint.WithSize(cell_area.Width) : SizeConstraint.Unconstrained;
			CellView.ApplicationContext.InvokeUserCode (delegate {
				size = CellView.GetRequiredSize (widthConstraint);
			});
			width = (int) size.Width;
			height = (int) size.Height;
			x_offset = y_offset = 0;
		}
	}
}

