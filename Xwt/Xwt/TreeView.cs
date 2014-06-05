// 
// TreeView.cs
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
using Xwt.Drawing;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Xwt.Backends;


namespace Xwt
{
	[BackendType (typeof(ITreeViewBackend))]
	public class TreeView: Widget, IColumnContainer, IScrollableWidget
	{
		ListViewColumnCollection columns;
		ITreeDataSource dataSource;
		SelectionMode mode;
		
		protected new class WidgetBackendHost: Widget.WidgetBackendHost<TreeView,ITreeViewBackend>, ITreeViewEventSink
		{
			protected override void OnBackendCreated ()
			{
				base.OnBackendCreated ();
				Backend.Initialize (this);
				Parent.columns.Attach (Backend);
			}
			
			public void OnSelectionChanged ()
			{
				((TreeView)Parent).OnSelectionChanged (EventArgs.Empty);
			}
			
			public void OnRowActivated (TreePosition position)
			{
				((TreeView)Parent).OnRowActivated (new TreeViewRowEventArgs (position));
			}

			public void OnRowExpanded (TreePosition position)
			{
				((TreeView)Parent).OnRowExpanded (new TreeViewRowEventArgs (position));
			}
			
			public void OnRowExpanding (TreePosition position)
			{
				((TreeView)Parent).OnRowExpanding (new TreeViewRowEventArgs (position));
			}
			
			public override Size GetDefaultNaturalSize ()
			{
				return Xwt.Backends.DefaultNaturalSizes.TreeView;
			}
		}
		
		static TreeView ()
		{
			MapEvent (TableViewEvent.SelectionChanged, typeof(TreeView), "OnSelectionChanged");
			MapEvent (TreeViewEvent.RowActivated, typeof(TreeView), "OnRowActivated");
			MapEvent (TreeViewEvent.RowExpanded, typeof(TreeView), "OnRowExpanded");
			MapEvent (TreeViewEvent.RowExpanding, typeof(TreeView), "OnRowExpanding");
		}
	
		/// <summary>
		/// Initializes a new instance of the <see cref="Xwt.TreeView"/> class.
		/// </summary>
		public TreeView ()
		{
			columns = new ListViewColumnCollection (this);
			VerticalScrollPolicy = HorizontalScrollPolicy = ScrollPolicy.Automatic;
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Xwt.TreeView"/> class.
		/// </summary>
		/// <param name='source'>
		/// Data source
		/// </param>
		public TreeView (ITreeDataSource source): this ()
		{
			VerifyConstructorCall (this);
			DataSource = source;
		}
		
		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}
		
		ITreeViewBackend Backend {
			get { return (ITreeViewBackend) BackendHost.Backend; }
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

		/// <summary>
		/// Gets the tree columns.
		/// </summary>
		/// <value>
		/// The columns.
		/// </value>
		public ListViewColumnCollection Columns {
			get {
				return columns;
			}
		}
		
		/// <summary>
		/// Gets or sets the data source.
		/// </summary>
		/// <value>
		/// The data source.
		/// </value>
		public ITreeDataSource DataSource {
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

		/// <summary>
		/// Gets or sets a value indicating whether headers are visible or not.
		/// </summary>
		/// <value>
		/// <c>true</c> if headers are visible; otherwise, <c>false</c>.
		/// </value>
		public bool HeadersVisible {
			get {
				return Backend.HeadersVisible;
			}
			set {
				Backend.HeadersVisible = value;
			}
		}

		public GridLines GridLines
		{
			get { return Backend.GridLines; }
			set { Backend.GridLines = value; }
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
		/// Gets or sets the row the current event applies to.
		/// The behavior of this property is undefined when used outside an
		/// event that supports it.
		/// </summary>
		/// <value>
		/// The current event row.
		/// </value>
		public TreePosition CurrentEventRow {
			get {
				return Backend.CurrentEventRow;
			}
		}
		
		/// <summary>
		/// Gets the selected row.
		/// </summary>
		/// <value>
		/// The selected row.
		/// </value>
		public TreePosition SelectedRow {
			get {
				var items = SelectedRows;
				if (items.Length == 0)
					return null;
				else
					return items [0];
			}
		}
		
		/// <summary>
		/// Gets the selected rows.
		/// </summary>
		/// <value>
		/// The selected rows.
		/// </value>
		public TreePosition[] SelectedRows {
			get {
				return Backend.SelectedRows;
			}
		}
		
		/// <summary>
		/// Selects a row.
		/// </summary>
		/// <param name='pos'>
		/// Position of the row
		/// </param>
		public void SelectRow (TreePosition pos)
		{
			Backend.SelectRow (pos);
		}
		
		/// <summary>
		/// Unselects a row.
		/// </summary>
		/// <param name='pos'>
		/// Position of the row
		/// </param>
		public void UnselectRow (TreePosition pos)
		{
			Backend.UnselectRow (pos);
		}
		
		/// <summary>
		/// Selects all rows
		/// </summary>
		public void SelectAll ()
		{
			Backend.SelectAll ();
		}
		
		/// <summary>
		/// Unselects all rows
		/// </summary>
		public void UnselectAll ()
		{
			Backend.UnselectAll ();
		}
		
		/// <summary>
		/// Determines whether the row at the specified position is selected
		/// </summary>
		/// <returns>
		/// <c>true</c> if the row is selected, <c>false</c> otherwise.
		/// </returns>
		/// <param name='pos'>
		/// Row position
		/// </param>
		public bool IsRowSelected (TreePosition pos)
		{
			return Backend.IsRowSelected (pos);
		}
		
		/// <summary>
		/// Determines whether the row at the specified position is expanded
		/// </summary>
		/// <returns>
		/// <c>true</c> if the row is expanded, <c>false</c> otherwise.
		/// </returns>
		/// <param name='pos'>
		/// Row position
		/// </param>
		public bool IsRowExpanded (TreePosition pos)
		{
			return Backend.IsRowExpanded (pos);
		}
		
		/// <summary>
		/// Expands a row.
		/// </summary>
		/// <param name='pos'>
		/// Position of the row
		/// </param>
		/// <param name='expandChildren'>
		/// If True, all children are recursively expanded
		/// </param>
		public void ExpandRow (TreePosition pos, bool expandChildren)
		{
			Backend.ExpandRow (pos, expandChildren);
		}
		
		/// <summary>
		/// Collapses a row.
		/// </summary>
		/// <param name='pos'>
		/// Position of the row
		/// </param>
		public void CollapseRow (TreePosition pos)
		{
			Backend.CollapseRow (pos);
		}

		/// <summary>
		/// Recursively expands all nodes of the tree
		/// </summary>
		public void ExpandAll ()
		{
			if (DataSource != null) {
				var nc = DataSource.GetChildrenCount (null);
				for (int n=0; n<nc; n++) {
					var p = DataSource.GetChild (null, n);
					Backend.ExpandRow (p, true);
				}
			}
		}
		
		/// <summary>
		/// Saves the status of the tree
		/// </summary>
		/// <returns>
		/// A status object
		/// </returns>
		/// <param name='idField'>
		/// Field to be used to identify each row
		/// </param>
		/// <remarks>
		/// The status information includes node expansion and selection status. The returned object
		/// can be used to restore the status by calling RestoreStatus.
		/// The provided field is used to generate an identifier for each row. When restoring the
		/// status, those ids are used to find matching rows.
		/// </remarks>
		public TreeViewStatus SaveStatus (IDataField idField)
		{
			return new TreeViewStatus (this, idField.Index);
		}
		
		/// <summary>
		/// Restores the status of the tree
		/// </summary>
		/// <param name='status'>
		/// Status object
		/// </param>
		/// <remarks>
		/// The status information includes node expansion and selection status. The provided object
		/// must have been generated with a SaveStatus call on this same tree.
		/// </remarks>
		public void RestoreStatus (TreeViewStatus status)
		{
			status.Load (this);
		}
		
		public void ScrollToRow (TreePosition pos)
		{
			Backend.ScrollToRow (pos);
		}
		
		public void ExpandToRow (TreePosition pos)
		{
			Backend.ExpandToRow (pos);
		}
		
		public bool GetDropTargetRow (double x, double y, out RowDropPosition pos, out TreePosition nodePosition)
		{
			return Backend.GetDropTargetRow (x, y, out pos, out nodePosition);
		}
		
		internal protected sealed override bool SupportsCustomScrolling {
			get {
				return false;
			}
		}
		
		void IColumnContainer.NotifyColumnsChanged ()
		{
		}
		
		/// <summary>
		/// Raises the selection changed event.
		/// </summary>
		/// <param name='a'>
		/// Event arguments
		/// </param>
		protected virtual void OnSelectionChanged (EventArgs a)
		{
			if (selectionChanged != null)
				selectionChanged (this, a);
		}
		
		EventHandler selectionChanged;
		
		/// <summary>
		/// Occurs when the selection changes
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
		/// Raises the row activated event.
		/// </summary>
		/// <param name="a">The alpha component.</param>
		protected virtual void OnRowActivated (TreeViewRowEventArgs a)
		{
			if (rowActivated != null)
				rowActivated (this, a);
		}
		
		EventHandler<TreeViewRowEventArgs> rowActivated;

		/// <summary>
		/// Occurs when the user double-clicks on a row
		/// </summary>
		public event EventHandler<TreeViewRowEventArgs> RowActivated {
			add {
				BackendHost.OnBeforeEventAdd (TreeViewEvent.RowActivated, rowActivated);
				rowActivated += value;
			}
			remove {
				rowActivated -= value;
				BackendHost.OnAfterEventRemove (TreeViewEvent.RowActivated, rowActivated);
			}
		}
		
		/// <summary>
		/// Raises the row expanding event.
		/// </summary>
		/// <param name="a">The alpha component.</param>
		protected virtual void OnRowExpanding (TreeViewRowEventArgs a)
		{
			if (rowExpanding != null)
				rowExpanding (this, a);
		}
		
		EventHandler<TreeViewRowEventArgs> rowExpanding;
		
		/// <summary>
		/// Occurs just before a row is expanded
		/// </summary>
		public event EventHandler<TreeViewRowEventArgs> RowExpanding {
			add {
				BackendHost.OnBeforeEventAdd (TreeViewEvent.RowExpanding, rowExpanding);
				rowExpanding += value;
			}
			remove {
				rowExpanding -= value;
				BackendHost.OnAfterEventRemove (TreeViewEvent.RowExpanding, rowExpanding);
			}
		}
		
		/// <summary>
		/// Raises the row expanding event.
		/// </summary>
		/// <param name="a">The alpha component.</param>
		protected virtual void OnRowExpanded (TreeViewRowEventArgs a)
		{
			if (rowExpanded != null)
				rowExpanded (this, a);
		}
		
		EventHandler<TreeViewRowEventArgs> rowExpanded;
		
		/// <summary>
		/// Occurs just before a row is expanded
		/// </summary>
		public event EventHandler<TreeViewRowEventArgs> RowExpanded {
			add {
				BackendHost.OnBeforeEventAdd (TreeViewEvent.RowExpanded, rowExpanded);
				rowExpanded += value;
			}
			remove {
				rowExpanded -= value;
				BackendHost.OnAfterEventRemove (TreeViewEvent.RowExpanded, rowExpanded);
			}
		}
	}
	
	interface IColumnContainer
	{
		void NotifyColumnsChanged ();
	}
	
	interface ICellContainer
	{
		void NotifyCellChanged ();
	}
	

}

