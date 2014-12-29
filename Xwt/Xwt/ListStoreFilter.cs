//
// ListStoreFilter.cs
//
// Author:
//       Vsevolod Kukol <sevo@sevo.org>
//
// Copyright (c) 2014 Vsevolod Kukol
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
using System.Collections.Generic;

namespace Xwt
{
	/// <summary>
	/// List store filter.
	/// </summary>
	[BackendType (typeof(IListStoreFilterBackend))]
	public class ListStoreFilter: ListStoreBase
	{
		public IListDataSource ChildStore {
			get;
			private set;
		}

		class ListStoreFilterBackendHost: BackendHost<ListStoreFilter,IListStoreFilterBackend>
		{
			protected override IBackend OnCreateBackend ()
			{
				var b = base.OnCreateBackend ();
				if (b == null)
					b = new DefaultListStoreFilterBackend ();
				((IListStoreFilterBackend)b).Initialize (Parent.ChildStore);
				return b;
			}

			protected override void OnBackendCreated ()
			{
				Backend.Initialize (Parent.ChildStore);
				base.OnBackendCreated ();
			}
		}

		protected override BackendHost CreateBackendHost ()
		{
			return new ListStoreFilterBackendHost ();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Xwt.ListStoreFilter"/> class.
		/// </summary>
		/// <param name="store">The list data store to pass through the filter.</param>
		public ListStoreFilter (IListDataSource store)
		{
			ChildStore = store;
		}

		IListStoreFilterBackend Backend {
			get { return (IListStoreFilterBackend) BackendHost.Backend; }
		}

		protected override Type[] GetColumnTypes ()
		{
			return ChildStore.ColumnTypes;
		}

		/// <summary>
		/// Sets the filter function, which should be used to filter single rows.
		/// </summary>
		/// <value>The filter function.</value>
		/// <remarks>
		/// The function must return <c>true</c> for rows to be filtered (hidden).
		/// The index of the row to apply the filter function on is passed as the parameter to the function.
		/// </remarks>
		public Func<int, bool> FilterFunction {
			set {
				Backend.FilterFunction = value;
			}
		}

		/// <summary>
		/// Converts the row index from the child store to its index inside the filter store.
		/// </summary>
		/// <returns>The index of the row inside the filtered store, or -1 if the row has been filtered.</returns>
		/// <param name="row">The child store index to convert</param>
		public int ConvertChildPositionToPosition (int row)
		{
			return Backend.ConvertChildPositionToPosition (row);
		}

		/// <summary>
		/// Converts the row index from the filter store to its index inside the child store.
		/// </summary>
		/// <returns>The index of the row inside the child store.</returns>
		/// <param name="row">The filter store index to convert.</param>
		public int ConvertPositionToChildPosition (int row)
		{
			return Backend.ConvertPositionToChildPosition (row);
		}

		/// <summary>
		/// Forces the filter store to re-evaluate all rows in the child store.
		/// </summary>
		public void Refilter()
		{
			Backend.Refilter ();
		}
	}

	public class DefaultListStoreFilterBackend: IListStoreFilterBackend
	{

		List<int> list = new List<int> ();

		public event EventHandler<ListRowEventArgs> RowInserted;
		public event EventHandler<ListRowEventArgs> RowDeleted;
		public event EventHandler<ListRowEventArgs> RowChanged;
		public event EventHandler<ListRowOrderEventArgs> RowsReordered;

		public void InitializeBackend (object frontend, ApplicationContext context)
		{
		}

		IListDataSource childStore;
		public IListDataSource ChildStore {
			get {
				return childStore;
			}
		}

		public void Initialize (IListDataSource store)
		{
			if (store == null)
				throw new ArgumentNullException ("store");
			childStore = store;
			childStore.RowChanged += HandleRowChanged;
			childStore.RowDeleted += HandleRowDeleted;
			childStore.RowInserted += HandleRowInserted;
			childStore.RowsReordered += HandleRowsReordered;
			Refilter ();
		}

		void HandleRowsReordered (object sender, ListRowOrderEventArgs e)
		{
			Refilter();
			int row = ConvertChildPositionToPosition (e.Row);
			if (row >= 0 && RowChanged != null)
				RowChanged (this, new ListRowEventArgs (row));
		}

		void HandleRowInserted (object sender, ListRowEventArgs e)
		{
			Refilter();
			int row = ConvertChildPositionToPosition (e.Row);
			if (row >= 0 && RowInserted != null)
				RowInserted (this, new ListRowEventArgs (row));
		}

		void HandleRowDeleted (object sender, ListRowEventArgs e)
		{
			int row = ConvertChildPositionToPosition (e.Row);
			Refilter ();
			if (row >= 0 && RowDeleted != null)
				RowDeleted (this, new ListRowEventArgs (row));
		}

		void HandleRowChanged (object sender, ListRowEventArgs e)
		{
			if (filterFunction == null)
				return;

			int row = ConvertChildPositionToPosition (e.Row);
			bool filter = filterFunction (e.Row);
			if (row < 0 && !filter) {
				for (int i = 0; i <= list.Count; i++) {
					if (i == list.Count || list [i] > e.Row) {
						list.Insert (i, e.Row);
						if (RowInserted != null)
							RowInserted (this, new ListRowEventArgs (i));
					}
				}
			}else if (row >= 0 && filter) {
				list.RemoveAt (row);
				if (RowDeleted != null)
					RowDeleted (this, new ListRowEventArgs (row));
			} else 
				if (row >= 0 && RowChanged != null)
					RowChanged (this, new ListRowEventArgs (row));
		}

		public void Refilter ()
		{

			list.Clear ();

			for (int i = 0; i < ChildStore.RowCount; i++) {
				if (filterFunction != null && !filterFunction(i)) {
					list.Add (i);
				}
			}
			if (RowsReordered != null)
				RowsReordered (this, new ListRowOrderEventArgs (-1, new int[] {}));
		}

		Func<int, bool> filterFunction;
		public Func<int, bool> FilterFunction {
			set {
				filterFunction = value;
				Refilter ();
			}
		}

		public int ConvertChildPositionToPosition (int row)
		{
			return list.IndexOf (row);
		}

		public int ConvertPositionToChildPosition (int row)
		{
			if (row >= 0 && row < list.Count)
				return list [row];
			return -1;
		}

		public object GetValue (int row, int column)
		{
			var childPos = ConvertPositionToChildPosition (row);
			if (childPos >= 0)
				return ChildStore.GetValue (childPos, column);
			return null;
		}

		public void SetValue (int row, int column, object value)
		{
			var childPos = ConvertPositionToChildPosition (row);
			if (childPos >= 0)
				ChildStore.SetValue(childPos, column, value);
			if (RowChanged != null)
				RowChanged (this, new ListRowEventArgs (row));
		}

		public int RowCount {
			get {
				return list.Count;
			}
		}

		public Type[] ColumnTypes {
			get {
				return ChildStore.ColumnTypes;
			}
		}

		public void EnableEvent (object eventId)
		{
		}

		public void DisableEvent (object eventId)
		{
		}
	}
}

