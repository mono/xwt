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
using MonoMac.AppKit;
using Xwt.Backends;
using System.Collections.Generic;
using MonoMac.Foundation;

namespace Xwt.Mac
{
	public class ListViewBackend: TableViewBackend<NSTableView, IListViewEventSink>, IListViewBackend
	{
		IListDataSource source;
		ListSource tsource;

		protected override NSTableView CreateView ()
		{
			return new NSTableViewBackend (EventSink, ApplicationContext);
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
					((IListViewEventSink)EventSink).OnRowActivated (cr);
				});
			}
		}
		
		public virtual void SetSource (IListDataSource source, IBackend sourceBackend)
		{
			this.source = source;
			tsource = new ListSource (source);
			Table.DataSource = tsource;
		}
		
		public int[] SelectedRows {
			get {
				int[] sel = new int [Table.SelectedRowCount];
				int i = 0;
				foreach (int r in Table.SelectedRows)
					sel [i++] = r;
				return sel;
			}
		}
		
		public void SelectRow (int pos)
		{
			Table.SelectRow (pos, false);
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

		// TODO
		public bool BorderVisible { get; set; }


		public int GetRowAtPosition (Point p)
		{
			return Table.GetRow (new System.Drawing.PointF ((float)p.X, (float)p.Y));
		}

		public Rectangle GetCellBounds (int row, CellView cell, bool includeMargin)
		{
			return Rectangle.Zero;
		}

		public bool TryGetRowHeight (out double rowHeight)
		{
			rowHeight = 0;
			return false;
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

		public override bool AcceptDrop (NSTableView tableView, NSDraggingInfo info, int row, NSTableViewDropOperation dropOperation)
		{
			return false;
		}

		public override string[] FilesDropped (NSTableView tableView, MonoMac.Foundation.NSUrl dropDestination, MonoMac.Foundation.NSIndexSet indexSet)
		{
			return new string [0];
		}

		public override MonoMac.Foundation.NSObject GetObjectValue (NSTableView tableView, NSTableColumn tableColumn, int row)
		{
			return NSObject.FromObject (row);
		}

		public override int GetRowCount (NSTableView tableView)
		{
			return source.RowCount;
		}

		public override void SetObjectValue (NSTableView tableView, MonoMac.Foundation.NSObject theObject, NSTableColumn tableColumn, int row)
		{
		}

		public override void SortDescriptorsChanged (NSTableView tableView, MonoMac.Foundation.NSSortDescriptor[] oldDescriptors)
		{
		}

		public override NSDragOperation ValidateDrop (NSTableView tableView, NSDraggingInfo info, int row, NSTableViewDropOperation dropOperation)
		{
			return NSDragOperation.None;
		}

		public override bool WriteRows (NSTableView tableView, MonoMac.Foundation.NSIndexSet rowIndexes, NSPasteboard pboard)
		{
			return false;
		}
	}
}

