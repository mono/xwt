// 
// TreeStoreBackend.cs
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
using Xwt.Backends;
using Xwt.WPFBackend.Utilities;
using SWC=System.Windows.Controls;

namespace Xwt.WPFBackend
{
	class TreeNode : TreePosition
	{
		public IList<Tuple<TreeViewBackend, SWC.ItemsControl>> TreeViewData = new List<Tuple<TreeViewBackend, SWC.ItemsControl>> ();
		public object[] Values;
		public IList<TreeNode> Children = new List<TreeNode>();
		public TreeNode Previous;
		public TreeNode Next;
		public TreeNode Parent;

		public TreeNode LastChild
		{
			get
			{
				return Children.Count == 0 ? null : Children[Children.Count - 1];
			}
		}
	}

	public class TreeStoreBackend: ITreeStoreBackend
	{
		public event EventHandler<TreeNodeEventArgs> NodeInserted;
		public event EventHandler<TreeNodeChildEventArgs> NodeDeleted;
		public event EventHandler<TreeNodeEventArgs> NodeChanged;
		public event EventHandler<TreeNodeOrderEventArgs> NodesReordered;

		private Type[] columnTypes;
		private TreeNode rootNode = new TreeNode ();

		public void Initialize (Type[] columnTypes)
		{
			this.columnTypes = new Type [columnTypes.Length];
			for (int n = 0; n < columnTypes.Length; n++)
			{
				this.columnTypes[n] = columnTypes[n];
			}
		}

		public void Initialize (object frontend)
		{
		}

		public void Clear ()
		{
			rootNode.Children.Clear ();
		}

		internal TreeNode RootNode
		{
			get
			{
				return rootNode;
			}
		}

		public TreePosition AddChild (TreePosition pos)
		{
			TreeNode parent = pos as TreeNode ?? rootNode;
			TreeNode node = new TreeNode();
			node.Previous = parent.LastChild;
			node.Values = new object[columnTypes.Length];
			node.Parent = parent;
			if (parent.LastChild != null)
				parent.LastChild.Next = node;
			parent.Children.Add (node);

			foreach (Tuple<TreeViewBackend, SWC.ItemsControl> parentData in parent.TreeViewData)
			{
				MultiColumnTreeViewItem childItem = parentData.Item1.GenerateTreeViewItem (node);
				node.TreeViewData.Add (Tuple.Create<TreeViewBackend, SWC.ItemsControl>(parentData.Item1, childItem));
				parentData.Item2.Items.Add (childItem);
			}

			return node;
		}

		public TreePosition GetPrevious (TreePosition pos)
		{
			TreeNode node = pos as TreeNode;
			return node.Previous;
		}

		public TreePosition GetNext (TreePosition pos)
		{
			TreeNode node = pos as TreeNode;
			return node.Next;
		}

		public void SetValue (TreePosition pos, int column, object value)
		{
			TreeNode node = pos as TreeNode;

			if (node == rootNode)
				throw new InvalidOperationException("Root node can not have data");

			node.Values[column] = value;

			foreach (var treeViewData in node.TreeViewData)
			{
				//We can perform this cast since Item2 is always a MultiColumnTreeViewItem for any non-root node.
				MultiColumnTreeViewItem treeViewItem = (MultiColumnTreeViewItem)treeViewData.Item2;
				treeViewItem.UpdateColumn (column, value);
			}
		}

		public object GetValue (TreePosition pos, int column)
		{
			TreeNode node = pos as TreeNode;
			return node.Values[column];
		}

		public TreePosition GetParent (TreePosition pos)
		{
			TreeNode node = pos as TreeNode;
			TreeNode parent = node.Parent;
			if (parent == rootNode)
				return null;
			return parent;
		}

		public int GetChildrenCount (TreePosition pos)
		{
			TreeNode node = pos as TreeNode;
			return node.Children.Count;
		}

		public TreePosition GetChild (TreePosition pos, int index)
		{
			TreeNode node = pos as TreeNode;
			return node.Children[index];
		}

		public void Remove (TreePosition pos)
		{
			TreeNode node = pos as TreeNode;

			if (node == rootNode)
				throw new InvalidOperationException ("Can not remove root node");

			node.Parent.Children.Remove (node);

			foreach (var treeViewData in node.TreeViewData)
			{
				var item = treeViewData.Item2;
				var parent = item.Parent;
				((SWC.ItemsControl)parent).Items.Remove (item);
			}
		}

		public TreePosition InsertBefore (TreePosition pos)
		{
			throw new NotImplementedException ();
		}

		public TreePosition InsertAfter (TreePosition pos)
		{
			throw new NotImplementedException ();
		}

		public Type[] ColumnTypes
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		public void EnableEvent (object eventId)
		{
		}

		public void DisableEvent (object eventId)
		{
		}
	}
}
