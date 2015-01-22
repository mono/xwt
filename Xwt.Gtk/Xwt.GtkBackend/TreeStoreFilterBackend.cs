//
// TreeStoreFilterBackend.cs
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
using Gtk;
using Xwt.Backends;


#if XWT_GTK3
using TreeDragDest = Gtk.ITreeDragDest;
using TreeDragDestImplementor = Gtk.ITreeDragDestImplementor;
using TreeModel = Gtk.ITreeModel;
#endif

namespace Xwt.GtkBackend
{
	public class TreeStoreFilterBackend: TreeStoreBackendBase, ITreeStoreFilterBackend
	{
		public CustomTreeModelFilter FilterTree {
			get { return (CustomTreeModelFilter) Store; }
		}

		public TreeModel ChildModel {
			get;
			private set;
		}

		public void Initialize (ITreeDataSource store)
		{
			var xwtComponent = store as XwtComponent;
			if (xwtComponent != null) {
				var backend = xwtComponent.GetBackend () as TreeStoreBackend;
				if (backend != null)
					ChildModel = backend.Store;
			}
			if (ChildModel == null)
				ChildModel = new CustomTreeModel (store).Store;

			base.Initialize (store.ColumnTypes);
		}

		public override TreeModel InitializeModel (Type[] columnTypes)
		{
			if (ChildModel == null) {
				var def = new TreeStoreBackend ();
				def.Initialize (columnTypes);
				ChildModel = def.Store;
			}

			var filterTree = new CustomTreeModelFilter(ChildModel, null);

			filterTree.VisibleFunc = VisibleFunc;

			ChildModel.RowDeleted += (o, args) => InvalidateTree ();
			ChildModel.RowInserted += (o, args) => InvalidateTree ();
			ChildModel.RowsReordered += (o, args) => InvalidateTree ();

			return filterTree;
		}

		Func<TreePosition, bool> filterFunct;
		public Func<TreePosition, bool> FilterFunction {
			set {
				filterFunct = value;
			}
		}

		bool VisibleFunc (TreeModel rawModel, TreeIter iter)
		{
			if (filterFunct != null)
				return !filterFunct (new IterPos(-1, iter));
			return true;
		}

		public void Refilter ()
		{
			FilterTree.Refilter ();
		}

		public TreePosition ConvertChildPositionToPosition (TreePosition pos)
		{
			var iter = FilterTree.ConvertChildIterToIter (((IterPos) pos).Iter);
			if (iter.Equals (Gtk.TreeIter.Zero))
				return null;
			return new IterPos(Version, iter);
		}

		public TreePosition ConvertPositionToChildPosition (TreePosition pos)
		{
			var iter = ((IterPos)pos).Iter;
			if (iter.Equals (Gtk.TreeIter.Zero))
				return null;
			iter = FilterTree.ConvertIterToChildIter (iter);
			if (iter.Equals (Gtk.TreeIter.Zero))
				return null;
			return new IterPos(-1, iter);
		}
	}

	public class CustomTreeModelFilter : Gtk.TreeModelFilter, TreeDragDestImplementor, TreeModel
	{
		public CustomTreeModelFilter (TreeModel childModel, TreePath root) : base (childModel, root)
		{
		}

		void TreeModel.SetValue (TreeIter iter, int column, object value)
		{
			var it = ConvertIterToChildIter (iter);
			ChildModel.SetValue (it, column, value);
		}

		void TreeModel.SetValue (TreeIter iter, int column, double value)
		{
			var it = ConvertIterToChildIter (iter);
			ChildModel.SetValue (it, column, value);
		}

		void TreeModel.SetValue (TreeIter iter, int column, bool value)
		{
			var it = ConvertIterToChildIter (iter);
			ChildModel.SetValue (it, column, value);
		}

		void TreeModel.SetValue (TreeIter iter, int column, string value)
		{
			var it = ConvertIterToChildIter (iter);
			ChildModel.SetValue (it, column, value);
		}

		void TreeModel.SetValue (TreeIter iter, int column, float value)
		{
			var it = ConvertIterToChildIter (iter);
			ChildModel.SetValue (it, column, value);
		}

		void TreeModel.SetValue (TreeIter iter, int column, uint value)
		{
			var it = ConvertIterToChildIter (iter);
			ChildModel.SetValue (it, column, value);
		}

		void TreeModel.SetValue (TreeIter iter, int column, int value)
		{
			var it = ConvertIterToChildIter (iter);
			ChildModel.SetValue (it, column, value);
		}

		public bool DragDataReceived (TreePath dest, SelectionData selection_data)
		{
			var child = Model as TreeDragDest;

			if (child != null)
				return child.DragDataReceived (ConvertPathToChildPath (dest), selection_data);
			return false;
		}

		public bool RowDropPossible (TreePath dest_path, SelectionData selection_data)
		{
			var child = ChildModel as TreeDragDest;

			if (child != null)
				return child.DragDataReceived (ConvertPathToChildPath (dest_path), selection_data);
			return false;
		}
	}
}

