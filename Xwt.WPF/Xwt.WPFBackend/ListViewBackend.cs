// 
// ListViewBackend.cs
//  
// Author:
//       Eric Maupin <ermau@xamarin.com>
// 
// Copyright (c) 2012 Xamarin, Inc.
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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Xwt.WPFBackend.Utilities;
using SWC = System.Windows.Controls;
using Xwt.Backends;

namespace Xwt.WPFBackend
{
	public class ListViewBackend
		: WidgetBackend, IListViewBackend
	{
		public ListViewBackend()
		{
			ListView = new ExListView();
			ListView.View = this.view;
		}

		public bool HeadersVisible {
			get { return this.headersVisible; }
			set
			{
				this.headersVisible = value;
				if (value) {
					if (this.view.ColumnHeaderContainerStyle != null)
						this.view.ColumnHeaderContainerStyle.Setters.Clear ();
				}
				else {
					this.view.ColumnHeaderContainerStyle = new Style();
					this.view.ColumnHeaderContainerStyle.Setters.Add (
						new Setter (UIElement.VisibilityProperty, Visibility.Collapsed));
				}
			}
		}

		public int[] SelectedRows {
			get { return ListView.SelectedIndexes.ToArray (); }
		}

		public object AddColumn (ListViewColumn col)
		{
			var column = new GridViewColumn ();
			column.CellTemplate = new DataTemplate { VisualTree = CellUtil.CreateBoundColumnTemplate (col) };
			if (col.HeaderView != null)
				column.HeaderTemplate = new DataTemplate { VisualTree = CellUtil.CreateBoundCellRenderer (col.HeaderView) };
			else
				column.Header = col.Title;

			this.view.Columns.Add (column);

			return column;
		}

		public void RemoveColumn (ListViewColumn col, object handle)
		{
			this.view.Columns.Remove ((GridViewColumn) handle);
		}

		public void UpdateColumn (ListViewColumn col, object handle, ListViewColumnChange change)
		{
			var column = (GridViewColumn) handle;
			column.CellTemplate = new DataTemplate { VisualTree = CellUtil.CreateBoundColumnTemplate (col) };
			if (col.HeaderView != null)
				column.HeaderTemplate = new DataTemplate { VisualTree = CellUtil.CreateBoundCellRenderer (col.HeaderView) };
			else
				column.Header = col.Title;
		}

		public void SetSelectionMode (SelectionMode mode)
		{
			switch (mode) {
			case SelectionMode.Single:
				ListView.SelectionMode = SWC.SelectionMode.Single;
				break;

			case SelectionMode.Multiple:
				ListView.SelectionMode = SWC.SelectionMode.Extended;
				break;
			}
		}

		public void SelectAll ()
		{
			ListView.SelectAll();
		}

		public void UnselectAll ()
		{
			ListView.UnselectAll();
		}

		public void SetSource (IListDataSource source, IBackend sourceBackend)
		{
			ListView.ItemsSource = new ListSourceNotifyWrapper (source);
		}

		public void SelectRow (int pos)
		{
			ListView.SelectedIndexes.Add (pos);
		}

		public void UnselectRow (int pos)
		{
			ListView.SelectedIndexes.Remove (pos);
		}

		private bool headersVisible;
		private readonly GridView view = new GridView();

		protected ExListView ListView {
			get { return (ExListView) Widget; }
			set { Widget = value; }
		}
	}
}
