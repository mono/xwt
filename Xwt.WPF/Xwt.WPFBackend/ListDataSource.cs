// 
// ListDataSource.cs
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
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Xwt.Backends;

namespace Xwt.WPFBackend
{
	public class ListDataSource
		: Backend, IListStoreBackend, INotifyCollectionChanged, IEnumerable
	{
		public event EventHandler<ListRowEventArgs> RowInserted;
		public event EventHandler<ListRowEventArgs> RowDeleted;
		public event EventHandler<ListRowEventArgs> RowChanged;
		public event EventHandler<ListRowOrderEventArgs> RowsReordered;

		public event NotifyCollectionChangedEventHandler CollectionChanged
		{
			add { this.rows.CollectionChanged += value; }
			remove { this.rows.CollectionChanged -= value; }
		}

		public int RowCount
		{
			get { return this.rows.Count; }
		}

		public Type[] ColumnTypes
		{
			get { return this.columnTypes; }
		}

		public void Initialize (Type[] types)
		{
			if (types == null)
				throw new ArgumentNullException ("types");

			this.columnTypes = types;
		}

		public int AddRow ()
		{
			this.rows.Add (new ValuesContainer (this.columnTypes.Length));
			int index = this.rows.Count - 1;

			OnRowInserted (new ListRowEventArgs (index));

			return index;
		}

		public int InsertRowAfter (int row)
		{
			row++;

			if (row > this.rows.Count)
				throw new ArgumentOutOfRangeException ("row");

			if (row == this.rows.Count)
				return AddRow ();

			this.rows.Insert (row, new ValuesContainer (this.columnTypes.Length));

			OnRowInserted (new ListRowEventArgs (row));

			return row;
		}

		public int InsertRowBefore (int row)
		{
			if (row > this.rows.Count)
				throw new ArgumentOutOfRangeException ("row");

			if (row == this.rows.Count)
				return AddRow ();

			this.rows.Insert (row, new ValuesContainer (this.columnTypes.Length));

			OnRowInserted (new ListRowEventArgs (row));

			return row;
		}

		public void RemoveRow (int row)
		{
			if (row >= this.rows.Count)
				throw new ArgumentOutOfRangeException ("row");

			this.rows.RemoveAt (row);

			OnRowDeleted (new ListRowEventArgs (row));
		}

		public object GetValue (int row, int column)
		{
			if (row >= this.rows.Count)
				throw new ArgumentOutOfRangeException ("row");
			if (column >= this.ColumnTypes.Length)
				throw new ArgumentOutOfRangeException ("column");

			return this.rows [row] [column];
		}

		public void SetValue (int row, int column, object value)
		{
			int rowsInserted = 0;
			while (row >= this.rows.Count) {
				this.rows.Add (new ValuesContainer (this.columnTypes.Length));
				rowsInserted++;
			}

			ValuesContainer orow = this.rows[row];
			orow[column] = value;

			if (rowsInserted == 0) {
				OnRowChanged (new ListRowEventArgs (row));
			} else {
				for (int i = rowsInserted; i > 0; --i) {
					int r = rowsInserted - i;
					OnRowInserted (new ListRowEventArgs (r));
				}
			}
		}

		public void Clear()
		{
			int count = this.rows.Count;
			this.rows.Clear();

			for (int i = 0; i < count; i++) {
				OnRowDeleted (new ListRowEventArgs (i));
			}
		}

		public override void EnableEvent (object eventId)
		{
		}

		public override void DisableEvent (object eventId)
		{
		}

		public IEnumerator GetEnumerator ()
		{
			return this.rows.GetEnumerator ();
		}

		private readonly ObservableCollection<ValuesContainer> rows = new ObservableCollection<ValuesContainer> ();
		private Type[] columnTypes;

		private void OnRowInserted (ListRowEventArgs e)
		{
			var inserted = RowInserted;
			if (inserted != null)
			    inserted (this, e);
		}

		private void OnRowChanged (ListRowEventArgs e)
		{
			var changed = RowChanged;
			if (changed != null)
				changed (this, e);
		}

		private void OnRowDeleted (ListRowEventArgs e)
		{
			var deleted = RowDeleted;
			if (deleted != null)
				deleted (this, e);
		}
	}
}