//
// ExTreeView.cs
//
// Author:
//       Eric Maupin <ermau@xamarin.com>
//       David Karlaš <david.karlas@gmail.com>
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
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using SWC = System.Windows.Controls;
using SWM = System.Windows.Media;
using WKey = System.Windows.Input.Key;
using System.Windows.Automation.Peers;

namespace Xwt.WPFBackend
{
	using Keyboard = System.Windows.Input.Keyboard;

	public class ExTreeView
		: SWC.TreeView, IWpfWidget
	{
		private ScrollViewer scrollViewer;
		private bool needColumnWidthsUpdate;

		public ExTreeView ()
		{
			SelectedItems = new ObservableCollection<object> ();
			Loaded += OnLoaded;
			View.Columns.CollectionChanged += OnColumnsCollectionChanged;
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			if (SelectionMode == SWC.SelectionMode.Single)
			{
				if (SelectedItems.Count == 0)
				{
					if (Items.Count > 0)
						SelectItem(Items[0]);
				}
			}
		}

		private void OnColumnsCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateColumnWidths ();
		}

		public event EventHandler SelectedItemsChanged;

		public WidgetBackend Backend
		{
			get;
			set;
		}

		private readonly GridView view = new GridView();
		public GridView View
		{
			get { return this.view; }
		}

		public static readonly DependencyProperty SelectionModeProperty = DependencyProperty.Register ("SelectionMode",
			typeof (System.Windows.Controls.SelectionMode), typeof (ExTreeView),
			new UIPropertyMetadata (System.Windows.Controls.SelectionMode.Single, OnSelectionModePropertyChanged));

		public System.Windows.Controls.SelectionMode SelectionMode
		{
			get { return (System.Windows.Controls.SelectionMode) GetValue (SelectionModeProperty); }
			set { SetValue (SelectionModeProperty, value); }
		}

		public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register ("SelectedItems",
			typeof (IList), typeof (ExTreeView),
			new UIPropertyMetadata (OnSelectedItemsPropertyChanged));

		public IList SelectedItems
		{
			get { return (IList) GetValue (SelectedItemsProperty); }
			set { SetValue (SelectedItemsProperty, value); }
		}

		public void SelectAllExpanded()
		{
			if (Items == null || Items.Count == 0)
				return;

			foreach (object item in Items) {
				var treeItem = ((ExTreeViewItem) ItemContainerGenerator.ContainerFromItem (item));
				treeItem.SelectChildren ((i,t) => t.IsExpanded);
			}
		}

		public void UnselectAll()
		{
			if (Items == null || Items.Count == 0)
				return;

			SelectedItems.Clear();
		}

		public ExTreeViewItem ContainerFromObject (object item)
		{
			ExTreeViewItem treeItem = null;
			TraverseTree ((o,i) => {
				if (o == item) {
					treeItem = i;
					return false;
				}

				return true;
			});

			return treeItem;
		}

		public void UpdateColumnWidths ()
		{
			needColumnWidthsUpdate = true;
			InvalidateVisual ();
		}

		protected override void OnRenderSizeChanged (SizeChangedInfo sizeInfo)
		{
			UpdateColumnWidths ();
			base.OnRenderSizeChanged (sizeInfo);
		}

		protected override void OnRender (SWM.DrawingContext drawingContext)
		{
			EnsureColumnWidthUpdated ();
			base.OnRender (drawingContext);
		}

		protected override bool IsItemItsOwnContainerOverride (object item)
		{
			return item is ExTreeViewItem;
		}

		protected override DependencyObject GetContainerForItemOverride()
		{
			return new ExTreeViewItem (this);
		}

		protected override System.Windows.Size MeasureOverride (System.Windows.Size constraint)
	    {
	        var s = base.MeasureOverride (constraint);
	        if (ScrollViewer.GetHorizontalScrollBarVisibility (this) != ScrollBarVisibility.Hidden)
	            s.Width = 0;
	        if (ScrollViewer.GetVerticalScrollBarVisibility (this) != ScrollBarVisibility.Hidden)
	            s.Height = SystemParameters.CaptionHeight;

	        return Backend.MeasureOverride (constraint, s);
	    }

		protected virtual void OnSelectionModeChanged (DependencyPropertyChangedEventArgs e)
		{
			System.Windows.Controls.SelectionMode oldMode = (System.Windows.Controls.SelectionMode) e.OldValue;

			object lastSelected = SelectedItem;

			if (oldMode == System.Windows.Controls.SelectionMode.Multiple) {
				UnselectAll ();
				if (lastSelected != null)
					SelectedItems.Add (lastSelected);
			}
		}

		protected virtual void OnSelectedItemsChanged (EventArgs e)
		{
			var handler = SelectedItemsChanged;
			if (handler != null)
				handler (this, e);
		}

		protected virtual void OnSelectedItemsPropertyChanged (DependencyPropertyChangedEventArgs e)
		{
			var oldNotifying = e.OldValue as INotifyCollectionChanged;
			if (oldNotifying != null)
				oldNotifying.CollectionChanged -= SelectedItemsCollectionChanged;

			var newNotifying = e.NewValue as INotifyCollectionChanged;
			if (newNotifying != null)
				newNotifying.CollectionChanged += SelectedItemsCollectionChanged;
		}

		private void EnsureColumnWidthUpdated ()
		{
			if (!needColumnWidthsUpdate)
				return;

			UpdateExpandableColumnWidths ();
			needColumnWidthsUpdate = false;
		}

		private ScrollViewer FindScrollViewer ()
		{
			DependencyObject currentObj = this;
			while (currentObj != null) {
				var scrollViewer = FindAmongChildren<ScrollViewer> (currentObj);
				if (scrollViewer == null) {
					currentObj = SWM.VisualTreeHelper.GetParent (currentObj);
					continue;
				}

				scrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
				return scrollViewer;
			}
			return null;
		}

		private static T FindAmongChildren<T> (DependencyObject obj)
			where T : DependencyObject
		{
			int childrenCount = SWM.VisualTreeHelper.GetChildrenCount (obj);
			for (int i = 0; i < childrenCount; i++) {
				var child = SWM.VisualTreeHelper.GetChild (obj, i);
				if (child is T)
					return (T) child;

				var result = FindAmongChildren<T> (child);
				if (result != null)
					return result;
			}
			return null;
		}

		private void OnScrollViewerScrollChanged (object sender, ScrollChangedEventArgs e)
		{
			UpdateColumnWidths ();
		}

		private double CalculateTreeContentWidth ()
		{
			if (scrollViewer == null)
				scrollViewer = FindScrollViewer ();

			double width = ActualWidth;
			if (scrollViewer != null) {
				if (scrollViewer?.ComputedVerticalScrollBarVisibility == Visibility.Visible)
					width -= SystemParameters.VerticalScrollBarWidth;
			}

			const double HorizontalMargin = 10; // Prevents horizontal scroll bar to appear before it should
			return width - HorizontalMargin;
		}

		private void UpdateExpandableColumnWidths ()
		{
			double totalWidth = CalculateTreeContentWidth ();
			if (totalWidth <= 0.0)
				return;

			var columns = View.Columns.Select (c => (ExGridViewColumn) c);
			if (!columns.Any ())
				return;

			var expandableColumns = columns.Where (c => c.Expands).ToList ();
			var nonExpandableColumns = columns.Where (c => !c.Expands).ToList ();

			if (expandableColumns.Count == 0) {
				var lastColumn = columns.Last ();
				expandableColumns.Add (lastColumn);
				nonExpandableColumns.Remove (lastColumn);
			}

			double totalFixedWidth = nonExpandableColumns.Sum (c => c.ActualWidth);
			double totalWidthForExpanding = totalWidth - totalFixedWidth;
			double newExpandableWidth = Math.Max (totalWidthForExpanding / expandableColumns.Count, 0.0);

			foreach (var column in expandableColumns)
				column.SetWidthForced (newExpandableWidth);
		}

		private bool TraverseTree (Func<object, ExTreeViewItem, bool> action, ExTreeViewItem parent = null)
		{
			ItemContainerGenerator g = ItemContainerGenerator;
			IEnumerable items = Items;
			if (parent != null) {
				items = parent.Items;
				g = parent.ItemContainerGenerator;
			}

			foreach (object item in items) {
				var treeItem = (ExTreeViewItem)g.ContainerFromItem (item);
				if (treeItem == null)
					continue;

				if (!action (item, treeItem))
					return false;
				
				if (!TraverseTree (action, treeItem))
					return false;
			}

			return true;
		}

		private ExTreeViewItem GetTreeItem (object item)
		{
			return (ExTreeViewItem) ItemContainerGenerator.ContainerFromItem (item);
		}

		private static readonly PropertyInfo IsSelectionChangeActiveProperty =
			typeof (System.Windows.Controls.TreeView).GetProperty ("IsSelectionChangeActive", BindingFlags.NonPublic | BindingFlags.Instance);

		private void SelectedItemsCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			bool changeActive = (bool)IsSelectionChangeActiveProperty.GetValue (this, null);
			if (!changeActive)
				IsSelectionChangeActiveProperty.SetValue (this, true, null);

			switch (e.Action) {
			case NotifyCollectionChangedAction.Reset:
				var selectedItems = new HashSet<object> (SelectedItems.Cast<object> ());
				TraverseTree ((o, i) => {
					i.IsSelected = selectedItems.Remove (o);
					return true;
				});

				foreach (object item in SelectedItems)
					GetTreeItem (item).IsSelected = true;
			break;

			default:
				if (!changeActive && SelectionMode == System.Windows.Controls.SelectionMode.Single) {
					if (SelectedItems.Count > 0)
						SelectedItems.Clear();
					if (e.NewItems != null && e.NewItems.Count > 0)
						SelectedItems.Add (e.NewItems [0]);
				}

				if (e.NewItems != null || e.OldItems != null) {
					HashSet<object> newItems = (e.NewItems != null)
												? new HashSet<object> (e.NewItems.Cast<object> ())
												: new HashSet<object> ();

					HashSet<object> oldItems = (e.OldItems != null)
												? new HashSet<object> (e.OldItems.Cast<object> ())
												: new HashSet<object> ();

					TraverseTree ((o, ti) => {
						if (newItems.Remove (o))
							ti.IsSelected = true;
						else if (SelectionMode == SWC.SelectionMode.Single || oldItems.Remove (o))
							ti.IsSelected = false;

						return (newItems.Count + oldItems.Count > 0);
					});
				}
				break;
			}

			OnSelectedItemsChanged (EventArgs.Empty);

			if (!changeActive)
				IsSelectionChangeActiveProperty.SetValue (this, changeActive, null);
		}

		private static void OnSelectedItemsPropertyChanged (DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			var view = (ExTreeView) dependencyObject;
			view.OnSelectedItemsPropertyChanged (e);
		}

		private static void OnSelectionModePropertyChanged (DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			var view = (ExTreeView) dependencyObject;
			view.OnSelectionModeChanged (e);
		}

		public void SelectItem(object item)
		{
			SelectItem(ContainerFromObject(item));
		}

		ExTreeViewItem shiftStart = null;
		ExTreeViewItem shiftEnd = null;

		private bool ShiftPressed
		{
			get
			{
				return Keyboard.IsKeyDown(WKey.RightShift) || Keyboard.IsKeyDown(WKey.LeftShift);
			}
		}

		private bool CtrlPressed
		{
			get
			{
				return Keyboard.IsKeyDown(WKey.RightCtrl) || Keyboard.IsKeyDown(WKey.LeftCtrl);
			}
		}

		protected override void OnGotFocus(RoutedEventArgs e)
		{
			base.OnGotFocus(e);
			FocusItem(focusedTreeViewItem);
		}

		public void SelectItem(ExTreeViewItem item)
		{
			if (item == null)
				return;

			FocusItem(item);
			if (!CtrlPressed)
				SelectedItems.Clear();
			if (ShiftPressed)
			{
				if (shiftStart == null)
					shiftStart = item;
				if (shiftEnd != null)//Erase previous selection of shift
					foreach (var forEachItem in GetItemsBetween(shiftStart, shiftEnd))
						SelectedItems.Remove(forEachItem.DataContext);
				shiftEnd = item;
				if (this.SelectionMode == SWC.SelectionMode.Single) {
					SelectedItems.Add(item.DataContext);
				} else {
					foreach (var forEachItem in GetItemsBetween(shiftStart, shiftEnd))
						SelectedItems.Add(forEachItem.DataContext);
				}
			}
			else
			{
				shiftEnd = null;
				shiftStart = item;
				if (CtrlPressed && SelectedItems.Contains(item.DataContext))
					SelectedItems.Remove(item.DataContext);
				else
					SelectedItems.Add(item.DataContext);
			}
		}

		private IEnumerable<ExTreeViewItem> GetAllVisibleItems(ItemsControl itemsControl)
		{
			foreach (var item in itemsControl.Items)
			{
				ExTreeViewItem treeViewItem = (ExTreeViewItem)itemsControl.ItemContainerGenerator.ContainerFromItem(item);
				if (treeViewItem == null)
					continue;
				if (treeViewItem.IsVisible && treeViewItem.Visibility == Visibility.Visible)
				{
					yield return treeViewItem;
					if (treeViewItem.IsExpanded)
						foreach (var childItem in GetAllVisibleItems(treeViewItem))
							yield return childItem;
				}
			}
		}

		private IEnumerable<ExTreeViewItem> GetItemsBetween(ExTreeViewItem startItem, ExTreeViewItem endItem)
		{
			var allVisibleItems = GetAllVisibleItems(this).ToList();
			var startIndex = allVisibleItems.IndexOf(startItem);
			var endIndex = allVisibleItems.IndexOf(endItem);

			// Handle empty selection
			if (startIndex == -1)
				startIndex = 0;

			if (endIndex == startIndex)
				return new List<ExTreeViewItem> { endItem };
			var filteredItems = new List<ExTreeViewItem>();
			if (endIndex > startIndex)
				for (int i = startIndex; i <= endIndex; i++)
					filteredItems.Add(allVisibleItems[i]);
			else
				for (int i = startIndex; i >= endIndex; i--)
					filteredItems.Add(allVisibleItems[i]);
			return filteredItems;
		}

		private ExTreeViewItem focusedTreeViewItem = null;
		public ExTreeViewItem FocusedItem {
			get {
				return focusedTreeViewItem;
			}
			set {
				FocusItem (value);
			}
		}

		protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
		{
			e.Handled = true;
			if (e.Key == WKey.Down)
			{
				if (CtrlPressed && !ShiftPressed)
				{
					FocusItem(GetNextItem(focusedTreeViewItem));
				}
				else
				{
					SelectItem(GetNextItem(focusedTreeViewItem));
				}
			}
			else if (e.Key == WKey.Up)
			{
				if (CtrlPressed && !ShiftPressed)
				{
					FocusItem(GetPrevItem(focusedTreeViewItem));
				}
				else
				{
					SelectItem(GetPrevItem(focusedTreeViewItem));
				}
			}
			else if (e.Key == WKey.Space)
			{
				SelectItem(focusedTreeViewItem);
			}
			else if (e.Key == WKey.Right && !CtrlPressed)
			{
				focusedTreeViewItem.IsExpanded = true;
			}
			else if (e.Key == WKey.Left && !CtrlPressed)
			{
				focusedTreeViewItem.IsExpanded = false;
			}
			else
			{
				e.Handled = false;
			}
			base.OnKeyDown(e);
		}

		private void FocusItem(ExTreeViewItem exTreeViewItem)
		{
			if (exTreeViewItem != null)
			{
				focusedTreeViewItem = exTreeViewItem;
				exTreeViewItem.Focus();
			}
		}

		private ExTreeViewItem GetPrevItem(ExTreeViewItem p)
		{
			var items = GetAllVisibleItems(this).ToList();
			int indexOfP = items.IndexOf(p);
			if (indexOfP == 0)
				return p;
			else if (indexOfP == -1) {
				if (items.Count > 0)
					return items [items.Count - 1];

				return null;
 			} else
				return items[indexOfP - 1];
		}

		private ExTreeViewItem GetNextItem(ExTreeViewItem p)
		{
			var items = GetAllVisibleItems(this).ToList();
			int indexOfP = items.IndexOf(p);
			if (indexOfP + 1 == items.Count)
				return p;
			else
				return items[indexOfP + 1];
		}

		protected override AutomationPeer OnCreateAutomationPeer ()
		{
			var backend = this.Backend as TreeViewBackend;
			var treeView = backend?.Frontend as Xwt.TreeView;

			var peer = treeView?.Accessible.GetChildren ()?.FirstOrDefault () as AutomationPeer;
			if (peer != null)
				return peer;

			return base.OnCreateAutomationPeer ();
		}

		internal AutomationPeer CreateChildAutomationPeer (object itemData, TreeViewItem itemView)
		{
			var backend = this.Backend as TreeViewBackend;
			var treeView = backend?.Frontend as Xwt.TreeView;

			var peer = treeView?.Accessible.GetChildren ()?.FirstOrDefault () as ExTreeViewAutomationPeer;
			if (peer != null)
				return peer.CreateChildAutomationPeer (itemData, itemView);

			return null;
		}
	}

	public abstract class ExTreeViewAutomationPeer: TreeViewAutomationPeer
	{
		public ExTreeViewAutomationPeer (SWC.TreeView owner): base (owner)
		{
		}

		public abstract TreeViewItemAutomationPeer CreateChildAutomationPeer (object item, TreeViewItem viewItem);
	}
}