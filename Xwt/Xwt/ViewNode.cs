//
// ViewNode.cs
//
// Author:
//       Lluis Sanchez Gual <lluis@xamarin.com>
//
// Copyright (c) 2014 Xamarin, Inc (http://www.xamarin.com)
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

namespace Xwt
{
	public struct ViewNode
	{
		ITreeStoreBackend backend;
		TreePosition pos;

		public ViewNode Next {
			get {
				return new ViewNode () { pos = backend.GetNext (pos), backend = backend };
			}
		}

		public ViewNode Previous {
			get {
				return new ViewNode () { pos = backend.GetPrevious (pos), backend = backend };
			}
		}

		public ViewNode FirstChild {
			get {
				return new ViewNode () { pos = backend.GetChild (pos, 0), backend = backend };
			}
		}

		public ViewNode GetChild (int index)
		{
			return new ViewNode () { pos = backend.GetChild (pos, index), backend = backend };
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

		public T GetValue<T> (IDataField<T> field)
		{
			var value = backend.GetValue (pos, field.Index);
			return value == null ? default(T) : (T)value;
		}

		public ViewNode SetValue<T> (IDataField<T> field, T data)
		{
			backend.SetValue (pos, field.Index, data);
			return this;
		}

		public ViewNode AddChild ()
		{
			return new ViewNode () { backend = backend, pos = backend.AddChild (pos) };
		}

		public ViewNode InsertBefore ()
		{
			return new ViewNode () { backend = backend, pos = backend.InsertBefore (pos) };
		}

		public ViewNode InsertAfter ()
		{
			return new ViewNode () { backend = backend, pos = backend.InsertAfter (pos) };
		}
	}
}

