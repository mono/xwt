//
// ExTreeViewItem.cs
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SWC = System.Windows.Controls;
using WKey = System.Windows.Input.Key;

namespace Xwt.WPFBackend
{
	public class ExTreeViewItem
		: TreeViewItem
	{
		public ExTreeViewItem()
		{
		}

		public ExTreeViewItem (ExTreeView view)
		{
			this.view = view;
		}

		public int Level {
			get {
				if (this.level == -1) {
					ExTreeViewItem parent = ItemsControlFromItemContainer (this) as ExTreeViewItem;
					this.level = (parent != null) ? parent.Level + 1 : 0;
				}

				return this.level;
			}
		}

		public void SelectChildren (Func<object, ExTreeViewItem, bool> selector)
		{
			TreeView.SelectedItems.Add (DataContext);

			foreach (var item in Items) {
				var treeItem = GetTreeItem (item);
				if (selector (item, treeItem))
					treeItem.SelectChildren (selector);
			}
		}

		public void UnselectChildren (Func<object, ExTreeViewItem, bool> selector)
		{
			TreeView.SelectedItems.Remove (DataContext);

			foreach (var item in Items) {
				var treeItem = GetTreeItem (item);
				if (selector (item, treeItem))
					treeItem.UnselectChildren (selector);
			}
		}

		private int level = -1;
		private ExTreeView view;

		protected override DependencyObject GetContainerForItemOverride()
		{
			return new ExTreeViewItem (this.view);
		}

		protected override void OnMouseLeftButtonDown (MouseButtonEventArgs e)
		{
			bool add = true;

			if (TreeView.SelectedItems.Count > 0 && this.view.SelectionMode == SWC.SelectionMode.Extended) {
				if (!(Keyboard.IsKeyDown (WKey.LeftCtrl) || Keyboard.IsKeyDown (WKey.RightCtrl))) {
					if (Keyboard.IsKeyDown (WKey.LeftShift)) {
						var lastSelected = TreeView.ContainerFromObject (TreeView.SelectedItems [TreeView.SelectedItems.Count - 1]);

						TreeView.SelectedItems.Clear();

						ExTreeViewItem top, bottom;
						if (Level == lastSelected.Level) {
							top = this;
							bottom = lastSelected;
						} else {
							top = (Level < lastSelected.Level) ? this : lastSelected;
							bottom = (Level > lastSelected.Level) ? this : lastSelected;
						}

						var itemControl = ItemsControlFromItemContainer (top);
						var items = itemControl.Items;
						var g = itemControl.ItemContainerGenerator;

						ExTreeViewItem currentTop = top;
						ExTreeViewItem currentBottom = FindSameLevelParent (bottom, top.Level);

						bool adding = false;
						for (int i = 0; i < items.Count; ++i) {
						    object item = items [i];
						    TreeViewItem container = (TreeViewItem)g.ContainerFromItem (item);

						    bool endpoint = (container == currentTop || container == currentBottom);

						    if (endpoint) {
								if (currentTop != currentBottom)
						    		adding = !adding;
						    }
						    else if (!adding)
						        continue;
							
						    TreeView.SelectedItems.Add (item);

						    if (endpoint && !adding) {
								if (bottom != currentBottom) {
									i = -1;

									items = currentBottom.Items;
									g = currentBottom.ItemContainerGenerator;
									currentTop = (ExTreeViewItem)g.ContainerFromItem (items [0]);
									currentBottom = FindSameLevelParent (bottom, currentBottom.Level + 1);
								}
								else
						    		break;
						    }
						}
					}
					else
						TreeView.SelectedItems.Clear ();
				} else if (IsSelected)
					add = false;
			}

			if (add)
				TreeView.SelectedItems.Add (DataContext);
			else
				TreeView.SelectedItems.Remove (DataContext);

			e.Handled = true;
			base.OnMouseLeftButtonDown(e);
		}

		private ExTreeView TreeView
		{
			get
			{
				if (this.view == null)
					FindParent();

				return this.view;
			}
		}

		private ExTreeViewItem FindSameLevelParent (ExTreeViewItem item, int parentLevel)
		{
			while (item.Level != parentLevel) {
				item = ItemsControlFromItemContainer (item) as ExTreeViewItem;
				if (item == null)
					return null;
			}

			return item;
		}

		private void FindParent()
		{
			FrameworkElement e = this;
			while (this.view == null && e != null) {
				this.view = e.Parent as ExTreeView;
				e = (FrameworkElement)e.Parent;
			}
		}

		private ExTreeViewItem GetTreeItem (object item)
		{
			return item as ExTreeViewItem ?? (ExTreeViewItem) TreeView.ItemContainerGenerator.ContainerFromItem (item);
		}
	}
}
