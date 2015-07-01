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
using System.Windows.Media;
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

		public ScrollViewer ScrollViewer {
			get {
				Decorator border = System.Windows.Media.VisualTreeHelper.GetChild(ListBox, 0) as Decorator;
				if (border != null)
					return border.Child as ScrollViewer;
				else
					return null;
			}
		}

		bool gridLinesVisible;
		public bool GridLinesVisible {
			get {
				return gridLinesVisible;
			}
			set {
				gridLinesVisible = value;
				if (!value) {
					if (this.ListBox.ItemContainerStyle != null) {
						this.ListBox.ItemContainerStyle.Setters.Remove (GridHorizontalSetter);
						this.ListBox.ItemContainerStyle.Setters.Remove (BorderBrushSetter);
					}
				} else {
					if (this.ListBox.ItemContainerStyle == null)
						this.ListBox.ItemContainerStyle = new Style ();

					this.ListBox.ItemContainerStyle.Setters.Add (GridHorizontalSetter);
					this.ListBox.ItemContainerStyle.Setters.Add (BorderBrushSetter);
				}
			}
		}

		private static readonly Setter GridHorizontalSetter = new Setter (ListBoxItem.BorderThicknessProperty, new Thickness (0, 0, 0, 1));
		private static readonly Setter BorderBrushSetter = new Setter (ListBoxItem.BorderBrushProperty, System.Windows.Media.Brushes.LightGray);

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

		public IScrollControlBackend CreateVerticalScrollControl()
		{
			return new ScrollControlBackend(ScrollViewer, true);
		}

		public IScrollControlBackend CreateHorizontalScrollControl()
		{
			return new ScrollControlBackend(ScrollViewer, false);
		}

		public void SetViews (CellViewCollection views)
		{
			ListBox.DisplayMemberPath = null;
            ListBox.ItemTemplate = new DataTemplate { VisualTree = CellUtil.CreateBoundColumnTemplate(Context, Frontend, views) };
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

		public void ScrollToRow (int row)
		{
			ListBox.ScrollIntoView (ListBox.Items [row]);
		}

		public int[] SelectedRows {
			get { return ListBox.SelectedItems.Cast<object>().Select (ListBox.Items.IndexOf).ToArray(); }
		}

		public int FocusedRow {
			get {
				if (ListBox.FocusedItem != null)
					return ListBox.ItemContainerGenerator.IndexFromContainer(ListBox.FocusedItem);
				return -1;
			}
			set {
				ListBoxItem item = null;
				if (value >= 0) {
					item = ListBox.ItemContainerGenerator.ContainerFromIndex(value) as ListBoxItem;
				}
				ListBox.FocusItem(item);
			}
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

		public int GetRowAtPosition(Point p)
		{
			var result = VisualTreeHelper.HitTest (ListBox, new System.Windows.Point (p.X, p.Y)) as PointHitTestResult;

			var element = (result != null) ? result.VisualHit as FrameworkElement : null;
			while (element != null) {
				if (element is ListBoxItem)
					break;
				if (element is ExListBox) // We can't succeed past this point
					return -1;

				element = VisualTreeHelper.GetParent (element) as FrameworkElement;
			}

			if (element == null)
				return -1;

			int index = ListBox.ItemContainerGenerator.IndexFromContainer(element);
			return index;
		}

		public Rectangle GetRowBounds (int row, bool includeMargin)
		{
			ListBoxItem item = ListBox.ItemContainerGenerator.ContainerFromIndex (row) as ListBoxItem;
			if (item == null)
				return Rectangle.Zero;

			// this works only if the wpf layout remains the same
			try {
				var border = VisualTreeHelper.GetChild (item, 0) as FrameworkElement;

				var rect = Rectangle.Zero;
				if (includeMargin) {
					var position = border.TransformToAncestor (ListBox).Transform (new System.Windows.Point (0, 0));
					rect = new Rect (position, border.RenderSize).ToXwtRect();
					rect.X -= ListBox.Padding.Left + ListBox.BorderThickness.Left;
				} else {
					var contentpresenter = VisualTreeHelper.GetChild (border, 0) as FrameworkElement;
					var child = VisualTreeHelper.GetChild (contentpresenter, 0) as FrameworkElement;
					var position = child.TransformToAncestor (ListBox).Transform(new System.Windows.Point(0,0));
					rect = new Rect (position, child.RenderSize).ToXwtRect();
					rect.X -= ListBox.Padding.Left + ListBox.BorderThickness.Left;
				}

				return rect;
			} catch (ArgumentOutOfRangeException) {
				return Rectangle.Zero;
			} catch (ArgumentNullException) {
				return Rectangle.Zero;
			}
		}
	}
}