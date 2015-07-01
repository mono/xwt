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
	/// <summary>
	/// A list of selectable items
	/// </summary>
	[BackendType (typeof(IListBoxBackend))]
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
			
			public void OnRowActivated (int rowIndex)
			{
				((ListBox)Parent).OnRowActivated (new ListViewRowEventArgs (rowIndex));
			}
			
			public override Size GetDefaultNaturalSize ()
			{
				return Xwt.Backends.DefaultNaturalSizes.ComboBox;
			}
		}
		
		static ListBox ()
		{
			MapEvent (TableViewEvent.SelectionChanged, typeof(ListView), "OnSelectionChanged");
			MapEvent (ListViewEvent.RowActivated, typeof(ListView), "OnRowActivated");
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
					views.Clear ();
					views.Add (new TextCellView (itemCollection.LabelField));
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
				BackendHost.ToolkitEngine.ValidateObject (value);
				if (source != null) {
					source.RowChanged -= HandleModelChanged;
					source.RowDeleted -= HandleModelChanged;
					source.RowInserted -= HandleModelChanged;
					source.RowsReordered -= HandleModelChanged;
				}
				
				source = value;
				Backend.SetSource (source, source is IFrontend ? (IBackend) Toolkit.GetBackend (source) : null);

				if (source != null) {
					source.RowChanged += HandleModelChanged;
					source.RowDeleted += HandleModelChanged;
					source.RowInserted += HandleModelChanged;
					source.RowsReordered += HandleModelChanged;
				}
			}
		}

		public bool GridLinesVisible
		{
			get { return Backend.GridLinesVisible; }
			set { Backend.GridLinesVisible = value; }
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
		/// Gets or sets the focused row.
		/// </summary>
		/// <value>The row with the keyboard focus.</value>
		public int FocusedRow {
			get {
				return Backend.FocusedRow;
			}
			set {
				Backend.FocusedRow = value;
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

		public void ScrollToRow (int row)
		{
			Backend.ScrollToRow (row);
		}

		/// <summary>
		/// Returns the row at the given widget coordinates
		/// </summary>
		/// <returns>The row index</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public int GetRowAtPosition (double x, double y)
		{
			return GetRowAtPosition (new Point (x, y));
		}

		/// <summary>
		/// Returns the row at the given widget coordinates
		/// </summary>
		/// <returns>The row index</returns>
		/// <param name="p">A position, in widget coordinates</param>
		public int GetRowAtPosition (Point p)
		{
			return Backend.GetRowAtPosition (p);
		}

		/// <summary>
		/// Gets the bounds of the given row.
		/// </summary>
		/// <returns>The row bounds inside the widget, relative to the widget bounds.</returns>
		/// <param name="row">The row index.</param>
		/// <param name="includeMargin">If set to <c>true</c> include margin (the background of the row).</param>
		public Rectangle GetRowBounds (int row, bool includeMargin)
		{
			return Backend.GetRowBounds (row, includeMargin);
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
				BackendHost.OnBeforeEventAdd (TableViewEvent.SelectionChanged, selectionChanged);
				selectionChanged += value;
			}
			remove {
				selectionChanged -= value;
				BackendHost.OnAfterEventRemove (TableViewEvent.SelectionChanged, selectionChanged);
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

		/// <summary>
		/// Raises the row activated event.
		/// </summary>
		/// <param name="a">The alpha component.</param>
		protected virtual void OnRowActivated (ListViewRowEventArgs a)
		{
			if (rowActivated != null)
				rowActivated (this, a);
		}
		
		EventHandler<ListViewRowEventArgs> rowActivated;
		
		/// <summary>
		/// Occurs when the user double-clicks on a row
		/// </summary>
		public event EventHandler<ListViewRowEventArgs> RowActivated {
			add {
				BackendHost.OnBeforeEventAdd (ListViewEvent.RowActivated, rowActivated);
				rowActivated += value;
			}
			remove {
				rowActivated -= value;
				BackendHost.OnAfterEventRemove (ListViewEvent.RowActivated, rowActivated);
			}
		}
	}
}

