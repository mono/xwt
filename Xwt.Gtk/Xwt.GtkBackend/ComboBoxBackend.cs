// 
// ComboBoxBackend.cs
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
	public class ComboBoxBackend: WidgetBackend, IComboBoxBackend, ICellRendererTarget
	{
		public ComboBoxBackend ()
		{
		}

		public override void Initialize ()
		{
			Widget = (Gtk.ComboBox) CreateWidget ();
			var cr = new Gtk.CellRendererText ();
			Widget.PackStart (cr, false);
			Widget.AddAttribute (cr, "text", 0);
			Widget.Show ();
			Widget.RowSeparatorFunc = IsRowSeparator;
		}
		
		protected virtual Gtk.Widget CreateWidget ()
		{
			return new Gtk.ComboBox ();
		}
		
		protected new Gtk.ComboBox Widget {
			get { return (Gtk.ComboBox)base.Widget; }
			set { base.Widget = value; }
		}
		
		protected new IComboBoxEventSink EventSink {
			get { return (IComboBoxEventSink)base.EventSink; }
		}
		
		bool IsRowSeparator (Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			Gtk.TreePath path = model.GetPath (iter);
			return EventSink.RowIsSeparator (path.Indices[0]);
		}
		
		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is ComboBoxEvent) {
				if ((ComboBoxEvent)eventId == ComboBoxEvent.SelectionChanged)
					Widget.Changed += HandleChanged;
			}
		}
		
		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is ComboBoxEvent) {
				if ((ComboBoxEvent)eventId == ComboBoxEvent.SelectionChanged)
					Widget.Changed -= HandleChanged;
			}
		}

		void HandleChanged (object sender, EventArgs e)
		{
			EventSink.OnSelectionChanged ();
		}

		#region IComboBoxBackend implementation
		public void SetViews (CellViewCollection views)
		{
			Widget.Clear ();
			foreach (var v in views)
				CellUtil.CreateCellRenderer (this, null, v);
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

		public int SelectedRow {
			get {
				return Widget.Active;
			}
			set {
				Widget.Active = value;
			}
		}
		#endregion

		#region ICellRendererTarget implementation
		public void PackStart (object target, Gtk.CellRenderer cr, bool expand)
		{
			Widget.PackStart (cr, expand);
		}

		public void PackEnd (object target, Gtk.CellRenderer cr, bool expand)
		{
			Widget.PackEnd (cr, expand);
		}

		public void AddAttribute (object target, Gtk.CellRenderer cr, string field, int column)
		{
			Widget.AddAttribute (cr, field, column);
		}
		#endregion
	}
}

