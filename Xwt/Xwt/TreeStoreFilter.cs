//
// TreeFilterStore.cs
//
// Author:
//       Vsevolod Kukol <sevo@sevo.org>
//
// Copyright (c) 2014 Vsevolod Kukol
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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;

namespace Xwt
{
	[BackendType (typeof(ITreeStoreFilterBackend))]
	public class TreeStoreFilter: TreeStoreBase
	{
		ITreeDataSource childStore;

		public ITreeDataSource ChildStore {
			get {
				return childStore;
			}
		}

		class TreeFilterStoreBackendHost: BackendHost<TreeStoreFilter,ITreeStoreFilterBackend>
		{
			protected override IBackend OnCreateBackend ()
			{
				var b = base.OnCreateBackend ();
				if (b == null)
					b = new DefaultTreeStoreFilterBackend ();
				((ITreeStoreFilterBackend)b).Initialize (Parent.ChildStore);
				return b;
			}
		}

		protected override BackendHost CreateBackendHost ()
		{
			return new TreeFilterStoreBackendHost ();
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="Xwt.TreeStoreFilter"/> class.
		/// </summary>
		/// <param name="store">The tree data store to pass through the filter.</param>
		public TreeStoreFilter (ITreeDataSource store)
		{
			if (store == null)
				throw new ArgumentNullException ("store");
			childStore = store;
		}

		ITreeStoreFilterBackend Backend {
			get { return (ITreeStoreFilterBackend)BackendHost.Backend; }
		}

		/// <summary>
		/// Sets the filter function, which should be used to filter single nodes.
		/// </summary>
		/// <value>The filter function.</value>
		/// <remarks>
		/// The function must return <c>true</c> for nodes to be filtered (hidden).
		/// The position of the node to apply the filter function on is passed as the parameter to the function.
		/// </remarks>
		public Func<TreePosition, bool> FilterFunction {
			set {
				Backend.FilterFunction = value;
			}
		}

		/// <summary>
		/// Converts the node position from the child store to its position inside the filter store.
		/// </summary>
		/// <returns>The position of the node inside the filtered store, or null if the row has been filtered.</returns>
		/// <param name="pos">The child store position to convert</param>
		public TreePosition ConvertChildPositionToPosition (TreePosition pos)
		{
			return Backend.ConvertChildPositionToPosition (pos);
		}

		/// <summary>
		/// Converts the node position from the filter store to its position inside the child store.
		/// </summary>
		/// <returns>The position of the node inside the child store.</returns>
		/// <param name="pos">The filter store position to convert.</param>
		public TreePosition ConvertPositionToChildPosition (TreePosition pos)
		{
			return Backend.ConvertPositionToChildPosition (pos);
		}

		/// <summary>
		/// Forces the filter store to re-evaluate all nodes in the child store.
		/// </summary>
		public void Refilter()
		{
			Backend.Refilter ();
		}

		public TreeViewNavigator GetFirstNode ()
		{
			var p = Backend.GetChild (null, 0);
			var i = Backend.GetParentChildIndex (null);
			return new TreeViewNavigator (Backend, p, i);
		}

		public TreeViewNavigator GetNavigatorAt (TreePosition pos)
		{
			var i = Backend.GetParentChildIndex (pos);
			return new TreeViewNavigator (Backend, pos, i);
		}

		protected override Type[] GetColumnTypes ()
		{
			return ChildStore.ColumnTypes;
		}
	}

	class DefaultTreeStoreFilterBackend: ITreeStoreFilterBackend
	{
		public TreePosition ConvertChildPositionToPosition (TreePosition pos)
		{
			if (!childToFilterPos.ContainsKey (pos))
				return null;
			return childToFilterPos [pos];
		}

		public TreePosition ConvertPositionToChildPosition (TreePosition pos)
		{
			var tpos = GetPosition (pos);
			if (!filterToChildPos.ContainsKey (tpos))
				return null;
			return filterToChildPos [tpos];
		}

		struct FilterNode {
			public TreePosition ChildStorePosition;
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

		class NodeList: List<FilterNode>
		{
			public NodePosition Parent;
		}



		ITreeDataSource childStore;

		NodeList rootNodes = new NodeList ();
		int version;
		int nextNodeId;

		Dictionary<TreePosition, NodePosition> childToFilterPos = new Dictionary<TreePosition, NodePosition>();
		Dictionary<NodePosition, TreePosition> filterToChildPos = new Dictionary<NodePosition, TreePosition>();

		public event EventHandler<TreeNodeEventArgs> NodeInserted;
		public event EventHandler<TreeNodeChildEventArgs> NodeDeleted;
		public event EventHandler<TreeNodeEventArgs> NodeChanged;
		public event EventHandler<TreeNodeOrderEventArgs> NodesReordered;

		public ITreeDataSource ChildStore {
			get {
				return childStore;
			}
		}

		Func<TreePosition, bool> filterFunction;
		public Func<TreePosition, bool> FilterFunction {
			set {
				filterFunction = value;
				Refilter ();
			}
		}

		public void Initialize (ITreeDataSource store)
		{
			childStore = store;
			childStore.NodeChanged += HandleChildNodeChanged;
			childStore.NodeDeleted += HandleChildNodeDeleted;
			childStore.NodeInserted += HandleChildNodeInserted;
			childStore.NodesReordered += HandleChildNodesReordered;
			Refilter ();
		}

		public void Initialize (Type[] columnTypes)
		{
			var defaultStore = new DefaultTreeStoreBackend ();
			defaultStore.Initialize (columnTypes);
			Initialize (defaultStore);
		}

		void HandleChildNodesReordered (object sender, TreeNodeOrderEventArgs e)
		{
			if (childToFilterPos.ContainsKey (e.Node)) {
				ScanChildNode (childToFilterPos[e.Node], e.Node);
			}
		}

		void HandleChildNodeInserted (object sender, TreeNodeEventArgs e)
		{
			if (filterFunction (e.Node))
				return;
			var parentChild = ChildStore.GetParent (e.Node);
			if (parentChild != null && childToFilterPos.ContainsKey (parentChild))
				parentChild = childToFilterPos [parentChild];
			//if (childToFilterPos.ContainsKey (parentChild)) {

			TreePosition newNode = null;
			var next = ChildStore.GetNext (e.Node);
			if (next != null && childToFilterPos.ContainsKey (next))
				newNode = InsertBefore (childToFilterPos [next]);

			var prev = ChildStore.GetPrevious (e.Node);
			if (prev != null && childToFilterPos.ContainsKey (prev))
				newNode = InsertAfter (childToFilterPos [prev]);

			if (newNode == null)
				newNode = AddChild (parentChild);

			NodePosition newNPos = GetPosition (newNode);
			childToFilterPos [e.Node] = newNPos;
			filterToChildPos [newNPos] = e.Node;
			ScanChildNode (newNode, e.Node);
			//}
		}

		void HandleChildNodeDeleted (object sender, TreeNodeChildEventArgs e)
		{
			Refilter ();
		}

		void HandleChildNodeChanged (object sender, TreeNodeEventArgs e)
		{
			Refilter ();
		}

		public void Refilter ()
		{
			nextNodeId = 0;
			version = 0;
			rootNodes.Clear ();
			childToFilterPos.Clear ();
			filterToChildPos.Clear ();

			ScanChildNode (null, null);
		}

		void ScanChildNode (TreePosition pos, TreePosition childPos)
		{
			for (int i = 0; i < childStore.GetChildrenCount (childPos); i++) {
				var node = childStore.GetChild (childPos, i);
				if (node == null || filterFunction == null || filterFunction (node)) {
					if (childToFilterPos.ContainsKey (node)) {
						if (filterToChildPos.ContainsKey (childToFilterPos [node]))
							filterToChildPos.Remove (childToFilterPos [node]);
						childToFilterPos.Remove (node);
					}
					continue;
				}

				TreePosition newPos = AddChild (pos);
				NodePosition newNPos = GetPosition (newPos);
				childToFilterPos [node] = newNPos;
				filterToChildPos [newNPos] = node;
				ScanChildNode (newPos, node);
			}
		}

		public void SetValue (TreePosition pos, int column, object value)
		{
			var childPos = ConvertPositionToChildPosition (pos);
			if (childPos != null)
				ChildStore.SetValue(childPos, column, value);
			if (NodeChanged != null)
				NodeChanged (this, new TreeNodeEventArgs (pos));
		}

		TreePosition AddChild (TreePosition pos)
		{
			NodePosition np = GetPosition (pos);

			FilterNode nn = new FilterNode ();
			nn.NodeId = nextNodeId++;

			NodeList list;

			if (pos == null) {
				list = rootNodes;
			} else {
				FilterNode n = np.ParentList [np.NodeIndex];
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
				NodeInserted (this, new TreeNodeEventArgs (node));
			return node;
		}


		TreePosition InsertBefore (TreePosition pos)
		{
			NodePosition np = GetPosition (pos);
			FilterNode nn = new FilterNode ();
			nn.NodeId = nextNodeId++;

			np.ParentList.Insert (np.NodeIndex, nn);
			version++;

			// Update the NodePosition since it is now invalid
			np.NodeIndex++;
			np.StoreVersion = version;

			var node = new NodePosition () { ParentList = np.ParentList, NodeId = nn.NodeId, NodeIndex = np.NodeIndex - 1, StoreVersion = version };
			if (NodeInserted != null)
				NodeInserted (this, new TreeNodeEventArgs (node));
			return node;
		}

		TreePosition InsertAfter (TreePosition pos)
		{
			NodePosition np = GetPosition (pos);
			FilterNode nn = new FilterNode ();
			nn.NodeId = nextNodeId++;

			np.ParentList.Insert (np.NodeIndex + 1, nn);
			version++;

			// Update the provided position is still valid
			np.StoreVersion = version;

			var node = new NodePosition () { ParentList = np.ParentList, NodeId = nn.NodeId, NodeIndex = np.NodeIndex + 1, StoreVersion = version };
			if (NodeInserted != null)
				NodeInserted (this, new TreeNodeEventArgs (node));
			return node;
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
				//if (np.NodeIndex == -1)
				//throw new InvalidOperationException ("Invalid node position");
				np.StoreVersion = version;
			}
			return np;
		}

		object ITreeDataSource.GetValue (TreePosition pos, int column)
		{
			var childPos = ConvertPositionToChildPosition (pos);
			if (childPos != null)
				return ChildStore.GetValue (childPos, column);
			return null;
		}

		TreePosition ITreeDataSource.GetChild (TreePosition pos, int index)
		{
			if (pos == null) {
				if (rootNodes.Count == 0)
					return null;
				FilterNode n = rootNodes[index];
				return new NodePosition () { ParentList = rootNodes, NodeId = n.NodeId, NodeIndex = index, StoreVersion = version };
			} else {
				NodePosition np = GetPosition (pos);
				FilterNode n = np.ParentList[np.NodeIndex];
				if (n.Children == null || index >= n.Children.Count)
					return null;
				return new NodePosition () { ParentList = n.Children, NodeId = n.Children[index].NodeId, NodeIndex = index, StoreVersion = version };
			}
		}

		TreePosition ITreeDataSource.GetParent (TreePosition pos)
		{
			NodePosition np = GetPosition (pos);
			if (np.ParentList == rootNodes)
				return null;
			var parent = np.ParentList.Parent;
			return new NodePosition () { ParentList = parent.ParentList, NodeId = parent.NodeId, NodeIndex = parent.NodeIndex, StoreVersion = version };
		}

		TreePosition ITreeDataSource.GetNext (TreePosition pos)
		{
			NodePosition np = GetPosition (pos);
			if (np.NodeIndex >= np.ParentList.Count)
				return null;
			FilterNode n = np.ParentList[np.NodeIndex + 1];
			return new NodePosition () { ParentList = np.ParentList, NodeId = n.NodeId, NodeIndex = np.NodeIndex + 1, StoreVersion = version };
		}

		TreePosition ITreeDataSource.GetPrevious (TreePosition pos)
		{
			NodePosition np = GetPosition (pos);
			if (np.NodeIndex <= 0)
				return null;
			FilterNode n = np.ParentList[np.NodeIndex - 1];
			return new NodePosition () { ParentList = np.ParentList, NodeId = n.NodeId, NodeIndex = np.NodeIndex - 1, StoreVersion = version };
		}

		int ITreeDataSource.GetChildrenCount (TreePosition pos)
		{
			if (pos == null)
				return rootNodes.Count;

			NodePosition np = GetPosition (pos);
			FilterNode n = np.ParentList[np.NodeIndex];
			return n.Children != null ? n.Children.Count : 0;
		}

		int ITreeDataSource.GetParentChildIndex (TreePosition pos)
		{
			NodePosition np = GetPosition (pos);
			return np.NodeIndex;
		}

		void Remove (TreePosition pos)
		{
			NodePosition np = GetPosition (pos);
			if (np == null)
				return;

			if (filterToChildPos.ContainsKey (np)) {
				childToFilterPos.Remove (filterToChildPos[np]);
				filterToChildPos.Remove (np);
			}

			np.ParentList.RemoveAt (np.NodeIndex);
			var parent = np.ParentList.Parent;
			var index = np.NodeIndex;
			version++;
			if (NodeDeleted != null)
				NodeDeleted (this, new TreeNodeChildEventArgs (parent, index));
		}

		public Type[] ColumnTypes {
			get {
				return ChildStore.ColumnTypes;
			}
		}

		public void InitializeBackend (object frontend, ApplicationContext context)
		{
		}
		public void EnableEvent (object eventId)
		{
		}
		public void DisableEvent (object eventId)
		{
		}
	}
}

