//
// TreeStoreBase.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2014 Vsevolod Kukol
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
using System.Linq;

namespace Xwt
{
	public abstract class TreeStoreBase: XwtComponent, ITreeDataSource
	{
		protected abstract Type[] GetColumnTypes();

		ITreeDataSource Backend {
			get { return (ITreeDataSource) BackendHost.Backend; }
		}

		event EventHandler<TreeNodeEventArgs> ITreeDataSource.NodeInserted {
			add { Backend.NodeInserted += value; }
			remove { Backend.NodeInserted -= value; }
		}
		event EventHandler<TreeNodeChildEventArgs> ITreeDataSource.NodeDeleted {
			add { Backend.NodeDeleted += value; }
			remove { Backend.NodeDeleted -= value; }
		}
		event EventHandler<TreeNodeEventArgs> ITreeDataSource.NodeChanged {
			add { Backend.NodeChanged += value; }
			remove { Backend.NodeChanged -= value; }
		}
		event EventHandler<TreeNodeOrderEventArgs> ITreeDataSource.NodesReordered {
			add { Backend.NodesReordered += value; }
			remove { Backend.NodesReordered -= value; }
		}

		TreePosition ITreeDataSource.GetChild (TreePosition pos, int index)
		{
			return Backend.GetChild (pos, index);
		}

		TreePosition ITreeDataSource.GetNext (TreePosition pos)
		{
			return Backend.GetNext (pos);
		}

		TreePosition ITreeDataSource.GetPrevious (TreePosition pos)
		{
			return Backend.GetPrevious (pos);
		}

		TreePosition ITreeDataSource.GetParent (TreePosition pos)
		{
			return Backend.GetParent (pos);
		}

		int ITreeDataSource.GetChildrenCount (TreePosition pos)
		{
			return Backend.GetChildrenCount (pos);
		}

		int ITreeDataSource.GetParentChildIndex (TreePosition pos)
		{
			return Backend.GetParentChildIndex (pos);
		}

		object ITreeDataSource.GetValue (TreePosition pos, int column)
		{
			return Backend.GetValue (pos, column);
		}

		void ITreeDataSource.SetValue (TreePosition pos, int column, object val)
		{
			Backend.SetValue (pos, column, val);
		}

		Type[] ITreeDataSource.ColumnTypes {
			get {
				return GetColumnTypes();
			}
		}
	}
}

