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
using System.Collections.Generic;

#if MONOMAC
using MonoMac.AppKit;
using MonoMac.Foundation;
using nint = System.Int32;
using nfloat = System.Single;
using CGPoint = System.Drawing.PointF;
#else
using AppKit;
using Foundation;
using CoreGraphics;
#endif

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
				var height = outlineView.RowHeight;
				var row = outlineView.RowForItem (item);
				for (int i = 0; i < outlineView.ColumnCount; i++) {
					var cell = outlineView.GetCell (i, row);
					if (cell != null)
						height = (nfloat) Math.Max (height, cell.CellSize.Height);
				}
				return height;
			}
		}
		
		NSOutlineView Tree {
			get { return (NSOutlineView) Table; }
		}
		
		protected override NSTableView CreateView ()
		{
			var t = new OutlineViewBackend (EventSink, ApplicationContext);
			t.Delegate = new TreeDelegate () { Backend = this };
			return t;
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
			tsource = new TreeSource (source);
			Tree.DataSource = tsource;

			source.NodeInserted += (sender, e) => Tree.ReloadItem (tsource.GetItem (source.GetParent(e.Node)), true);
			source.NodeDeleted += (sender, e) => Tree.ReloadItem (tsource.GetItem (e.Node), true);
			source.NodeChanged += (sender, e) => Tree.ReloadItem (tsource.GetItem (e.Node), false);
			source.NodesReordered += (sender, e) => Tree.ReloadItem (tsource.GetItem (e.Node), true);
		}
		
		public override object GetValue (object pos, int nField)
		{
			return source.GetValue ((TreePosition)pos, nField);
		}

		public override void SetValue (object pos, int nField, object value)
		{
			source.SetValue ((TreePosition)pos, nField, value);
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
			while (p != null) {
				var it = tsource.GetItem (p);
				if (it == null)
					break;
				Tree.ExpandItem (it, false);
				p = source.GetParent (p);
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
		
		// TODO: remove unused positions
		Dictionary<TreePosition,TreeItem> items = new Dictionary<TreePosition, TreeItem> ();
		
		public TreeSource (ITreeDataSource source)
		{
			this.source = source;

			source.NodeInserted += (sender, e) => {
				if (!items.ContainsKey (e.Node))
					items.Add (e.Node, new TreeItem { Position = e.Node });
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

