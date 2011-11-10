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
		public void SetSource (ITreeViewSource source, IBackend sourceBackend)
		{
			TreeStoreBackend b = sourceBackend as TreeStoreBackend;
			if (b == null)
				throw new NotSupportedException ("Custom tree stores are not supported");
			Widget.Model = b.Store;
		}

		public TreePosition[] SelectedItems {
			get {
				var rows = Widget.Selection.GetSelectedRows ();
				IterPos[] sel = new IterPos [rows.Length];
				for (int i = 0; i < rows.Length; i++) {
					Gtk.TreeIter it;
					Widget.Model.GetIter (out it, rows[i]);
					sel[i] = new IterPos (it);
				}
				return sel;
			}
		}
		
		public void SelectItem (TreePosition pos)
		{
			Widget.Selection.SelectIter (((IterPos)pos).Iter);
		}
		
		public void UnselectItem (TreePosition pos)
		{
			Widget.Selection.UnselectIter (((IterPos)pos).Iter);
		}
	}
}

