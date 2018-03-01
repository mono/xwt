// 
// TableViewBackend.cs
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
using Xwt.Backends;
using Gtk;
using System.Collections.Generic;
using System.Linq;
using Gdk;
#if XWT_GTK3
using TreeModel = Gtk.ITreeModel;
#endif

namespace Xwt.GtkBackend
{
	public class TableViewBackend: WidgetBackend, ICellRendererTarget
	{
		Dictionary<CellView,CellInfo> cellViews = new Dictionary<CellView, CellInfo> ();

		class CellInfo {
			public CellViewBackend Renderer;
			public Gtk.TreeViewColumn Column;
		}

		protected override void OnSetBackgroundColor (Xwt.Drawing.Color color)
		{
			// Gtk3 workaround (by rpc-scandinavia, see https://github.com/mono/xwt/pull/411)
			var selectedColor = Widget.GetBackgroundColor(StateType.Selected);
			Widget.SetBackgroundColor (StateType.Normal, Xwt.Drawing.Colors.Transparent);
			Widget.SetBackgroundColor (StateType.Selected, selectedColor);
			base.OnSetBackgroundColor (color);
		}

		public TableViewBackend ()
		{
			var sw = new Gtk.ScrolledWindow ();
			sw.ShadowType = Gtk.ShadowType.In;
			sw.Child = new CustomTreeView (this);
			sw.Child.Show ();
			sw.Show ();
			base.Widget = sw;

			Widget.EnableSearch = false;
		}
		
		protected new Gtk.TreeView Widget {
			get { return (Gtk.TreeView)ScrolledWindow.Child; }
		}
		
		protected Gtk.ScrolledWindow ScrolledWindow {
			get { return (Gtk.ScrolledWindow)base.Widget; }
		}
		
		protected new ITableViewEventSink EventSink {
			get { return (ITableViewEventSink)base.EventSink; }
		}

		protected override Gtk.Widget EventsRootWidget {
			get {
				return ScrolledWindow.Child;
			}
		}

		public ScrollPolicy VerticalScrollPolicy {
			get {
				return ScrolledWindow.VscrollbarPolicy.ToXwtValue ();
			}
			set {
				ScrolledWindow.VscrollbarPolicy = value.ToGtkValue ();
			}
		}
		
		public ScrollPolicy HorizontalScrollPolicy {
			get {
				return ScrolledWindow.HscrollbarPolicy.ToXwtValue ();
			}
			set {
				ScrolledWindow.HscrollbarPolicy = value.ToGtkValue ();
			}
		}

		public GridLines GridLinesVisible
		{
			get { return Widget.EnableGridLines.ToXwtValue (); }
			set { Widget.EnableGridLines = value.ToGtkValue (); }
		}

		public IScrollControlBackend CreateVerticalScrollControl ()
		{
			return new ScrollControltBackend (ScrolledWindow.Vadjustment);
		}

		public IScrollControlBackend CreateHorizontalScrollControl ()
		{
			return new ScrollControltBackend (ScrolledWindow.Hadjustment);
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is TableViewEvent) {
				if (((TableViewEvent)eventId) == TableViewEvent.SelectionChanged)
					Widget.Selection.Changed += HandleWidgetSelectionChanged;
			}
		}
		
		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is TableViewEvent) {
				if (((TableViewEvent)eventId) == TableViewEvent.SelectionChanged)
					Widget.Selection.Changed -= HandleWidgetSelectionChanged;
			}
		}

		void HandleWidgetSelectionChanged (object sender, EventArgs e)
		{
			ApplicationContext.InvokeUserCode (EventSink.OnSelectionChanged);
		}
		
		public object AddColumn (ListViewColumn col)
		{
			Gtk.TreeViewColumn tc = new Gtk.TreeViewColumn ();
			tc.Title = col.Title;
			tc.Resizable = col.CanResize;
			tc.Alignment = col.Alignment.ToGtkAlignment ();
			tc.Expand = col.Expands;
			tc.SortIndicator = col.SortIndicatorVisible;
			tc.SortOrder = (SortType)col.SortDirection;
			if (col.SortDataField != null)
				tc.SortColumnId = col.SortDataField.Index;
			Widget.AppendColumn (tc);
			MapTitle (col, tc);
			MapColumn (col, tc);
			return tc;
		}
		
		void MapTitle (ListViewColumn col, Gtk.TreeViewColumn tc)
		{
			if (col.HeaderView == null)
				tc.Title = col.Title;
			else {
				tc.Widget = CellUtil.CreateCellRenderer (ApplicationContext, col.HeaderView);
				tc.Widget?.Show ();
			}
		}
		
		void MapColumn (ListViewColumn col, Gtk.TreeViewColumn tc)
		{
			foreach (var k in cellViews.Where (e => e.Value.Column == tc).Select (e => e.Key).ToArray ())
				cellViews.Remove (k);
			foreach (var v in col.Views) {
				var r = CellUtil.CreateCellRenderer (ApplicationContext, Frontend, this, tc, v);
				cellViews [v] = new CellInfo {
					Column = tc,
					Renderer = r
				};
			}
		}
		
		public void RemoveColumn (ListViewColumn col, object handle)
		{
			Widget.RemoveColumn ((Gtk.TreeViewColumn)handle);
		}

		public void UpdateColumn (ListViewColumn col, object handle, ListViewColumnChange change)
		{
			Gtk.TreeViewColumn tc = (Gtk.TreeViewColumn) handle;

			switch (change) {
				case ListViewColumnChange.Cells:
					tc.Clear ();
					MapColumn (col, tc);
					break;
				case ListViewColumnChange.Title:
					MapTitle (col, tc);
					break;
				case ListViewColumnChange.CanResize:
					tc.Resizable = col.CanResize;
					break;
				case ListViewColumnChange.SortIndicatorVisible:
					tc.SortIndicator = col.SortIndicatorVisible;
					break;
				case ListViewColumnChange.SortDirection:
					tc.SortOrder = (SortType)col.SortDirection;
					break;
				case ListViewColumnChange.SortDataField:
					if (col.SortDataField != null)
						tc.SortColumnId = col.SortDataField.Index;
					break;
				case ListViewColumnChange.Alignment:
					tc.Alignment = col.Alignment.ToGtkAlignment ();
					break;
				case ListViewColumnChange.Expanding:
					tc.Expand = col.Expands;
					break;
			}
		}

		public void ScrollToRow (TreeIter pos)
		{
			if (Widget.Columns.Length > 0)
				Widget.ScrollToCell (Widget.Model.GetPath (pos), Widget.Columns[0], false, 0, 0);
		}

		public void SelectAll ()
		{
			Widget.Selection.SelectAll ();
		}
		
		public void UnselectAll ()
		{
			Widget.Selection.UnselectAll ();
		}

		public void SetSelectionMode (SelectionMode mode)
		{
			switch (mode) {
			case SelectionMode.None: Widget.Selection.Mode = Gtk.SelectionMode.None; break;
			case SelectionMode.Single: Widget.Selection.Mode = Gtk.SelectionMode.Single; break;
			case SelectionMode.Multiple: Widget.Selection.Mode = Gtk.SelectionMode.Multiple; break;
			}
		}

		protected Gtk.CellRenderer GetCellRenderer (CellView cell)
		{
			return cellViews [cell].Renderer.CellRenderer;
		}

		protected Gtk.TreeViewColumn GetCellColumn (CellView cell)
		{
			CellInfo ci;
			if (cellViews.TryGetValue (cell, out ci))
				return ci.Column;
			return null;
		}

		#region ICellRendererTarget implementation
		public void PackStart (object target, Gtk.CellRenderer cr, bool expand)
		{
			#if !XWT_GTK3
			// Gtk2 tree background color workaround
			if (UsingCustomBackgroundColor)
				cr.CellBackgroundGdk = BackgroundColor.ToGtkValue ();
			#endif
			((Gtk.TreeViewColumn)target).PackStart (cr, expand);
		}

		public void PackEnd (object target, Gtk.CellRenderer cr, bool expand)
		{
			((Gtk.TreeViewColumn)target).PackEnd (cr, expand);
		}
		
		public void AddAttribute (object target, Gtk.CellRenderer cr, string field, int col)
		{
			((Gtk.TreeViewColumn)target).AddAttribute (cr, field, col);
		}

		public void SetCellDataFunc (object target, Gtk.CellRenderer cr, Gtk.CellLayoutDataFunc dataFunc)
		{
			((Gtk.TreeViewColumn)target).SetCellDataFunc (cr, dataFunc);
		}

		Rectangle ICellRendererTarget.GetCellBounds (object target, Gtk.CellRenderer cra, Gtk.TreeIter iter)
		{
			var col = (TreeViewColumn)target;
			var path = Widget.Model.GetPath (iter);

			col.CellSetCellData (Widget.Model, iter, false, false);

			Gdk.Rectangle column_cell_area = Widget.GetCellArea (path, col);
			CellRenderer[] renderers = col.GetCellRenderers();

			for (int i = 0; i < renderers.Length; i++)
			{
				var cr = renderers [i];
				if (cr == cra) {
					int position_x, width, height;
					int position_y = column_cell_area.Y;

					col.CellGetPosition (cr, out position_x, out width);

					if (i == renderers.Length - 1) {
						#if XWT_GTK3
						// Gtk3 allocates all available space to the last cell in the column
						width = column_cell_area.Width - position_x;
						#else
						// Gtk2 sets the cell_area size to fit the largest cell in the inner column (row-wise)
						// since we would have to scan the tree for the largest cell at this (horizontal) cell position
						// we return the width of the current cell.
						int padding_x, padding_y;
						var cell_area = new Gdk.Rectangle(column_cell_area.X + position_x, position_y, width, column_cell_area.Height);
						cr.GetSize (col.TreeView, ref cell_area, out padding_x, out padding_y, out width, out height);
						position_x += padding_x;
						// and add some padding at the end if it would not exceed column bounds
						if (position_x + width + 2 <= column_cell_area.Width)
							width += 2;
						#endif
					} else {
						int position_x_next, width_next;
						col.CellGetPosition (renderers [i + 1], out position_x_next, out width_next);
						width = position_x_next - position_x;
					}

					position_x += column_cell_area.X;
					height = column_cell_area.Height;
					Widget.ConvertBinWindowToWidgetCoords (position_x, position_y, out position_x, out position_y);
					return new Rectangle (position_x, position_y, width, height);
				}
			}
			return Rectangle.Zero;
		}

		Rectangle ICellRendererTarget.GetCellBackgroundBounds (object target, Gtk.CellRenderer cra, Gtk.TreeIter iter)
		{
			var col = (TreeViewColumn)target;
			var path = Widget.Model.GetPath (iter);

			col.CellSetCellData (Widget.Model, iter, false, false);

			Gdk.Rectangle column_cell_area = Widget.GetCellArea (path, col);
			Gdk.Rectangle column_cell_bg_area = Widget.GetBackgroundArea (path, col);

			CellRenderer[] renderers = col.Cells;

			foreach (var cr in renderers) {
				if (cr == cra) {
					int position_x, width;

					col.CellGetPosition (cr, out position_x, out width);

					// Gtk aligns the bg area of a renderer to the cell area and not the bg area
					// so we add the cell area offset to the position here
					position_x += column_cell_bg_area.X + (column_cell_area.X - column_cell_bg_area.X);

					// last widget gets the rest
					if (cr == renderers[renderers.Length - 1])
						width = column_cell_bg_area.Width - (position_x - column_cell_bg_area.X);

					var cell_bg_bounds = new Gdk.Rectangle (position_x,
					                                        column_cell_bg_area.Y,
					                                        width,
					                                        column_cell_bg_area.Height);

					Widget.ConvertBinWindowToWidgetCoords (cell_bg_bounds.X, cell_bg_bounds.Y, out cell_bg_bounds.X, out cell_bg_bounds.Y);
					return new Rectangle (cell_bg_bounds.X, cell_bg_bounds.Y, cell_bg_bounds.Width, cell_bg_bounds.Height);
				}
			}

			return Rectangle.Zero;
		}

		protected Rectangle GetRowBounds (Gtk.TreeIter iter)
		{
			var rect = Rectangle.Zero;

			foreach (var col in Widget.Columns) {
				foreach (var cr in col.GetCellRenderers()) {
					Rectangle cell_rect = ((ICellRendererTarget)this).GetCellBounds (col, cr, iter);
					if (rect == Rectangle.Zero)
						rect = new Rectangle (cell_rect.X, cell_rect.Y, cell_rect.Width, cell_rect.Height);
					else
						rect = rect.Union (new Rectangle (cell_rect.X, cell_rect.Y, cell_rect.Width, cell_rect.Height));
				}
			}
			return rect;
		}

		protected Rectangle GetRowBackgroundBounds (Gtk.TreeIter iter)
		{
			var path = Widget.Model.GetPath (iter);
			var rect = Rectangle.Zero;

			foreach (var col in Widget.Columns) {
				Gdk.Rectangle cell_rect = Widget.GetBackgroundArea (path, col);
				Widget.ConvertBinWindowToWidgetCoords (cell_rect.X, cell_rect.Y, out cell_rect.X, out cell_rect.Y);

				if (rect == Rectangle.Zero)
					rect = new Rectangle (cell_rect.X, cell_rect.Y, cell_rect.Width, cell_rect.Height);
				else
					rect = rect.Union (new Rectangle (cell_rect.X, cell_rect.Y, cell_rect.Width, cell_rect.Height));
			}
			return rect;
		}

		protected Gtk.TreePath GetPathAtPosition (Point p)
		{
			Gtk.TreePath path;
			int x, y;
			Widget.ConvertWidgetToBinWindowCoords ((int)p.X, (int)p.Y, out x, out y);

			if (Widget.GetPathAtPos (x, y, out path))
				return path;
			return null;
		}

		protected override ButtonEventArgs GetButtonPressEventArgs (ButtonPressEventArgs args)
		{
			int x, y;
			Widget.ConvertBinWindowToWidgetCoords ((int)args.Event.X, (int)args.Event.Y, out x, out y);
			var xwt_args = base.GetButtonPressEventArgs (args);
			xwt_args.X = x;
			xwt_args.Y = y;
			return xwt_args;
		}

		protected override ButtonEventArgs GetButtonReleaseEventArgs (ButtonReleaseEventArgs args)
		{
			int x, y;
			Widget.ConvertBinWindowToWidgetCoords ((int)args.Event.X, (int)args.Event.Y, out x, out y);
			var xwt_args = base.GetButtonReleaseEventArgs (args);
			xwt_args.X = x;
			xwt_args.Y = y;
			return xwt_args;
		}

		protected override MouseMovedEventArgs GetMouseMovedEventArgs (MotionNotifyEventArgs args)
		{
			int x, y;
			Widget.ConvertBinWindowToWidgetCoords ((int)args.Event.X, (int)args.Event.Y, out x, out y);
			return new MouseMovedEventArgs ((long) args.Event.Time, x, y);
		}

		public virtual void SetCurrentEventRow (string path)
		{
		}

		Gtk.Widget ICellRendererTarget.EventRootWidget {
			get { return Widget; }
		}

		TreeModel ICellRendererTarget.Model {
			get { return Widget.Model; }
		}

		Gtk.TreeIter ICellRendererTarget.PressedIter { get; set; }

		CellViewBackend ICellRendererTarget.PressedCell { get; set; }


		public bool GetCellPosition (Gtk.CellRenderer r, int ex, int ey, out int cx, out int cy, out Gtk.TreeIter it)
		{
			Gtk.TreeViewColumn col;
			Gtk.TreePath path;
			int _cellx, _celly;
			cx = cy = 0;
			it = Gtk.TreeIter.Zero;

			if (!Widget.GetPathAtPos (ex, ey, out path, out col, out _cellx, out _celly))
				return false;

			if (!Widget.Model.GetIter (out it, path))
				return false;

			var cellArea = Widget.GetCellArea (path, col);
			var cellx = ex - cellArea.X;

			var renderers = col.GetCellRenderers ();
			int i = Array.IndexOf (renderers, r);

			int rendererX, rendererWidth;
			if (col.CellGetPosition (r, out rendererX, out rendererWidth)) {
				if (i < renderers.Length - 1) {
					int nextX, _w;
					// The width returned by CellGetPosition is not reliable. Calculate the width
					// by getting the position of the next renderer.
					if (col.CellGetPosition (renderers [i + 1], out nextX, out _w))
						rendererWidth = nextX - rendererX;
				} else {
					// Last renderer of the column. Its width is what's left in the cell area.
					rendererWidth = cellArea.Width - rendererX;
				}
				
				if (cellx >= rendererX && cellx < rendererX + rendererWidth) {
					Widget.ConvertBinWindowToWidgetCoords (ex, ey, out cx, out cy);
					return true;
				}
			}
			return false;
		}

		public void QueueDraw (object target, Gtk.TreeIter iter)
		{
			var p = Widget.Model.GetPath (iter);
			var r = Widget.GetBackgroundArea (p, (Gtk.TreeViewColumn)target);
			int x, y;
			Widget.ConvertBinWindowToWidgetCoords (r.X, r.Y, out x, out y);
			Widget.QueueDrawArea (x, y, r.Width, r.Height);
		}

		public void QueueResize (object target, Gtk.TreeIter iter)
		{
			var path = Widget.Model.GetPath (iter);
			Widget.Model.EmitRowChanged (path, iter);
		}

		#endregion
	}
	
	class CustomTreeView: Gtk.TreeView
	{
		WidgetBackend backend;
		TreePath delayedSelection;
		TreeViewColumn delayedSelectionColumn;

		public CustomTreeView (WidgetBackend b)
		{
			backend = b;
			base.DragBegin += (_, __) =>
				delayedSelection = null;
		}

		static CustomTreeView ()
		{
			// On Mac we want to be able to handle the backspace key (labeled "delete") in a custom way
			// through a normal keypressed event but a default binding prevents this from happening because
			// it maps it to "select-cursor-parent". However, that event is also bound to Ctrl+Backspace anyway
			// so we can just kill one of those binding
			if (Platform.IsMac)
				GtkWorkarounds.RemoveKeyBindingFromClass (Gtk.TreeView.GType, Gdk.Key.BackSpace, Gdk.ModifierType.None);
		}

		protected override bool OnButtonPressEvent (EventButton evnt)
		{
			if (Selection.Mode == Gtk.SelectionMode.Multiple) {
				// If we are clicking on already selected row, delay the selection until we are certain that
				// the user is not starting a DragDrop operation. 
				// This is needed to allow user to drag multiple selected rows.
				TreePath treePath;
				TreeViewColumn column;
				GetPathAtPos ((int)evnt.X, (int)evnt.Y, out treePath, out column);

				var ctrlShiftMask = (evnt.State & (Gdk.ModifierType.ShiftMask | Gdk.ModifierType.ControlMask | Gdk.ModifierType.Mod2Mask));
				if (treePath != null && evnt.Button == 1 && this.Selection.PathIsSelected (treePath) && this.Selection.CountSelectedRows() > 1 && ctrlShiftMask == 0) {
					delayedSelection = treePath;
					delayedSelectionColumn = column;
					Selection.SelectFunction = (_, __, ___, ____) => false;
					var result = false;
					try {
						result = base.OnButtonPressEvent (evnt);
					} finally {
						Selection.SelectFunction = (_, __, ___, ____) => true;
					}
					return result;
				}
			}
			return base.OnButtonPressEvent (evnt);
		}

		protected override bool OnButtonReleaseEvent (EventButton evnt)
		{
			// Now, if mouse hadn't moved, we are certain that this was just a click. Proceed as usual.
			if (delayedSelection != null) {
				SetCursor (delayedSelection, delayedSelectionColumn, false);
				delayedSelection = null;
				delayedSelectionColumn = null;
			}
			return base.OnButtonReleaseEvent (evnt);
		}

		protected override void OnDragDataDelete (Gdk.DragContext context)
		{
			// This method is override to avoid the default implementation
			// being called. The default implementation deletes the
			// row being dragged, and we don't want that
			backend.DoDragaDataDelete ();
		}
	}
}
