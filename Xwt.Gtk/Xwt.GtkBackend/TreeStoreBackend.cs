// 
// TreeStoreBackend.cs
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
using Xwt.Drawing;
using Gtk;
#if XWT_GTK3
using TreeModel = Gtk.ITreeModel;
#endif


namespace Xwt.GtkBackend
{
	class IterPos: TreePosition
	{
		public IterPos ()
		{
		}
		
		public IterPos (int treeVersion, Gtk.TreeIter iter)
		{
			this.Iter = iter;
			this.Version = treeVersion;
		}
		
		public Gtk.TreeIter Iter;
		public Gtk.TreeIter LastChildIter;
		public int LastChildIndex = -1;
		public int ChildrenCount = -1;
		public int Version;
	}

	public abstract class TreeStoreBackendBase: TableStoreBackend, ITreeDataSource
	{
		int version;

		protected int Version {
			get {
				return version;
			}
		}

		public event EventHandler<TreeNodeEventArgs> NodeInserted;
		public event EventHandler<TreeNodeChildEventArgs> NodeDeleted;
		public event EventHandler<TreeNodeEventArgs> NodeChanged;
		public event EventHandler<TreeNodeOrderEventArgs> NodesReordered;

		protected virtual void InvalidateTree ()
		{
			version++;
		}

		internal IterPos GetIterPos (TreePosition pos)
		{
			IterPos tpos = (IterPos) pos;
			if (tpos != null && tpos.Version != version) {
				tpos.LastChildIndex = -1;
				tpos.ChildrenCount = -1;
			}
			return tpos;
		}

		public TreePosition GetChild (TreePosition pos, int index)
		{
			IterPos tpos = GetIterPos (pos);
			if (tpos != null && tpos.LastChildIndex == index)
				return new IterPos (version, tpos.LastChildIter);
			if (index == 0) {
				if (tpos != null) {
					Gtk.TreeIter it;
					if (Store.IterChildren (out it, tpos.Iter)) {
						tpos.LastChildIter = it;
						tpos.LastChildIndex = 0;
						return new IterPos (version, it);
					}
				} else {
					Gtk.TreeIter it;
					if (Store.GetIterFirst (out it))
						return new IterPos (version, it);
				}
				return null;
			}
			
			if (tpos == null) {
				Gtk.TreeIter it;
				if (Store.IterNthChild (out it, index))
					return new IterPos (version, it);
				else
					return null;
			}
			
			if (tpos.LastChildIndex == -1 || tpos.LastChildIndex > index) {
				Gtk.TreeIter it;
				if (Store.IterNthChild (out it, tpos.Iter, index)) {
					tpos.LastChildIter = it;
					tpos.LastChildIndex = index;
					return new IterPos (version, it);
				} else
					return null;
			}
			
			// tpos.LastChildIndex < index
			
			Gtk.TreeIter iter = tpos.LastChildIter;
			for (int n = tpos.LastChildIndex; n < index; n++) {
				if (!Store.IterNext (ref iter))
					return null;
			}
			tpos.LastChildIter = iter;
			tpos.LastChildIndex = index;
			return new IterPos (version, iter);
		}
		
		public int GetChildrenCount (TreePosition pos)
		{
			if (pos == null)
				return Store.IterNChildren ();
			
			IterPos tpos = GetIterPos (pos);
			
			if (tpos.ChildrenCount != -1)
				return tpos.ChildrenCount;
			
			return tpos.ChildrenCount = Store.IterNChildren (tpos.Iter);
		}

		public int GetParentChildIndex (TreePosition pos)
		{
			if (pos == null)
				return -1;

			IterPos tpos = GetIterPos (pos);
			var path = Store.GetPath (tpos.Iter);
			if (path != null)
				return path.Indices [path.Depth - 1];
			return -1;
		}

		public void SetValue (TreePosition pos, int column, object value)
		{
			IterPos tpos = GetIterPos (pos);
			SetValue (tpos.Iter, column, value);
			if (NodeChanged != null)
				NodeChanged (this, new TreeNodeEventArgs (pos));
		}

		public object GetValue (TreePosition pos, int column)
		{
			IterPos tpos = GetIterPos (pos);
			return GetValue (tpos.Iter, column);
		}

		public TreePosition GetNext (TreePosition pos)
		{
			IterPos tpos = GetIterPos (pos);
			Gtk.TreeIter it = tpos.Iter;
			if (!Store.IterNext (ref it))
				return null;
			return new IterPos (version, it);
		}

		public TreePosition GetPrevious (TreePosition pos)
		{
			IterPos tpos = GetIterPos (pos);
			Gtk.TreePath path = Store.GetPath (tpos.Iter);
			int [] indices = path.Indices;
			if (indices.Length < 1 || indices [indices.Length - 1] == 0)
				return null;
			indices [indices.Length - 1] --;
			Gtk.TreePath previousPath = new Gtk.TreePath (indices);
			Gtk.TreeIter previous;
			if (!Store.GetIter (out previous, previousPath))
				return null;
			return new IterPos (version, previous);
		}

		public TreePosition GetParent (TreePosition pos)
		{
			IterPos tpos = GetIterPos (pos);
			Gtk.TreeIter it;
			if (!Store.IterParent (out it, tpos.Iter))
				return null;
			return new IterPos (version, it);
		}

		protected void RaiseNodeInserted (TreePosition node)
		{
			if (NodeInserted != null)
				NodeInserted (this, new TreeNodeEventArgs (node));
		}

		protected void RaiseNodeDeleted (TreePosition parentNode, int index)
		{
			if (NodeDeleted != null)
				NodeDeleted (this, new TreeNodeChildEventArgs (parentNode, index));
		}

		protected void RaiseNodeChanged (TreePosition parentNode)
		{
			if (NodeChanged != null)
				NodeChanged (this, new TreeNodeEventArgs (parentNode));
		}

		protected void RaiseNodesReordered (TreePosition parentNode, int[] childrenOrder)
		{
			if (NodesReordered != null)
				NodesReordered (this, new TreeNodeOrderEventArgs (parentNode, childrenOrder));
		}

		public void EnableEvent (object eventId)
		{
		}

		public void DisableEvent (object eventId)
		{
		}
	}
	
	public class TreeStoreBackend: TreeStoreBackendBase, ITreeStoreBackend
	{
		public Gtk.TreeStore Tree {
			get { return (Gtk.TreeStore) Store; }
		}

		public override TreeModel InitializeModel (Type[] columnTypes)
		{
			return new Gtk.TreeStore (columnTypes);
		}
		
		public void Clear ()
		{
			InvalidateTree ();
			Tree.Clear ();
		}

		public TreePosition InsertBefore (TreePosition pos)
		{
			InvalidateTree ();
			IterPos tpos = GetIterPos (pos);
			var p = Tree.InsertNodeBefore (tpos.Iter);

			var node = new IterPos (Version, p);
			RaiseNodeInserted (node);
			return node;
		}

		public TreePosition InsertAfter (TreePosition pos)
		{
			InvalidateTree ();
			IterPos tpos = GetIterPos (pos);
			var p = Tree.InsertNodeAfter (tpos.Iter);

			var node = new IterPos (Version, p);
			RaiseNodeInserted (node);
			return node;
		}

		public TreePosition AddChild (TreePosition pos)
		{
			InvalidateTree ();
			IterPos tpos = GetIterPos (pos);
			Gtk.TreeIter it;
			if (pos == null)
				it = Tree.AppendNode ();
			else
				it = Tree.AppendNode (tpos.Iter);

			var node = new IterPos (Version, it);
			RaiseNodeInserted (node);
			return node;
		}
		
		public void Remove (TreePosition pos)
		{
			InvalidateTree ();
			IterPos tpos = GetIterPos (pos);
			IterPos parent = (IterPos)GetParent (tpos);
			int index = GetParentChildIndex (tpos);

			Gtk.TreeIter it = tpos.Iter;
			Tree.Remove (ref it);
			RaiseNodeDeleted (parent, index);
		}
	}
}

