//
// ListStoreFilterBackend.cs
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
using Xwt.Drawing;
using Gtk;
#if XWT_GTK3
using TreeModel = Gtk.ITreeModel;
#endif

namespace Xwt.GtkBackend
{
	public class ListStoreFilterBackend: ListStoreBackendBase, IListStoreFilterBackend
	{
		public CustomTreeModelFilter FilterTree {
			get { return (CustomTreeModelFilter) Store; }
		}

		public TreeModel ChildModel {
			get;
			private set;
		}

		public void Initialize (IListDataSource store)
		{
			var xwtComponent = store as XwtComponent;
			if (xwtComponent != null) {
				var backend = xwtComponent.GetBackend () as ListStoreBackendBase;
				if (backend != null)
					ChildModel = backend.Store;
			}
			if (ChildModel == null)
				throw new InvalidOperationException ("No custom list stores supported");

			base.Initialize (store.ColumnTypes);
		}

		public override TreeModel InitializeModel (Type[] columnTypes)
		{
			if (ChildModel == null) {
				var def = new ListStoreBackend ();
				def.Initialize (columnTypes);
				ChildModel = def.Store;
			}

			var filterTree = new CustomTreeModelFilter(ChildModel, null);
			filterTree.VisibleFunc = VisibleFunc;
			return filterTree;
		}

		public void Refilter ()
		{
			FilterTree.Refilter ();
		}

		Func<int, bool> filterFunct;
		public Func<int, bool> FilterFunction {
			set {
				filterFunct = value;
			}
		}

		bool VisibleFunc (TreeModel rawModel, TreeIter iter)
		{
			if (filterFunct != null) {
				var path = ChildModel.GetPath (iter);
				return !filterFunct (path.Indices[0]);
			}
			return true;
		}

		public int ConvertChildPositionToPosition (int row)
		{
			Gtk.TreePath childPath = new Gtk.TreePath (new [] { row });
			var path = FilterTree.ConvertChildPathToPath (childPath);
			if (path == null)
				return -1;
			return path.Indices [0];
		}

		public int ConvertPositionToChildPosition (int row)
		{
			Gtk.TreePath path = new Gtk.TreePath (new [] { row });
			var childPath = FilterTree.ConvertPathToChildPath (path);
			if (path == null)
				return -1;
			return path.Indices [0];
		}
	}
}

