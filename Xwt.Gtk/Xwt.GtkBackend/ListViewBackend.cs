// 
// ListViewBackend.cs
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

namespace Xwt.GtkBackend
{
	public class ListViewBackend<T, S>: TableViewBackend<T,S>, IListViewBackend where T:Gtk.TreeView where S:IListViewEventSink
	{
		public void SetSource (IListViewSource source, IBackend sourceBackend)
		{
			ListStoreBackend b = sourceBackend as ListStoreBackend;
			if (b == null)
				throw new NotSupportedException ("Custom list stores are not supported");
			Widget.Model = b.Store;
		}

		public void SelectRow (int row)
		{
			Gtk.TreeIter it;
			if (!Widget.Model.IterNthChild (out it, row))
				return;
			Widget.Selection.SelectIter (it);
		}

		public void UnselectRow (int row)
		{
			Gtk.TreeIter it;
			if (!Widget.Model.IterNthChild (out it, row))
				return;
			Widget.Selection.UnselectIter (it);
		}

		public int[] SelectedRows {
			get {
				var sel = Widget.Selection.GetSelectedRows ();
				int[] res = new int [sel.Length];
				for (int n=0; n<sel.Length; n++)
					res [n] = sel [n].Indices[0];
				return res;
			}
		}
	}
}

