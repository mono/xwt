//
// ExTreeViewItem.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using SWC = System.Windows.Controls;
using WKey = System.Windows.Input.Key;
using Xwt.Backends;

namespace Xwt.WPFBackend
{
	using Keyboard = System.Windows.Input.Keyboard;

	public class ExTreeViewItem
		: TreeViewItem
	{
		public ExTreeViewItem()
		{
			Loaded += OnLoaded;
			HorizontalContentAlignment = HorizontalAlignment.Stretch;
		}

		public ExTreeViewItem (ExTreeView view)
			: this()
		{
			this.view = view;
		}

		protected override void OnExpanded (RoutedEventArgs e)
		{
			if (!(DataContext is TreeStoreNode))
				return;

			var node = (TreeStoreNode)DataContext;
			view.Backend.Context.InvokeUserCode (delegate {
				((ITreeViewEventSink)view.Backend.EventSink).OnRowExpanding (node);
			});

			base.OnExpanded (e);

			view.Backend.Context.InvokeUserCode (delegate {
				((ITreeViewEventSink)view.Backend.EventSink).OnRowExpanded (node);
			});
		}

		protected override void OnCollapsed(RoutedEventArgs e)
		{
			if (!(DataContext is TreeStoreNode))
				return;

			var node = (TreeStoreNode)DataContext;
			if (!IsExpanded)
				UnselectChildren((object o, ExTreeViewItem i) =>
				{
					return i != this;
				});
			view.Backend.Context.InvokeUserCode (delegate {
				((ITreeViewEventSink)view.Backend.EventSink).OnRowCollapsing (node);
			});
			base.OnCollapsed(e);
			view.Backend.Context.InvokeUserCode (delegate {
				((ITreeViewEventSink)view.Backend.EventSink).OnRowCollapsed (node);
			});
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
			if (selector(this.DataContext, this))
				TreeView.SelectedItems.Add (DataContext);

			foreach (var item in Items) {
				var treeItem = (ExTreeViewItem)ItemContainerGenerator.ContainerFromItem(item);
				if (treeItem != null && selector (item, treeItem))
					treeItem.SelectChildren (selector);
			}
		}

		public void UnselectChildren (Func<object, ExTreeViewItem, bool> selector)
		{
			if (selector(this.DataContext, this))
				TreeView.SelectedItems.Remove(DataContext);

			foreach (var item in Items) {
				var treeItem = (ExTreeViewItem)ItemContainerGenerator.ContainerFromItem(item);
				if (treeItem != null && selector (item, treeItem))
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
			var args = e.ToXwtButtonArgs (view.Backend.Widget);
			view.Backend.Context.InvokeUserCode (delegate () {
				view.Backend.EventSink.OnButtonPressed (args);
			});
			if (args.Handled) {
				e.Handled = true;
				return;
			}

			if (!view.SelectedItems.Contains (this.DataContext) || CtrlPressed)
				view.SelectItem (this);
			view.Backend.WidgetMouseDownForDragHandler (this, e);

			e.Handled = true;
			base.OnMouseLeftButtonDown (e);
		}

		protected override void OnMouseLeftButtonUp (MouseButtonEventArgs e)
		{
			var args = e.ToXwtButtonArgs (view.Backend.Widget);
			view.Backend.Context.InvokeUserCode (delegate () {
				view.Backend.EventSink.OnButtonReleased (args);
			});
			if (args.Handled) {
				e.Handled = true;
				return;
			}

			if (view.SelectedItems.Contains (this.DataContext) && !CtrlPressed)
				view.SelectItem (this);
			view.Backend.WidgetMouseUpForDragHandler(this, e);

			e.Handled = true;
			base.OnMouseLeftButtonUp (e);
		}

		protected override void OnMouseDoubleClick (MouseButtonEventArgs e)
		{
			if ((view.Backend as TreeViewBackend)?.RowActivatedEventEnabled == true && IsSelected)
			{
				var node = (TreeStoreNode)DataContext;
				view.Backend.Context.InvokeUserCode(delegate
				{
					((ITreeViewEventSink)view.Backend.EventSink).OnRowActivated(node);
				});
			}
			base.OnMouseDoubleClick(e);
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


		private void FindParent()
		{
			FrameworkElement e = this;
			while (this.view == null && e != null) {
				this.view = e.Parent as ExTreeView;
				e = (FrameworkElement)e.Parent;
			}
		}

		private void OnLoaded (object sender, RoutedEventArgs routedEventArgs)
		{
			ItemsControl parent = ItemsControlFromItemContainer (this);
			if (parent == null)
				return;

			int index = parent.Items.IndexOf (DataContext);
			if (index != parent.Items.Count - 1)
				return;

			foreach (var column in this.view.View.Columns) {
				if (Double.IsNaN (column.Width))
					column.Width = column.ActualWidth;

				column.Width = Double.NaN;
			}
		}

		protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
		{
			//We can't allow TreeViewItem(our base class) to get this message(OnKeyDown) because it will mess with our ExTreeView handling
		}

		protected override void OnGotFocus(RoutedEventArgs e)
		{
			BringIntoView();
			//We can't allow TreeViewItem(our base class) to get this message(OnGotFocus) because it will also select this item which we don't want
		}

		private bool CtrlPressed
		{
			get
			{
				return Keyboard.IsKeyDown (WKey.RightCtrl) || Keyboard.IsKeyDown (WKey.LeftCtrl);
			}
		}

		protected override AutomationPeer OnCreateAutomationPeer ()
		{
			return new ExTreeViewItemAutomationPeer (this);
		}

		class ExTreeViewItemAutomationPeer : TreeViewItemAutomationPeer
		{
			public ExTreeViewItemAutomationPeer (ExTreeViewItem owner) : base (owner)
			{
			}

			protected override List<AutomationPeer> GetChildrenCore ()
			{
				List<AutomationPeer> defaultChildren = base.GetChildrenCore ();
				if (defaultChildren == null)
					return null;

				// We only want to include TreeView items in the a11y tree, not their constituent image/text/etc controls -
				// for one thing including all controls messes up the "item 3 of 5" style counts announced by the
				// narrator, as those controls would be include
				List<AutomationPeer> children = defaultChildren.Where (
					child => child is TreeViewItemAutomationPeer || child is TreeViewDataItemAutomationPeer).ToList ();
				return children;
			}
		}
	}
}
