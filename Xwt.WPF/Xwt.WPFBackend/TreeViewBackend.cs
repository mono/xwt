// 
// TreeViewBackend.cs
//  
// Author:
//       Luís Reis <luiscubal@gmail.com>
// 
// Copyright (c) 2011 Luís Reis
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
using SWC=System.Windows.Controls;

namespace Xwt.WPFBackend
{
	public class TreeViewBackend : WidgetBackend, ITreeViewBackend
	{
		public SWC.TreeView Tree
		{
			get { return (SWC.TreeView)Widget; }
		}

		public TreeViewBackend ()
		{
			Widget = new SWC.TreeView ();
		}

		public void UnselectRow (TreePosition pos)
		{
			throw new NotImplementedException ();
		}

		public void SetSource (ITreeDataSource source, IBackend sourceBackend)
		{
			throw new NotImplementedException ();
		}

		public void SelectRow (TreePosition pos)
		{
		}

		public TreePosition[] SelectedRows
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		public bool IsRowSelected (TreePosition row)
		{
			throw new NotImplementedException ();
		}

		public bool IsRowExpanded (TreePosition row)
		{
			throw new NotImplementedException ();
		}

		public bool HeadersVisible
		{
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}

		public void ExpandRow (TreePosition row, bool expandChildren)
		{
			throw new NotImplementedException ();
		}

		public void CollapseRow (TreePosition row)
		{
			throw new NotImplementedException ();
		}

		public void UnselectAll ()
		{
			throw new NotImplementedException ();
		}

		public void SetSelectionMode (SelectionMode mode)
		{
			throw new NotImplementedException ();
		}

		public void SelectAll ()
		{
			throw new NotImplementedException ();
		}

		public void UpdateColumn (ListViewColumn column, object handle, ListViewColumnChange change)
		{
			throw new NotImplementedException ();
		}

		public void RemoveColumn (ListViewColumn column, object handle)
		{
			throw new NotImplementedException ();
		}

		public object AddColumn (ListViewColumn column)
		{
			throw new NotImplementedException ();
		}
	}
}
