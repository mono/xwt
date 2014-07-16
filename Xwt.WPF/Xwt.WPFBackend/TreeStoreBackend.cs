// 
// TreeStoreBackend.cs
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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Xwt.Backends;

namespace Xwt.WPFBackend
{
	public class TreeStoreBackend
		: Backend, ITreeStoreBackend, INotifyPropertyChanged, INotifyCollectionChanged, IEnumerable
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public event NotifyCollectionChangedEventHandler CollectionChanged
		{
			add { this.topNodes.CollectionChanged += value; }
			remove { this.topNodes.CollectionChanged -= value; }
		}

		public event EventHandler<TreeNodeEventArgs> NodeInserted;
		public event EventHandler<TreeNodeChildEventArgs> NodeDeleted;
		public event EventHandler<TreeNodeEventArgs> NodeChanged;
		public event EventHandler<TreeNodeOrderEventArgs> NodesReordered;

		public Type[] ColumnTypes
		{
			get { return this.columnTypes; }
		}

		public void Initialize (Type[] columnTypes)
		{
			this.columnTypes = columnTypes.ToArray ();
			OnPropertyChanged ("ColumnTypes");
		}

		public TreePosition GetParent (TreePosition pos)
		{
			var node = (TreeStoreNode) pos;
			if (node.Parent == null)
				return null;

			return node.Parent;
		}

		public TreePosition GetChild (TreePosition pos, int index)
		{
			var node = (TreeStoreNode) pos;
			var list = GetListForNode (node);
			if (list.Count == 0 || index >= list.Count)
				return null;

			return list [index];
		}

		public int GetChildrenCount (TreePosition pos)
		{
			return GetListForNode ((TreeStoreNode) pos).Count;
		}

		public object GetValue (TreePosition pos, int column)
		{
			return ((TreeStoreNode) pos)[column];
		}

		public void SetValue (TreePosition pos, int column, object value)
		{
			var node = (TreeStoreNode) pos;
			node[column] = value;

			OnNodeChanged (new TreeNodeEventArgs (pos));
		}

		public TreePosition InsertBefore (TreePosition pos)
		{
			var node = (TreeStoreNode) pos;

			var newNode = new TreeStoreNode (
				new object[this.columnTypes.Length],
				node.Parent);

			var list = GetContainingList (node);
			int index = list.IndexOf (node);
			list.Insert (index, newNode);
			
			OnNodeInserted (new TreeNodeEventArgs (newNode));

			return newNode;
		}

		public TreePosition InsertAfter (TreePosition pos)
		{
			var node = (TreeStoreNode) pos;

			var newNode = new TreeStoreNode (
				new object[this.columnTypes.Length],
				node.Parent);

			var list = GetContainingList (node);
			int index = list.IndexOf (node);
			list.Insert (index + 1, newNode);
			
			OnNodeInserted (new TreeNodeEventArgs (newNode));

			return newNode;
		}

		public TreePosition AddChild (TreePosition pos)
		{
			var parent = (TreeStoreNode) pos;

			var childNode = new TreeStoreNode (
				new object[this.columnTypes.Length],
				parent);

			GetListForNode (parent).Add (childNode);

			OnNodeInserted (new TreeNodeEventArgs (childNode));

			return childNode;
		}

		public void Remove (TreePosition pos)
		{
			var node = (TreeStoreNode) pos;

			var list = GetContainingList (node);
			int index = list.IndexOf (node);
			list.RemoveAt (index);

			OnNodeDeleted (new TreeNodeChildEventArgs (node.Parent, index));
		}

		public TreePosition GetNext (TreePosition pos)
		{
			var node = (TreeStoreNode) pos;

			var list = GetContainingList (node);
			int index = list.IndexOf (node) + 1;

			return (index < list.Count) ? list [index] : null;
		}

		public TreePosition GetPrevious (TreePosition pos)
		{
			var node = (TreeStoreNode) pos;

			var list = GetContainingList (node);
			int index = list.IndexOf (node) - 1;

			return (index >= 0) ? list [index] : null;
		}

		public void Clear ()
		{
			this.topNodes.Clear();
		}

		public IEnumerator GetEnumerator ()
		{
			return this.topNodes.GetEnumerator ();
		}

		private Type[] columnTypes;
		private readonly ObservableCollection<TreeStoreNode> topNodes = new ObservableCollection<TreeStoreNode> ();

		private ObservableCollection<TreeStoreNode> GetContainingList (TreeStoreNode node)
		{
			return (node.Parent == null) ? this.topNodes : node.Parent.Children;
		}

		private ObservableCollection<TreeStoreNode> GetListForNode (TreeStoreNode node)
		{
			return (node == null) ? this.topNodes : node.Children;
		}

		private void OnPropertyChanged (string name)
		{
			var changed = PropertyChanged;
			if (changed != null)
				changed (this, new PropertyChangedEventArgs (name));
		}

		private void OnNodeInserted (TreeNodeEventArgs e)
		{
			var handler = NodeInserted;
			if (handler != null)
				handler (this, e);
		}

		private void OnNodeDeleted (TreeNodeChildEventArgs e)
		{
			var handler = NodeDeleted;
			if (handler != null)
				handler (this, e);
		}

		private void OnNodeChanged (TreeNodeEventArgs e)
		{
			var handler = NodeChanged;
			if (handler != null)
				handler (this, e);
		}

		private void OnNodesReordered (TreeNodeOrderEventArgs e)
		{
			var handler = NodesReordered;
			if (handler != null)
				handler (this, e);
		}
	}
}
