using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using SWC=System.Windows.Controls;

namespace Xwt.WPFBackend.Utilities
{
	public class MultiColumnTreeViewItem : SWC.TreeViewItem
	{
		internal TreeViewBackend TreeView { get; private set; }
		internal TreeNode Node { get; private set; }
		public SWC.DockPanel DockPanel;
		private IList<CellView> cellViews = new List<CellView> ();

		internal MultiColumnTreeViewItem (TreeViewBackend treeView, TreeNode node)
		{
			Node = node;
			TreeView = treeView;
			(Node as TreeNode).TreeViewData.Add (Tuple.Create<TreeViewBackend, SWC.ItemsControl>(TreeView, this));
			Header = DockPanel = new SWC.DockPanel ();
		}

		public void AddColumn (ListViewColumn column)
		{
			foreach (CellView view in column.Views)
			{
				cellViews.Add (view);
				AddControlForView (view);
			}
		}

		private void AddControlForView (CellView view)
		{
			FrameworkElement elem = CellUtil.CreateCellRenderer (Node, view);
			if (elem == null)
				return;
			
			DockPanel.Children.Add (elem);
			SWC.DockPanel.SetDock (elem, SWC.Dock.Left);
		}

		public void UpdateColumn (int column, object newValue)
		{
			DockPanel.Children.Clear ();

			foreach (CellView view in cellViews)
				AddControlForView (view);
		}

		public void AppendSelections (IList<TreePosition> positions)
		{
			if (IsSelected)
				positions.Add (Node);
			foreach (SWC.TreeViewItem item in Items)
			{
				MultiColumnTreeViewItem multiColumnItem = (MultiColumnTreeViewItem)item;
				multiColumnItem.AppendSelections (positions);
			}
		}
	}
}
