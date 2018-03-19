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
using System.Collections.Generic;
using AppKit;
using CoreGraphics;
using Foundation;
using Xwt.Backends;

namespace Xwt.Mac
{
	public class TreeViewBackend: TableViewBackend<NSOutlineView,ITreeViewEventSink>, ITreeViewBackend
	{
		ITreeDataSource source;
		TreeSource tsource;

		class TreeDelegate: NSOutlineViewDelegate
		{
			public TreeViewBackend Backend;

			public override void ItemDidExpand (NSNotification notification)
			{
				Backend.EventSink.OnRowExpanded (((TreeItem)notification.UserInfo["NSObject"]).Position);
			}

			public override void ItemWillExpand (NSNotification notification)
			{
				Backend.EventSink.OnRowExpanding (((TreeItem)notification.UserInfo["NSObject"]).Position);
			}

			public override void ItemDidCollapse (NSNotification notification)
			{
				Backend.EventSink.OnRowCollapsed (((TreeItem)notification.UserInfo["NSObject"]).Position);
			}

			public override void ItemWillCollapse (NSNotification notification)
			{
				Backend.EventSink.OnRowCollapsing (((TreeItem)notification.UserInfo["NSObject"]).Position);
			}

			public override nfloat GetRowHeight (NSOutlineView outlineView, NSObject item)
			{
				nfloat height;
				var treeItem = (TreeItem)item;
				if (!Backend.RowHeights.TryGetValue (treeItem, out height) || height <= 0)
					height = Backend.RowHeights [treeItem] = Backend.CalcRowHeight (treeItem, false);
				
				return height;
			}

			public override NSTableRowView RowViewForItem (NSOutlineView outlineView, NSObject item)
			{
				return outlineView.GetRowView (outlineView.RowForItem (item), false) ?? new TableRowView ();
			}

			public override NSView GetView (NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
			{
				var col = tableColumn as TableColumn;
				var cell = outlineView.MakeView (tableColumn.Identifier, this) as CompositeCell;
				if (cell == null)
					cell = col.CreateNewView ();
				if (cell.ObjectValue != item)
					cell.ObjectValue = item;
				return cell;
			}

			public override nfloat GetSizeToFitColumnWidth (NSOutlineView outlineView, nint column)
			{
				var tableColumn = Backend.Columns[(int)column] as TableColumn;
				var width = tableColumn.HeaderCell.CellSize.Width;

				CompositeCell templateCell = null;
				for (int i = 0; i < outlineView.RowCount; i++) {
					var cellView = outlineView.GetView (column, i, false) as CompositeCell;
					if (cellView == null) { // use template for invisible rows
						cellView = templateCell ?? (templateCell = (tableColumn as TableColumn)?.DataView?.Copy () as CompositeCell);
						if (cellView != null)
							cellView.ObjectValue = outlineView.ItemAtRow (i);
					}
					if (cellView != null) {
						if (column == 0) // first column contains expanders
							width = (nfloat)Math.Max (width, cellView.Frame.X + cellView.FittingSize.Width);
						else
							width = (nfloat)Math.Max (width, cellView.FittingSize.Width);
					}
				}
				return width;
			}

			public override NSIndexSet GetSelectionIndexes(NSOutlineView outlineView, NSIndexSet proposedSelectionIndexes)
			{
				return Backend.SelectionMode != SelectionMode.None ? proposedSelectionIndexes : new NSIndexSet();
			}
		}
		
		OutlineViewBackend Tree {
			get { return (OutlineViewBackend) Table; }
		}
		
		protected override NSTableView CreateView ()
		{
			var t = new OutlineViewBackend (this);
			t.Delegate = new TreeDelegate () { Backend = this };
			return t;
		}

		public bool AnimationsEnabled {
			get {
				return Tree.AnimationsEnabled;
			}
			set {
				Tree.AnimationsEnabled = value;
			}
		}
		
		protected override string SelectionChangeEventName {
			get { return "NSOutlineViewSelectionDidChangeNotification"; }
		}

		public TreePosition CurrentEventRow { get; internal set; }

		public override NSTableColumn AddColumn (ListViewColumn col)
		{
			NSTableColumn tcol = base.AddColumn (col);
			if (Tree.OutlineTableColumn == null)
				Tree.OutlineTableColumn = tcol;
			return tcol;
		}
		
		public void SetSource (ITreeDataSource source, IBackend sourceBackend)
		{
			this.source = source;
			RowHeights.Clear ();
			tsource = new TreeSource (source);
			Tree.DataSource = tsource;

			source.NodeInserted += (sender, e) => {
				var parent = tsource.GetItem (source.GetParent (e.Node));
				Tree.ReloadItem (parent, parent == null || Tree.IsItemExpanded (parent));
			};
			source.NodeDeleted += (sender, e) => {
				var parent = tsource.GetItem (e.Node);
				var item = tsource.GetItem(e.Child);
				if (item != null)
					RowHeights.Remove (null);
				Tree.ReloadItem (parent, parent == null || Tree.IsItemExpanded (parent));
			};
			source.NodeChanged += (sender, e) => {
				var item = tsource.GetItem (e.Node);
				if (item != null) {
					Tree.ReloadItem (item, false);
					UpdateRowHeight (item);
				}
			};
			source.NodesReordered += (sender, e) => {
				var parent = tsource.GetItem (e.Node);
				Tree.ReloadItem (parent, parent == null || Tree.IsItemExpanded (parent));
			};
			source.Cleared += (sender, e) =>
			{
				Tree.ReloadData ();
				RowHeights.Clear ();
			};
		}
		
		public override object GetValue (object pos, int nField)
		{
			return source.GetValue ((TreePosition)pos, nField);
		}

		public override void SetValue (object pos, int nField, object value)
		{
			source.SetValue ((TreePosition)pos, nField, value);
		}

		public override void InvalidateRowHeight (object pos)
		{
			UpdateRowHeight (tsource.GetItem((TreePosition)pos));
		}

		Dictionary<TreeItem, nfloat> RowHeights = new Dictionary<TreeItem, nfloat> ();
		bool updatingRowHeight;

		void UpdateRowHeight (TreeItem pos)
		{
			if (updatingRowHeight)
				return;
			var row = Tree.RowForItem (pos);
			if (row >= 0) {
				// calculate new height now by reusing the visible cell to avoid using the template cell with unnecessary data reloads
				// NOTE: cell reusing is not supported in Delegate.GetRowHeight and would require an other data reload to the template cell
				RowHeights[pos] = CalcRowHeight (pos);
				Table.NoteHeightOfRowsWithIndexesChanged (NSIndexSet.FromIndex (row));
			} else // Invalidate the height, to force recalculation in Delegate.GetRowHeight
				RowHeights[pos] = -1;
		}

		nfloat CalcRowHeight (TreeItem pos, bool tryReuse = true)
		{
			updatingRowHeight = true;
			var height = Table.RowHeight;
			var row = Tree.RowForItem (pos);

			for (int i = 0; i < Columns.Count; i++) {
				CompositeCell cell = tryReuse && row >= 0 ? Tree.GetView (i, row, false) as CompositeCell : null;
				if (cell == null) {
					cell = (Columns [i] as TableColumn)?.DataView as CompositeCell;
					cell.ObjectValue = pos;
					height = (nfloat)Math.Max (height, cell.FittingSize.Height);
				} else {
					height = (nfloat)Math.Max (height, cell.GetRequiredHeightForWidth (cell.Frame.Width));
				}
			}
			updatingRowHeight = false;
			return height;
		}
		
		public TreePosition[] SelectedRows {
			get {
				TreePosition[] res = new TreePosition [Table.SelectedRowCount];
				int n = 0;
				if (Table.SelectedRowCount > 0) {
					foreach (var i in Table.SelectedRows) {
						res [n] = ((TreeItem)Tree.ItemAtRow ((int)i)).Position;
					}
				}
				return res;
			}
		}

		public TreePosition FocusedRow {
			get {
				if (Table.SelectedRowCount > 0)
					return ((TreeItem)Tree.ItemAtRow ((int)Table.SelectedRows.FirstIndex)).Position;
				return null;
			}
			set {
				SelectRow (value);
				ScrollToRow (value);
			}
		}

		public override void SetCurrentEventRow (object pos)
		{
			CurrentEventRow = (TreePosition)pos;
		}

		public void SelectRow (TreePosition pos)
		{
			var it = tsource.GetItem (pos);
			if (it != null)
				Table.SelectRow ((int)Tree.RowForItem (it), Table.AllowsMultipleSelection);
		}

		public void UnselectRow (TreePosition pos)
		{
			var it = tsource.GetItem (pos);
			if (it != null)
				Table.DeselectRow (Tree.RowForItem (it));
		}
		
		public bool IsRowSelected (TreePosition pos)
		{
			var it = tsource.GetItem (pos);
			return it != null && Table.IsRowSelected (Tree.RowForItem (it));
		}
		
		public bool IsRowExpanded (TreePosition pos)
		{
			var it = tsource.GetItem (pos);
			return it != null && Tree.IsItemExpanded (it);
		}
		
		public void ExpandRow (TreePosition pos, bool expandChildren)
		{
			var it = tsource.GetItem (pos);
			if (it != null)
				Tree.ExpandItem (it, expandChildren);
		}
		
		public void CollapseRow (TreePosition pos)
		{
			var it = tsource.GetItem (pos);
			if (it != null)
				Tree.CollapseItem (it);
		}
		
		public void ScrollToRow (TreePosition pos)
		{
			var it = tsource.GetItem (pos);
			if (it != null)
				ScrollToRow ((int)Tree.RowForItem (it));
		}
		
		public void ExpandToRow (TreePosition pos)
		{
			var p = source.GetParent (pos);
			if (p == null)
				return;
			var s = new Stack<TreePosition> ();
			while (p != null) {
				s.Push (p);
				p = source.GetParent (p);
			}

			while (s.Count > 0) {
				var it = tsource.GetItem (s.Pop ());
				if (it == null)
					break;
				Tree.ExpandItem (it, false);
			}
		}

		public TreePosition GetRowAtPosition (Point p)
		{
			var row = Table.GetRow (new CGPoint ((float)p.X, (float)p.Y));
			return row >= 0 ? ((TreeItem)Tree.ItemAtRow (row)).Position : null;
		}

		public Rectangle GetCellBounds (TreePosition pos, CellView cell, bool includeMargin)
		{
			var it = tsource.GetItem (pos);
			if (it == null)
				return Rectangle.Zero;
			var row = (int)Tree.RowForItem (it);
			return GetCellBounds (row, cell, includeMargin);
		}

		public Rectangle GetRowBounds (TreePosition pos, bool includeMargin)
		{
			var it = tsource.GetItem (pos);
			if (it == null)
				return Rectangle.Zero;
			var row = (int)Tree.RowForItem (it);
			return GetRowBounds (row, includeMargin);
		}
		
		public bool GetDropTargetRow (double x, double y, out RowDropPosition pos, out TreePosition nodePosition)
		{
			// Get row
			nint row = Tree.GetRow(new CGPoint ((nfloat)x, (nfloat)y));
			pos = RowDropPosition.Into;
			nodePosition = null;
			if (row >= 0) {
				nodePosition = ((TreeItem)Tree.ItemAtRow (row)).Position;
			}
			return nodePosition != null;
		}
		
/*		protected override void OnDragOverCheck (NSDraggingInfo di, DragOverCheckEventArgs args)
		{
			base.OnDragOverCheck (di, args);
			var row = Tree.GetRow (new CGPoint (di.DraggingLocation.X, di.DraggingLocation.Y));
			if (row != -1) {
				var item = Tree.ItemAtRow (row);
				Tree.SetDropItem (item, row);
			}
		}
		
		protected override void OnDragOver (NSDraggingInfo di, DragOverEventArgs args)
		{
			base.OnDragOver (di, args);
			var p = Tree.ConvertPointFromView (di.DraggingLocation, null);
			var row = Tree.GetRow (p);
			if (row != -1) {
				Tree.SetDropRowDropOperation (row, NSTableViewDropOperation.On);
				var item = Tree.ItemAtRow (row);
				Tree.SetDropItem (item, 0);
			}
		}*/
	}
	
	class TreeItem: NSObject, ITablePosition
	{
		public TreePosition Position;
		
		public TreeItem ()
		{
		}
		
		public TreeItem (IntPtr p): base (p)
		{
		}
		
		object ITablePosition.Position {
			get { return Position; }
		}

		public override NSObject Copy ()
		{
			return new TreeItem { Position = this.Position };
		}
	}
	
	
	class TreeSource: NSOutlineViewDataSource
	{
		ITreeDataSource source;

		Dictionary<TreePosition,TreeItem> items = new Dictionary<TreePosition, TreeItem> ();

		public TreeSource (ITreeDataSource source)
		{
			this.source = source;

			source.NodeInserted += (sender, e) => {
				if (!items.ContainsKey (e.Node))
					items.Add (e.Node, new TreeItem { Position = e.Node });
			};
			source.NodeDeleted += (sender, e) => {
				items.Remove (e.Child);
			};
			source.Cleared += (sender, e) => {
				items.Clear ();
			};
		}
		
		public TreeItem GetItem (TreePosition pos)
		{
			if (pos == null)
				return null;
			TreeItem it;
			items.TryGetValue (pos, out it);
			return it;
		}
		
		public override bool AcceptDrop (NSOutlineView outlineView, NSDraggingInfo info, NSObject item, nint index)
		{
			return false;
		}

		public override string[] FilesDropped (NSOutlineView outlineView, NSUrl dropDestination, NSArray items)
		{
			throw new NotImplementedException ();
		}

		public override NSObject GetChild (NSOutlineView outlineView, nint childIndex, NSObject item)
		{
			var treeItem = (TreeItem) item;
			var pos = source.GetChild (treeItem != null ? treeItem.Position : null, (int) childIndex);
			if (pos != null) {
				TreeItem res;
				if (!items.TryGetValue (pos, out res))
					items [pos] = res = new TreeItem () { Position = pos };
				return res;
			}
			else
				return null;
		}

		public override nint GetChildrenCount (NSOutlineView outlineView, NSObject item)
		{
			var it = (TreeItem) item;
			return source.GetChildrenCount (it != null ? it.Position : null);
		}

		public override NSObject GetObjectValue (NSOutlineView outlineView, NSTableColumn forTableColumn, NSObject byItem)
		{
			return byItem;
		}

		public override void SetObjectValue (NSOutlineView outlineView, NSObject theObject, NSTableColumn tableColumn, NSObject item)
		{
		}

		public override bool ItemExpandable (NSOutlineView outlineView, NSObject item)
		{
			return GetChildrenCount (outlineView, item) > 0;
		}

		public override NSObject ItemForPersistentObject (NSOutlineView outlineView, NSObject theObject)
		{
			return null;
		}

		public override bool OutlineViewwriteItemstoPasteboard (NSOutlineView outlineView, NSArray items, NSPasteboard pboard)
		{
			return false;
		}

		public override NSObject PersistentObjectForItem (NSOutlineView outlineView, NSObject item)
		{
			return null;
		}

		public override void SortDescriptorsChanged (NSOutlineView outlineView, NSSortDescriptor[] oldDescriptors)
		{
		}

		public override NSDragOperation ValidateDrop (NSOutlineView outlineView, NSDraggingInfo info, NSObject item, nint index)
		{
			return NSDragOperation.None;
		}
		
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}
	}
}

