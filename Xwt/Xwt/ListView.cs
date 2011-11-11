// 
// ListView.cs
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
using System.ComponentModel;

namespace Xwt
{
	public class ListView: Widget, IColumnContainer
	{
		ListViewColumnCollection columns;
		IListDataSource dataSource;
		SelectionMode mode;
		
		protected new class EventSink: Widget.EventSink, IListViewEventSink
		{
			public void OnSelectionChanged ()
			{
				((ListView)Parent).OnSelectionChanged (EventArgs.Empty);
			}
		}
		
		static ListView ()
		{
			MapEvent (TableViewEvent.SelectionChanged, typeof(ListView), "OnSelectionChanged");
		}
		
		public ListView (IListDataSource source): this ()
		{
			DataSource = source;
		}
		
		public ListView ()
		{
			columns = new ListViewColumnCollection (this);
		}
		
		protected override Widget.EventSink CreateEventSink ()
		{
			return new EventSink ();
		}
		
		new IListViewBackend Backend {
			get { return (IListViewBackend) base.Backend; }
		}
		
		protected override void OnBackendCreated ()
		{
			base.OnBackendCreated ();
			columns.Attach (Backend);
		}
		
		public ListViewColumnCollection Columns {
			get {
				return columns;
			}
		}
		
		public IListDataSource DataSource {
			get {
				return dataSource;
			}
			set {
				if (dataSource != value) {
					dataSource = value;
					Backend.SetSource (dataSource, dataSource is XwtComponent ? GetBackend ((XwtComponent)dataSource) : null);
				}
			}
		}
		
		public SelectionMode SelectionMode {
			get {
				return mode;
			}
			set {
				mode = value;
				Backend.SetSelectionMode (mode);
			}
		}
		
		public int SelectedRow {
			get {
				var items = SelectedRows;
				if (items.Length == 0)
					return -1;
				else
					return items [0];
			}
		}
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int[] SelectedRows {
			get {
				return Backend.SelectedRows;
			}
		}
		
		public void SelectRow (int row)
		{
			Backend.SelectRow (row);
		}
		
		public void UnselectRow (int row)
		{
			Backend.UnselectRow (row);
		}
		
		public void SelectAll ()
		{
			Backend.SelectAll ();
		}
		
		public void UnselectAll ()
		{
			Backend.UnselectAll ();
		}
		
		void IColumnContainer.NotifyColumnsChanged ()
		{
		}
		
		protected virtual void OnSelectionChanged (EventArgs a)
		{
			if (selectionChanged != null)
				selectionChanged (this, a);
		}
		
		EventHandler selectionChanged;
		
		public event EventHandler SelectionChanged {
			add {
				OnBeforeEventAdd (TableViewEvent.SelectionChanged, selectionChanged);
				selectionChanged += value;
			}
			remove {
				selectionChanged -= value;
				OnAfterEventRemove (TableViewEvent.SelectionChanged, selectionChanged);
			}
		}	}
}

