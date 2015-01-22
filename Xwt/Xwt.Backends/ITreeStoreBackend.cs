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
		void Clear ();
	}

	public interface ITreeStoreFilterBackend: ITreeDataSource, IBackend
	{
		/// <summary>
		/// Initializes the backend with the given <paramref name="store"/>.
		/// </summary>
		/// <param name="store">The tree data store to pass through the filter.</param>
		/// <exception cref="ArgumentNullException"><paramref name="store"/> is <c>null</c>.</exception>
		void Initialize (ITreeDataSource store);

		/// <summary>
		/// Sets the filter function, which should be used to filter single nodes.
		/// </summary>
		/// <value>The filter function.</value>
		/// <remarks>
		/// The function must return <c>true</c> for nodes to be filtered (hidden).
		/// The position of the node to apply the filter function on is passed as the parameter to the function.
		Func<TreePosition, bool> FilterFunction { set; }

		/// <summary>
		/// Forces the filter store to re-evaluate all nodes in the child store.
		/// </summary>
		void Refilter();

		/// <summary>
		/// Converts the node position from the child store to its position inside the filter store.
		/// </summary>
		/// <returns>The position of the node inside the filtered store, or null if the row has been filtered.</returns>
		/// <param name="pos">The child store position to convert</param>
		TreePosition ConvertChildPositionToPosition (TreePosition pos);

		/// <summary>
		/// Converts the node position from the filter store to its position inside the child store.
		/// </summary>
		/// <returns>The position of the node inside the child store.</returns>
		/// <param name="pos">The filter store position to convert.</param>
		TreePosition ConvertPositionToChildPosition (TreePosition pos);
	}
}

