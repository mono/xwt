// 
// TreeNavigator.cs
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
using Xwt.Backends;

namespace Xwt
{
	public struct NodePosition
	{
		internal TreePosition ParentPos;
		internal int Index;
	}
	
	public class TreeNavigator
	{
		ITreeStoreBackend backend;
		TreePosition pos;
		
		internal TreeNavigator (ITreeStoreBackend backend, TreePosition pos)
		{
			this.backend = backend;
			this.pos = pos;
		}
		
		public TreePosition CurrentPosition {
			get { return pos; }
		}
		
		public TreeNavigator Clone ()
		{
			return new TreeNavigator (backend, pos);
		}
		
		bool CommitPos (TreePosition newPosition)
		{
			if (newPosition != null) {
				pos = newPosition;
				return true;
			}
			else
				return false;
		}
		
		public bool MoveToFirst ()
		{
			return CommitPos (backend.GetChild (null, 0));
		}

		public bool MoveNext ()
		{
			return CommitPos (backend.GetNext (pos));
		}

		public bool MovePrevious ()
		{
			return CommitPos (backend.GetPrevious (pos));
		}

		public bool MoveToChild ()
		{
			return CommitPos (backend.GetChild (pos, 0));
		}

		public bool MoveToParent ()
		{
			return CommitPos (backend.GetParent (pos));
		}

		public TreeNavigator InsertBefore ()
		{
			pos = backend.InsertBefore (pos);
			return this;
		}
		
		public TreeNavigator InsertAfter ()
		{
			pos = backend.InsertAfter (pos);
			return this;
		}
		
		public TreeNavigator AddChild ()
		{
			pos = backend.AddChild (pos);
			return this;
		}
		
		public TreeNavigator SetValue<T> (IDataField<T> field, T data)
		{
			backend.SetValue (pos, field.Index, data);
			return this;
		}
		
		public T GetValue<T> (IDataField<T> field)
		{
			return (T) backend.GetValue (pos, field.Index);
		}
		
		public void Remove ()
		{
			backend.Remove (pos);
		}
		
		public void RemoveChildren ()
		{
			TreePosition child = backend.GetChild (pos, 0);
			while (child != null) {
				backend.Remove (child);
				child = backend.GetChild (pos, 0);
			}
		}
	}
}
