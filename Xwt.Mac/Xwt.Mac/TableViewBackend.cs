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
using MonoMac.AppKit;
using Xwt.Backends;
using System.Collections.Generic;
using MonoMac.Foundation;
using Xwt.Engine;

namespace Xwt.Mac
{
	public abstract class TableViewBackend<T,S>: ViewBackend<NSScrollView,S>, ICellSource where T:NSTableView where S:ITableViewEventSink
	{
		List<NSTableColumn> cols = new List<NSTableColumn> ();
		protected NSTableView Table;
		ScrollView scroll;
		NSObject selChangeObserver;
		
		public TableViewBackend ()
		{
		}
		
		public override void Initialize ()
		{
			Table = CreateView ();
			scroll = new ScrollView ();
			scroll.DocumentView = Table;
			ViewObject = scroll;
			Table.SizeToFit ();
			Widget.AutoresizingMask = NSViewResizingMask.HeightSizable | NSViewResizingMask.WidthSizable;
			Widget.AutoresizesSubviews = true;
		}
		
		protected abstract NSTableView CreateView ();
		protected abstract string SelectionChangeEventName { get; }
		
		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is TableViewEvent) {
				switch ((TableViewEvent)eventId) {
				case TableViewEvent.SelectionChanged:
					selChangeObserver = NSNotificationCenter.DefaultCenter.AddObserver (new NSString (SelectionChangeEventName), HandleTreeSelectionDidChange, Table);
					break;
				}
			}
		}
		
		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is TableViewEvent) {
				switch ((TableViewEvent)eventId) {
				case TableViewEvent.SelectionChanged:
					if (selChangeObserver != null)
						NSNotificationCenter.DefaultCenter.RemoveObserver (selChangeObserver);
					break;
				}
			}
		}

		void HandleTreeSelectionDidChange (NSNotification notif)
		{
			Toolkit.Invoke (delegate {
				EventSink.OnSelectionChanged ();
			});
		}
		
		public void SetSelectionMode (SelectionMode mode)
		{
			Table.AllowsMultipleSelection = mode == SelectionMode.Multiple;
		}

		public virtual object AddColumn (ListViewColumn col)
		{
			var tcol = new NSTableColumn ();
			cols.Add (tcol);
			var c = CellUtil.CreateCell (this, col.Views);
			tcol.DataCell = c;
			Table.AddColumn (tcol);
			var hc = new NSTableHeaderCell ();
			hc.Title = col.Title;
			tcol.HeaderCell = hc;
			return tcol;
		}
		
		public void RemoveColumn (ListViewColumn col, object handle)
		{
			Table.RemoveColumn ((NSTableColumn)handle);
		}

		public void UpdateColumn (ListViewColumn col, object handle, ListViewColumnChange change)
		{
			NSTableColumn tcol = (NSTableColumn) handle;
			tcol.DataCell = CellUtil.CreateCell (this, col.Views);
		}

		public void SelectAll ()
		{
			Table.SelectAll (null);
		}

		public void UnselectAll ()
		{
			Table.DeselectAll (null);
		}
		
		public abstract object GetValue (object pos, int nField);
		
		float ICellSource.RowHeight {
			get { return Table.RowHeight; }
			set { Table.RowHeight = value; }
		}
	}
	
	class ScrollView: NSScrollView, IViewObject<NSScrollView>
	{
		public Widget Frontend { get; set; }
		public NSScrollView View {
			get { return this; }
		}
	}
}

