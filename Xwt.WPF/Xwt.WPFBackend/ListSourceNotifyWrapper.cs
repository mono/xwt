// 
// ListSourceNotifyWrapper.cs
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
using System.Collections.Specialized;

namespace Xwt.WPFBackend
{
	internal class ListSourceNotifyWrapper
		: IEnumerable, INotifyCollectionChanged
	{
		public ListSourceNotifyWrapper (IListDataSource source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			this.source = source;
			this.source.RowInserted += OnRowsUpdated;
			this.source.RowChanged += OnRowsUpdated;
			this.source.RowsReordered += OnRowsUpdated;
			this.source.RowDeleted += OnRowsUpdated;
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public IEnumerator GetEnumerator ()
		{
			int cols = this.source.ColumnTypes.Length;
			for (int i = 0; i < this.source.RowCount; ++i) {
				object[] row = new object[cols];
				for (int c = 0; c < row.Length; ++c)
					row [c] = this.source.GetValue (i, c);

				yield return row;
			}
		}

		private readonly IListDataSource source;
		
		private void OnRowsUpdated (object sender, ListRowEventArgs e)
		{
			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset));
		}

		private void OnCollectionChanged (NotifyCollectionChangedEventArgs e)
		{
			var changed = this.CollectionChanged;
			if (changed != null)
				changed (this, e);
		}
	}
}