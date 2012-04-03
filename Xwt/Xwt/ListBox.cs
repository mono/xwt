// 
// ListBox.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2012 Xamarin Inc
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
	public class ListBox: Widget
	{
		CellViewCollection views;
		IListDataSource source;
		ItemCollection itemCollection;
		SelectionMode mode;
		
		protected new class EventSink: Widget.EventSink, IListBoxEventSink, ICellContainer
		{
			public void NotifyCellChanged ()
			{
				((ListBox)Parent).OnCellChanged ();
			}
			
			public void OnSelectionChanged ()
			{
				((ListBox)Parent).OnSelectionChanged (EventArgs.Empty);
			}
			
			public override Size GetDefaultNaturalSize ()
			{
				return Xwt.Engine.DefaultNaturalSizes.ComboBox;
			}
		}
		
		new IListBoxBackend Backend {
			get { return (IListBoxBackend) base.Backend; }
		}
		
		public ListBox ()
		{
			views = new CellViewCollection ((ICellContainer)WidgetEventSink);
		}
		
		protected override Widget.EventSink CreateEventSink ()
		{
			return new EventSink ();
		}
		
		public CellViewCollection Views {
			get { return views; }
		}
		
		public ItemCollection Items {
			get {
				if (itemCollection == null) {
					itemCollection = new ItemCollection ();
					DataSource = itemCollection.DataSource;
				} else {
					if (DataSource != itemCollection.DataSource)
						throw new InvalidOperationException ("The Items collection can't be used when a custom DataSource is set");
				}
				return itemCollection;
			}
		}
		
		public IListDataSource DataSource {
			get { return source; }
			set {
				if (source != null) {
					source.RowChanged -= HandleModelChanged;
					source.RowDeleted -= HandleModelChanged;
					source.RowInserted -= HandleModelChanged;
					source.RowsReordered -= HandleModelChanged;
				}
				
				source = value;
				Backend.SetSource (source, source is XwtComponent ? GetBackend ((XwtComponent)source) : null);
				
				if (source != null) {
					source.RowChanged += HandleModelChanged;
					source.RowDeleted += HandleModelChanged;
					source.RowInserted += HandleModelChanged;
					source.RowsReordered += HandleModelChanged;
				}
			}
		}
		
		public ScrollPolicy VerticalScrollPolicy {
			get { return Backend.VerticalScrollPolicy; }
			set { Backend.VerticalScrollPolicy = value; }
		}
		
		public ScrollPolicy HorizontalScrollPolicy {
			get { return Backend.HorizontalScrollPolicy; }
			set { Backend.HorizontalScrollPolicy = value; }
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
		
		void HandleModelChanged (object sender, ListRowEventArgs e)
		{
			OnPreferredSizeChanged ();
		}
		
		void OnCellChanged ()
		{
			Backend.SetViews (views);
		}
		
		EventHandler selectionChanged;
		
		public event EventHandler SelectionChanged {
			add {
				OnBeforeEventAdd (ComboBoxEvent.SelectionChanged, selectionChanged);
				selectionChanged += value;
			}
			remove {
				selectionChanged -= value;
				OnAfterEventRemove (ComboBoxEvent.SelectionChanged, selectionChanged);
			}
		}
		
		protected virtual void OnSelectionChanged (EventArgs args)
		{
			if (selectionChanged != null)
				selectionChanged (this, args);
		}
	}
}

