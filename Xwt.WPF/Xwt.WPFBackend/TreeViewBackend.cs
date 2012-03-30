// 
// TreeViewBackend.cs
//  
// Author:
//       Luís Reis <luiscubal@gmail.com>
// 
// Copyright (c) 2011 Luís Reis
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
using Xwt.Engine;
using Xwt.Backends;
using Xwt.WPFBackend.Utilities;
using System.Windows;
using SWC=System.Windows.Controls;
using System.Windows.Controls;

namespace Xwt.WPFBackend
{
	public class TreeViewBackend : WidgetBackend, ITreeViewBackend
	{
		private IList<ListViewColumn> columns = new List<ListViewColumn> ();

		protected new ITreeViewEventSink EventSink
		{
			get { return (ITreeViewEventSink)base.EventSink; }
		}

		public TreeViewBackend ()
		{
			Tree = new WpfTreeView ();
		}

		protected override double DefaultNaturalHeight {
			get { return -1; }
		}

		protected override double DefaultNaturalWidth {
			get { return -1; }
		}

		public SWC.TreeView Tree
		{
			get
			{
				return (SWC.TreeView)Widget;
			}
			set
			{
				Widget = value;
			}
		}

		public void SetSource (ITreeDataSource source, IBackend sourceBackend)
		{
			Tree.Items.Clear ();
			TreeStoreBackend treeStore = sourceBackend as TreeStoreBackend;
			if (treeStore != null)
			{
				TreeNode node = treeStore.RootNode;
				node.TreeViewData.Add (Tuple.Create<TreeViewBackend, SWC.ItemsControl>(this, Tree));
				foreach (TreeNode child in node.Children)
				{
					Tree.Items.Add (GenerateTreeViewItem (child));
				}
			}
		}
		
		// TODO
		public Xwt.ScrollPolicy VerticalScrollPolicy
		{
			get { return Xwt.ScrollPolicy.Automatic; }
			set {  }
		}

		// TODO
		public Xwt.ScrollPolicy HorizontalScrollPolicy
		{
			get { return Xwt.ScrollPolicy.Automatic; }
			set {  }
		}

		internal MultiColumnTreeViewItem GenerateTreeViewItem (TreeNode node)
		{
			MultiColumnTreeViewItem item = new MultiColumnTreeViewItem (this, node);

			foreach (ListViewColumn column in columns)
			{
				item.AddColumn (column);
			}

			foreach (TreeNode child in node.Children)
			{
				item.Items.Add (GenerateTreeViewItem (child));
			}

			return item;
		}

		public TreePosition[] SelectedRows
		{
			get
			{
				List<TreePosition> Positions = new List<TreePosition> ();
				foreach (SWC.TreeViewItem item in Tree.Items)
				{
					MultiColumnTreeViewItem multiColumnItem = (MultiColumnTreeViewItem)item;
					multiColumnItem.AppendSelections (Positions);
				}
				return Positions.ToArray ();
			}
		}

		// TODO
		public bool HeadersVisible
		{
			get;
			set;
		}

		public void SelectRow (TreePosition pos)
		{
			// TODO
		}

		public void UnselectRow (TreePosition pos)
		{
			// TODO
		}

		public bool IsRowSelected (TreePosition row)
		{
			// TODO
			return false;
		}

		public bool IsRowExpanded (TreePosition row)
		{
			// TODO
			return false;
		}

		public void ExpandToRow (TreePosition pos)
		{
			// TODO
		}

		public bool GetDropTargetRow (double x, double y, out RowDropPosition pos, out TreePosition nodePosition)
		{
			throw new NotImplementedException();
		}

		public void ExpandRow (TreePosition row, bool expandChildren)
		{
			// TODO
		}

		public void CollapseRow (TreePosition row)
		{
			// TODO
		}

		public void ScrollToRow (TreePosition pos)
		{
			// TODO
		}

		public void UnselectAll ()
		{
			// TODO
		}

		public void SetSelectionMode (SelectionMode mode)
		{
			// TODO
		}

		public void SelectAll ()
		{
			// TODO
		}

		public void UpdateColumn (ListViewColumn column, object handle, ListViewColumnChange change)
		{
			throw new NotImplementedException ();
		}

		public void RemoveColumn (ListViewColumn column, object handle)
		{
			throw new NotImplementedException ();
		}

		public object AddColumn (ListViewColumn column)
		{
			columns.Add (column);

			foreach (SWC.TreeViewItem item in Tree.Items)
				((MultiColumnTreeViewItem)item).AddColumn (column);

			return column;
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is TableViewEvent)
			{
				if (((TableViewEvent)eventId) == TableViewEvent.SelectionChanged)
					Tree.SelectedItemChanged += HandleWidgetSelectionChanged;
			}
		}

		void HandleWidgetSelectionChanged (object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
		{
			Toolkit.Invoke (delegate
			{
				EventSink.OnSelectionChanged ();
			});
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is TableViewEvent)
			{
				if (((TableViewEvent)eventId) == TableViewEvent.SelectionChanged)
					Tree.SelectedItemChanged -= HandleWidgetSelectionChanged;
			}
		}
	}

	class WpfTreeView : SWC.TreeView, IWpfWidget
	{
		public WidgetBackend Backend { get; set; }

		protected override System.Windows.Size MeasureOverride (System.Windows.Size constraint)
		{
			var s = base.MeasureOverride (constraint);
			if (ScrollViewer.GetHorizontalScrollBarVisibility (this) != ScrollBarVisibility.Hidden)
				s.Width = 0;
			if (ScrollViewer.GetVerticalScrollBarVisibility (this) != ScrollBarVisibility.Hidden)
				s.Height = SystemParameters.CaptionHeight;
			s = Backend.MeasureOverride (constraint, s);
			return Backend.MeasureOverride (constraint, s);
		}
	}
}
