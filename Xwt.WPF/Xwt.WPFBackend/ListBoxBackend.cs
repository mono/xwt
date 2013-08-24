//
// ListBoxBackend.cs
//
// Author:
//       Eric Maupin <ermau@xamarin.com>
//
// Copyright (c) 2012 Xamarin, Inc.
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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Xwt.Backends;
using Xwt.WPFBackend.Utilities;

namespace Xwt.WPFBackend
{
	public class ListBoxBackend
		: WidgetBackend, IListBoxBackend
	{
		public ListBoxBackend()
		{
			ListBox = new ExListBox();
			ListBox.DisplayMemberPath = ".[0]";
		}

		public ScrollPolicy VerticalScrollPolicy
		{
			get { return ScrollViewer.GetVerticalScrollBarVisibility (ListBox).ToXwtScrollPolicy(); }
			set { ScrollViewer.SetVerticalScrollBarVisibility (ListBox, value.ToWpfScrollBarVisibility ()); }
		}

		public ScrollPolicy HorizontalScrollPolicy
		{
			get { return ScrollViewer.GetHorizontalScrollBarVisibility (ListBox).ToXwtScrollPolicy (); }
			set { ScrollViewer.SetVerticalScrollBarVisibility (ListBox, value.ToWpfScrollBarVisibility ()); }
		}

		public void SetViews (CellViewCollection views)
		{
			ListBox.DisplayMemberPath = null;
			ListBox.ItemTemplate = new DataTemplate { VisualTree = CellUtil.CreateBoundColumnTemplate (views) };
		}

		public void SetSource (IListDataSource source, IBackend sourceBackend)
		{
			var dataSource = sourceBackend as ListDataSource;
			if (dataSource != null)
				ListBox.ItemsSource = dataSource;
			else
				ListBox.ItemsSource = new ListSourceNotifyWrapper (source);
		}

		public void SetSelectionMode (SelectionMode mode)
		{
			switch (mode) {
			case SelectionMode.Single:
				ListBox.SelectionMode = System.Windows.Controls.SelectionMode.Single;
				break;
			case SelectionMode.Multiple:
				ListBox.SelectionMode = System.Windows.Controls.SelectionMode.Extended;
				break;
			}
		}

		public void SelectAll ()
		{
			ListBox.SelectAll();
		}

		public void UnselectAll ()
		{
			ListBox.UnselectAll();
		}

		public int[] SelectedRows {
			get { return ListBox.SelectedItems.Cast<object>().Select (ListBox.Items.IndexOf).ToArray(); }
		}

		public void SelectRow (int pos)
		{
			object item = ListBox.Items [pos];
			if (ListBox.SelectionMode == System.Windows.Controls.SelectionMode.Single)
				ListBox.SelectedItem = item;
			else
				ListBox.SelectedItems.Add (item);
		}

		public void UnselectRow (int pos)
		{
			object item = ListBox.Items [pos];
			if (ListBox.SelectionMode == System.Windows.Controls.SelectionMode.Extended)
				ListBox.SelectedItems.Remove (item);
			else if (ListBox.SelectedItem == item)
				ListBox.SelectedItem = null;
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is TableViewEvent) {
				switch ((TableViewEvent)eventId) {
				case TableViewEvent.SelectionChanged:
					ListBox.SelectionChanged += OnSelectionChanged;
					break;
				}
			}
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is TableViewEvent) {
				switch ((TableViewEvent)eventId) {
				case TableViewEvent.SelectionChanged:
					ListBox.SelectionChanged -= OnSelectionChanged;
					break;
				}
			}
		}

		private void OnSelectionChanged (object sender, SelectionChangedEventArgs e)
		{
			ListBoxEventSink.OnSelectionChanged();
		}

		protected ExListBox ListBox {
			get { return (ExListBox) Widget; }
			set { Widget = value; }
		}

		protected IListBoxEventSink ListBoxEventSink {
			get { return (IListBoxEventSink) EventSink; }
		}
	}
}