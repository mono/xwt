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
using Xwt.Backends;

#if MONOMAC
using nint = System.Int32;
using nfloat = System.Single;
using CGRect = System.Drawing.RectangleF;
using CGPoint = System.Drawing.PointF;
using CGSize = System.Drawing.SizeF;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
#else
using Foundation;
using AppKit;
using CoreGraphics;
#endif

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
					((IListViewEventSink)EventSink).OnRowActivated ((int)cr);
				});
			}
		}
		
		public virtual void SetSource (IListDataSource source, IBackend sourceBackend)
		{
			this.source = source;
			tsource = new ListSource (source);
			Table.DataSource = tsource;

			//TODO: Reloading single rows would be slightly more efficient.
			//      According to NSTableView.ReloadData() documentation,
			//      only the visible rows are reloaded.
			source.RowInserted += (sender, e) => Table.ReloadData();
			source.RowDeleted += (sender, e) => Table.ReloadData();
			source.RowChanged += (sender, e) => Table.ReloadData();
			source.RowsReordered += (sender, e) => Table.ReloadData();
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

		// TODO
		public bool BorderVisible { get; set; }


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

