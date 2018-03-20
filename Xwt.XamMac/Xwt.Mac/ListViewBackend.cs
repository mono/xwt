// 
// ListViewBackend.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//       Jan Strnadek <jan.strnadek@gmail.com>
//              - GetAtRowPosition
//              - NSTableView for mouse events
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
using System.Linq;
using AppKit;
using CoreGraphics;
using Foundation;
using Xwt.Backends;

namespace Xwt.Mac
{
	public class ListViewBackend: TableViewBackend<NSTableView, IListViewEventSink>, IListViewBackend
	{
		class ListDelegate: NSTableViewDelegate
		{
			public ListViewBackend Backend;
			
			public override nfloat GetRowHeight (NSTableView tableView, nint row)
			{
				// GetView() and GetRowView() can't be used here, hence we use cached
				// sizes calculated by the backend or calcuate the height using the template view
				nfloat height = Backend.RowHeights[(int)row];
				if (height <= -1)
					height = Backend.RowHeights [(int)row] = Backend.CalcRowHeight (row, false);
				return height;
			}

			public override NSTableRowView CoreGetRowView (NSTableView tableView, nint row)
			{
				return tableView.GetRowView (row, false) ?? new TableRowView ();
			}

			public override NSView GetViewForItem (NSTableView tableView, NSTableColumn tableColumn, nint row)
			{
				var col = tableColumn as TableColumn;
				var cell = tableView.MakeView (tableColumn.Identifier, this) as CompositeCell;
				if (cell == null)
					cell = col.CreateNewView ();
				cell.ObjectValue = NSNumber.FromNInt (row);
				return cell;
			}

			public override nfloat GetSizeToFitColumnWidth (NSTableView tableView, nint column)
			{
				var tableColumn = Backend.Columns[(int)column] as TableColumn;
				var width = tableColumn.HeaderCell.CellSize.Width;

				CompositeCell templateCell = null;
				for (int i = 0; i < tableView.RowCount; i++) {
					var cellView = tableView.GetView (column, i, false) as CompositeCell;
					if (cellView == null) { // use template for invisible rows
						cellView = templateCell ?? (templateCell = (tableColumn as TableColumn)?.DataView?.Copy () as CompositeCell);
						if (cellView != null)
							cellView.ObjectValue = NSNumber.FromInt32 (i);
					}
					width = (nfloat)Math.Max (width, cellView.FittingSize.Width);
				}
				return width;
			}

			public override NSIndexSet GetSelectionIndexes(NSTableView tableView, NSIndexSet proposedSelectionIndexes)
			{
				return Backend.SelectionMode != SelectionMode.None ? proposedSelectionIndexes : new NSIndexSet();
			}
		}

		IListDataSource source;
		ListSource tsource;

		protected override NSTableView CreateView ()
		{
			var listView = new NSTableViewBackend (this);
			listView.Delegate = new ListDelegate { Backend = this };
			return listView;
		}

		protected override string SelectionChangeEventName {
			get { return "NSTableViewSelectionDidChangeNotification"; }
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is ListViewEvent) {
				switch ((ListViewEvent)eventId) {
				case ListViewEvent.RowActivated:
					Table.DoubleClick += HandleDoubleClick;
					break;
				}
			}
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is ListViewEvent) {
				switch ((ListViewEvent)eventId) {
				case ListViewEvent.RowActivated:
					Table.DoubleClick -= HandleDoubleClick;
					Table.DoubleAction = null;
					break;
				}
			}
		}

		void HandleDoubleClick (object sender, EventArgs e)
		{
			var cr = Table.ClickedRow;
			if (cr >= 0) {
				ApplicationContext.InvokeUserCode (delegate {
					((IListViewEventSink)EventSink).OnRowActivated ((int)cr);
				});
			}
		}
		
		public virtual void SetSource (IListDataSource source, IBackend sourceBackend)
		{
			this.source = source;

			RowHeights = new List<nfloat> ();
			for (int i = 0; i < source.RowCount; i++)
				RowHeights.Add (-1);

			tsource = new ListSource (source);
			Table.DataSource = tsource;

			//TODO: Reloading single rows would be slightly more efficient.
			//      According to NSTableView.ReloadData() documentation,
			//      only the visible rows are reloaded.
			source.RowInserted += (sender, e) => {
				RowHeights.Insert (e.Row, -1);
				Table.ReloadData ();
			};
			source.RowDeleted += (sender, e) => {
				RowHeights.RemoveAt (e.Row);
				Table.ReloadData ();
			};
			source.RowChanged += (sender, e) => {
				UpdateRowHeight (e.Row);
				Table.ReloadData (NSIndexSet.FromIndex (e.Row), NSIndexSet.FromNSRange (new NSRange (0, Table.ColumnCount)));
			};
			source.RowsReordered += (sender, e) => { RowHeights.Clear (); Table.ReloadData (); };
		}

		public override void InvalidateRowHeight (object pos)
		{
			UpdateRowHeight((int)pos);
		}
		
		List<nfloat> RowHeights = new List<nfloat> ();
		bool updatingRowHeight;
		public void UpdateRowHeight (nint row)
		{
			if (updatingRowHeight)
				return;
			RowHeights[(int)row] = CalcRowHeight (row);
			Table.NoteHeightOfRowsWithIndexesChanged (NSIndexSet.FromIndex (row));
		}

		nfloat CalcRowHeight (nint row, bool tryReuse = true)
		{
			updatingRowHeight = true;
			var height = Table.RowHeight;

			for (int i = 0; i < Columns.Count; i++) {
				CompositeCell cell = tryReuse ? Table.GetView (i, row, false) as CompositeCell : null;
				if (cell == null) {
					cell = (Columns [i] as TableColumn)?.DataView as CompositeCell;
					cell.ObjectValue = NSNumber.FromNInt (row);
					height = (nfloat)Math.Max (height, cell.FittingSize.Height);
				} else {
					height = (nfloat)Math.Max (height, cell.GetRequiredHeightForWidth (cell.Frame.Width));
				}
			}
			updatingRowHeight = false;
			return height;
		}
		
		public int[] SelectedRows {
			get {
				int[] sel = new int [Table.SelectedRowCount];
				if (sel.Length > 0) {
					int i = 0;
					foreach (int r in Table.SelectedRows)
						sel [i++] = r;
				}
				return sel;
			}
		}

		public int FocusedRow {
			get {
				if (Table.SelectedRowCount > 0)
					return (int)Table.SelectedRows.FirstIndex;
				return -1;
			}
			set {
				SelectRow (value);
			}
		}
		
		public void SelectRow (int pos)
		{
			Table.SelectRow (pos, Table.AllowsMultipleSelection);
		}
		
		public void UnselectRow (int pos)
		{
			Table.DeselectRow (pos);
		}
		
		public override object GetValue (object pos, int nField)
		{
			return source.GetValue ((int)pos, nField);
		}
		
		public override void SetValue (object pos, int nField, object value)
		{
			source.SetValue ((int)pos, nField, value);
		}

		public int CurrentEventRow { get; internal set; }

		public override void SetCurrentEventRow (object pos)
		{
			CurrentEventRow = (int)pos;
		}

		public int GetRowAtPosition (Point p)
		{
			return (int) Table.GetRow (new CGPoint ((nfloat)p.X, (nfloat)p.Y));
		}
	}
	
	class TableRow: NSObject, ITablePosition
	{
		public int Row;
		
		public object Position {
			get { return Row; }
		}
	}
	
	class ListSource: NSTableViewDataSource
	{
		IListDataSource source;
		
		public ListSource (IListDataSource source)
		{
			this.source = source;
		}

		public override bool AcceptDrop (NSTableView tableView, NSDraggingInfo info, nint row, NSTableViewDropOperation dropOperation)
		{
			return false;
		}

		public override string[] FilesDropped (NSTableView tableView, NSUrl dropDestination, NSIndexSet indexSet)
		{
			return new string [0];
		}

		public override NSObject GetObjectValue (NSTableView tableView, NSTableColumn tableColumn, nint row)
		{
			return NSNumber.FromInt32 ((int)row);
		}

		public override nint GetRowCount (NSTableView tableView)
		{
			return source.RowCount;
		}

		public override void SetObjectValue (NSTableView tableView, NSObject theObject, NSTableColumn tableColumn, nint row)
		{
		}

		public override void SortDescriptorsChanged (NSTableView tableView, NSSortDescriptor[] oldDescriptors)
		{
		}

		public override NSDragOperation ValidateDrop (NSTableView tableView, NSDraggingInfo info, nint row, NSTableViewDropOperation dropOperation)
		{
			return NSDragOperation.None;
		}

		public override bool WriteRows (NSTableView tableView, NSIndexSet rowIndexes, NSPasteboard pboard)
		{
			return false;
		}
	}
}

