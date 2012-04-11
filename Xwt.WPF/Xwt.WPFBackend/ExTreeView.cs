//
// ExTreeView.cs
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
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using SWC = System.Windows.Controls;

namespace Xwt.WPFBackend
{
	public class ExTreeView
		: SWC.TreeView, IWpfWidget
	{
		public ExTreeView()
		{
			SelectedItems = new ObservableCollection<object> ();
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

		private bool TraverseTree (Func<object, ExTreeViewItem, bool> action, ExTreeViewItem parent = null)
		{
			ItemContainerGenerator g = ItemContainerGenerator;
			IEnumerable items = Items;
			if (parent != null) {
				items = parent.Items;
				g = parent.ItemContainerGenerator;
			}

			foreach (object item in items) {
				var treeItem = (ExTreeViewItem) g.ContainerFromItem (item);
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
	}
}