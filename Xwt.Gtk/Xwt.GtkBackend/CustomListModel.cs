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
	public class CustomListModel: GLib.Object, TreeModelImplementor
	{
		IListDataSource source;
		Dictionary<int,int> nodeHash = new Dictionary<int,int> ();
		Dictionary<int,int> handleHash = new Dictionary<int,int> ();
		Type[] colTypes;
		int counter = 1;
		Gtk.TreeModelAdapter adapter;
		Gtk.Widget parent;
		
		public CustomListModel (IntPtr p): base (p)
		{
		}
		
		public CustomListModel (IListDataSource source, Gtk.Widget w)
		{
			parent = w;
			this.source = source;
			adapter = new Gtk.TreeModelAdapter (this);
			colTypes = source.ColumnTypes;
			source.RowChanged += HandleRowChanged;
			source.RowDeleted += HandleRowDeleted;
			source.RowInserted += HandleRowInserted;
			source.RowsReordered += HandleRowsReordered;
		}

		void HandleRowsReordered (object sender, ListRowOrderEventArgs e)
		{
			var p = new Gtk.TreePath (new int[] { e.Row });
			var it = IterFromNode (e.Row);
			adapter.EmitRowsReordered (p, it, e.ChildrenOrder);
			parent.QueueResize ();
		}

		void HandleRowInserted (object sender, ListRowEventArgs e)
		{
			var p = new Gtk.TreePath (new int[] { e.Row });
			var it = IterFromNode (e.Row);
			adapter.EmitRowInserted (p, it);
			parent.QueueResize ();
		}

		void HandleRowDeleted (object sender, ListRowEventArgs e)
		{
			var p = new Gtk.TreePath (new int[] { e.Row });
			adapter.EmitRowDeleted (p);
			parent.QueueResize ();
		}

		void HandleRowChanged (object sender, ListRowEventArgs e)
		{
			var p = new Gtk.TreePath (new int[] { e.Row });
			var it = IterFromNode (e.Row);
			adapter.EmitRowChanged (p, it);
			parent.QueueResize ();
		}
		
		public Gtk.TreeModelAdapter Store {
			get { return adapter; }
		}
		
		Gtk.TreeIter IterFromNode (int node)
		{
			int gch;
			if (!handleHash.TryGetValue (node, out gch)) {
				gch = counter++;
				handleHash [node] = gch;
				nodeHash [gch] = node;
			}
			Gtk.TreeIter result = Gtk.TreeIter.Zero;
			result.UserData = (IntPtr)gch;
			return result;
		}
		
		int NodeFromIter (Gtk.TreeIter iter)
		{
			int node;
			int gch = (int)iter.UserData;
			nodeHash.TryGetValue (gch, out node);
			return node;
		}
		
		#region TreeModelImplementor implementation
		public GLib.GType GetColumnType (int index)
		{
			return (GLib.GType)colTypes [index];
		}

		public bool GetIter (out Gtk.TreeIter iter, Gtk.TreePath path)
        {
            iter = Gtk.TreeIter.Zero;
			if (path.Indices.Length == 0)
				return false;
			int row = path.Indices [0];
			if (row >= source.RowCount) {
				return false;
			}
			iter = IterFromNode (row);
			return true;
		}

		public Gtk.TreePath GetPath (Gtk.TreeIter iter)
		{
			int row = NodeFromIter (iter);
			return new Gtk.TreePath (new int[] { row });
		}
		
		public void GetValue (Gtk.TreeIter iter, int column, ref GLib.Value value)
		{
			int row = NodeFromIter (iter);
			var v = source.GetValue (row, column);
			value = v != null ? new GLib.Value (v) : GLib.Value.Empty;
		}

		public bool IterNext (ref Gtk.TreeIter iter)
		{
			int row = NodeFromIter (iter);
			if (++row < source.RowCount) {
				iter = IterFromNode (row);
				return true;
			} else
				return false;
		}

		#if XWT_GTK3
		public bool IterPrevious (ref Gtk.TreeIter iter)
		{
			int row = NodeFromIter (iter);
			if (--row >= 0) {
				iter = IterFromNode (row);
				return true;
			} else
				return false;
		}
		#endif

		public bool IterChildren (out Gtk.TreeIter iter, Gtk.TreeIter parent)
        {
            iter = Gtk.TreeIter.Zero;
			return false;
		}

		public bool IterHasChild (Gtk.TreeIter iter)
		{
			return false;
		}

		public int IterNChildren (Gtk.TreeIter iter)
		{
			if (iter.Equals (Gtk.TreeIter.Zero))
				return source.RowCount;
			else
				return 0;
		}

		public bool IterNthChild (out Gtk.TreeIter iter, Gtk.TreeIter parent, int n)
		{
			if (parent.Equals (Gtk.TreeIter.Zero)) {
				iter = IterFromNode (n);
				return true;
			} else {
				iter = Gtk.TreeIter.Zero;
				return false;
			}
		}

		public bool IterParent (out Gtk.TreeIter iter, Gtk.TreeIter child)
		{
			iter = Gtk.TreeIter.Zero;
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
				return Gtk.TreeModelFlags.ItersPersist | Gtk.TreeModelFlags.ListOnly;
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

