//
// TreeViewBackend.cs
//
// Author:
//       Eric Maupin <ermau@xamarin.com>
//
// Copyright (c) 2012 Eric Maupin
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;
using Xwt.Engine;
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
		}
		
		public ScrollPolicy VerticalScrollPolicy
		{
			get { return ScrollViewer.GetVerticalScrollBarVisibility (Tree).ToXwtScrollPolicy (); }
			set { ScrollViewer.SetVerticalScrollBarVisibility (Tree, value.ToWpfScrollBarVisibility ()); }
		}

		public ScrollPolicy HorizontalScrollPolicy
		{
			get { return ScrollViewer.GetHorizontalScrollBarVisibility (Tree).ToXwtScrollPolicy (); }
			set { ScrollViewer.SetHorizontalScrollBarVisibility (Tree, value.ToWpfScrollBarVisibility ()); }
		}

		public TreePosition[] SelectedRows
		{
			get { return Tree.SelectedItems.Cast<TreePosition> ().ToArray (); }
		}

		// TODO
		public bool HeadersVisible
		{
			get;
			set;
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
			GetVisibleTreeItem (pos).BringIntoView();
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
			UpdateColumn (column, col, ListViewColumnChange.Cells);

			this.columns.Add (column);

			UpdateTemplate ();

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
				//col.CellTemplate = new DataTemplate
				//    { VisualTree = CellUtil.CreateBoundColumnTemplate (column.Views, "Values") };
				break;
			}
		}

		public void RemoveColumn (ListViewColumn column, object handle)
		{
			this.columns.Remove (column);

			UpdateTemplate ();
		}

		public bool GetDropTargetRow (double x, double y, out RowDropPosition pos, out TreePosition nodePosition)
		{
			pos = default(RowDropPosition);
			nodePosition = default(TreePosition);
			// TODO
			return false;
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is TableViewEvent)
			{
				switch ((TableViewEvent)eventId) {
				case TableViewEvent.SelectionChanged:
					Tree.SelectedItemsChanged += OnSelectedItemsChanged;
					break;
				}
			}
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is TableViewEvent)
			{
				switch ((TableViewEvent)eventId) {
				case TableViewEvent.SelectionChanged:
					Tree.SelectedItemsChanged -= OnSelectedItemsChanged;
					break;
				}
			}
		}

		private readonly List<ListViewColumn> columns = new List<ListViewColumn> ();

		protected ExTreeView Tree
		{
			get { return (ExTreeView)Widget; }
			set { Widget = value; }
		}

		protected ITreeViewEventSink TreeViewEventSink
		{
			get { return (ITreeViewEventSink)EventSink; }
		}

		protected override double DefaultNaturalHeight {
			get { return -1; }
		}

		protected override double DefaultNaturalWidth {
			get { return -1; }
		}

		private void OnSelectedItemsChanged (object sender, EventArgs e)
		{
			Toolkit.Invoke (TreeViewEventSink.OnSelectionChanged);
		}

		private ExTreeViewItem GetVisibleTreeItem (TreePosition pos, Action<ExTreeViewItem> walk = null)
		{
			Stack<TreeStoreNode> nodes = new Stack<TreeStoreNode> ();

			TreeStoreNode node = (TreeStoreNode) pos;
			do {
				nodes.Push (node);
				node = node.Parent;
			} while (node != null);

			ExTreeViewItem treeItem = null;
			ItemContainerGenerator g = this.Tree.ItemContainerGenerator;
			while (nodes.Count > 0) {
				node = nodes.Pop ();
				treeItem = (ExTreeViewItem) g.ContainerFromItem (node);
				treeItem.UpdateLayout ();
				g = treeItem.ItemContainerGenerator;

				if (walk != null)
					walk (treeItem);
			}

			return treeItem;
		}

		private void UpdateTemplate()
		{
			// Multi-column currently isn't supported

			if (this.columns.Count == 0) {
				Tree.ItemTemplate = null;
				return;
			}

			Tree.ItemTemplate = new HierarchicalDataTemplate {
				VisualTree = CellUtil.CreateBoundColumnTemplate (this.columns[0].Views),
				ItemsSource = new Binding ("Children")
			};
		}
	}
}
