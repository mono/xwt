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
	public class TreeView: Widget, IColumnContainer
	{
		ListViewColumnCollection columns;
		ITreeDataSource dataSource;
		SelectionMode mode;
		
		protected new class EventSink: Widget.EventSink, ITreeViewEventSink
		{
			public void OnSelectionChanged ()
			{
				((TreeView)Parent).OnSelectionChanged (EventArgs.Empty);
			}
		}
		
		static TreeView ()
		{
			MapEvent (TableViewEvent.SelectionChanged, typeof(TreeView), "OnSelectionChanged");
		}
	
		/// <summary>
		/// Initializes a new instance of the <see cref="Xwt.TreeView"/> class.
		/// </summary>
		public TreeView ()
		{
			columns = new ListViewColumnCollection (this);
		}
		
		protected override Widget.EventSink CreateEventSink ()
		{
			return new EventSink ();
		}
		
		new ITreeViewBackend Backend {
			get { return (ITreeViewBackend) base.Backend; }
		}
		
		protected override void OnBackendCreated ()
		{
			base.OnBackendCreated ();
			Backend.Initialize ((EventSink)WidgetEventSink);
			columns.Attach (Backend);
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Xwt.TreeView"/> class.
		/// </summary>
		/// <param name='source'>
		/// Data source
		/// </param>
		public TreeView (ITreeDataSource source): this ()
		{
			DataSource = source;
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
					dataSource = value;
					Backend.SetSource (dataSource, dataSource is XwtComponent ? GetBackend ((XwtComponent)dataSource) : null);
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
		
		protected sealed override bool SupportsCustomScrolling {
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
				OnBeforeEventAdd (TableViewEvent.SelectionChanged, selectionChanged);
				selectionChanged += value;
			}
			remove {
				selectionChanged -= value;
				OnAfterEventRemove (TableViewEvent.SelectionChanged, selectionChanged);
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

