// 
// ItemCollection.cs
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
using System.Collections.ObjectModel;

namespace Xwt
{
	public class ItemCollection: Collection<Object>, IListDataSource
	{
		internal ItemCollection ()
		{
		}
		
		public event EventHandler<ListRowEventArgs> RowInserted;
		public event EventHandler<ListRowEventArgs> RowDeleted;
		public event EventHandler<ListRowEventArgs> RowChanged;
		public event EventHandler<ListRowOrderEventArgs> RowsReordered;

		protected override void InsertItem (int index, object item)
		{
			base.InsertItem (index, item);
			if (RowInserted != null)
				RowInserted (this, new ListRowEventArgs (index));
		}
		
		protected override void RemoveItem (int index)
		{
			base.RemoveItem (index);
			if (RowDeleted != null)
				RowDeleted (this, new ListRowEventArgs (index));
		}
		
		protected override void SetItem (int index, object item)
		{
			base.SetItem (index, item);
			if (RowChanged != null)
				RowChanged (this, new ListRowEventArgs (index));
		}
		
		protected override void ClearItems ()
		{
			int count = Count;
			base.ClearItems ();
			for (int n=count - 1; n >= 0; n--) {
				if (RowDeleted != null)
					RowDeleted (this, new ListRowEventArgs (n));
			}
		}

		#region IListViewSource implementation
		object IListDataSource.GetValue (int row, int column)
		{
			if (column != 0)
				throw new InvalidOperationException ("Not data for column " + column);
			return this [row];
		}

		void IListDataSource.SetValue (int row, int column, object value)
		{
			if (column != 0)
				throw new InvalidOperationException ("Not data for column " + column);
			this [row] = value;
		}

		int IListDataSource.RowCount {
			get {
				return Count;
			}
		}

		Type[] IListDataSource.ColumnTypes {
			get {
				return new Type[] { typeof(string) };
			}
		}
		#endregion
	}
}

