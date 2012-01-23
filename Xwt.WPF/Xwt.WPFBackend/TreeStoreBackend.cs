// 
// TreeStoreBackend.cs
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
using Xwt.Backends;

namespace Xwt.WPFBackend
{
	public class TreeStoreBackend: ITreeStoreBackend
	{
		public event EventHandler<TreeNodeEventArgs> NodeInserted;
		public event EventHandler<TreeNodeChildEventArgs> NodeDeleted;
		public event EventHandler<TreeNodeEventArgs> NodeChanged;
		public event EventHandler<TreeNodeOrderEventArgs> NodesReordered;

		public void Initialize (Type[] columnTypes)
		{
			throw new NotImplementedException ();
		}

		public void Initialize (object frontend)
		{
		}

		public void Clear ()
		{
			throw new NotImplementedException ();
		}

		public TreePosition AddChild (TreePosition pos)
		{
			throw new NotImplementedException ();
		}

		public TreePosition GetPrevious (TreePosition pos)
		{
			throw new NotImplementedException ();
		}

		public TreePosition GetNext (TreePosition pos)
		{
			throw new NotImplementedException ();
		}

		public void SetValue (TreePosition pos, int column, object value)
		{
			throw new NotImplementedException ();
		}

		public object GetValue (TreePosition pos, int column)
		{
			throw new NotImplementedException ();
		}

		public TreePosition GetParent (TreePosition pos)
		{
			throw new NotImplementedException ();
		}

		public int GetChildrenCount (TreePosition pos)
		{
			throw new NotImplementedException ();
		}

		public TreePosition GetChild (TreePosition pos, int index)
		{
			throw new NotImplementedException ();
		}

		public void Remove (TreePosition pos)
		{
			throw new NotImplementedException ();
		}

		public TreePosition InsertBefore (TreePosition pos)
		{
			throw new NotImplementedException ();
		}

		public TreePosition InsertAfter (TreePosition pos)
		{
			throw new NotImplementedException ();
		}

		public Type[] ColumnTypes
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		public void EnableEvent (object eventId)
		{
		}

		public void DisableEvent (object eventId)
		{
		}
	}
}
