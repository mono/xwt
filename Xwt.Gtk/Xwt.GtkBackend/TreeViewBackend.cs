// 
// TreeViewBackend.cs
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
	public class TreeViewBackend: TableViewBackend, ITreeViewBackend
	{
		public void SetSource (ITreeDataSource source, IBackend sourceBackend)
		{
			TreeStoreBackend b = sourceBackend as TreeStoreBackend;
			if (b == null) {
				CustomTreeModel model = new CustomTreeModel (source);
				Widget.Model = model.Store;
			} else
				Widget.Model = b.Store;
		}

		public TreePosition[] SelectedRows {
			get {
				var rows = Widget.Selection.GetSelectedRows ();
				IterPos[] sel = new IterPos [rows.Length];
				for (int i = 0; i < rows.Length; i++) {
					Gtk.TreeIter it;
					Widget.Model.GetIter (out it, rows[i]);
					sel[i] = new IterPos (-1, it);
				}
				return sel;
			}
		}
		
		public void SelectRow (TreePosition pos)
		{
			Widget.Selection.SelectIter (((IterPos)pos).Iter);
		}
		
		public void UnselectRow (TreePosition pos)
		{
			Widget.Selection.UnselectIter (((IterPos)pos).Iter);
		}
		
		public bool IsRowSelected (TreePosition pos)
		{
			return Widget.Selection.IterIsSelected (((IterPos)pos).Iter);
		}
		
		public bool IsRowExpanded (TreePosition pos)
		{
			return Widget.GetRowExpanded (Widget.Model.GetPath (((IterPos)pos).Iter));
		}
		
		public void ExpandRow (TreePosition pos, bool expandedChildren)
		{
			Widget.ExpandRow (Widget.Model.GetPath (((IterPos)pos).Iter), expandedChildren);
		}
		
		public void CollapseRow (TreePosition pos)
		{
			Widget.CollapseRow (Widget.Model.GetPath (((IterPos)pos).Iter));
		}
		
		public bool HeadersVisible {
			get {
				return Widget.HeadersVisible;
			}
			set {
				Widget.HeadersVisible = value;
			}
		}
	}
}

