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
using MonoMac.AppKit;
using MonoMac.Foundation;
using Xwt.Backends;
using System.Collections.Generic;
using System.Reflection;

namespace Xwt.Mac
{
	public class TreeViewBackend: TableViewBackend<NSOutlineView,ITreeViewEventSink>, ITreeViewBackend
	{
		ITreeViewSource source;
		TreeSource tsource;
		
		NSOutlineView Tree {
			get { return (NSOutlineView) Table; }
		}
		
		protected override NSTableView CreateView ()
		{
			return new NSOutlineView ();
		}
		
		protected override string SelectionChangeEventName {
			get { return "NSOutlineViewSelectionDidChangeNotification"; }
		}
		
		public override object AddColumn (ListViewColumn col)
		{
			NSTableColumn tcol = (NSTableColumn) base.AddColumn (col);
			if (Tree.OutlineTableColumn == null)
				Tree.OutlineTableColumn = tcol;
			return tcol;
		}
		
		public void SetSource (ITreeViewSource source, IBackend sourceBackend)
		{
			this.source = source;
			tsource = new TreeSource (source);
			Tree.DataSource = tsource;
		}
		
		public override object GetValue (object pos, int nField)
		{
			return source.GetValue ((TreePosition)pos, nField);
		}
		
		public TreePosition[] SelectedItems {
			get {
				TreePosition[] res = new TreePosition [Table.SelectedRowCount];
				int n = 0;
				foreach (var i in Table.SelectedRows) {
					res [n] = ((TreeItem)Tree.ItemAtRow ((int)i)).Position;
				}
				return res;
			}
		}

		public void SelectItem (TreePosition pos)
		{
			var it = tsource.GetItem (pos);
			if (it != null)
				Table.SelectRow (Tree.RowForItem (it), false);
		}

		public void UnselectItem (TreePosition pos)
		{
			var it = tsource.GetItem (pos);
			if (it != null)
				Table.DeselectRow (Tree.RowForItem (it));
		}
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
	}
	
	
	class TreeSource: NSOutlineViewDataSource
	{
		ITreeViewSource source;
		
		// TODO: remove unused positions
		Dictionary<TreePosition,TreeItem> items = new Dictionary<TreePosition, TreeItem> ();
		
		public TreeSource (ITreeViewSource source)
		{
			this.source = source;
		}
		
		public TreeItem GetItem (TreePosition pos)
		{
			TreeItem it;
			items.TryGetValue (pos, out it);
			return it;
		}
		
		public override bool AcceptDrop (NSOutlineView outlineView, NSDraggingInfo info, NSObject item, int index)
		{
			return false;
		}

		public override string[] FilesDropped (NSOutlineView outlineView, NSUrl dropDestination, NSArray items)
		{
			throw new NotImplementedException ();
		}

		public override NSObject GetChild (NSOutlineView outlineView, int childIndex, NSObject ofItem)
		{
			var item = (TreeItem) ofItem;
			var pos = source.GetChild (item != null ? item.Position : null, childIndex);
			if (pos != null) {
				TreeItem res;
				if (!items.TryGetValue (pos, out res))
					items [pos] = res = new TreeItem () { Position = pos };
				return res;
			}
			else
				return null;
		}

		public override int GetChildrenCount (NSOutlineView outlineView, NSObject item)
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

		public override NSDragOperation ValidateDrop (NSOutlineView outlineView, NSDraggingInfo info, NSObject item, int index)
		{
			return NSDragOperation.None;
		}
		
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}
	}
}

