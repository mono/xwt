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
using System.Collections.Generic;
using Xwt.Engine;
using Xwt.Backends;
using Xwt.WPFBackend.Utilities;
using SWC=System.Windows.Controls;

namespace Xwt.WPFBackend
{
	public class TreeViewBackend : WidgetBackend, ITreeViewBackend
	{
		private IList<ListViewColumn> columns = new List<ListViewColumn> ();

		protected new ITreeViewEventSink EventSink
		{
			get { return (ITreeViewEventSink)base.EventSink; }
		}

		public TreeViewBackend ()
		{
			Tree = new SWC.TreeView ();
		}

		public SWC.TreeView Tree
		{
			get
			{
				return (SWC.TreeView)Widget;
			}
			set
			{
				Widget = value;
			}
		}

		public void UnselectRow (TreePosition pos)
		{
			throw new NotImplementedException ();
		}

		public void SetSource (ITreeDataSource source, IBackend sourceBackend)
		{
			Tree.Items.Clear ();
			TreeStoreBackend treeStore = sourceBackend as TreeStoreBackend;
			if (treeStore != null)
			{
				TreeNode node = treeStore.RootNode;
				foreach (TreeNode child in node.Children)
				{
					Tree.Items.Add (GenerateTreeViewItem (child));
				}
			}
		}

		private MultiColumnTreeViewItem GenerateTreeViewItem (TreeNode node)
		{
			MultiColumnTreeViewItem item = new MultiColumnTreeViewItem (node);

			foreach (ListViewColumn column in columns)
			{
				item.AddColumn (column);
			}

			foreach (TreeNode child in node.Children)
			{
				item.Items.Add (GenerateTreeViewItem (child));
			}

			return item;
		}

		public void SelectRow (TreePosition pos)
		{
			throw new NotImplementedException ();
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
			columns.Add (column);

			foreach (SWC.TreeViewItem item in Tree.Items)
				((MultiColumnTreeViewItem)item).AddColumn (column);

			return column;
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is TableViewEvent)
			{
				if (((TableViewEvent)eventId) == TableViewEvent.SelectionChanged)
					Tree.SelectedItemChanged += HandleWidgetSelectionChanged;
			}
		}

		void HandleWidgetSelectionChanged (object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
		{
			Toolkit.Invoke (delegate
			{
				EventSink.OnSelectionChanged ();
			});
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is TableViewEvent)
			{
				if (((TableViewEvent)eventId) == TableViewEvent.SelectionChanged)
					Tree.SelectedItemChanged -= HandleWidgetSelectionChanged;
			}
		}
	}
}
