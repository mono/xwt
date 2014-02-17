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
using Xwt.Backends;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;


namespace Xwt
{
	[BackendType (typeof(IListStoreBackend))]
	public class ListStore: XwtComponent, INotifyCollectionChanged
	{
		IDataField[] fields;

		class ListStoreBackendHost: BackendHost<ListStore,IListStoreBackend>, IDataStore
		{
			protected override void OnBackendCreated ()
			{
				Backend.Initialize (Parent.fields.Select (f => f.FieldType).ToArray ());
				base.OnBackendCreated ();
			}

			#region ITableStore implementation

			public object GetValue (object pos, int column)
			{
				return Parent.Backend.GetValue ((int)pos, column);
			}

			public void SetValue (object pos, int column, object value)
			{
				Parent.Backend.SetValue ((int)pos, column, value);
			}

			#endregion
		}
		
		protected override Xwt.Backends.BackendHost CreateBackendHost ()
		{
			return new ListStoreBackendHost ();
		}

		#region INotifyCollectionChanged implementation

		event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged {
			add {
				Backend.CollectionChanged += value;
			}
			remove {
				Backend.CollectionChanged -= value;
			}
		}

		void OnCollectionChanged (NotifyCollectionChangedEventArgs args)
		{
		}

		#endregion
		
		public ListStore (params IDataField[] fields)
		{
			for (int n=0; n<fields.Length; n++) {
				if (fields[n].Index != -1)
					throw new InvalidOperationException ("DataField object already belongs to another data store");
				((IDataFieldInternal)fields[n]).SetIndex ((ListStoreBackendHost)BackendHost, n);
			}
			this.fields = fields;
		}
		
		IListStoreBackend Backend {
			get { return (IListStoreBackend) BackendHost.Backend; }
		}
		
		public ListStore ()
		{
		}
		
		public int RowCount {
			get {
				return Backend.RowCount;
			}
		}
		
		public T GetValue<T> (int row, IDataField<T> column)
		{
			return (T) Backend.GetValue (row, column.Index);
		}
		
		public void SetValue<T> (int row, IDataField<T> column, T value)
		{
			Backend.SetValue (row, column.Index, value);
		}
		
		public int AddRow ()
		{
			return Backend.AddRow ();
		}
		
		public void SetValues<T1,T2> (int row, IDataField<T1> column1, T1 value1, IDataField<T2> column2, T2 value2)
		{
			SetValue (row, column1, value1);
			SetValue (row, column2, value2);
		}

		public void SetValues<T1,T2,T3> (int row, IDataField<T1> column1, T1 value1, IDataField<T2> column2, T2 value2, IDataField<T3> column3, T3 value3)
		{
			SetValue (row, column1, value1);
			SetValue (row, column2, value2);
			SetValue (row, column3, value3);
		}

		public void SetValues<T1,T2,T3,T4> (
			int row, 
			IDataField<T1> column1, T1 value1, 
			IDataField<T2> column2, T2 value2, 
			IDataField<T3> column3, T3 value3,
			IDataField<T4> column4, T4 value4
		)
		{
			SetValue (row, column1, value1);
			SetValue (row, column2, value2);
			SetValue (row, column3, value3);
			SetValue (row, column4, value4);
		}

		public void SetValues<T1,T2,T3,T4,T5> (
			int row, 
			IDataField<T1> column1, T1 value1, 
			IDataField<T2> column2, T2 value2, 
			IDataField<T3> column3, T3 value3,
			IDataField<T4> column4, T4 value4,
			IDataField<T5> column5, T5 value5
		)
		{
			SetValue (row, column1, value1);
			SetValue (row, column2, value2);
			SetValue (row, column3, value3);
			SetValue (row, column4, value4);
			SetValue (row, column5, value5);
		}

		public void SetValues<T1,T2,T3,T4,T5,T6> (
			int row, 
			IDataField<T1> column1, T1 value1, 
			IDataField<T2> column2, T2 value2, 
			IDataField<T3> column3, T3 value3,
			IDataField<T4> column4, T4 value4,
			IDataField<T5> column5, T5 value5,
			IDataField<T6> column6, T6 value6
		)
		{
			SetValue (row, column1, value1);
			SetValue (row, column2, value2);
			SetValue (row, column3, value3);
			SetValue (row, column4, value4);
			SetValue (row, column5, value5);
			SetValue (row, column6, value6);
		}

		public void SetValues<T1,T2,T3,T4,T5,T6,T7> (
			int row, 
			IDataField<T1> column1, T1 value1, 
			IDataField<T2> column2, T2 value2, 
			IDataField<T3> column3, T3 value3,
			IDataField<T4> column4, T4 value4,
			IDataField<T5> column5, T5 value5,
			IDataField<T6> column6, T6 value6,
			IDataField<T7> column7, T7 value7
		)
		{
			SetValue (row, column1, value1);
			SetValue (row, column2, value2);
			SetValue (row, column3, value3);
			SetValue (row, column4, value4);
			SetValue (row, column5, value5);
			SetValue (row, column6, value6);
			SetValue (row, column7, value7);
		}

		public void SetValues<T1,T2,T3,T4,T5,T6,T7,T8> (
			int row, 
			IDataField<T1> column1, T1 value1, 
			IDataField<T2> column2, T2 value2, 
			IDataField<T3> column3, T3 value3,
			IDataField<T4> column4, T4 value4,
			IDataField<T5> column5, T5 value5,
			IDataField<T6> column6, T6 value6,
			IDataField<T7> column7, T7 value7,
			IDataField<T8> column8, T8 value8
		)
		{
			SetValue (row, column1, value1);
			SetValue (row, column2, value2);
			SetValue (row, column3, value3);
			SetValue (row, column4, value4);
			SetValue (row, column5, value5);
			SetValue (row, column6, value6);
			SetValue (row, column7, value7);
			SetValue (row, column8, value8);
		}

		public void SetValues<T1,T2,T3,T4,T5,T6,T7,T8,T9> (
			int row, 
			IDataField<T1> column1, T1 value1, 
			IDataField<T2> column2, T2 value2, 
			IDataField<T3> column3, T3 value3,
			IDataField<T4> column4, T4 value4,
			IDataField<T5> column5, T5 value5,
			IDataField<T6> column6, T6 value6,
			IDataField<T7> column7, T7 value7,
			IDataField<T8> column8, T8 value8,
			IDataField<T9> column9, T9 value9
		)
		{
			SetValue (row, column1, value1);
			SetValue (row, column2, value2);
			SetValue (row, column3, value3);
			SetValue (row, column4, value4);
			SetValue (row, column5, value5);
			SetValue (row, column6, value6);
			SetValue (row, column7, value7);
			SetValue (row, column8, value8);
			SetValue (row, column9, value9);
		}

		public void SetValues<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10> (
			int row, 
			IDataField<T1> column1, T1 value1, 
			IDataField<T2> column2, T2 value2, 
			IDataField<T3> column3, T3 value3,
			IDataField<T4> column4, T4 value4,
			IDataField<T5> column5, T5 value5,
			IDataField<T6> column6, T6 value6,
			IDataField<T7> column7, T7 value7,
			IDataField<T8> column8, T8 value8,
			IDataField<T9> column9, T9 value9,
			IDataField<T10> column10, T10 value10
		)
		{
			SetValue (row, column1, value1);
			SetValue (row, column2, value2);
			SetValue (row, column3, value3);
			SetValue (row, column4, value4);
			SetValue (row, column5, value5);
			SetValue (row, column6, value6);
			SetValue (row, column7, value7);
			SetValue (row, column8, value8);
			SetValue (row, column9, value9);
			SetValue (row, column10, value10);
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
	
	class DefaultListStoreBackend: IListStoreBackend
	{
		List<object[]> list = new List<object[]> ();
		Type[] columnTypes;

		public void InitializeBackend (object frontend, ApplicationContext context)
		{
		}
		
		public void Initialize (Type[] columnTypes)
		{
			this.columnTypes = columnTypes;
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		void NotifyChanged (NotifyCollectionChangedEventArgs args)
		{
			if (CollectionChanged != null)
				CollectionChanged (this, args);
		}
		
		public object GetValue (int row, int column)
		{
			return list [row][column];
		}

		public void SetValue (int row, int column, object value)
		{
			list [row] [column] = value;
			NotifyChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, row));
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
			NotifyChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, row, row));
			return row;
		}
		
		public int InsertRowAfter (int row)
		{
			object[] data = new object [columnTypes.Length];
			list.Insert (row + 1, data);
			NotifyChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, row + 1, row + 1));
			return row + 1;
		}
		
		public int InsertRowBefore (int row)
		{
			object[] data = new object [columnTypes.Length];
			list.Insert (row, data);
			NotifyChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, row, row));
			return row;
		}
		
		public void RemoveRow (int row)
		{
			list.RemoveAt (row);
			NotifyChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, row, row));
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
				NotifyChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, n, n));
			}
		}
	}
}

