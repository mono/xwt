// 
// TableViewBackend.cs
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
	public class TableViewBackend<T,S>: WidgetBackend<T,S> where T:Gtk.TreeView where S:ITableViewEventSink
	{
		public TableViewBackend ()
		{
			Widget = (T) new Gtk.TreeView ();
			Widget.Show ();
		}
		
		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is TableViewEvent) {
				if (((TableViewEvent)eventId) == TableViewEvent.SelectionChanged)
					Widget.Selection.Changed += HandleWidgetSelectionChanged;
			}
		}
		
		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is TableViewEvent) {
				if (((TableViewEvent)eventId) == TableViewEvent.SelectionChanged)
					Widget.Selection.Changed -= HandleWidgetSelectionChanged;
			}
		}

		void HandleWidgetSelectionChanged (object sender, EventArgs e)
		{
			EventSink.OnSelectionChanged ();
		}
		
		public object AddColumn (ListViewColumn col)
		{
			Gtk.TreeViewColumn tc = new Gtk.TreeViewColumn ();
			tc.Title = col.Title;
			Widget.AppendColumn (tc);
			MapTitle (col, tc);
			MapColumn (col, tc);
			return tc;
		}
		
		void MapTitle (ListViewColumn col, Gtk.TreeViewColumn tc)
		{
			if (col.HeaderView == null)
				tc.Title = col.Title;
			else
				tc.Widget = CellUtil.CreateCellRenderer (col.HeaderView);
		}
		
		void MapColumn (ListViewColumn col, Gtk.TreeViewColumn tc)
		{
			foreach (var v in col.Views) {
				CellUtil.CreateCellRenderer (tc, v);
			}
		}
		
		public void RemoveColumn (ListViewColumn col, object handle)
		{
			Widget.RemoveColumn ((Gtk.TreeViewColumn)handle);
		}

		public void UpdateColumn (ListViewColumn col, object handle, ListViewColumnChange change)
		{
			Gtk.TreeViewColumn tc = (Gtk.TreeViewColumn) handle;
			if (change == ListViewColumnChange.Cells) {
				tc.Clear ();
				MapColumn (col, tc);
			}
			else if (change == ListViewColumnChange.Title)
				MapTitle (col, tc);
		}

		public void SelectAll ()
		{
			Widget.Selection.SelectAll ();
		}
		
		public void UnselectAll ()
		{
			Widget.Selection.UnselectAll ();
		}

		public void SetSelectionMode (SelectionMode mode)
		{
			switch (mode) {
			case SelectionMode.Single: Widget.Selection.Mode = Gtk.SelectionMode.Single; break;
			case SelectionMode.Multiple: Widget.Selection.Mode = Gtk.SelectionMode.Multiple; break;
			}
		}
	}
}

