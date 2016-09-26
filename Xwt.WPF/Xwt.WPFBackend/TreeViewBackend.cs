//
// TreeViewBackend.cs
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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

using Xwt.Backends;
using Xwt.WPFBackend.Utilities;
using System.Windows;
using SWC=System.Windows.Controls;
using System.Windows.Controls;

namespace Xwt.WPFBackend
{
	public class TreeViewBackend
		: WidgetBackend, ITreeViewBackend
	{
		Dictionary<CellView,CellInfo> cellViews = new Dictionary<CellView, CellInfo> ();

		class CellInfo {
			public ListViewColumn Column;
			public int CellIndex;
			public int ColumnIndex;
		}

		private static readonly ResourceDictionary TreeResourceDictionary;
		static TreeViewBackend()
		{
			Uri uri = new Uri ("pack://application:,,,/Xwt.WPF;component/XWT.WPFBackend/TreeView.xaml");
			TreeResourceDictionary = (ResourceDictionary)XamlReader.Load (System.Windows.Application.GetResourceStream (uri).Stream);
		}

		public TreeViewBackend ()
		{
			Tree = new ExTreeView();
			Tree.Resources.MergedDictionaries.Add (TreeResourceDictionary);
			Tree.ItemTemplate = new HierarchicalDataTemplate { ItemsSource = new Binding ("Children") };
			Tree.SetValue (VirtualizingStackPanel.IsVirtualizingProperty, true);
		}

		public ScrollViewer ScrollViewer {
			get {
				Decorator border = System.Windows.Media.VisualTreeHelper.GetChild(Tree, 0) as Decorator;
				if (border != null)
					return border.Child as ScrollViewer;
				else
					return null;
			}
		}

		public TreePosition CurrentEventRow { get; set;  }
		
		public ScrollPolicy VerticalScrollPolicy {
			get { return ScrollViewer.GetVerticalScrollBarVisibility (Tree).ToXwtScrollPolicy (); }
			set { ScrollViewer.SetVerticalScrollBarVisibility (Tree, value.ToWpfScrollBarVisibility ()); }
		}

		public ScrollPolicy HorizontalScrollPolicy {
			get { return ScrollViewer.GetHorizontalScrollBarVisibility (Tree).ToXwtScrollPolicy (); }
			set { ScrollViewer.SetHorizontalScrollBarVisibility (Tree, value.ToWpfScrollBarVisibility ()); }
		}

		public IScrollControlBackend CreateVerticalScrollControl()
		{
			return new ScrollControlBackend(ScrollViewer, true);
		}

		public IScrollControlBackend CreateHorizontalScrollControl()
		{
			return new ScrollControlBackend(ScrollViewer, false);
		}

		public TreePosition[] SelectedRows {
			get { return Tree.SelectedItems.Cast<TreePosition> ().ToArray (); }
		}


		public TreePosition FocusedRow {
			get {
				if (Tree.FocusedItem != null)
					return Tree.FocusedItem.DataContext as TreePosition;
				return null;
			}
			set {
				ExTreeViewItem item = GetVisibleTreeItem (value);
				Tree.FocusedItem = item;
			}
		}

		private bool headersVisible = true;
		public bool HeadersVisible {
			get { return this.headersVisible; }
			set
			{
				this.headersVisible = value;
				if (value) {
					if (Tree.View.ColumnHeaderContainerStyle != null)
						Tree.View.ColumnHeaderContainerStyle.Setters.Remove (HideHeaderSetter);
				} else {
					if (Tree.View.ColumnHeaderContainerStyle == null)
						Tree.View.ColumnHeaderContainerStyle = new Style();

				    Tree.View.ColumnHeaderContainerStyle.Setters.Add (HideHeaderSetter);
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
			}
		}

		public void SelectRow (TreePosition pos)
		{
			Tree.SelectedItems.Add (pos);
		}

		public void UnselectRow (TreePosition pos)
		{
			Tree.SelectedItems.Remove (pos);
		}

		public bool IsRowSelected (TreePosition row)
		{
			return Tree.SelectedItems.Contains (row);
		}

		public bool IsRowExpanded (TreePosition row)
		{
			return ((TreeStoreNode) row).IsExpanded;
		}

		public void ExpandToRow (TreePosition pos)
		{
			TreeStoreNode parent = ((TreeStoreNode) pos).Parent;
			if (parent != null)
			    parent.IsExpanded = true;
		}

		public void ExpandRow (TreePosition row, bool expandChildren)
		{
			TreeStoreNode node = ((TreeStoreNode) row);
			node.IsExpanded = true;

			if (expandChildren) {
			    foreach (TreeStoreNode childNode in node.Children)
			        childNode.IsExpanded = true;
			}
		}

		public void CollapseRow (TreePosition row)
		{
			((TreeStoreNode) row).IsExpanded = false;
		}

		public void ScrollToRow (TreePosition pos)
		{
			ExTreeViewItem item = GetVisibleTreeItem (pos);
			if (item != null)
				item.BringIntoView ();
		}

		public void SetSelectionMode (SelectionMode mode)
		{
			Tree.SelectionMode = (mode == SelectionMode.Single)
			                     	? SWC.SelectionMode.Single
			                     	: SWC.SelectionMode.Extended;
		}

		public void SelectAll ()
		{
			Tree.SelectAllExpanded();
		}

		public void UnselectAll ()
		{
			Tree.UnselectAll();
		}

		public void SetSource (ITreeDataSource source, IBackend sourceBackend)
		{
			Tree.ItemsSource = (TreeStoreBackend) sourceBackend;
		}

		public object AddColumn (ListViewColumn column)
		{
			var col = new GridViewColumn ();

			UpdateColumn (column, col, ListViewColumnChange.Title);

			Tree.View.Columns.Add (col);
			
			UpdateColumn (column, col, ListViewColumnChange.Cells);

			return col;
		}

		public void UpdateColumn (ListViewColumn column, object handle, ListViewColumnChange change)
		{
			var col = ((GridViewColumn) handle);
			switch (change) {
			case ListViewColumnChange.Title:
				col.Header = column.Title;
				break;

			case ListViewColumnChange.Cells:
                var cellTemplate = CellUtil.CreateBoundColumnTemplate(Context, Frontend, column.Views);

				col.CellTemplate = new DataTemplate { VisualTree = cellTemplate };

				int index = Tree.View.Columns.IndexOf (col);
				if (index == 0) {
					var dockFactory = CreateExpanderDock ();
					dockFactory.AppendChild (cellTemplate);

					col.CellTemplate.VisualTree = dockFactory;
				}

				MapColumn (column, col);

				break;
			case ListViewColumnChange.Alignment:
				var style = new Style(typeof(GridViewColumnHeader));
				style.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, Util.ToWpfHorizontalAlignment(column.Alignment)));
				col.HeaderContainerStyle = style;
				break;
			}
		}

		public void RemoveColumn (ListViewColumn column, object handle)
		{
			Tree.View.Columns.Remove ((GridViewColumn) handle);
			foreach (var k in cellViews.Where (e => e.Value.Column == column).Select (e => e.Key).ToArray ())
				cellViews.Remove (k);
		}

		void MapColumn (ListViewColumn col, GridViewColumn handle)
		{
			foreach (var k in cellViews.Where (e => e.Value.Column == col).Select (e => e.Key).ToArray ())
				cellViews.Remove (k);

			var colindex = Tree.View.Columns.IndexOf (handle);
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

		private RowDropPosition dropPosition;
		private ExTreeViewItem dropTarget;
		private Adorner dropAdorner;
		public bool GetDropTargetRow (double x, double y, out RowDropPosition pos, out TreePosition nodePosition)
		{
			nodePosition = null;
			this.dropPosition = pos = RowDropPosition.Into;

			var result = VisualTreeHelper.HitTest (Tree, new System.Windows.Point (x, y)) as PointHitTestResult;

			var element = (result != null) ? result.VisualHit as FrameworkElement : null;
			while (element != null) {
				if (element is ExTreeViewItem)
					break;
				if (element is ExTreeView) // We can't succeed past this point
					return false;

				element = VisualTreeHelper.GetParent (element) as FrameworkElement;
			}
			
			this.dropTarget = (ExTreeViewItem)element;
			
			if (element == null)
				return false;

			var point = ((UIElement)result.VisualHit).TranslatePoint (result.PointHit, element);

			double edge = element.ActualHeight * 0.15;
			if (point.Y <= edge)
				this.dropPosition = pos = RowDropPosition.Before;
			else if (point.Y >= element.ActualHeight - edge)
				this.dropPosition = pos = RowDropPosition.After;

			nodePosition = element.DataContext as TreeStoreNode;
			return true;
		}

		internal bool RowActivatedEventEnabled { get; private set; }

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is TableViewEvent) {
				switch ((TableViewEvent)eventId) {
				case TableViewEvent.SelectionChanged:
					Tree.SelectedItemsChanged += OnSelectedItemsChanged;
					break;
				}
			}

			if (eventId is TreeViewEvent)
			{
				switch ((TreeViewEvent)eventId)
				{
					case TreeViewEvent.RowActivated:
						RowActivatedEventEnabled = true;
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
					Tree.SelectedItemsChanged -= OnSelectedItemsChanged;
					break;
				}
			}

			if (eventId is TreeViewEvent)
			{
				switch ((TreeViewEvent)eventId)
				{
					case TreeViewEvent.RowActivated:
						RowActivatedEventEnabled = false;
						break;
				}
			}
		}

		protected ExTreeView Tree {
			get { return (ExTreeView)Widget; }
			set { Widget = value; }
		}

		protected ITreeViewEventSink TreeViewEventSink {
			get { return (ITreeViewEventSink)EventSink; }
		}

		protected override double DefaultNaturalHeight {
			get { return -1; }
		}

		protected override double DefaultNaturalWidth {
			get { return -1; }
		}

		public override Size GetPreferredSize (SizeConstraint widthConstraint, SizeConstraint heightConstraint)
		{
			return Size.Zero;
		}

		private void OnSelectedItemsChanged (object sender, EventArgs e)
		{
			Context.InvokeUserCode (TreeViewEventSink.OnSelectionChanged);
		}

		protected override void OnDragOver (object sender, DragOverEventArgs e)
		{
			AdornerLayer layer = AdornerLayer.GetAdornerLayer (Widget);
			if (this.dropAdorner != null)
			    layer.Remove (this.dropAdorner);

			base.OnDragOver (sender, e);

			if (this.dropTarget != null)
			    layer.Add (this.dropAdorner = new TreeViewDropAdorner (this.dropTarget, this.dropPosition));
		}

		protected override void OnDragLeave (object sender, EventArgs e)
		{
			base.OnDragLeave (sender, e);

			if (this.dropAdorner != null)
				AdornerLayer.GetAdornerLayer (Widget).Remove (this.dropAdorner);
		}

		protected override void OnDragFinished (object sender, DragFinishedEventArgs e)
		{
			base.OnDragFinished (sender, e);

			if (this.dropAdorner != null)
				AdornerLayer.GetAdornerLayer (Widget).Remove (this.dropAdorner);
		}

		private ExTreeViewItem GetVisibleTreeItem (TreePosition pos, Action<ExTreeViewItem> walk = null)
		{
			Tree.UpdateLayout();
			Stack<TreeStoreNode> nodes = new Stack<TreeStoreNode> ();

			TreeStoreNode node = (TreeStoreNode) pos;
			do {
				nodes.Push (node);
				node = node.Parent;
			} while (node != null);

			ExTreeViewItem treeItem = null;
			ItemContainerGenerator g = Tree.ItemContainerGenerator;
			while (nodes.Count > 0) {
				node = nodes.Pop ();
				treeItem = (ExTreeViewItem) g.ContainerFromItem (node);
				if (treeItem == null)
					continue;

				treeItem.UpdateLayout ();
				g = treeItem.ItemContainerGenerator;

				if (walk != null)
					walk (treeItem);
			}

			return treeItem;
		}

		private FrameworkElementFactory CreateExpanderDock ()
		{
			var dockFactory = new FrameworkElementFactory (typeof (DockPanel));

			var source = new RelativeSource (RelativeSourceMode.FindAncestor, typeof (ExTreeViewItem), 1);

			Style expanderStyle = new Style (typeof (SWC.Primitives.ToggleButton));
			expanderStyle.Setters.Add (new Setter (UIElement.FocusableProperty, false));
			expanderStyle.Setters.Add (new Setter (FrameworkElement.WidthProperty, 19d));
			expanderStyle.Setters.Add (new Setter (FrameworkElement.HeightProperty, 13d));

			var expanderTemplate = new ControlTemplate (typeof (SWC.Primitives.ToggleButton));

			var outerBorderFactory = new FrameworkElementFactory (typeof (Border));
			outerBorderFactory.SetValue (FrameworkElement.WidthProperty, 19d);
			outerBorderFactory.SetValue (FrameworkElement.HeightProperty, 13d);
			outerBorderFactory.SetValue (Control.BackgroundProperty, Brushes.Transparent);
			outerBorderFactory.SetBinding (UIElement.VisibilityProperty,
				new Binding ("HasItems") { RelativeSource = source, Converter = BoolVisibilityConverter });

			var innerBorderFactory = new FrameworkElementFactory (typeof (Border));
			innerBorderFactory.SetValue (FrameworkElement.WidthProperty, 9d);
			innerBorderFactory.SetValue (FrameworkElement.HeightProperty, 9d);
			innerBorderFactory.SetValue (Control.BorderThicknessProperty, new Thickness (1));
			innerBorderFactory.SetValue (Control.BorderBrushProperty, new SolidColorBrush (Color.FromRgb (120, 152, 181)));
			innerBorderFactory.SetValue (Border.CornerRadiusProperty, new CornerRadius (1));
			innerBorderFactory.SetValue (UIElement.SnapsToDevicePixelsProperty, true);

			innerBorderFactory.SetValue (Control.BackgroundProperty, ExpanderBackgroundBrush);

			var pathFactory = new FrameworkElementFactory (typeof (Path));
			pathFactory.SetValue (FrameworkElement.MarginProperty, new Thickness (1));
			pathFactory.SetValue (Shape.FillProperty, Brushes.Black);
			pathFactory.SetBinding (Path.DataProperty, 
				new Binding ("IsChecked") {
					RelativeSource = new RelativeSource (RelativeSourceMode.FindAncestor, typeof (SWC.Primitives.ToggleButton), 1),
					Converter = BooleanGeometryConverter
			});

			innerBorderFactory.AppendChild (pathFactory);
			outerBorderFactory.AppendChild (innerBorderFactory);

			expanderTemplate.VisualTree = outerBorderFactory;

			expanderStyle.Setters.Add (new Setter (Control.TemplateProperty, expanderTemplate));

			var toggleFactory = new FrameworkElementFactory (typeof (SWC.Primitives.ToggleButton));
			toggleFactory.SetValue (FrameworkElement.StyleProperty, expanderStyle);
			toggleFactory.SetBinding (FrameworkElement.MarginProperty,
				new Binding ("Level") { RelativeSource = source, Converter = LevelConverter });
			toggleFactory.SetBinding (SWC.Primitives.ToggleButton.IsCheckedProperty,
				new Binding ("IsExpanded") { RelativeSource = source });
			toggleFactory.SetValue (SWC.Primitives.ButtonBase.ClickModeProperty, ClickMode.Press);

			dockFactory.AppendChild (toggleFactory);
			return dockFactory;
		}

		private static readonly LevelToIndentConverter LevelConverter = new LevelToIndentConverter();
		private static readonly BooleanToVisibilityConverter BoolVisibilityConverter = new BooleanToVisibilityConverter();

		private static readonly LinearGradientBrush ExpanderBackgroundBrush = new LinearGradientBrush {
			StartPoint = new System.Windows.Point (0, 0),
			EndPoint = new System.Windows.Point (1, 1),
			GradientStops = new GradientStopCollection {
				new GradientStop (Colors.White, 0.2),
				new GradientStop (Color.FromRgb (192, 183, 166), 1)
			}
		};

		private static readonly Geometry Plus = Geometry.Parse ("M 0 2 L 0 3 L 2 3 L 2 5 L 3 5 L 3 3 L 5 3 L 5 2 L 3 2 L 3 0 L 2 0 L 2 2 Z");
		private static readonly Geometry Minus = Geometry.Parse ("M 0 2 L 0 3 L 5 3 L 5 2 Z");

		private static readonly BooleanToValueConverter BooleanGeometryConverter = 
			new BooleanToValueConverter { TrueValue = Minus, FalseValue = Plus };

		private static readonly Setter HideHeaderSetter = new Setter (UIElement.VisibilityProperty, Visibility.Collapsed);

		public TreePosition GetRowAtPosition (Point p)
		{
			var result = VisualTreeHelper.HitTest (Tree, new System.Windows.Point (p.X, p.Y)) as PointHitTestResult;

			var element = (result != null) ? result.VisualHit as FrameworkElement : null;
			while (element != null) {
				if (element is ExTreeViewItem)
					break;
				if (element is ExTreeView) // We can't succeed past this point
					return null;

				element = VisualTreeHelper.GetParent (element) as FrameworkElement;
			}

			if (element == null)
				return null;

			return (element.DataContext as TreeStoreNode);
		}

		public Rectangle GetCellBounds (TreePosition pos, CellView cell, bool includeMargin)
		{
			ExTreeViewItem item = GetVisibleTreeItem (pos);
			if (item == null)
				return Rectangle.Zero;

			// this works only if the wpf layout remains the same
			try {
				var stackpanel = VisualTreeHelper.GetChild (item, 0);
				var border = VisualTreeHelper.GetChild (stackpanel, 0);
				var rowpresenter = VisualTreeHelper.GetChild (border, 0) as FrameworkElement;

				if (VisualTreeHelper.GetChildrenCount (rowpresenter) < cellViews [cell].ColumnIndex)
					return Rectangle.Zero;

				var colpresenter =  VisualTreeHelper.GetChild (rowpresenter, cellViews [cell].ColumnIndex) as FrameworkElement;
				var colchild =  VisualTreeHelper.GetChild (colpresenter, 0) as FrameworkElement;

				if (cellViews [cell].Column.Views.Count > 1 && colchild is System.Windows.Controls.StackPanel) {
					var childStack = colchild as System.Windows.Controls.StackPanel;
					if (childStack == null || VisualTreeHelper.GetChildrenCount (childStack) < cellViews [cell].CellIndex)
						return Rectangle.Zero;
					var cellpresenter = VisualTreeHelper.GetChild (childStack, cellViews [cell].ColumnIndex == 0 ? cellViews [cell].CellIndex + 1 : cellViews [cell].CellIndex) as FrameworkElement;
					var position = cellpresenter.TransformToAncestor (Tree).Transform(new System.Windows.Point(-Tree.Padding.Left, 0));
					var rect = new Rect (position, cellpresenter.RenderSize);
					return rect.ToXwtRect ();
				} else {
					if (cellViews [cell].ColumnIndex == 0)
						colchild = VisualTreeHelper.GetChild (colchild, 1) as FrameworkElement;
					var position = colchild.TransformToAncestor (Tree).Transform(new System.Windows.Point(-Tree.Padding.Left, 0));
					var rect = new Rect (position, colchild.RenderSize);
					return rect.ToXwtRect ();
				}
			} catch (ArgumentOutOfRangeException) {
				return Rectangle.Zero;
			} catch (ArgumentNullException) {
				return Rectangle.Zero;
			}
		}

		public Rectangle GetRowBounds (TreePosition pos, bool includeMargin)
		{
			ExTreeViewItem item = GetVisibleTreeItem (pos);
			if (item == null)
				return Rectangle.Zero;

			// this works only if the wpf layout remains the same
			try {
				var stackpanel = VisualTreeHelper.GetChild (item, 0);
				var border = VisualTreeHelper.GetChild (stackpanel, 0) as FrameworkElement;

				var rect = Rectangle.Zero;
				if (includeMargin) {
					var position = border.TransformToAncestor (Tree).Transform (new System.Windows.Point (0, 0));
					rect = new Rect (position, border.RenderSize).ToXwtRect();
					rect.X -= Tree.Padding.Left;
				} else {
					var rowpresenter = VisualTreeHelper.GetChild (border, 0) as FrameworkElement;
					for (int i = 0; i < VisualTreeHelper.GetChildrenCount (rowpresenter); i++)
					{
						var colpresenter =  VisualTreeHelper.GetChild (rowpresenter, i) as FrameworkElement;
						var colchild =  VisualTreeHelper.GetChild (colpresenter, 0) as FrameworkElement;
						var cellcount = VisualTreeHelper.GetChildrenCount (colchild);
						if (cellcount > 1)
							for (int j = 0; j < cellcount; j++) {
								var cell =  VisualTreeHelper.GetChild (colchild, j) as FrameworkElement;
								var position = cell.TransformToAncestor (Tree).Transform(new System.Windows.Point(-Tree.Padding.Left, 0));
								var cell_rect = new Rect (position, cell.RenderSize).ToXwtRect();
								if (rect == Rectangle.Zero)
									rect = cell_rect;
								else
									rect = rect.Union(cell_rect);
							}
						else {
							var position = colchild.TransformToAncestor (Tree).Transform(new System.Windows.Point(-Tree.Padding.Left, 0));
							var cell_rect = new Rect (position, colchild.RenderSize).ToXwtRect();
							if (rect == Rectangle.Zero)
								rect = cell_rect;
							else
								rect = rect.Union(cell_rect);
						}
					}
					//var position = rowpresenter.TransformToAncestor (Tree).Transform(new System.Windows.Point(0,0));
					//rect = new Rect (position, rowpresenter.RenderSize).ToXwtRect();
					//rect.X -= Tree.Padding.Left + Tree.BorderThickness.Left;
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
