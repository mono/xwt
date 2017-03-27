// 
// ListViewColumn.cs
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
using System.ComponentModel;

namespace Xwt
{
	/// <summary>
	/// Determines the direction of a sort.
	/// </summary>
	public enum ColumnSortDirection
	{
		/// <summary>
		/// Sorts the column in ascending order.
		/// </summary>
		Ascending,

		/// <summary>
		/// Sorts the column in descending order.
		/// </summary>
		Descending
	}

	public class ListViewColumn: ICellContainer
	{
		CellViewCollection views;
		string title;
		CellView headerView;
		Alignment alignment;

		internal IColumnContainerBackend Parent { get; set; }
		internal object Handle { get; set; }
		
		public ListViewColumn ()
		{
			views = new CellViewCollection (this);
		}
		
		public ListViewColumn (string title): this ()
		{
			Title = title;
		}
		
		public ListViewColumn (string title, CellView cellView): this ()
		{
			Title = title;
			Views.Add (cellView);
		}
		
		[DefaultValue (null)]
		public CellView HeaderView {
			get {
				return headerView;
			}
			set {
				headerView = value;
				if (Parent != null)
					Parent.UpdateColumn (this, Handle, ListViewColumnChange.Title);
			}
		}
		
		public string Title {
			get {
				return this.title;
			}
			set {
				title = value;
				if (Parent != null)
					Parent.UpdateColumn (this, Handle, ListViewColumnChange.Title);
			}
		}

		public Alignment Alignment {
			get {
				return alignment;
			}
			set {
				alignment = value;
				if (Parent != null)
					Parent.UpdateColumn (this, Handle, ListViewColumnChange.Alignment);
			}
		}

		public CellViewCollection Views {
			get { return views; }
		}
		
		void ICellContainer.NotifyCellChanged ()
		{
			if (Parent != null)
				Parent.UpdateColumn (this, Handle, ListViewColumnChange.Cells);
		}

		bool isResizeable;

		/// <summary>
		/// Gets or sets a value indicating whether this column is user resizeable.
		/// </summary>
		/// <value><c>true</c> if this column is user resizeable; otherwise, <c>false</c>.</value>
		public bool CanResize {
			get {
				return isResizeable;
			}
			set {
				isResizeable = value;
				if (Parent != null)
					Parent.UpdateColumn (this, Handle, ListViewColumnChange.CanResize);
			}
		}

		bool expands;

		public bool Expands {
			get { return expands; }
			set
			{
				expands = value;
				if (Parent != null)
					Parent.UpdateColumn (this, Handle, ListViewColumnChange.Expanding);
			}
		}

		ColumnSortDirection sortDirection;

		/// <summary>
		/// The direction the sort indicator should show.
		/// </summary>
		public ColumnSortDirection SortDirection {
			get {
				return sortDirection;
			}
			set {
				sortDirection = value;
				if (Parent != null)
					Parent.UpdateColumn (this, Handle, ListViewColumnChange.SortDirection);
			}
		}

		IDataField sortDataField;

		/// <summary>
		/// The column that is used for sorting if the column is selected for sorting.
		/// </summary>
		public IDataField SortDataField {
			get {
				return sortDataField;
			}
			set {
				sortDataField = value;
				if (Parent != null)
					Parent.UpdateColumn (this, Handle, ListViewColumnChange.SortDataField);
			}
		}

		bool isSortIndicatorShown;

		/// <summary>
		/// Gets or sets a value indicating whether a sort indicator is shown.
		/// </summary>
		public bool SortIndicatorVisible {
			get {
				return isSortIndicatorShown;
			}
			set {
				isSortIndicatorShown = value;
				if (Parent != null)
					Parent.UpdateColumn (this, Handle, ListViewColumnChange.SortIndicatorVisible);
			}
		}
	}
}
