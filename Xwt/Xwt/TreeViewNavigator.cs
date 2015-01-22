//
// TreeViewNavigator.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//       Vsevolod Kukol <sevo@sevo.org>
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

namespace Xwt
{
	public class TreeViewNavigator
	{
		readonly ITreeDataSource backend;
		protected ITreeDataSource Backend {
			get {
				return backend;
			}
		}

		[Obsolete]
		internal TreeViewNavigator (ITreeDataSource backend, TreePosition pos)
		{
			this.backend = backend;
			this.CurrentPosition = pos;
			CurrentIndex = backend.GetParentChildIndex (pos);
		}

		internal TreeViewNavigator (ITreeDataSource backend, TreePosition pos, int index)
		{
			this.backend = backend;
			this.CurrentPosition = pos;
			this.CurrentIndex = index;
		}

		public TreePosition CurrentPosition {
			get;
			private set;
		}

		public int CurrentIndex {
			get;
			protected set;
		}

		public TreeViewNavigator Clone ()
		{
			return new TreeViewNavigator (backend, CurrentPosition, CurrentIndex);
		}

		protected bool CommitPos (TreePosition newPosition)
		{
			if (newPosition != null) {
				CurrentPosition = newPosition;
				CurrentIndex = Backend.GetParentChildIndex (CurrentPosition);
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
			return CommitPos (backend.GetNext (CurrentPosition));
		}

		public bool MovePrevious ()
		{
			return CommitPos (backend.GetPrevious (CurrentPosition));
		}

		public bool MoveToChild ()
		{
			return CommitPos (backend.GetChild (CurrentPosition, 0));
		}

		public bool MoveToParent ()
		{
			return CommitPos (backend.GetParent (CurrentPosition));
		}

		public bool MoveToFirstSibling ()
		{
			return CommitPos (backend.GetChild (backend.GetParent (CurrentPosition), 0));
		}

		public bool MoveToLastSibling ()
		{
			return CommitPos (backend.GetChild (backend.GetParent (CurrentPosition), backend.GetChildrenCount (backend.GetParent (CurrentPosition)) - 1));
		}

		public T GetValue<T> (IDataField<T> field)
		{
			var value = backend.GetValue (CurrentPosition, field.Index);
			return value == null ? default(T) : (T)value;
		}
	}
}

