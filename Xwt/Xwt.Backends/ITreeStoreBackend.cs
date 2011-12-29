// 
// ITreeStoreBackend.cs
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

namespace Xwt.Backends
{
	public interface ITreeStoreBackend: ITreeDataSource, IBackend
	{
		// WARNING: You don't need to implement this backend.
		// Xwt provides a default implementation.
		// You only need to implement it if the underlying widget
		// toolkit has its own tree store implementation which
		// can be plugged into a TreeView
		
		void Initialize (Type[] columnTypes);
		TreePosition InsertBefore (TreePosition pos);
		TreePosition InsertAfter (TreePosition pos);
		TreePosition AddChild (TreePosition pos);
		void Remove (TreePosition pos);
		TreePosition GetNext (TreePosition pos);
		TreePosition GetPrevious (TreePosition pos);
		void Clear ();
	}
}

