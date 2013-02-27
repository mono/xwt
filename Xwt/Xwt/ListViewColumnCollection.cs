// 
// ListViewColumnCollection.cs
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

namespace Xwt
{
	public class ListViewColumnCollection: Collection<ListViewColumn>
	{
		IColumnContainer parent;
		IColumnContainerBackend container;
		
		internal ListViewColumnCollection (IColumnContainer parent)
		{
			this.parent = parent;
		}
		
		internal void Attach (IColumnContainerBackend container)
		{
			this.container = container;
			foreach (var col in this) {
				col.Parent = container;
				col.Handle = container.AddColumn (col);
			}
		}
		
		public ListViewColumn Add (string title, params IDataField[] fields)
		{
			ListViewColumn col = new ListViewColumn (title);
			foreach (var f in fields)
				col.Views.Add (CellView.GetDefaultCellView (f));
			base.Add (col);
			return col;
		}
		
		public ListViewColumn Add (string title, params CellView[] cells)
		{
			ListViewColumn col = new ListViewColumn (title);
			foreach (var c in cells)
				col.Views.Add (c);
			base.Add (col);
			return col;
		}

		public new ListViewColumn Add (ListViewColumn col)
		{
			base.Add (col);
			return col;
		}
		
		protected override void InsertItem (int index, ListViewColumn item)
		{
			if (item.Parent != null)
				throw new InvalidOperationException ("Column already belongs to a list or tree");
			if (container != null) {
				item.Handle = container.AddColumn (item);
				item.Parent = container;
			}
			base.InsertItem (index, item);
			parent.NotifyColumnsChanged ();
		}
		
		protected override void RemoveItem (int index)
		{
			if (container != null) {
				var col = this[index];
				container.RemoveColumn (col, col.Handle);
				col.Parent = null;
			}
			base.RemoveItem (index);
			parent.NotifyColumnsChanged ();
		}
		
		protected override void SetItem (int index, ListViewColumn item)
		{
			if (item.Parent != null)
				throw new InvalidOperationException ("Column already belongs to a list or tree");
			if (container != null) {
				var col = this[index];
				container.RemoveColumn (col, col.Handle);
				col.Parent = null;
				item.Handle = container.AddColumn (item);
				item.Parent = container;
			}
			base.SetItem (index, item);
			parent.NotifyColumnsChanged ();
		}
		
		protected override void ClearItems ()
		{
			if (container != null) {
				foreach (var col in this) {
					container.RemoveColumn (col, col.Handle);
					col.Parent = null;
				}
			}
			base.ClearItems ();
			parent.NotifyColumnsChanged ();
		}
	}
}
