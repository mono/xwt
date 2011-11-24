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
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Xwt
{
	public sealed class ItemCollection: Collection<Object>, IListDataSource
	{
		List<string> labels;
		
		internal ItemCollection ()
		{
		}
		
		public event EventHandler<ListRowEventArgs> RowInserted;
		public event EventHandler<ListRowEventArgs> RowDeleted;
		public event EventHandler<ListRowEventArgs> RowChanged;
		public event EventHandler<ListRowOrderEventArgs> RowsReordered;
		
		class ItemWithLabel {
			public object Item;
			public string Label;
		}
		
		public void Add (object item, string label)
		{
			Add (new ItemWithLabel () { Item = item, Label = label });
		}

		public void Insert (int index, object item, string label)
		{
			Insert (index, new ItemWithLabel () { Item = item, Label = label });
		}
		
		void InitLabelList (int index)
		{
			if (labels == null)
				labels = new List<string> ();
			for (int n=labels.Count - 1; n < index; n++)
				labels.Add (null);
		}

		protected override void InsertItem (int index, object item)
		{
			if (item is ItemWithLabel) {
				var itl = (ItemWithLabel) item;
				InitLabelList (index - 1);
				labels.Insert (index, itl.Label);
				item = itl.Item;
			}
			else if (labels != null && index < labels.Count)
				labels.Insert (index, null);
			
			base.InsertItem (index, item);
			if (RowInserted != null)
				RowInserted (this, new ListRowEventArgs (index));
		}
		
		protected override void RemoveItem (int index)
		{
			if (labels != null && index < labels.Count)
				labels.RemoveAt (index);
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
			if (labels != null)
				labels.Clear ();
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
			if (labels != null && row < labels.Count)
				return labels[row];
			else
				return this [row];
		}

		void IListDataSource.SetValue (int row, int column, object value)
		{
			if (column != 0)
				throw new InvalidOperationException ("Not data for column " + column);
			if (labels != null && row < labels.Count)
				labels [row] = (string)value;
			else
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

