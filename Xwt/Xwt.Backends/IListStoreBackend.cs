// 
// IListStoreBackend.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//       Eric Maupin <ermau@xamarin.com>
// 
// Copyright (c) 2011-2012 Xamarin, Inc.
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
	/// <summary>
	/// A ListStore backend.
	/// </summary>
	public interface IListStoreBackend: IListDataSource, IBackend
	{
		// WARNING: You don't need to implement this backend.
		// Xwt provides a default implementation.
		// You only need to implement it if the underlying widget
		// toolkit has its own list store implementation which
		// can be plugged into a ListView or ComboBox
		
		/// <summary>
		/// Initializes the backend with the given <paramref name="columnTypes"/>.
		/// </summary>
		/// <param name="columnTypes">The data types of the columns for this list store.</param>
		/// <exception cref="ArgumentNullException"><paramref name="columnTypes"/> is <c>null</c>.</exception>
		void Initialize (Type[] columnTypes);
		
		/// <summary>
		/// Adds a new row and returns the index.
		/// </summary>
		/// <returns>The index of the newly added row.</returns>
		int AddRow ();
		
		/// <summary>
		/// Inserts a new row after <paramref name="row"/> and returns the index.
		/// </summary>
		/// <param name="row">The index of the row to insert a new row after.</param>
		/// <returns>The index of the newly added row.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="row"/> is &gt;= <see cref="IListDataSource.RowCount" />
		/// </exception>
		int InsertRowAfter (int row);
		
		/// <summary>
		/// Inserts a new row before <paramref name="row"/> and returns the index.
		/// </summary>
		/// <param name="row">The index of the row to insert a new row before.</param>
		/// <returns>The index of the newly added row.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="row"/> is &gt;= <see cref="IListDataSource.RowCount" />
		/// </exception>
		int InsertRowBefore (int row);
		
		/// <summary>
		/// Removes a row at the given index (<paramref name="row"/>).
		/// </summary>
		/// <param name="row">The index of the row to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="row"/> is &gt;= <see cref="IListDataSource.RowCount" />
		/// </exception>
		void RemoveRow (int row);

		/// <summary>
		/// Removes all rows
		/// </summary>
		void Clear ();
	}
}

