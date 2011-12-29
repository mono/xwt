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
		
		public TreeView (ITreeDataSource source): this ()
		{
			DataSource = source;
		}
		
		public ListViewColumnCollection Columns {
			get {
				return columns;
			}
		}
		
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
		
		public TreePosition SelectedRow {
			get {
				var items = SelectedRows;
				if (items.Length == 0)
					return null;
				else
					return items [0];
			}
		}
		
		public TreePosition[] SelectedRows {
			get {
				return Backend.SelectedRows;
			}
		}
		
		public void SelectRow (TreePosition pos)
		{
			Backend.SelectRow (pos);
		}
		
		public void UnselectRow (TreePosition pos)
		{
			Backend.UnselectRow (pos);
		}
		
		public void SelectAll ()
		{
			Backend.SelectAll ();
		}
		
		public void UnselectAll ()
		{
			Backend.UnselectAll ();
		}
		
		public bool IsRowSelected (TreePosition pos)
		{
			return Backend.IsRowSelected (pos);
		}
		
		public bool IsRowExpanded (TreePosition pos)
		{
			return Backend.IsRowExpanded (pos);
		}
		
		public void ExpandRow (TreePosition pos, bool expandedChildren)
		{
			Backend.ExpandRow (pos, expandedChildren);
		}
		
		public void CollapseRow (TreePosition pos)
		{
			Backend.CollapseRow (pos);
		}
		
		/// <summary>
		/// Saves the status of the tree
		/// </summary>
		/// <remarks>
		/// The status information includes node expansion and selection status. The returned object
		/// can be used to restore the status by calling RestoreStatus.
		/// </remarks>
		public TreeViewStatus SaveStatus (IDataField idField)
		{
			return new TreeViewStatus (this, idField.Index);
		}
		
		public void RestoreStatus (TreeViewStatus status)
		{
			status.Load (this);
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

