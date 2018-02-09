// 
// ITreeViewSource.cs
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
using Xwt.Drawing;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Xwt
{
	public interface ITreeDataSource
	{
		TreePosition GetParent (TreePosition pos);
		TreePosition GetChild (TreePosition pos, int index);
		int GetChildrenCount (TreePosition pos);
		object GetValue (TreePosition pos, int column);
		void SetValue (TreePosition pos, int column, object value);
		Type[] ColumnTypes { get; }
		
		event EventHandler<TreeNodeEventArgs> NodeInserted;
		event EventHandler<TreeNodeChildEventArgs> NodeDeleted;
		event EventHandler<TreeNodeEventArgs> NodeChanged;
		event EventHandler<TreeNodeOrderEventArgs> NodesReordered;
		event EventHandler Cleared;
	}
	
	public class TreeNodeEventArgs: EventArgs
	{
		public TreeNodeEventArgs (TreePosition node): this (node, -1)
		{
		}

		public TreeNodeEventArgs (TreePosition node, int childIndex)
		{
			Node = node;
			ChildIndex = childIndex;
		}
		
		public TreePosition Node {
			get;
			private set;
		}

		public int ChildIndex {
			get;
			private set;
		}
	}
	
	public class TreeNodeChildEventArgs: TreeNodeEventArgs
	{
		[Obsolete ("Use the constructor that takes the child object")]
		public TreeNodeChildEventArgs (TreePosition parent, int childIndex): base (parent, childIndex)
		{
		}

#pragma warning disable CS0618 // Type or member is obsolete
		public TreeNodeChildEventArgs (TreePosition parent, int childIndex, TreePosition child): this (parent, childIndex)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			Child = child;
		}
		
		public TreePosition Child {
			get;
			private set;
		}
	}
	
	public class TreeNodeOrderEventArgs: TreeNodeEventArgs
	{
		public TreeNodeOrderEventArgs (TreePosition parentNode, int[] childrenOrder): base (parentNode, -1)
		{
			ChildrenOrder = childrenOrder;
		}
		
		public int[] ChildrenOrder {
			get;
			private set;
		}
	}}
