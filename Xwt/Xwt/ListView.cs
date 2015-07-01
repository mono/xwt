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
	[BackendType (typeof(IListViewBackend))]
	public class ListView: Widget, IColumnContainer, IScrollableWidget
	{
		ListViewColumnCollection columns;
		IListDataSource dataSource;
		SelectionMode mode;
		
		protected new class WidgetBackendHost: Widget.WidgetBackendHost<ListView,IListViewBackend>, IListViewEventSink
		{
			protected override void OnBackendCreated ()
			{
				base.OnBackendCreated ();
				Parent.columns.Attach (Backend);
			}
			
			public void OnSelectionChanged ()
			{
				Parent.OnSelectionChanged (EventArgs.Empty);
			}
			
			public void OnRowActivated (int rowIndex)
			{
				Parent.OnRowActivated (new ListViewRowEventArgs (rowIndex));
			}

			public override Size GetDefaultNaturalSize ()
			{
				return Xwt.Backends.DefaultNaturalSizes.ListView;
			}
		}
		
		static ListView ()
		{
			MapEvent (TableViewEvent.SelectionChanged, typeof(ListView), "OnSelectionChanged");
			MapEvent (ListViewEvent.RowActivated, typeof(ListView), "OnRowActivated");
		}
		
		public ListView (IListDataSource source): this ()
		{
			VerifyConstructorCall (this);
			DataSource = source;
		}
		
		public ListView ()
		{
			columns = new ListViewColumnCollection (this);
			VerticalScrollPolicy = HorizontalScrollPolicy = ScrollPolicy.Automatic;
		}
		
		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}
		
		IListViewBackend Backend {
			get { return (IListViewBackend) BackendHost.Backend; }
		}

		public bool BorderVisible
		{
			get { return Backend.BorderVisible; }
			set { Backend.BorderVisible = value; }
		}

		public GridLines GridLinesVisible
		{
			get { return Backend.GridLinesVisible; }
			set { Backend.GridLinesVisible = value; }
		}

		public ScrollPolicy VerticalScrollPolicy {
			get { return Backend.VerticalScrollPolicy; }
			set { Backend.VerticalScrollPolicy = value; }
		}
		
		public ScrollPolicy HorizontalScrollPolicy {
			get { return Backend.HorizontalScrollPolicy; }
			set { Backend.HorizontalScrollPolicy = value; }
		}

		ScrollControl verticalScrollAdjustment;
		public ScrollControl VerticalScrollControl {
			get {
				if (verticalScrollAdjustment == null)
					verticalScrollAdjustment = new ScrollControl (Backend.CreateVerticalScrollControl ());
				return verticalScrollAdjustment;
			}
		}

		ScrollControl horizontalScrollAdjustment;
		public ScrollControl HorizontalScrollControl {
			get {
				if (horizontalScrollAdjustment == null)
					horizontalScrollAdjustment = new ScrollControl (Backend.CreateHorizontalScrollControl ());
				return horizontalScrollAdjustment;
			}
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
					Backend.SetSource (value, value is IFrontend ? (IBackend)BackendHost.ToolkitEngine.GetSafeBackend (value) : null);
					dataSource = value;
				}
			}
		}
		
		public bool HeadersVisible {
			get {
				return Backend.HeadersVisible;
			}
			set {
				Backend.HeadersVisible = value;
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

		/// <summary>
		/// Gets or sets the row the current event applies to.
		/// The behavior of this property is undefined when used outside an
		/// event that supports it.
		/// </summary>
		/// <value>
		/// The current event row.
		/// </value>
		public int CurrentEventRow {
			get {
				return Backend.CurrentEventRow;
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
		/// Gets the bounds of a cell inside the given row.
		/// </summary>
		/// <returns>The cell bounds inside the widget, relative to the widget bounds.</returns>
		/// <param name="row">The row index.</param>
		/// <param name="cell">The cell view.</param>
		/// <param name="includeMargin">If set to <c>true</c> include margin (the background of the row).</param>
		public Rectangle GetCellBounds (int row, CellView cell, bool includeMargin)
		{
			return Backend.GetCellBounds (row, cell, includeMargin);
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
				BackendHost.OnBeforeEventAdd (TableViewEvent.SelectionChanged, selectionChanged);
				selectionChanged += value;
			}
			remove {
				selectionChanged -= value;
				BackendHost.OnAfterEventRemove (TableViewEvent.SelectionChanged, selectionChanged);
			}
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

