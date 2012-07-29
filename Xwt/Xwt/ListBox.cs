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
using Xwt.Engine;

namespace Xwt
{
	/// <summary>
	/// A list of selectable items
	/// </summary>
	public class ListBox: Widget
	{
		CellViewCollection views;
		IListDataSource source;
		ItemCollection itemCollection;
		SelectionMode mode;
		
		protected new class WidgetBackendHost: Widget.WidgetBackendHost, IListBoxEventSink, ICellContainer
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
				return Xwt.Backends.DefaultNaturalSizes.ComboBox;
			}
		}
		
		IListBoxBackend Backend {
			get { return (IListBoxBackend) BackendHost.Backend; }
		}
		
		public ListBox ()
		{
			views = new CellViewCollection ((ICellContainer)BackendHost);
		}
		
		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}
		
		/// <summary>
		/// Views to be used to display the data of the items
		/// </summary>
		public CellViewCollection Views {
			get { return views; }
		}

		/// <summary>
		/// Items shown in the list
		/// </summary>
		/// <remarks>
		/// The Items collection can only be used when no custom DataSource is set.
		/// </remarks>
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

		/// <summary>
		/// Gets or sets the data source from which to get the data of the items
		/// </summary>
		/// <value>
		/// The data source.
		/// </value>
		/// <remarks>
		/// Then a DataSource is set, the Items collection can't be used.
		/// </remarks>
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
				Backend.SetSource (source, source is IFrontend ? (IBackend) WidgetRegistry.MainRegistry.GetBackend (source) : null);
				
				if (source != null) {
					source.RowChanged += HandleModelChanged;
					source.RowDeleted += HandleModelChanged;
					source.RowInserted += HandleModelChanged;
					source.RowsReordered += HandleModelChanged;
				}
			}
		}
		
		/// <summary>
		/// Gets or sets the vertical scroll policy.
		/// </summary>
		/// <value>
		/// The vertical scroll policy.
		/// </value>
		public ScrollPolicy VerticalScrollPolicy {
			get { return Backend.VerticalScrollPolicy; }
			set { Backend.VerticalScrollPolicy = value; }
		}
		
		/// <summary>
		/// Gets or sets the horizontal scroll policy.
		/// </summary>
		/// <value>
		/// The horizontal scroll policy.
		/// </value>
		public ScrollPolicy HorizontalScrollPolicy {
			get { return Backend.HorizontalScrollPolicy; }
			set { Backend.HorizontalScrollPolicy = value; }
		}
		
		/// <summary>
		/// Gets or sets the selection mode.
		/// </summary>
		/// <value>
		/// The selection mode.
		/// </value>
		public SelectionMode SelectionMode {
			get {
				return mode;
			}
			set {
				mode = value;
				Backend.SetSelectionMode (mode);
			}
		}
		
		/// <summary>
		/// Gets the selected row.
		/// </summary>
		/// <value>
		/// The selected row.
		/// </value>
		public int SelectedRow {
			get {
				var items = SelectedRows;
				if (items.Length == 0)
					return -1;
				else
					return items [0];
			}
		}
		
		public object SelectedItem {
			get {
				if (SelectedRow == -1)
					return null;
				return Items [SelectedRow];
			}
			set {
				if (SelectionMode == Xwt.SelectionMode.Multiple)
					UnselectAll ();
				var i = Items.IndexOf (value);
				if (i != -1)
					SelectRow (i);
				else
					UnselectAll ();
			}
		}
		
		/// <summary>
		/// Gets the selected rows.
		/// </summary>
		/// <value>
		/// The selected rows.
		/// </value>
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int[] SelectedRows {
			get {
				return Backend.SelectedRows;
			}
		}
		
		/// <summary>
		/// Selects a row.
		/// </summary>
		/// <param name='row'>
		/// a row.
		/// </param>
		/// <remarks>
		/// In single selection mode, the row will be selected and the previously selected row will be deselected.
		/// In multiple selection mode, the row will be added to the set of selected rows.
		/// </remarks>
		public void SelectRow (int row)
		{
			Backend.SelectRow (row);
		}
		
		/// <summary>
		/// Unselects a row.
		/// </summary>
		/// <param name='row'>
		/// A row
		/// </param>
		public void UnselectRow (int row)
		{
			Backend.UnselectRow (row);
		}
		
		/// <summary>
		/// Selects all rows
		/// </summary>
		public void SelectAll ()
		{
			Backend.SelectAll ();
		}
		
		/// <summary>
		/// Clears the selection
		/// </summary>
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
		
		/// <summary>
		/// Occurs when selection changes
		/// </summary>
		public event EventHandler SelectionChanged {
			add {
				BackendHost.OnBeforeEventAdd (ListBoxEvent.SelectionChanged, selectionChanged);
				selectionChanged += value;
			}
			remove {
				selectionChanged -= value;
				BackendHost.OnAfterEventRemove (ListBoxEvent.SelectionChanged, selectionChanged);
			}
		}
		
		/// <summary>
		/// Raises the selection changed event.
		/// </summary>
		/// <param name='args'>
		/// Arguments.
		/// </param>
		protected virtual void OnSelectionChanged (EventArgs args)
		{
			if (selectionChanged != null)
				selectionChanged (this, args);
		}
	}
}

