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
using System.Collections.Specialized;
using System.Collections;
using System.Linq;

namespace Xwt.GtkBackend
{
	public class CustomListModel: GLib.Object, Gtk.TreeModelImplementor
	{
		object source;
		Gtk.TreeModelAdapter adapter;
		Gtk.Widget parentWidget;
		int count = -1;
		RowNode root;

		static CollectionData emptyCollection = new CollectionData ();
		static object doneMarker = new object ();

		internal class RowNode {
			public object Item;
			public int Row;
			public IntPtr Handle;
			public RowNode Next;
			public RowNode Previous;
			public RowNode Parent;
			public CollectionData Children;

			public void IncRow (int amount)
			{
				var r = this;
				while (r != null) {
					r.Row += amount;
					r = r.Next;
				}
			}
		}

		internal class CollectionData {
			public object Enumerator;
			public IEnumerable Collection;
			public int Count;
			public RowNode FirstChild;
			public RowNode LastChild;
			public RowNode Parent;
		}

		public CustomListModel (IntPtr p): base (p)
		{
		}
		
		public CustomListModel (object source, Gtk.Widget w)
		{
			root = new RowNode ();
			parentWidget = w;
			this.source = source;
			adapter = new Gtk.TreeModelAdapter (this);
		}

		void HandleCollectionChanged (CollectionData data, NotifyCollectionChangedEventArgs e)
		{
			bool reset = true;
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add:
				if (e.NewStartingIndex != -1) {
					reset = false;
					var index = e.NewStartingIndex;
					int count = e.NewItems.Count;
					var node = GetNodeAtRow (data.Parent, index);
					var last = node.Previous;
					foreach (var it in e.NewItems) {
						var newNode = CreateNode (index, it);
						if (last != null)
							last.Next = newNode;
						newNode.Previous = last;
						newNode.Parent = node.Parent;
						last = newNode;
					}
					node.Previous = last;
					if (last != null)
						last.Next = node;
					node.IncRow (e.NewItems.Count);
				}
				break;
			case NotifyCollectionChangedAction.Remove:
				if (e.OldStartingIndex != -1) {
					reset = false;
				}
				for (int n = e.OldStartingIndex; n < e.OldStartingIndex + e.OldItems.Count; n++) {
					var p = new Gtk.TreePath (new int[] { n });
					adapter.EmitRowDeleted (p);
				}
				break;
			case NotifyCollectionChangedAction.Move:
				{
					var p = new Gtk.TreePath ("");
					var it = Gtk.TreeIter.Zero;
					var count = IterNChildren (it);
					int[] newOrder = new int[count];
					for (int n = 0; n < count; n++)
						newOrder [n] = n;
					for (int n = 0; n < e.NewItems.Count; n++) {
						newOrder [e.OldStartingIndex + n] = e.NewStartingIndex + n;
						newOrder [e.NewStartingIndex + n] = e.OldStartingIndex + n;
					}
					adapter.EmitRowsReordered (p, it, newOrder);
					break;
				}
			case NotifyCollectionChangedAction.Replace:
				{
					var nod = GetNodeAtRow (data.Parent, e.NewStartingIndex);
					var index = e.NewStartingIndex;
					var count = e.NewItems.Count;
					while (count-- > 0 && nod != null) {
						var p = new Gtk.TreePath (new int[] { index });
						var it = IterFromNode (nod);
						adapter.EmitRowChanged (p, it);
						nod = nod.Next;
					}
					break;
				}
			}
			parentWidget.QueueResize ();
		}

		public Gtk.TreeModelAdapter Store {
			get { return adapter; }
		}

		internal virtual IEnumerable GetChildrenCollection (RowNode node)
		{
			if (node == root)
				return source as IEnumerable;
			else
				return null;
		}

		void InitChildrenData (RowNode node)
		{
			if (node.Children != null)
				return;
			var col = GetChildrenCollection (node);
			if (col == null) {
				node.Children = emptyCollection;
				return;
			}
			node.Children = new CollectionData {
				Collection = col
			};

			INotifyCollectionChanged ccol = source as INotifyCollectionChanged;
			if (ccol != null) {
				ccol.CollectionChanged += delegate(object sender, NotifyCollectionChangedEventArgs e) {
					HandleCollectionChanged (node.Children, e);
				};
			}
		}

		int GetChildCount (RowNode parent)
		{
			InitChildrenData (parent);

			if (parent.Children == emptyCollection)
				return 0;

			ICollection col = parent.Children.Collection as ICollection;
			if (col != null)
				return col.Count;

			var c = GetFirstChild (parent);
			if (c == null)
				return 0;
			c = parent.Children.LastChild;
			while (c != null)
				c = GetNext (c);

			return parent.Children.LastChild.Row + 1;
		}

		RowNode GetFirstChild (RowNode parent)
		{
			InitChildrenData (parent);

			if (parent.Children == emptyCollection)
				return null;

			if (parent.Children.FirstChild != null)
				return parent.Children.FirstChild;

			var col = parent.Children.Collection;

			IList list = col as IList;
			if (list != null) {
				if (list.Count > 0) {
					parent.Children.FirstChild = parent.Children.LastChild = CreateNode (0, list [0]);
					parent.Children.Enumerator = 0;
					return parent.Children.FirstChild;
				} else {
					parent.Children = emptyCollection;
					return null;
				}
			} else if (col is ICollection) {
				if (((ICollection)col).Count == 0) {
					parent.Children = emptyCollection;
					return null;
				}
			}
			var en = col.GetEnumerator ();
			parent.Children.Enumerator = en;
			if (en.MoveNext ())
				return parent.Children.FirstChild = parent.Children.LastChild = CreateNode (0, en.Current);
			else {
				parent.Children = emptyCollection;
				return null;
			}
		}

		RowNode CreateNode (int row, object item)
		{
			var node = new RowNode {
				Item = item,
				Row = row
			};
			GCHandle gch = GCHandle.Alloc (node);
			node.Handle = (IntPtr)gch;
			return node;
		}

		RowNode GetNext (RowNode node)
		{
			if (node.Next == null) {
				var parent = node.Parent;
				if (parent.Children.Enumerator == doneMarker)
					return null;
				int nextRow = node.Row + 1;
				object nextItem;
				if (parent.Children.Collection is IList) {
					var list = (IList)parent.Children.Collection;
					if (nextRow >= list.Count) {
						parent.Children.Enumerator = doneMarker;
						return null;
					}
					nextItem = list [nextRow];
				}
				else if (((IEnumerator)parent.Children.Enumerator).MoveNext ()) {
					nextItem = ((IEnumerator)parent.Children.Enumerator).Current;
				} else {
					parent.Children.Enumerator = doneMarker;
					return null;
				}
				var next = CreateNode (nextRow, nextItem);
				node.Next = next;
				next.Previous = node;
				next.Parent = node.Parent;
				parent.Children.LastChild = next;
			}
			return node.Next;
		}
		
		RowNode GetNodeAtRow (RowNode parent, int row)
		{
			var node = GetFirstChild (parent);
			if (node == null)
				return null;

			for (int n=0; n<row; n++) {
				node = GetNext (node);
				if (node == null)
					return null;
			}
			return node;
		}

		Gtk.TreeIter IterFromNode (RowNode node)
		{
			Gtk.TreeIter result = Gtk.TreeIter.Zero;
			result.UserData = node.Handle;
			return result;
		}

		RowNode NodeFromIter (Gtk.TreeIter iter)
		{
			GCHandle gch = (GCHandle)iter.UserData;
			return (RowNode)gch.Target;
		}
		
		#region TreeModelImplementor implementation
		public GLib.GType GetColumnType (int index)
		{
			return (GLib.GType)typeof(object);
		}

		public bool GetIter (out Gtk.TreeIter iter, Gtk.TreePath path)
        {
			iter = Gtk.TreeIter.Zero;
			if (path.Indices.Length == 0)
				return false;

			var indices = path.Indices;
			var node = root;
			for (int n=0; n<indices.Length && node != null; n++) {
				node = GetNodeAtRow (node, indices [n]);
			}
			if (node == null)
				return false;

			iter = IterFromNode (node);
			return true;
		}

		public Gtk.TreePath GetPath (Gtk.TreeIter iter)
		{
			var node = NodeFromIter (iter);
			List<int> path = new List<int> ();
			while (node != null) {
				path.Insert (0, node.Row);
				node = node.Parent;
			}
			return new Gtk.TreePath (path.ToArray ());
		}
		
		public void GetValue (Gtk.TreeIter iter, int column, ref GLib.Value value)
		{
			var row = NodeFromIter (iter);
			value = row.Item != null ? new GLib.Value (row.Item) : GLib.Value.Empty;
		}

		public bool IterNext (ref Gtk.TreeIter iter)
		{
			var node = NodeFromIter (iter);
			if (node == null) {
				iter = Gtk.TreeIter.Zero;
				return false;
			}
			var next = GetNext (node);
			if (next == null) {
				iter = Gtk.TreeIter.Zero;
				return false;
			}
			iter = IterFromNode (node);
			return true;
		}

		public bool IterChildren (out Gtk.TreeIter iter, Gtk.TreeIter parent)
        {
			var node = !parent.Equals (Gtk.TreeIter.Zero) ? NodeFromIter (parent) : root;
			if (node == null) {
				iter = Gtk.TreeIter.Zero;
				return false;
			}
			node = GetFirstChild (node);
			if (node == null) {
				iter = Gtk.TreeIter.Zero;
				return false;
			}
			iter = IterFromNode (node);
			return true;
		}

		public bool IterHasChild (Gtk.TreeIter parent)
		{
			var node = NodeFromIter (parent);
			return node != null && GetFirstChild (node) != null;
		}

		public int IterNChildren (Gtk.TreeIter iter)
		{
			var node = NodeFromIter (iter);
			return GetChildCount (node);
		}

		public bool IterNthChild (out Gtk.TreeIter iter, Gtk.TreeIter parent, int n)
		{
			RowNode parentNode;
			if (parent.Equals (Gtk.TreeIter.Zero))
				parentNode = root;
			else
				parentNode = NodeFromIter (parent);

			var node = GetNodeAtRow (parentNode, n);
			if (node != null) {
				iter = IterFromNode (node);
				return true;
			} else {
				iter = Gtk.TreeIter.Zero;
				return false;
			}
		}

		public bool IterParent (out Gtk.TreeIter iter, Gtk.TreeIter child)
		{
			var node = NodeFromIter (child);
			if (node == root) {
				iter = Gtk.TreeIter.Zero;
				return false;
			}
			iter = IterFromNode (node.Parent);
			return true;
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
				return 1;
			}
		}
		#endregion
	}
}

