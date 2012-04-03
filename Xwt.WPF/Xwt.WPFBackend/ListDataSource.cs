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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
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
		public event NotifyCollectionChangedEventHandler CollectionChanged;

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
			this.rows.Add (new object[this.columnTypes.Length]);
			int index = this.rows.Count - 1;

			OnRowInserted (new NotifyCollectionChangedEventArgs (
				NotifyCollectionChangedAction.Add, (object)this.rows[index], index));

			return index;
		}

		public int InsertRowAfter (int row)
		{
			row++;

			if (row > this.rows.Count)
				throw new ArgumentOutOfRangeException ("row");

			if (row == this.rows.Count)
				return AddRow ();

			this.rows.Insert (row, new object[this.columnTypes.Length]);

			OnRowInserted (new NotifyCollectionChangedEventArgs (
				NotifyCollectionChangedAction.Add, (object)this.rows[row], row));

			return row;
		}

		public int InsertRowBefore (int row)
		{
			if (row > this.rows.Count)
				throw new ArgumentOutOfRangeException ("row");

			if (row == this.rows.Count)
				return AddRow ();

			row--;
			this.rows.Insert (row, new object[this.columnTypes.Length]);

			OnRowInserted (new NotifyCollectionChangedEventArgs (
				NotifyCollectionChangedAction.Add, (object)this.rows[row], row));

			return row;
		}

		public void RemoveRow (int row)
		{
			if (row >= this.rows.Count)
				throw new ArgumentOutOfRangeException ("row");

			object value = this.rows [row];
			this.rows.RemoveAt (row);

			OnRowDeleted (new NotifyCollectionChangedEventArgs (
				NotifyCollectionChangedAction.Remove, value, row));
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
				object[] nrow = new object[this.ColumnTypes.Length];
				this.rows.Add (nrow);
				rowsInserted++;
			}

			object[] orow = this.rows[row];
			orow[column] = value;

			if (rowsInserted == 0) {
				OnRowChanged (new NotifyCollectionChangedEventArgs (
				              	NotifyCollectionChangedAction.Replace, (object)orow, (object)orow, row));
			} else {
				for (int i = rowsInserted; i > 0; --i) {
					int r = rowsInserted - i;
					OnRowInserted (new NotifyCollectionChangedEventArgs (
						NotifyCollectionChangedAction.Add, (object)orow, r));
				}
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

		private readonly List<object[]> rows = new List<object[]> ();
		private Type[] columnTypes;

		private void OnRowInserted (NotifyCollectionChangedEventArgs e)
		{
			var inserted = RowInserted;
			if (inserted != null)
			    inserted (this, new ListRowEventArgs (e.NewStartingIndex));

			var collChanged = CollectionChanged;
			if (collChanged != null)
			    collChanged (this, e);
		}

		private void OnRowChanged (NotifyCollectionChangedEventArgs e)
		{
			var changed = RowChanged;
			if (changed != null)
				changed (this, new ListRowEventArgs (e.OldStartingIndex));

			var collChanged = CollectionChanged;
			if (collChanged != null)
				collChanged (this, e);
		}

		private void OnRowDeleted (NotifyCollectionChangedEventArgs e)
		{
			var deleted = RowDeleted;
			if (deleted != null)
				deleted (this, new ListRowEventArgs (e.OldStartingIndex));

			var collChanged = CollectionChanged;
			if (collChanged != null)
				collChanged (this, e);
		}
	}
}