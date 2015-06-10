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
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnSelectionChanged ();
			});
		}
		
		public object AddColumn (ListViewColumn col)
		{
			Gtk.TreeViewColumn tc = new Gtk.TreeViewColumn ();
			tc.Title = col.Title;
			tc.Resizable = col.CanResize;
			tc.Alignment = col.Alignment.ToGtkAlignment ();
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
			else
				tc.Widget = CellUtil.CreateCellRenderer (ApplicationContext, col.HeaderView);
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
			return cellViews [cell].Column;
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

			Gdk.Rectangle rect = Widget.GetCellArea (path, col);

			int x = 0;
			int th = 0;
			CellRenderer[] renderers = col.GetCellRenderers();

			foreach (CellRenderer cr in renderers) {
				int sp, wi, he, xo, yo;
				col.CellGetSize (rect, out xo, out yo, out wi, out he);
				col.CellGetPosition (cr, out sp, out wi);
				Gdk.Rectangle crect = new Gdk.Rectangle (x, rect.Y, wi, rect.Height);
				#if !XWT_GTK3
				cr.GetSize (Widget, ref crect, out xo, out yo, out wi, out he);
				#endif
				if (cr == cra) {
					Widget.ConvertBinWindowToWidgetCoords (rect.X + x, rect.Y, out xo, out yo);
					// There seems to be a 1px vertical padding
					yo++; rect.Height -= 2;
					return new Rectangle (xo, yo + 1, wi, rect.Height - 2);
				}
				if (cr != renderers [renderers.Length - 1])
					x += crect.Width + col.Spacing + 1;
				else
					x += wi + 1;
				if (he > th) th = he;
			}
			return Rectangle.Zero;
		}

		Rectangle ICellRendererTarget.GetCellBackgroundBounds (object target, Gtk.CellRenderer cr, Gtk.TreeIter iter)
		{
			var col = (TreeViewColumn)target;
			var path = Widget.Model.GetPath (iter);
			var a = Widget.GetBackgroundArea (path, (TreeViewColumn)target);
			int x, y, w, h;
			col.CellGetSize (a, out x, out y, out w, out h);
			return new Rectangle (a.X + x, a.Y + y, w, h);
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
			int cellx, celly;
			cx = cy = 0;
			it = Gtk.TreeIter.Zero;

			if (!Widget.GetPathAtPos (ex, ey, out path, out col, out cellx, out celly))
				return false;

			if (!Widget.Model.GetIterFromString (out it, path.ToString ()))
				return false;

			int sp, w;
			if (col.CellGetPosition (r, out sp, out w)) {
				if (cellx >= sp && cellx < sp + w) {
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

		#endregion
	}
	
	class CustomTreeView: Gtk.TreeView
	{
		WidgetBackend backend;
		
		public CustomTreeView (WidgetBackend b)
		{
			backend = b;
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

