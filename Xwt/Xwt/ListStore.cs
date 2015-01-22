// 
// ListStore.cs
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
using System.Collections.Generic;
using System.Linq;
using Xwt.Backends;


namespace Xwt
{
	[BackendType (typeof(IListStoreBackend))]
	public class ListStore: ListStoreBase
	{
		IDataField[] fields;

		class ListStoreBackendHost: BackendHost<ListStore,IListStoreBackend>
		{
			protected override IBackend OnCreateBackend ()
			{
				var b = base.OnCreateBackend ();
				if (b == null)
					b = new DefaultListStoreBackend ();
				return b;
			}

			protected override void OnBackendCreated ()
			{
				Backend.Initialize (Parent.fields.Select (f => f.FieldType).ToArray ());
				base.OnBackendCreated ();
			}
		}
		
		protected override BackendHost CreateBackendHost ()
		{
			return new ListStoreBackendHost ();
		}
		
		public ListStore (params IDataField[] fields)
		{
			for (int n=0; n<fields.Length; n++) {
				if (fields[n].Index != -1)
					throw new InvalidOperationException ("DataField object already belongs to another data store");
				((IDataFieldInternal)fields[n]).SetIndex (n);
			}
			this.fields = fields;
		}
		
		IListStoreBackend Backend {
			get { return (IListStoreBackend) BackendHost.Backend; }
		}
		
		public ListStore ()
		{
		}

		protected override Type[] GetColumnTypes ()
		{
			return fields.Select (f => f.FieldType).ToArray ();
		}
		
		public int AddRow ()
		{
			return Backend.AddRow ();
		}

		public int InsertRowAfter (int row)
		{
			return Backend.InsertRowAfter (row);
		}
		
		public int InsertRowBefore (int row)
		{
			return Backend.InsertRowBefore (row);
		}
		
		public void RemoveRow (int row)
		{
			Backend.RemoveRow (row);
		}
		
		public void Clear ()
		{
			Backend.Clear ();
		}
	}
	
	public class DefaultListStoreBackend: IListStoreBackend
	{
		List<object[]> list = new List<object[]> ();
		Type[] columnTypes;
		
		public event EventHandler<ListRowEventArgs> RowInserted;
		public event EventHandler<ListRowEventArgs> RowDeleted;
		public event EventHandler<ListRowEventArgs> RowChanged;
		public event EventHandler<ListRowOrderEventArgs> RowsReordered;

		public void InitializeBackend (object frontend, ApplicationContext context)
		{
		}
		
		public void Initialize (Type[] columnTypes)
		{
			this.columnTypes = columnTypes;
		}
		
		public object GetValue (int row, int column)
		{
			return list [row][column];
		}

		public void SetValue (int row, int column, object value)
		{
			list [row] [column] = value;
			if (RowChanged != null)
				RowChanged (this, new ListRowEventArgs (row));
		}
		
		public Type[] ColumnTypes {
			get {
				return columnTypes;
			}
		}

		public int RowCount {
			get {
				return list.Count;
			}
		}
		
		public int AddRow ()
		{
			object[] data = new object [columnTypes.Length];
			list.Add (data);
			int row = list.Count - 1;
			if (RowInserted != null)
				RowInserted (this, new ListRowEventArgs (row));
			return row;
		}
		
		public int InsertRowAfter (int row)
		{
			object[] data = new object [columnTypes.Length];
			list.Insert (row + 1, data);
			if (RowInserted != null)
				RowInserted (this, new ListRowEventArgs (row + 1));
			return row + 1;
		}
		
		public int InsertRowBefore (int row)
		{
			object[] data = new object [columnTypes.Length];
			list.Insert (row, data);
			if (RowInserted != null)
				RowInserted (this, new ListRowEventArgs (row));
			return row;
		}
		
		public void RemoveRow (int row)
		{
			list.RemoveAt (row);
			if (RowDeleted != null)
				RowDeleted (this, new ListRowEventArgs (row));
		}
		
		public void EnableEvent (object eventId)
		{
		}
		
		public void DisableEvent (object eventId)
		{
		}
		
		public void Clear ()
		{
			int count = list.Count;
			list.Clear ();
			for (int n=0; n<count; n++) {
				if (RowDeleted != null)
					RowDeleted (this, new ListRowEventArgs (n));
			}
		}
	}
}

