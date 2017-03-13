// 
// ListViewBackend.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Xwt.WPFBackend.Utilities;
using SWC = System.Windows.Controls;
using Xwt.Backends;
using System.Windows.Input;

namespace Xwt.WPFBackend
{
	public class ListViewBackend
		: WidgetBackend, IListViewBackend
	{
		Dictionary<CellView,CellInfo> cellViews = new Dictionary<CellView, CellInfo> ();

		class CellInfo {
			public ListViewColumn Column;
			public int CellIndex;
			public int ColumnIndex;
		}

		public ListViewBackend()
		{
			ListView = new ExListView();
			ListView.View = this.view;
        }

		public ScrollViewer ScrollViewer {
			get {
	            Decorator border = System.Windows.Media.VisualTreeHelper.GetChild(ListView, 0) as Decorator;
	            if (border != null)
	                return border.Child as ScrollViewer;
	            else
	                return null;
	        }
        }

        public int CurrentEventRow { get; set;  }
		
		public ScrollPolicy VerticalScrollPolicy {
			get { return ScrollViewer.GetVerticalScrollBarVisibility (this.ListView).ToXwtScrollPolicy (); }
			set { ScrollViewer.SetVerticalScrollBarVisibility (ListView, value.ToWpfScrollBarVisibility ()); }
		}

		public ScrollPolicy HorizontalScrollPolicy {
			get { return ScrollViewer.GetHorizontalScrollBarVisibility (this.ListView).ToXwtScrollPolicy (); }
			set { ScrollViewer.SetHorizontalScrollBarVisibility (ListView, value.ToWpfScrollBarVisibility ()); }
		}

		public IScrollControlBackend CreateVerticalScrollControl()
		{
			return new ScrollControlBackend(ScrollViewer, true);
		}

		public IScrollControlBackend CreateHorizontalScrollControl()
		{
			return new ScrollControlBackend(ScrollViewer, false);
		}
       
        private bool borderVisible = true;
		public bool BorderVisible
		{
			get { return this.borderVisible; }
			set
			{
				if (this.borderVisible == value)
					return;

				if (value)
					ListView.ClearValue (Control.BorderBrushProperty);
				else
					ListView.BorderBrush = null;

				this.borderVisible = value;
			}
		}

		public bool HeadersVisible {
			get { return this.headersVisible; }
			set {
				this.headersVisible = value;
				if (value) {
				    if (this.view.ColumnHeaderContainerStyle != null)
						this.view.ColumnHeaderContainerStyle.Setters.Remove (HideHeadersSetter);
				} else {
					if (this.view.ColumnHeaderContainerStyle == null)
						this.view.ColumnHeaderContainerStyle = new Style();

					this.view.ColumnHeaderContainerStyle.Setters.Add (HideHeadersSetter);
				}
			}
		}

		GridLines gridLinesVisible;
		public GridLines GridLinesVisible {
			get {
				return gridLinesVisible;
			}
			set {
				gridLinesVisible = value;
				// we support only horizontal grid lines for now
				// vertical lines are tricky and have to be drawn manually...
				if (value == GridLines.None) {
					if (this.ListView.ItemContainerStyle != null) {
						this.ListView.ItemContainerStyle.Setters.Remove (GridHorizontalSetter);
						this.ListView.ItemContainerStyle.Setters.Remove (BorderBrushSetter);
					}
				} else {
					if (this.ListView.ItemContainerStyle == null)
						this.ListView.ItemContainerStyle = new Style ();

					this.ListView.ItemContainerStyle.Setters.Add (GridHorizontalSetter);
					this.ListView.ItemContainerStyle.Setters.Add (BorderBrushSetter);
				}
			}
		}

		public int[] SelectedRows {
			get { return ListView.SelectedItems.Cast<object>().Select (ListView.Items.IndexOf).ToArray (); }
		}

		public int FocusedRow {
			get {
				if (ListView.FocusedItem != null)
					return ListView.ItemContainerGenerator.IndexFromContainer(ListView.FocusedItem);
				return -1;
			}
			set {
				ListViewItem item = null;
				if (value >= 0) {
					item = ListView.ItemContainerGenerator.ContainerFromIndex(value) as ListViewItem;
				}
				ListView.FocusItem(item);
			}
		}

		public object AddColumn (ListViewColumn col)
		{
			var column = new GridViewColumn ();
			column.CellTemplate = new DataTemplate { VisualTree = CellUtil.CreateBoundColumnTemplate (Context, Frontend, col.Views) };
			if (col.HeaderView != null)
				column.HeaderTemplate = new DataTemplate { VisualTree = CellUtil.CreateBoundCellRenderer (Context, Frontend, col.HeaderView) };
			else
				column.Header = col.Title;

			this.view.Columns.Add (column);

			MapColumn (col, column);

			return column;
		}

		public void RemoveColumn (ListViewColumn col, object handle)
		{
			this.view.Columns.Remove ((GridViewColumn) handle);
		}

		public void UpdateColumn (ListViewColumn col, object handle, ListViewColumnChange change)
		{
			var column = (GridViewColumn) handle;
            column.CellTemplate = new DataTemplate { VisualTree = CellUtil.CreateBoundColumnTemplate(Context, Frontend, col.Views) };
			if (col.HeaderView != null)
                column.HeaderTemplate = new DataTemplate { VisualTree = CellUtil.CreateBoundCellRenderer(Context, Frontend, col.HeaderView) };
			else
				column.Header = col.Title;

			MapColumn (col, column);
		}

		void MapColumn (ListViewColumn col, GridViewColumn handle)
		{
			foreach (var k in cellViews.Where (e => e.Value.Column == col).Select (e => e.Key).ToArray ())
				cellViews.Remove (k);

			var colindex = view.Columns.IndexOf (handle);
			for (int i = 0; i < col.Views.Count; i++) {
				var v = col.Views [i];
				var cellindex = col.Views.IndexOf (v);
				cellViews [v] = new CellInfo {
					Column = col,
					CellIndex = cellindex,
					ColumnIndex = colindex
				};
			}
		}

		public void SetSelectionMode (SelectionMode mode)
		{
			switch (mode) {
			case SelectionMode.Single:
				ListView.SelectionMode = SWC.SelectionMode.Single;
				break;

			case SelectionMode.Multiple:
				ListView.SelectionMode = SWC.SelectionMode.Extended;
				break;
			}
		}

		public void SelectAll ()
		{
			ListView.SelectAll();
		}

		public void UnselectAll ()
		{
			ListView.UnselectAll();
		}

		public void ScrollToRow (int row)
		{
			ListView.ScrollIntoView (ListView.Items [row]);
		}

		public void StartEditingCell (int row, CellView cell)
		{
			// TODO
		}

		public void SetSource (IListDataSource source, IBackend sourceBackend)
		{
			var dataSource = sourceBackend as ListDataSource;
			if (dataSource != null)
				ListView.ItemsSource = dataSource;
			else
				ListView.ItemsSource = new ListSourceNotifyWrapper (source);
		}

		public void SelectRow (int pos)
		{
			object item = ListView.Items [pos];
			if (ListView.SelectionMode == System.Windows.Controls.SelectionMode.Single)
				ListView.SelectedItem = item;
			else
				ListView.SelectedItems.Add (item);
		}

		public void UnselectRow (int pos)
		{
			object item = ListView.Items [pos];
			if (ListView.SelectionMode == System.Windows.Controls.SelectionMode.Extended)
				ListView.SelectedItems.Remove (item);
			else if (ListView.SelectedItem == item)
				ListView.SelectedItem = null;
		}

		public override void EnableEvent(object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is TableViewEvent) {
				switch ((TableViewEvent)eventId) {
				case TableViewEvent.SelectionChanged:
					ListView.SelectionChanged += OnSelectionChanged;
					break;
				}
			}

			if (eventId is ListViewEvent) {
				switch ((ListViewEvent)eventId) {
				case ListViewEvent.RowActivated:
					ListView.MouseDoubleClick += OnMouseDoubleClick;
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
					ListView.SelectionChanged -= OnSelectionChanged;
					break;
				}
			}

			if (eventId is ListViewEvent) {
				switch ((ListViewEvent)eventId) {
				case ListViewEvent.RowActivated:
					ListView.MouseDoubleClick -= OnMouseDoubleClick;
					break;
				}
			}
		}

		private void OnSelectionChanged (object sender, SelectionChangedEventArgs e)
		{
			Context.InvokeUserCode (ListViewEventSink.OnSelectionChanged);
		}

		private void OnMouseDoubleClick (object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
				ListViewEventSink.OnRowActivated (ListView.SelectedIndex);
		}

		private bool headersVisible;
		private readonly GridView view = new GridView();

		protected ExListView ListView {
			get { return (ExListView) Widget; }
			set { Widget = value; }
		}

		protected IListViewEventSink ListViewEventSink {
			get { return (IListViewEventSink) EventSink; }
		}

		private static readonly Setter HideHeadersSetter = new Setter (UIElement.VisibilityProperty, Visibility.Collapsed);
		private static readonly Setter GridHorizontalSetter = new Setter (ListViewItem.BorderThicknessProperty, new Thickness (0, 0, 0, 1));
		private static readonly Setter BorderBrushSetter = new Setter (ListViewItem.BorderBrushProperty, System.Windows.Media.Brushes.LightGray);

        public int GetRowAtPosition(Point p)
		{
			var result = VisualTreeHelper.HitTest (ListView, new System.Windows.Point (p.X, p.Y)) as PointHitTestResult;

			var element = (result != null) ? result.VisualHit as FrameworkElement : null;
			while (element != null) {
				if (element is ExListViewItem)
					break;
				if (element is ExListView) // We can't succeed past this point
					return -1;

				element = VisualTreeHelper.GetParent (element) as FrameworkElement;
			}

			if (element == null)
				return -1;

			int index = ListView.ItemContainerGenerator.IndexFromContainer(element);
			return index;
        }

        public Rectangle GetCellBounds(int row, CellView cell, bool includeMargin)
        {
			ExListViewItem item = ListView.ItemContainerGenerator.ContainerFromIndex (row) as ExListViewItem;
			if (item == null)
				return Rectangle.Zero;

			// this works only if the wpf layout remains the same
			try {
				var stackpanel = VisualTreeHelper.GetChild (item, 0);
				var border = VisualTreeHelper.GetChild (stackpanel, 0);
				var grid = VisualTreeHelper.GetChild (border, 0);
				var rowpresenter = VisualTreeHelper.GetChild (grid, 1) as FrameworkElement;

				if (VisualTreeHelper.GetChildrenCount (rowpresenter) != view.Columns.Count)
					return Rectangle.Zero;

				var colpresenter =  VisualTreeHelper.GetChild (rowpresenter, cellViews [cell].ColumnIndex) as FrameworkElement;
				var colchild =  VisualTreeHelper.GetChild (colpresenter, 0) as FrameworkElement;

				if (cellViews [cell].Column.Views.Count > 1 && colchild is System.Windows.Controls.StackPanel) {
					var childStack = colchild as System.Windows.Controls.StackPanel;
					if (childStack == null || VisualTreeHelper.GetChildrenCount (childStack) < cellViews [cell].CellIndex)
						return Rectangle.Zero;
					var cellpresenter = VisualTreeHelper.GetChild (childStack, cellViews [cell].CellIndex) as FrameworkElement;
					var position = cellpresenter.TransformToAncestor (ListView).Transform(new System.Windows.Point(-ListView.Padding.Left, 0));
					var rect = new Rect (position, cellpresenter.RenderSize);
					return rect.ToXwtRect ();
				} else {
					var position = colchild.TransformToAncestor (ListView).Transform(new System.Windows.Point(-ListView.Padding.Left, 0));
					var rect = new Rect (position, colchild.RenderSize);
					return rect.ToXwtRect ();
				}
			} catch (ArgumentOutOfRangeException) {
				return Rectangle.Zero;
			} catch (ArgumentNullException) {
				return Rectangle.Zero;
			}
		}

		public Rectangle GetRowBounds (int row, bool includeMargin)
		{
			ExListViewItem item = ListView.ItemContainerGenerator.ContainerFromIndex (row) as ExListViewItem;
			if (item == null)
				return Rectangle.Zero;

			// this works only if the wpf layout remains the same
			try {
				var stackpanel = VisualTreeHelper.GetChild (item, 0);
				var border = VisualTreeHelper.GetChild (stackpanel, 0) as FrameworkElement;

				var rect = Rectangle.Zero;
				if (includeMargin) {
					var position = border.TransformToAncestor (ListView).Transform (new System.Windows.Point (0, 0));
					rect = new Rect (position, border.RenderSize).ToXwtRect();
					rect.X -= ListView.Padding.Left + ListView.BorderThickness.Left;
				} else {
					var grid = VisualTreeHelper.GetChild (border, 0);
					var rowpresenter = VisualTreeHelper.GetChild (grid, 1) as FrameworkElement;
					for (int i = 0; i < VisualTreeHelper.GetChildrenCount (rowpresenter); i++)
					{
						var colpresenter =  VisualTreeHelper.GetChild (rowpresenter, i) as FrameworkElement;
						var colchild =  VisualTreeHelper.GetChild (colpresenter, 0) as FrameworkElement;
						var cellcount = VisualTreeHelper.GetChildrenCount (colchild);
						if (cellcount > 1)
							for (int j = 0; j < cellcount; j++) {
								var cell =  VisualTreeHelper.GetChild (colchild, j) as FrameworkElement;
								var position = cell.TransformToAncestor (ListView).Transform(new System.Windows.Point(-ListView.Padding.Left, 0));
								var cell_rect = new Rect (position, cell.RenderSize).ToXwtRect();
								if (rect == Rectangle.Zero)
									rect = cell_rect;
								else
									rect = rect.Union(cell_rect);
							}
						else {
							var position = colchild.TransformToAncestor (ListView).Transform(new System.Windows.Point(-ListView.Padding.Left, 0));
							var cell_rect = new Rect (position, colchild.RenderSize).ToXwtRect();
							if (rect == Rectangle.Zero)
								rect = cell_rect;
							else
								rect = rect.Union(cell_rect);
						}
					}
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
