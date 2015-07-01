// 
// ListViewBackend.cs
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

namespace Xwt.GtkBackend
{
	public class ListViewBackend: TableViewBackend, IListViewBackend
	{
		bool showBorder;
		
		protected new IListViewEventSink EventSink {
			get { return (IListViewEventSink)base.EventSink; }
		}
		
		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is ListViewEvent) {
				if (((ListViewEvent)eventId) == ListViewEvent.RowActivated)
					Widget.RowActivated += HandleRowActivated;
			}
		}
		
		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is ListViewEvent) {
				if (((ListViewEvent)eventId) == ListViewEvent.RowActivated)
					Widget.RowActivated -= HandleRowActivated;
			}
		}
		
		void HandleRowActivated (object o, Gtk.RowActivatedArgs args)
		{
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnRowActivated (args.Path.Indices[0]);
			});
		}

		public void SetSource (IListDataSource source, IBackend sourceBackend)
		{
			ListStoreBackend b = sourceBackend as ListStoreBackend;
			if (b == null) {
				CustomListModel model = new CustomListModel (source, Widget);
				Widget.Model = model.Store;
			} else
				Widget.Model = b.Store;
		}

		public void SelectRow (int row)
		{
			Gtk.TreeIter it;
			if (!Widget.Model.IterNthChild (out it, row))
				return;
			Widget.Selection.SelectIter (it);
		}

		public void UnselectRow (int row)
		{
			Gtk.TreeIter it;
			if (!Widget.Model.IterNthChild (out it, row))
				return;
			Widget.Selection.UnselectIter (it);
		}

		public void ScrollToRow (int row)
		{
			Gtk.TreeIter it;
			if (!Widget.Model.IterNthChild (out it, row))
				return;
			ScrollToRow (it);
		}

		public int[] SelectedRows {
			get {
				var sel = Widget.Selection.GetSelectedRows ();
				int[] res = new int [sel.Length];
				for (int n=0; n<sel.Length; n++)
					res [n] = sel [n].Indices[0];
				return res;
			}
		}

		public int FocusedRow {
			get {
				Gtk.TreePath path;
				Gtk.TreeViewColumn column;
				Widget.GetCursor (out path, out column);
				if (path == null)
					return -1;
				return path.Indices [0];
			}
			set {
				Gtk.TreePath path = new Gtk.TreePath (new [] { value >= 0 ? value : int.MaxValue });
				Widget.SetCursor (path, null, false);
			}
		}

		public int CurrentEventRow {
			get;
			internal set;
		}

		public bool BorderVisible {
			get {
				return ScrolledWindow.ShadowType == Gtk.ShadowType.In;
			}
			set {
				showBorder = value;
				UpdateBorder ();
			}
		}
		
		void UpdateBorder ()
		{
			var shadowType = showBorder ? Gtk.ShadowType.In : Gtk.ShadowType.None;
			if (ScrolledWindow.Child is Gtk.Viewport)
				((Gtk.Viewport) ScrolledWindow.Child).ShadowType = shadowType;
			else
				ScrolledWindow.ShadowType = shadowType;
		}
		
		public bool HeadersVisible {
			get {
				return Widget.HeadersVisible;
			}
			set {
				Widget.HeadersVisible = value;
			}
		}

		public int GetRowAtPosition (Point p)
		{
			Gtk.TreePath path = GetPathAtPosition (p);
			if (path != null)
				return path.Indices [0];
			return -1;
		}

		public Rectangle GetCellBounds (int row, CellView cell, bool includeMargin)
		{
			var col = GetCellColumn (cell);
			var cr = GetCellRenderer (cell);
			Gtk.TreePath path = new Gtk.TreePath (new [] { row });

			Gtk.TreeIter iter;
			if (!Widget.Model.GetIterFromString (out iter, path.ToString ()))
				return Rectangle.Zero;

			if (includeMargin)
				return ((ICellRendererTarget)this).GetCellBackgroundBounds (col, cr, iter);
			else
				return ((ICellRendererTarget)this).GetCellBounds (col, cr, iter);
		}

		public Rectangle GetRowBounds (int row, bool includeMargin)
		{
			Gtk.TreePath path = new Gtk.TreePath (new [] { row });
			Gtk.TreeIter iter;
			if (!Widget.Model.GetIterFromString (out iter, path.ToString ()))
				return Rectangle.Zero;

			if (includeMargin)
				return GetRowBackgroundBounds (iter);
			else
				return GetRowBounds (iter);
		}

		public override void SetCurrentEventRow (string path)
		{
			if (path.Contains (":")) {
				path = path.Split (':') [0];
			}
			CurrentEventRow = int.Parse (path);
		}
	}
}

