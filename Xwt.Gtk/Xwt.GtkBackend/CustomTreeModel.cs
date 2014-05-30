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
	public class CustomTreeModel: TreeModelImplementor
	{

		ITreeDataSource source;
        Dictionary<GCHandle,TreePosition> nodeHash = new Dictionary<GCHandle,TreePosition> ();
        Dictionary<TreePosition,GCHandle> handleHash = new Dictionary<TreePosition,GCHandle> ();
		Type[] colTypes;
		Gtk.TreeModelAdapter adapter;
		
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
		
		public IntPtr Handle {
			get {
				return IntPtr.Zero;
			}
		}
		
        Gtk.TreeIter IterFromNode (TreePosition node)
		{
			GCHandle gch;
			if (!handleHash.TryGetValue (node, out gch)) {
				gch = GCHandle.Alloc (node);
				handleHash [node] = gch;
				nodeHash [gch] = node;
			}
			Gtk.TreeIter result = Gtk.TreeIter.Zero;
			result.UserData = (IntPtr)gch;
			return result;
		}
		
		TreePosition NodeFromIter (Gtk.TreeIter iter)
		{
			TreePosition node;
			GCHandle gch = (GCHandle)iter.UserData;
			nodeHash.TryGetValue (gch, out node);
			return node;
		}
		
		#region TreeModelImplementor implementation

		void HandleNodesReordered (object sender, TreeNodeOrderEventArgs e)
		{
			Gtk.TreeIter it = IterFromNode (e.Node);
			adapter.EmitRowsReordered (GetPath (it), it, e.ChildrenOrder);
		}

		void HandleNodeInserted (object sender, TreeNodeEventArgs e)
		{
			Gtk.TreeIter it = IterFromNode (e.Node);
			adapter.EmitRowInserted (GetPath (it), it);
		}

		void HandleNodeDeleted (object sender, TreeNodeChildEventArgs e)
		{
			if (e.Node == null) {
				adapter.EmitRowDeleted (new Gtk.TreePath (new int[] { e.ChildIndex }));
			} else {
				Gtk.TreeIter it = IterFromNode (e.Node);
				var p = GetPath (it);
				int[] idx = new int [p.Indices.Length + 1];
				p.Indices.CopyTo (idx, 0);
				idx [idx.Length - 1] = e.ChildIndex;
				adapter.EmitRowDeleted (new Gtk.TreePath (idx));
			}
		}

		void HandleNodeChanged (object sender, TreeNodeEventArgs e)
		{
			Gtk.TreeIter it = IterFromNode (e.Node);
			adapter.EmitRowChanged (GetPath (it), it);
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
			iter = IterFromNode (pos);
			return true;
		}

		public Gtk.TreePath GetPath (Gtk.TreeIter iter)
		{
			TreePosition pos = NodeFromIter (iter);
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
			int n = 0;
			do {
				var c = source.GetChild (parent, n);
				if (c == null)
					return -1;
				if (c == pos || c.Equals(pos))
					return n;
				n++;
			} while (true);
		}

		public void GetValue (Gtk.TreeIter iter, int column, ref GLib.Value value)
		{
			TreePosition pos = NodeFromIter (iter);
			var v = source.GetValue (pos, column);
			value = new GLib.Value (v);
		}

		public bool IterNext (ref Gtk.TreeIter iter)
		{
			TreePosition pos = NodeFromIter (iter);
			TreePosition parent = source.GetParent (pos);
			int i = GetIndex (parent, pos);
			if (source.GetChildrenCount (parent) >= i)
				return false;
			pos = source.GetChild (parent, i + 1);
			if (pos != null) {
				iter = IterFromNode (pos);
				return true;
			} else
				return false;
		}

		#if XWT_GTK3
		public bool IterPrevious (ref Gtk.TreeIter iter)
		{
			TreePosition pos = NodeFromIter (iter);
			TreePosition parent = source.GetParent (pos);
			int i = GetIndex (parent, pos);
			pos = source.GetChild (parent, i - 1);
			if (pos != null) {
				iter = IterFromNode (pos);
				return true;
			} else
				return false;
		}
		#endif

		public bool IterChildren (out Gtk.TreeIter iter, Gtk.TreeIter parent)
        {
            iter = Gtk.TreeIter.Zero;
			TreePosition pos = NodeFromIter (parent);
			pos = source.GetChild (pos, 0);
			if (pos != null) {
				iter = IterFromNode (pos);
				return true;
			} else
				return false;
		}

		public bool IterHasChild (Gtk.TreeIter iter)
		{
			TreePosition pos = NodeFromIter (iter);
			return source.GetChildrenCount (pos) != 0;
		}

		public int IterNChildren (Gtk.TreeIter iter)
		{
			TreePosition pos = NodeFromIter (iter);
			return source.GetChildrenCount (pos);
		}

		public bool IterNthChild (out Gtk.TreeIter iter, Gtk.TreeIter parent, int n)
        {
            iter = Gtk.TreeIter.Zero;
			TreePosition pos = NodeFromIter (parent);
			pos = source.GetChild (pos, n);
			if (pos != null) {
				iter = IterFromNode (pos);
				return true;
			} else
				return false;
		}

		public bool IterParent (out Gtk.TreeIter iter, Gtk.TreeIter child)
        {
            iter = Gtk.TreeIter.Zero;
			TreePosition pos = NodeFromIter (iter);
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

