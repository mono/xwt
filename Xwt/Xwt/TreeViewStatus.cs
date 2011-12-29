// 
// TreeViewStatus.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2011 Xamarin Inc
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

namespace Xwt
{
	public class TreeViewStatus
	{
		TreeView tree;
		int idColumn;
		List<NodeInfo> state;
		
		class NodeInfo {
			public object Id;
			public bool Expanded;
			public bool Selected;
			public List<NodeInfo> ChildInfo;
		}
		
		internal TreeViewStatus (TreeView tree, int idColumn)
		{
			this.tree = tree;
			this.idColumn = idColumn;
			Save ();
		}
		
		void Save ()
		{
			state = new List<NodeInfo> ();
			SaveChildren (state, null);
		}
		
		void SaveChildren (List<NodeInfo> info, TreePosition it)
		{
			int num = tree.DataSource.GetChildrenCount (it);
			for (int n=0; n<num; n++) {
				var child = tree.DataSource.GetChild (it, n);
				object id = tree.DataSource.GetValue (child, idColumn);
				NodeInfo ni = new NodeInfo ();
				ni.Id = id;
				ni.Expanded = tree.IsRowExpanded (child);
				ni.Selected = tree.IsRowSelected (child);
				info.Add (ni);
				if (tree.DataSource.GetChildrenCount (child) > 0) {
					ni.ChildInfo = new List<NodeInfo> ();
					SaveChildren (ni.ChildInfo, child);
				}
			}
		}
		
		internal void Load (TreeView tree)
		{
			if (this.tree != tree)
				throw new InvalidOperationException ("Invalid tree instance. The status can only be restored on the tree that generated the status object.");
			Load (state, null);
		}
		
		void Load (List<NodeInfo> info, TreePosition it)
		{
			bool oneSelected = false;
			var infoCopy = new List<NodeInfo> (info);
			Dictionary<NodeInfo,TreePosition> nodes = new Dictionary<NodeInfo, TreePosition> ();
			
			int num = tree.DataSource.GetChildrenCount (it);
			for (int n=0; n<num; n++) {
				var child = tree.DataSource.GetChild (it, n);
				object id = tree.DataSource.GetValue (child, idColumn);
				NodeInfo ni = ExtractNodeInfo (info, id);
				if (ni != null) {
					nodes [ni] = child;
					if (ni.Expanded)
						tree.ExpandRow (child, false);
					else
						tree.CollapseRow (child);
					
					if (ni.Selected) {
						oneSelected = true;
						tree.SelectRow (child);
					}
					else
						tree.UnselectRow (child);
					
					if (ni.ChildInfo != null)
						Load (ni.ChildInfo, child);
				}
			}
			
			// If this tree level had a selected node and this node has been deleted, then
			// try to select and adjacent node
			if (!oneSelected) {
				// 'info' contains the nodes that have not been inserted
				if (info.Any (n => n.Selected)) {
					NodeInfo an = FindAdjacentNode (infoCopy, nodes, info[0]);
					if (an != null) {
						it = nodes [an];
						tree.SelectRow (it);
					}
				}
			}
		}
		
		NodeInfo FindAdjacentNode (List<NodeInfo> infos, Dictionary<NodeInfo,TreePosition> nodes, NodeInfo referenceNode)
		{
			int i = infos.IndexOf (referenceNode);
			for (int n=i; n<infos.Count; n++) {
				if (nodes.ContainsKey (infos[n]))
					return infos[n];
			}
			for (int n=i-1; n>=0; n--) {
				if (nodes.ContainsKey (infos[n]))
					return infos[n];
			}
			return null;
		}
		
		NodeInfo ExtractNodeInfo (List<NodeInfo> info, object id)
		{
			for (int n=0; n<info.Count; n++) {
				NodeInfo ni = (NodeInfo) info [n];
				if (object.Equals (ni.Id, id)) {
					info.RemoveAt (n);
					return ni;
				}
			}
			return null;
		}
	}
}

