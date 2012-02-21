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
		internal TreeNode Node { get; private set; }
		public SWC.DockPanel DockPanel;

		internal MultiColumnTreeViewItem (TreeNode node)
		{
			Node = node;
			Header = DockPanel = new SWC.DockPanel ();
		}

		public void AddColumn (ListViewColumn column)
		{
			foreach (CellView view in column.Views)
			{
				FrameworkElement elem = CellUtil.CreateCellRenderer (Node, view);
				DockPanel.Children.Add (elem);
				SWC.DockPanel.SetDock (elem, SWC.Dock.Left);
			}
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
