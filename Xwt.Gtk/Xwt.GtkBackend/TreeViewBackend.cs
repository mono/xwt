// 
// TreeViewBackend.cs
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
using System.Linq;

namespace Xwt.GtkBackend
{
	public class TreeViewBackend: TableViewBackend, ITreeViewBackend
	{
		Gtk.TreePath autoExpandPath;
		uint expandTimer;
		CustomTreeModel customModel;

		protected new ITreeViewEventSink EventSink {
			get { return (ITreeViewEventSink)base.EventSink; }
		}
		
		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is TreeViewEvent) {
				switch ((TreeViewEvent)eventId) {
				case TreeViewEvent.RowActivated:
					Widget.RowActivated += HandleRowActivated;
					break;
				case TreeViewEvent.RowExpanding:
					Widget.TestExpandRow += HandleTestExpandRow;
					break;
				case TreeViewEvent.RowExpanded:
					Widget.RowExpanded += HandleRowExpanded;
					break;
				case TreeViewEvent.RowCollapsing:
					Widget.TestCollapseRow += HandleTestCollapseRow;
					break;
				case TreeViewEvent.RowCollapsed:
					Widget.RowCollapsed += HandleRowCollapsed;
					break;
				}
			}
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is TreeViewEvent) {
				switch ((TreeViewEvent)eventId) {
				case TreeViewEvent.RowActivated:
					Widget.RowActivated -= HandleRowActivated;
					break;
				case TreeViewEvent.RowExpanding:
					Widget.TestExpandRow -= HandleTestExpandRow;;
					break;
				case TreeViewEvent.RowExpanded:
					Widget.RowExpanded -= HandleRowExpanded;;
					break;
				case TreeViewEvent.RowCollapsing:
					Widget.TestCollapseRow -= HandleTestCollapseRow;
					break;
				case TreeViewEvent.RowCollapsed:
					Widget.RowCollapsed -= HandleRowCollapsed;
					break;
				}
			}
		}
		
		void HandleRowExpanded (object o, Gtk.RowExpandedArgs args)
		{
			Gtk.TreeIter it;
			if (Widget.Model.GetIter (out it, args.Path)) {
				CurrentEventRow = GetPositionFromIter (-1, it);
				ApplicationContext.InvokeUserCode (delegate {
					EventSink.OnRowExpanded (GetPositionFromIter (-1, it));
				});
			}
		}
		
		void HandleTestExpandRow (object o, Gtk.TestExpandRowArgs args)
		{
			Gtk.TreeIter it;
			if (Widget.Model.GetIter (out it, args.Path)) {
				CurrentEventRow = GetPositionFromIter (-1, it);
				ApplicationContext.InvokeUserCode (delegate {
					EventSink.OnRowExpanding (GetPositionFromIter (-1, it));
				});
			}
		}

		void HandleRowCollapsed (object o, Gtk.RowCollapsedArgs args)
		{
			Gtk.TreeIter it;
			if (Widget.Model.GetIter (out it, args.Path)) {
				CurrentEventRow = GetPositionFromIter (-1, it);
				ApplicationContext.InvokeUserCode (delegate {
					EventSink.OnRowCollapsed (GetPositionFromIter (-1, it));
				});
			}
		}

		void HandleTestCollapseRow (object o, Gtk.TestCollapseRowArgs args)
		{
			Gtk.TreeIter it;
			if (Widget.Model.GetIter (out it, args.Path)) {
				CurrentEventRow = GetPositionFromIter (-1, it);
				ApplicationContext.InvokeUserCode (delegate {
					EventSink.OnRowCollapsing (GetPositionFromIter (-1, it));
				});
			}
		}

		void HandleRowActivated (object o, Gtk.RowActivatedArgs args)
		{
			Gtk.TreeIter it;
			if (Widget.Model.GetIter (out it, args.Path)) {
				CurrentEventRow = GetPositionFromIter (-1, it);
				ApplicationContext.InvokeUserCode (delegate {
					EventSink.OnRowActivated (GetPositionFromIter (-1, it));
				});
			}
		}
		
		protected override void OnSetDragTarget (Gtk.TargetEntry[] table, Gdk.DragAction actions)
		{
			base.OnSetDragTarget (table, actions);
			Widget.EnableModelDragDest (table, actions);
		}
		
		protected override void OnSetDragSource (Gdk.ModifierType modifierType, Gtk.TargetEntry[] table, Gdk.DragAction actions)
		{
			base.OnSetDragSource (modifierType, table, actions);
			Widget.EnableModelDragSource (modifierType, table, actions);
		}
		
		protected override void OnSetDragStatus (Gdk.DragContext context, int x, int y, uint time, Gdk.DragAction action)
		{
			base.OnSetDragStatus (context, x, y, time, action);
			
			// We are overriding the TreeView methods for handling drag & drop, so we need
			// to manually highlight the selected row
			
			Gtk.TreeViewDropPosition tpos;
			Gtk.TreePath path;
			if (!Widget.GetDestRowAtPos (x, y, out path, out tpos))
				path = null;
			
			if (expandTimer == 0 || !object.Equals (autoExpandPath, path)) {
				if (expandTimer != 0)
					GLib.Source.Remove (expandTimer);
				if (path != null) {
					expandTimer = GLib.Timeout.Add (600, delegate {
						expandTimer = 0;
						Widget.ExpandRow (path, false);
						return false;
					});
				}
				autoExpandPath = path;
			}
			
			if (path != null && action != 0)
				Widget.SetDragDestRow (path, tpos);
			else
				Widget.SetDragDestRow (null, 0);
		}
		
		protected override void Dispose (bool disposing)
		{
			if (expandTimer != 0)
				GLib.Source.Remove (expandTimer);
			base.Dispose (disposing);
		}
		
		public void SetSource (ITreeDataSource source, IBackend sourceBackend)
		{
			TreeStoreBackend b = sourceBackend as TreeStoreBackend;
			customModel = null;
			if (b == null) {
				customModel = new CustomTreeModel (source);
				Widget.Model = customModel.Store;
			} else
				Widget.Model = b.Store;
		}

		public bool AnimationsEnabled { get; set; }

		public TreePosition[] SelectedRows {
			get {
				var rows = Widget.Selection.GetSelectedRows ();
				TreePosition[] sel = new TreePosition [rows.Length];
				for (int i = 0; i < rows.Length; i++) {
					Gtk.TreeIter it;
					Widget.Model.GetIter (out it, rows[i]);
					sel[i] = GetPositionFromIter (-1, it);
				}
				return sel;
			}
		}

		public TreePosition FocusedRow {
			get {
				Gtk.TreePath path;
				Gtk.TreeViewColumn column;
				Widget.GetCursor (out path, out column);

				Gtk.TreeIter it;
				if (path != null && Widget.Model.GetIter (out it, path))
					return GetPositionFromIter (-1, it);
				return null;
			}
			set {
				Gtk.TreePath path = new Gtk.TreePath(new [] { int.MaxValue }); // set invalid path to unfocus
				if (value != null)
					path = Widget.Model.GetPath (GetIterFromPosition (value));
				Widget.SetCursor (path, null, false);
			}
		}

		public TreePosition CurrentEventRow {
			get;
			internal set;
		}
		
		public void SelectRow (TreePosition pos)
		{
			Widget.Selection.SelectIter (GetIterFromPosition (pos));
		}
		
		public void UnselectRow (TreePosition pos)
		{
			Widget.Selection.UnselectIter (GetIterFromPosition (pos));
		}
		
		public bool IsRowSelected (TreePosition pos)
		{
			return Widget.Selection.IterIsSelected (GetIterFromPosition (pos));
		}
		
		public bool IsRowExpanded (TreePosition pos)
		{
			return Widget.GetRowExpanded (Widget.Model.GetPath (GetIterFromPosition (pos)));
		}
		
		public void ExpandRow (TreePosition pos, bool expandedChildren)
		{
			Widget.ExpandRow (Widget.Model.GetPath (GetIterFromPosition (pos)), expandedChildren);
		}
		
		public void CollapseRow (TreePosition pos)
		{
			Widget.CollapseRow (Widget.Model.GetPath (GetIterFromPosition (pos)));
		}
		
		public void ScrollToRow (TreePosition pos)
		{
			ScrollToRow (GetIterFromPosition (pos));
		}
		
		public void ExpandToRow (TreePosition pos)
		{
			Widget.ExpandToPath (Widget.Model.GetPath (GetIterFromPosition (pos)));
		}

		public bool BorderVisible {
			get {
				return ScrolledWindow.ShadowType != Gtk.ShadowType.None;
			}
			set {
				ScrolledWindow.ShadowType = value ? Gtk.ShadowType.In : Gtk.ShadowType.None;
			}
		}

		public bool HeadersVisible {
			get {
				return Widget.HeadersVisible;
			}
			set {
				Widget.HeadersVisible = value;
			}
		}

		public bool UseAlternatingRowColors {
			get {
				return Widget.RulesHint;
			}
			set {
				Widget.RulesHint = value;
			}
		}
		
		public bool GetDropTargetRow (double x, double y, out RowDropPosition pos, out TreePosition nodePosition)
		{
			Gtk.TreeViewDropPosition tpos;
			Gtk.TreePath path;
			if (!Widget.GetDestRowAtPos ((int)x, (int)y, out path, out tpos)) {
				pos = RowDropPosition.Into;
				nodePosition = null;
				return false;
			}
			
			Gtk.TreeIter it;
			Widget.Model.GetIter (out it, path);
			nodePosition = GetPositionFromIter (-1, it);
			switch (tpos) {
			case Gtk.TreeViewDropPosition.After: pos = RowDropPosition.After; break;
			case Gtk.TreeViewDropPosition.Before: pos = RowDropPosition.Before; break;
			default: pos = RowDropPosition.Into; break;
			}
			return true;
		}

		public TreePosition GetRowAtPosition (Point p)
		{
			Gtk.TreePath path = GetPathAtPosition (p);
			if (path != null) {
				Gtk.TreeIter iter;
				Widget.Model.GetIter (out iter, path);
				return GetPositionFromIter (-1, iter);
			}
			return null;
		}

		public Rectangle GetCellBounds (TreePosition pos, CellView cell, bool includeMargin)
		{
			var col = GetCellColumn (cell);
			var cr = GetCellRenderer (cell);
			Gtk.TreeIter iter = GetIterFromPosition (pos);

			var rect = includeMargin ? ((ICellRendererTarget)this).GetCellBackgroundBounds (col, cr, iter) : ((ICellRendererTarget)this).GetCellBounds (col, cr, iter);
			return rect;
		}

		public Rectangle GetRowBounds (TreePosition pos, bool includeMargin)
		{
			Gtk.TreeIter iter = GetIterFromPosition (pos);
			Rectangle rect = includeMargin ? GetRowBackgroundBounds (iter) : GetRowBounds (iter);
			return rect;
		}

		public override void SetCurrentEventRow (string path)
		{
			var treeFrontend = (TreeView)Frontend;

			TreePosition toggledItem = null;

			var pathParts = path.Split (':').Select (int.Parse);

			foreach (int pathPart in pathParts) {
				toggledItem = treeFrontend.DataSource.GetChild (toggledItem, pathPart);
			}

			CurrentEventRow = toggledItem;
		}

		TreePosition GetPositionFromIter (int treeVersion, Gtk.TreeIter iter)
		{
			if (customModel != null) {
				TreePosition pos;
				customModel.NodeFromIter (iter, out pos);
				return pos;
			}
			return new IterPos (treeVersion, iter);
		}

		Gtk.TreeIter GetIterFromPosition (TreePosition position)
		{
			if (customModel != null)
				return customModel.IterFromNode (position);
			return ((IterPos)position).Iter;
		}
	}
}

