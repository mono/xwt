// 
// CustomTreeModel.cs
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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Gtk;
#if XWT_GTK3
using TreeModelImplementor = Gtk.ITreeModelImplementor;
#endif

namespace Xwt.GtkBackend
{
	public class CustomTreeModel: GLib.Object, TreeModelImplementor
	{

		ITreeDataSource source;
		Dictionary<TreePosition,NodeData> handleHash = new Dictionary<TreePosition,NodeData> ();
		Type[] colTypes;
		Gtk.TreeModelAdapter adapter;
		int stamp;

		/// <summary>
		/// Stores information about the index of a noda and the GCHandle
		/// used by the native reference
		/// </summary>
		class NodeData
		{
			public GCHandle Handle;
			public int Index;
		}

		public CustomTreeModel (ITreeDataSource source)
		{
			this.source = source;
			adapter = new Gtk.TreeModelAdapter (this);
			colTypes = source.ColumnTypes;
			
			source.NodeChanged += HandleNodeChanged;
			source.NodeDeleted += HandleNodeDeleted;
			source.NodeInserted += HandleNodeInserted;
			source.NodesReordered += HandleNodesReordered;
		}
		
		public Gtk.TreeModelAdapter Store {
			get { return adapter; }
		}

		public Gtk.TreeIter IterFromNode (TreePosition node, int index = -1)
		{
			// The returned iter will have a reference to the TreePosition
			// through a GCHandle. At the same time we store a NodeData
			// object to a hashtable, which contains the GCHandle and
			// the index of the node

			NodeData data;
			if (!handleHash.TryGetValue (node, out data)) {
				// It is the first time the node is referenced.
				// Store it in handleHash.
				handleHash [node] = data = new NodeData {
					Handle = GCHandle.Alloc (node),
					Index = index
				};
			}

			// If the index of the node changed, update it in its NodeData
			if (index != -1 && index != data.Index)
				data.Index = index;

			Gtk.TreeIter result = Gtk.TreeIter.Zero;
			result.UserData = (IntPtr)data.Handle;
			result.Stamp = stamp;
			return result;
		}

		int GetCachedIndex (TreePosition node)
		{
			// Gets the index of a node, or -1 if that information is not available
			NodeData data;
			if (handleHash.TryGetValue (node, out data))
				return data.Index;
			return -1;
		}
		
		void CacheIndex (TreePosition node, int index)
		{
			// Stores the index of a node
			NodeData data;
			if (handleHash.TryGetValue (node, out data))
				data.Index = index;
		}
		
		public bool NodeFromIter (Gtk.TreeIter iter, out TreePosition pos)
		{
			if (iter.UserData == IntPtr.Zero) {
				pos = null;
				return true;
			}
			if (iter.Stamp != stamp) {
				// The iter has been invalidated
				pos = null;
				return false;
			}
			GCHandle gch = GCHandle.FromIntPtr (iter.UserData);
			pos = (TreePosition) gch.Target;
			return true;
		}

		#region TreeModelImplementor implementation

		void HandleNodesReordered (object sender, TreeNodeOrderEventArgs e)
		{
			// Don't increase the stamp because no nodes have been removed, so all iters are still valid
			// Update the cached indexes for all the children that have chanfed
			for (int n = 0; n < e.ChildrenOrder.Length; n++) {
				if (e.ChildrenOrder [n] != n) {
					var child = source.GetChild (e.Node, n);
					if (child != null)
						CacheIndex (child, n);
				}
			} 
			adapter.EmitRowsReordered (GetPath (e.Node), IterFromNode (e.Node), e.ChildrenOrder);
		}

		void HandleNodeInserted (object sender, TreeNodeEventArgs e)
		{
			// Don't increase the stamp because no nodes have been removed, so all iters are still valid
			// Update the cached indexes
			var parent = source.GetParent (e.Node);
			var count = source.GetChildrenCount (parent);
			for (int n = e.ChildIndex + 1; n < count; n++) {
				var child = source.GetChild (parent, n);
				if (child != null)
					CacheIndex (child, n);
			} 
			var iter = IterFromNode (e.Node, e.ChildIndex);
			adapter.EmitRowInserted (GetPath (e.Node), iter);
		}

		void HandleNodeDeleted (object sender, TreeNodeChildEventArgs e)
		{
			if (e.Node != null && !handleHash.ContainsKey (e.Node))
				return;

			NodeData data;
			if (e.Child != null && handleHash.TryGetValue (e.Child, out data)) {
				// Increase the model stamp since the node is gone and there may
				// be iters referencing that node. Increasing the stamp will
				// invalidate all those nodes
				stamp++;
				data.Handle.Free ();
				handleHash.Remove (e.Child);

				// Update the indexes of the node following the deleted one
				var count = source.GetChildrenCount (e.Node);
				for (int n = e.ChildIndex; n < count; n++) {
					var child = source.GetChild (e.Node, n);
					if (child != null)
						CacheIndex (child, n);
				}
			}

			if (e.Node == null) {
				adapter.EmitRowDeleted (new Gtk.TreePath (new int[] { e.ChildIndex }));
			} else {
				var p = GetPath (e.Node);
				int[] idx = new int [p.Indices.Length + 1];
				p.Indices.CopyTo (idx, 0);
				idx [idx.Length - 1] = e.ChildIndex;
				adapter.EmitRowDeleted (new Gtk.TreePath (idx));
			}
		}

		void HandleNodeChanged (object sender, TreeNodeEventArgs e)
		{
			if (handleHash.ContainsKey (e.Node))
				adapter.EmitRowChanged (GetPath (e.Node), IterFromNode (e.Node));
		}
		
		public GLib.GType GetColumnType (int index)
		{
			return (GLib.GType) colTypes [index];
		}

		public bool GetIter (out Gtk.TreeIter iter, Gtk.TreePath path)
        {
            iter = Gtk.TreeIter.Zero;
			TreePosition pos = null;
			int[] indices = path.Indices;
			if (indices.Length == 0)
				return false;
			for (int n=0; n<indices.Length; n++) {
				pos = source.GetChild (pos, indices [n]);
				if (pos == null)
					return false;
			}
			iter = IterFromNode (pos, indices[indices.Length - 1]);
			return true;
		}

		public Gtk.TreePath GetPath (Gtk.TreeIter iter)
		{
			TreePosition pos;
			if (NodeFromIter (iter, out pos))
				return GetPath (pos);
			return Gtk.TreePath.NewFirst ();
		}
		
		public Gtk.TreePath GetPath (TreePosition pos)
		{
			List<int> idx = new List<int> ();
			while (pos != null) {
				var parent = source.GetParent (pos);
				idx.Add (GetIndex (parent, pos));
				pos = parent;
			}
			idx.Reverse ();
			return new Gtk.TreePath (idx.ToArray ());
		}
		
		int GetIndex (TreePosition parent, TreePosition pos)
		{
			var res = GetCachedIndex (pos);
			if (res != -1)
				return res;

			int n = 0;
			do {
				var c = source.GetChild (parent, n);
				if (c == null)
					return -1;
				if (c == pos || c.Equals (pos)) {
					CacheIndex (pos, n);
					return n;
				}
				n++;
			} while (true);
		}

		public void GetValue (Gtk.TreeIter iter, int column, ref GLib.Value value)
		{
			TreePosition pos;
			if (!NodeFromIter (iter, out pos)) {
				value = GLib.Value.Empty;
				return;
			}
			var v = source.GetValue (pos, column);
			if (v == null)
				value = GLib.Value.Empty;
			else
				value = new GLib.Value (v);
		}

		public bool IterNext (ref Gtk.TreeIter iter)
		{
			TreePosition pos;
			if (!NodeFromIter (iter, out pos))
				return false;
			TreePosition parent = source.GetParent (pos);
			int i = GetIndex (parent, pos);
			if (i == -1)
				return false;
			pos = source.GetChild (parent, i + 1);
			if (pos != null) {
				iter = IterFromNode (pos, i + 1);
				return true;
			} else
				return false;
		}

		#if XWT_GTK3
		public bool IterPrevious (ref Gtk.TreeIter iter)
		{
			TreePosition pos;
			if (!NodeFromIter (iter, out pos))
				return false;
			TreePosition parent = source.GetParent (pos);
			int i = GetIndex (parent, pos);
			pos = source.GetChild (parent, i - 1);
			if (pos != null) {
				iter = IterFromNode (pos, i - 1);
				return true;
			} else
				return false;
		}
		#endif

		public bool IterChildren (out Gtk.TreeIter iter, Gtk.TreeIter parent)
        {
			iter = parent;
			TreePosition pos;
			if (!NodeFromIter (parent, out pos))
				return false;
			pos = source.GetChild (pos, 0);
			if (pos != null) {
				iter = IterFromNode (pos, 0);
				return true;
			} else
				return false;
		}

		public bool IterHasChild (Gtk.TreeIter iter)
		{
			TreePosition pos;
			if (!NodeFromIter (iter, out pos))
				return false;
			return source.GetChildrenCount (pos) != 0;
		}

		public int IterNChildren (Gtk.TreeIter iter)
		{
			TreePosition pos;
			if (!NodeFromIter (iter, out pos))
				return 0;
			return source.GetChildrenCount (pos);
		}

		public bool IterNthChild (out Gtk.TreeIter iter, Gtk.TreeIter parent, int n)
        {
			iter = parent;
			TreePosition pos;
			if (!NodeFromIter (parent, out pos))
				return false;
			pos = source.GetChild (pos, n);
			if (pos != null) {
				iter = IterFromNode (pos, n);
				return true;
			} else
				return false;
		}

		public bool IterParent (out Gtk.TreeIter iter, Gtk.TreeIter child)
        {
			iter = child;
			TreePosition pos;
			if (!NodeFromIter (iter, out pos))
				return false;
			if (pos != null)
				pos = source.GetParent (pos);
			if (pos != null) {
				iter = IterFromNode (pos);
				return true;
			} else
				return false;
		}

		public void RefNode (Gtk.TreeIter iter)
		{
		}

		public void UnrefNode (Gtk.TreeIter iter)
		{
		}

		public Gtk.TreeModelFlags Flags {
			get {
				return Gtk.TreeModelFlags.ItersPersist;
			}
		}

		public int NColumns {
			get {
				return colTypes.Length;
			}
		}
		#endregion
	}
}

