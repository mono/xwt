// 
// TreeStore.cs
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
using System.Linq;
using System.Collections.Generic;
using Xwt.Backends;
using System.ComponentModel;


namespace Xwt
{
	[BackendType (typeof(ITreeStoreBackend))]
	public class TreeStore: XwtComponent, ITreeDataSource
	{
		IDataField[] fields;
		
		class TreeStoreBackendHost: BackendHost<TreeStore,ITreeStoreBackend>
		{
			protected override IBackend OnCreateBackend ()
			{
				var b = base.OnCreateBackend ();
				if (b == null)
					b = new DefaultTreeStoreBackend ();
				((ITreeStoreBackend)b).Initialize (Parent.fields.Select (f => f.FieldType).ToArray ());
				return b;
			}
		}
		
		protected override Xwt.Backends.BackendHost CreateBackendHost ()
		{
			return new TreeStoreBackendHost ();
		}
		
		public TreeStore (params IDataField[] fields)
		{
			for (int n=0; n<fields.Length; n++) {
				if (fields[n].Index != -1)
					throw new InvalidOperationException ("DataField object already belongs to another data store");
				((IDataFieldInternal)fields[n]).SetIndex (n);
			}
			this.fields = fields;
		}
		
		ITreeStoreBackend Backend {
			get { return (ITreeStoreBackend)BackendHost.Backend; }
		}
		
		public TreeNavigator GetFirstNode ()
		{
			var p = Backend.GetChild (null, 0);
			return new TreeNavigator (Backend, p);
		}
		
		public TreeNavigator GetNavigatorAt (TreePosition pos)
		{
			return new TreeNavigator (Backend, pos);
		}
		
		public TreeNavigator AddNode ()
		{
			var pos = Backend.AddChild (null);
			return new TreeNavigator (Backend, pos);
		}

		public TreeNavigator AddNode (TreePosition position)
		{
			var pos = Backend.AddChild (position);
			return new TreeNavigator (Backend, pos);
		}

		public TreeNavigator InsertNodeAfter (TreePosition positon)
		{
			var pos = Backend.InsertAfter (positon);
			return new TreeNavigator (Backend, pos);
		}

		public TreeNavigator InsertNodeBefore (TreePosition positon)
		{
			var pos = Backend.InsertBefore (positon);
			return new TreeNavigator (Backend, pos);
		}

		public IEnumerable<TreeNavigator> FindNavigators<T> (T fieldValue, IDataField<T> field)
		{
			if (fieldValue == null) {
				return Enumerable.Empty<TreeNavigator> ();
			}
			TreeNavigator navigator = GetFirstNode ();
			if (navigator.CurrentPosition == null) {
				return Enumerable.Empty<TreeNavigator> ();
			}
			return FindNavigators (fieldValue, field, navigator);
		}

		static IEnumerable<TreeNavigator> FindNavigators<T> (T fieldValue, IDataField<T> field, TreeNavigator navigator)
		{
			do {
				if (IsNavigator (navigator, fieldValue, field)) {
					yield return navigator.Clone ();
				}
				foreach (TreeNavigator foundChild in FindChildNavigators (navigator, fieldValue, field)) {
					yield return foundChild.Clone ();
				}
			} while (navigator.MoveNext());
		}

		static IEnumerable<TreeNavigator> FindChildNavigators<T> (TreeNavigator navigator, T fieldValue, IDataField<T> field)
		{
			if (!navigator.MoveToChild ()) {
				yield break;
			}
			foreach (var treeNavigator in FindNavigators (fieldValue, field, navigator)) {
				yield return treeNavigator;
			}
			navigator.MoveToParent ();
		}

		static bool IsNavigator<T> (TreeNavigator navigator, T fieldValue, IDataField<T> field)
		{
			T value = navigator.GetValue (field);
			return fieldValue.Equals (value);
		}
		
		public void Clear ()
		{
			Backend.Clear ();
		}

		event EventHandler<TreeNodeEventArgs> ITreeDataSource.NodeInserted {
			add { Backend.NodeInserted += value; }
			remove { Backend.NodeInserted -= value; }
		}
		event EventHandler<TreeNodeChildEventArgs> ITreeDataSource.NodeDeleted {
			add { Backend.NodeDeleted += value; }
			remove { Backend.NodeDeleted -= value; }
		}
		event EventHandler<TreeNodeEventArgs> ITreeDataSource.NodeChanged {
			add { Backend.NodeChanged += value; }
			remove { Backend.NodeChanged -= value; }
		}
		event EventHandler<TreeNodeOrderEventArgs> ITreeDataSource.NodesReordered {
			add { Backend.NodesReordered += value; }
			remove { Backend.NodesReordered -= value; }
		}
		event EventHandler ITreeDataSource.Cleared
		{
			add { Backend.Cleared += value; }
			remove { Backend.Cleared -= value; }
		}
		
		TreePosition ITreeDataSource.GetChild (TreePosition pos, int index)
		{
			return Backend.GetChild (pos, index);
		}

		TreePosition ITreeDataSource.GetParent (TreePosition pos)
		{
			return Backend.GetParent (pos);
		}

		int ITreeDataSource.GetChildrenCount (TreePosition pos)
		{
			return Backend.GetChildrenCount (pos);
		}

		object ITreeDataSource.GetValue (TreePosition pos, int column)
		{
			return Backend.GetValue (pos, column);
		}

		void ITreeDataSource.SetValue (TreePosition pos, int column, object val)
		{
			Backend.SetValue (pos, column, val);
		}
		
		Type[] ITreeDataSource.ColumnTypes {
			get {
				return fields.Select (f => f.FieldType).ToArray ();
			}
		}
	}
	
	class DefaultTreeStoreBackend: ITreeStoreBackend
	{
		struct Node {
			public object[] Data;
			public NodeList Children;
			public int NodeId;
		}
		
		class NodePosition: TreePosition
		{
			public NodeList ParentList;
			public int NodeIndex;
			public int NodeId;
			public int StoreVersion;
			
			public override bool Equals (object obj)
			{
				NodePosition other = (NodePosition) obj;
				if (other == null)
					return false;
				return ParentList == other.ParentList && NodeId == other.NodeId;
			}
			
			public override int GetHashCode ()
			{
				return ParentList.GetHashCode () ^ NodeId;
			}
		}
		
		class NodeList: List<Node>
		{
			public NodePosition Parent;
		}
		
		Type[] columnTypes;
		NodeList rootNodes = new NodeList ();
		int version;
		int nextNodeId;
		
		public event EventHandler<TreeNodeEventArgs> NodeInserted;
		public event EventHandler<TreeNodeChildEventArgs> NodeDeleted;
		public event EventHandler<TreeNodeEventArgs> NodeChanged;
		public event EventHandler<TreeNodeOrderEventArgs> NodesReordered;
		public event EventHandler Cleared;

		public void InitializeBackend (object frontend, ApplicationContext context)
		{
		}
		
		public void Initialize (Type[] columnTypes)
		{
			this.columnTypes = columnTypes;
		}
		
		public void Clear ()
		{
			rootNodes.Clear ();
			if (Cleared != null)
				Cleared (this, EventArgs.Empty);
		}
		
		NodePosition GetPosition (TreePosition pos)
		{
			if (pos == null)
				return null;
			NodePosition np = (NodePosition)pos;
			if (np.StoreVersion != version) {
				np.NodeIndex = -1;
				for (int i=0; i<np.ParentList.Count; i++) {
					if (np.ParentList [i].NodeId == np.NodeId) {
						np.NodeIndex = i;
						break;
					}
				}
				if (np.NodeIndex == -1)
					throw new InvalidOperationException ("Invalid node position");
				np.StoreVersion = version;
			}
			return np;
		}
		
		public void SetValue (TreePosition pos, int column, object value)
		{
			NodePosition n = GetPosition (pos);
			Node node = n.ParentList [n.NodeIndex];
			if (node.Data == null) {
				node.Data = new object [columnTypes.Length];
				n.ParentList [n.NodeIndex] = node;
			}
			node.Data [column] = value;
			if (NodeChanged != null)
				NodeChanged (this, new TreeNodeEventArgs (pos, n.NodeIndex));
		}

		public object GetValue (TreePosition pos, int column)
		{
			NodePosition np = GetPosition (pos);
			Node n = np.ParentList[np.NodeIndex];
			if (n.Data == null)
				return null;
			return n.Data [column];
		}

		public TreePosition GetChild (TreePosition pos, int index)
		{
			if (pos == null) {
				if (rootNodes.Count == 0)
					return null;
				Node n = rootNodes[index];
				return new NodePosition () { ParentList = rootNodes, NodeId = n.NodeId, NodeIndex = index, StoreVersion = version };
			} else {
				NodePosition np = GetPosition (pos);
				Node n = np.ParentList[np.NodeIndex];
				if (n.Children == null || index >= n.Children.Count)
					return null;
				return new NodePosition () { ParentList = n.Children, NodeId = n.Children[index].NodeId, NodeIndex = index, StoreVersion = version };
			}
		}
		
		public TreePosition GetNext (TreePosition pos)
		{
			NodePosition np = GetPosition (pos);
			if (np.NodeIndex >= np.ParentList.Count - 1)
				return null;
			Node n = np.ParentList[np.NodeIndex + 1];
			return new NodePosition () { ParentList = np.ParentList, NodeId = n.NodeId, NodeIndex = np.NodeIndex + 1, StoreVersion = version };
		}

		public TreePosition GetPrevious (TreePosition pos)
		{
			NodePosition np = GetPosition (pos);
			if (np.NodeIndex <= 0)
				return null;
			Node n = np.ParentList[np.NodeIndex - 1];
			return new NodePosition () { ParentList = np.ParentList, NodeId = n.NodeId, NodeIndex = np.NodeIndex - 1, StoreVersion = version };
		}

		public int GetChildrenCount (TreePosition pos)
		{
			if (pos == null)
				return rootNodes.Count;
			
			NodePosition np = GetPosition (pos);
			Node n = np.ParentList[np.NodeIndex];
			return n.Children != null ? n.Children.Count : 0;
		}

		public TreePosition InsertBefore (TreePosition pos)
		{
			NodePosition np = GetPosition (pos);
			Node nn = new Node ();
			nn.NodeId = nextNodeId++;
			
			np.ParentList.Insert (np.NodeIndex, nn);
			version++;
			
			// Update the NodePosition since it is now invalid
			np.NodeIndex++;
			np.StoreVersion = version;
			
			var node = new NodePosition () { ParentList = np.ParentList, NodeId = nn.NodeId, NodeIndex = np.NodeIndex - 1, StoreVersion = version };
			if (NodeInserted != null)
				NodeInserted (this, new TreeNodeEventArgs (node, node.NodeIndex));
			return node;
		}

		public TreePosition InsertAfter (TreePosition pos)
		{
			NodePosition np = GetPosition (pos);
			Node nn = new Node ();
			nn.NodeId = nextNodeId++;
			
			np.ParentList.Insert (np.NodeIndex + 1, nn);
			version++;
			
			// Update the provided position is still valid
			np.StoreVersion = version;
			
			var node = new NodePosition () { ParentList = np.ParentList, NodeId = nn.NodeId, NodeIndex = np.NodeIndex + 1, StoreVersion = version };
			if (NodeInserted != null)
				NodeInserted (this, new TreeNodeEventArgs (node, node.NodeIndex));
			return node;
		}
		
		public TreePosition AddChild (TreePosition pos)
		{
			NodePosition np = GetPosition (pos);
			
			Node nn = new Node ();
			nn.NodeId = nextNodeId++;
			
			NodeList list;
			
			if (pos == null) {
				list = rootNodes;
			} else {
				Node n = np.ParentList [np.NodeIndex];
				if (n.Children == null) {
					n.Children = new NodeList ();
					n.Children.Parent = new NodePosition () { ParentList = np.ParentList, NodeId = n.NodeId, NodeIndex = np.NodeIndex, StoreVersion = version };
					np.ParentList [np.NodeIndex] = n;
				}
				list = n.Children;
			}
			list.Add (nn);
			version++;
			
			// The provided position is unafected by this change. Keep it valid.
			if (np != null)
				np.StoreVersion = version;
			
			var node = new NodePosition () { ParentList = list, NodeId = nn.NodeId, NodeIndex = list.Count - 1, StoreVersion = version };
			if (NodeInserted != null)
				NodeInserted (this, new TreeNodeEventArgs (node, node.NodeIndex));
			return node;
		}
		
		public void Remove (TreePosition pos)
		{
			// Remove all child nodes in reverse order and notify client of each removed child.
			// This allows clients to keep track of all removed nodes, before the current node
			// will be removed and invalidated.
			for (int i = GetChildrenCount (pos) - 1; i >= 0 ; i--)
				Remove (GetChild (pos, i));
			NodePosition np = GetPosition (pos);
			np.ParentList.RemoveAt (np.NodeIndex);
			var parent = np.ParentList.Parent;
			var index = np.NodeIndex;
			version++;
			if (NodeDeleted != null)
				NodeDeleted (this, new TreeNodeChildEventArgs (parent, index, pos));
		}
		
		public TreePosition GetParent (TreePosition pos)
		{
			NodePosition np = GetPosition (pos);
			if (np.ParentList == rootNodes)
				return null;
			var parent = np.ParentList.Parent;
			return new NodePosition () { ParentList = parent.ParentList, NodeId = parent.NodeId, NodeIndex = parent.NodeIndex, StoreVersion = version };
		}
		
		public Type[] ColumnTypes {
			get {
				return columnTypes;
			}
		}
		
		public void EnableEvent (object eventId)
		{
		}
		
		public void DisableEvent (object eventId)
		{
		}

		protected virtual void OnNodesReordered(ListRowOrderEventArgs e)
		{
			if (NodesReordered != null) System.Diagnostics.Debug.WriteLine($"No support for {nameof(NodesReordered)} events from {nameof(DefaultTreeStoreBackend)}, sorry.");
		}
	}
}
