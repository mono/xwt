//
// TreeViewCustomStore.cs
//
// Author:
//       Lluis Sanchez <llsan@microsoft.com>
//
// Copyright (c) 2018 Microsoft
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
using Xwt;
using Xwt.Drawing;

namespace Samples
{
	public class TreeViewCustomStore: VBox
	{
		public TreeViewCustomStore ()
		{
			DataNode root = new DataNode ("Root",
				new DataNode [] {
					new DataNode ("One", new DataNode [] {
						new DataNode ("Elephant"),
						new DataNode ("Tiger")
					}),
					new DataNode ("Two", new DataNode [] {
						new DataNode ("Monkey"),
						new DataNode ("Lion")
					}),
				}
			);
			DataNode root2 = new DataNode ("Second Root",
				new DataNode [] {
					new DataNode ("One", new DataNode [] {
						new DataNode ("Elephant"),
						new DataNode ("Tiger")
					}),
					new DataNode ("Two", new DataNode [] {
						new DataNode ("Monkey"),
						new DataNode ("Lion")
					}),
				}
			);

			MyTreeSource source = new MyTreeSource ();
			source.Add (root);
			source.Add (root2);
			source.Add (new InfiniteDataNode ("Infinite"));

			TreeView tree = new TreeView ();

			var col = new ListViewColumn ("Data");
			col.Views.Add (new ImageCellView (source.ImageField));
			col.Views.Add (new TextCellView (source.LabelField));
			tree.Columns.Add (col);

			PackStart (tree, true);

			var hbox = new HBox ();
			var add = new Button ("Add nodes");
			var change = new Button ("Change nodes");
			var remove = new Button ("Remove nodes");

			hbox.PackStart (add);
			hbox.PackStart (change);
			hbox.PackStart (remove);
			PackStart (hbox);

			add.Clicked += delegate {
				var node = new DataNode ("New child");
				root.InsertChild (1, node);
				tree.SelectRow (node);
			};

			remove.Clicked += delegate {
				if (root.Children.Count > 0)
					root.RemoveChild (root.Children [root.Children.Count - 1]);
			};

			change.Clicked += delegate {
				var sel = (DataNode)tree.SelectedRow;
				if (sel != null)
					sel.Name = "Modified";
			};

			tree.DataSource = source;
		}
	}

	class DataNode: TreePosition
	{
		protected List<DataNode> children;
		public DataNode Parent { get; set; }
		public virtual List<DataNode> Children => children;
		string name;

		public string Name {
			get {
				return name;
			}

			set {
				name = value;
				GetEventSink ()?.NotifyNodeChanged (this, -1);
			}
		}
		Image image;

		public Image Image {
			get {
				return image;
			}

			set {
				image = value;
				GetEventSink ()?.NotifyNodeChanged (this, -1);
			}
		}

		public IEventSink EventSink { get; set; }

		IEventSink GetEventSink ()
		{
			return EventSink ?? Parent?.GetEventSink ();
		}

		public DataNode (string name, params DataNode[] children)
		{
			Name = name;
			if (children.Length > 0) {
				this.children = children.ToList ();
				foreach (var c in children)
					c.Parent = this;
			}
		}

		public void AddChild (DataNode node)
		{
			if (children == null)
				children = new List<DataNode> ();
			Children.Add (node);
			node.Parent = this;
			GetEventSink ()?.NotifyNodeAdded (node, Children.Count - 1);
		}

		public void InsertChild (int index, DataNode node)
		{
			if (children == null)
				children = new List<DataNode> ();
			Children.Insert (index, node);
			node.Parent = this;
			GetEventSink ()?.NotifyNodeAdded (node, index);
		}

		public void RemoveChild (DataNode node)
		{
			if (children != null) {
				int i = children.IndexOf (node);
				if (i != -1) {
					children.RemoveAt (i);
					GetEventSink ()?.NotifyNodeRemoved (this, i, node);
				}
			}
		}

        public override string ToString()
        {
			return "[" + Name + "]";
        }
    }

	class InfiniteDataNode: DataNode
	{
		public InfiniteDataNode (string name, params DataNode [] children) : base (name, children)
		{
			
		}

        public override List<DataNode> Children {
			get {
				if (children == null) {
					AddChild (new InfiniteDataNode ("Child 1"));
					AddChild (new InfiniteDataNode ("Child 2"));
				}
				return base.Children;
			}
		}
    }

	interface IEventSink
	{
		void NotifyNodeAdded (DataNode node, int childIndex);
		void NotifyNodeRemoved (TreePosition parent, int childIndex, TreePosition child);
		void NotifyNodeChanged (DataNode node, int childIndex);
	}

	class MyTreeSource : ITreeDataSource, IEventSink
	{
		public DataField<string> LabelField = new DataField<string> (0);
		public DataField<Image> ImageField = new DataField<Image> (1);

		List<DataNode> data = new List<DataNode> ();

		public Type [] ColumnTypes => new [] { typeof (string), typeof (Image) };

		public event EventHandler<TreeNodeEventArgs> NodeInserted;
		public event EventHandler<TreeNodeChildEventArgs> NodeDeleted;
		public event EventHandler<TreeNodeEventArgs> NodeChanged;
		public event EventHandler<TreeNodeOrderEventArgs> NodesReordered;
		public event EventHandler Cleared;

		public void Add (DataNode node)
		{
			data.Add (node);
			node.EventSink = this;
		}

		public TreePosition GetChild (TreePosition pos, int index)
		{
			var node = (DataNode)pos;
			var list = node != null ? node.Children : data;
			if (list == null || index >= list.Count)
				return null;
			return list [index];
		}

		public int GetChildrenCount (TreePosition pos)
		{
			var node = (DataNode)pos;
			var list = node != null ? node.Children : data;
			return list != null ? list.Count : 0;
		}

		public TreePosition GetParent (TreePosition pos)
		{
			var node = (DataNode)pos;
			return node?.Parent;
		}

		public object GetValue (TreePosition pos, int column)
		{
			var node = (DataNode)pos;
			if (column == 0)
				return node.Name;
			if (column == 1)
				return node.Image;
			return null;
		}

		public void SetValue (TreePosition pos, int column, object value)
		{
			var node = (DataNode)pos;
			if (column == 0)
				node.Name = (string) value;
			if (column == 1)
				node.Image = (Image) value;
		}

		void IEventSink.NotifyNodeAdded (DataNode node, int index)
		{
			NodeInserted?.Invoke (this, new TreeNodeEventArgs (node, index));
		}

		void IEventSink.NotifyNodeChanged (DataNode node, int index)
		{
			NodeChanged?.Invoke (this, new TreeNodeEventArgs (node, index));
		}

		void IEventSink.NotifyNodeRemoved (TreePosition parent, int childIndex, TreePosition child)
		{
			NodeDeleted?.Invoke (this, new TreeNodeChildEventArgs (parent, childIndex, child));
		}
	}
}
