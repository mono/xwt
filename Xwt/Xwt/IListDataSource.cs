// 
// IListViewSource.cs
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

namespace Xwt
{
	public interface IListDataSource
	{
		/// <summary>
		/// Gets the number of rows in the data source.
		/// </summary>
		int RowCount { get; }

		/// <summary>
		/// Gets the value at the specified <paramref name="row"/> and <paramref name="column"/>.
		/// </summary>
		/// <param name="row">The row to retrieve the value from.</param>
		/// <param name="column">The column to retrieve the value from.</param>
		/// <returns>A reference to the value.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="row"/> is &gt;= <see cref="RowCount"/></para>
		/// <para>-- or --</para>
		/// <para><paramref name="column"/> is &gt;= the number of columns (<see cref="ColumnTypes"/>).</para>
		/// </exception>
		/// <seealso cref="SetValue"/>
		object GetValue (int row, int column);

		/// <summary>
		/// Sets the value at the specified <paramref name="row"/> and <paramref name="column"/>.
		/// </summary>
		/// <param name="row">The row to set the value at.</param>
		/// <param name="column">The column to set the value at.</param>
		/// <param name="value">The value to set at the given <paramref name="row"/> and <paramref name="column"/>.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="row"/> is &gt;= <see cref="RowCount"/></para>
		/// <para>-- or --</para>
		/// <para><paramref name="column"/> is &gt;= the number of columns (<see cref="ColumnTypes"/>).</para>
		/// </exception>
		void SetValue (int row, int column, object value);

		/// <summary>
		/// Gets the column types of this data source.
		/// </summary>
		Type[] ColumnTypes { get; }
		
		/// <summary>
		/// Raised when a row is inserted into the data source.
		/// </summary>
		event EventHandler<ListRowEventArgs> RowInserted;

		/// <summary>
		/// Raised when a row is deleted from the data source.
		/// </summary>
		event EventHandler<ListRowEventArgs> RowDeleted;

		/// <summary>
		/// Raised when a column value of a row is changed in the data source.
		/// </summary>
		event EventHandler<ListRowEventArgs> RowChanged;

		/// <summary>
		/// Raised when rows in the data source is reordered.
		/// </summary>
		event EventHandler<ListRowOrderEventArgs> RowsReordered;
	}
	
	public class ListRowEventArgs: EventArgs
	{
		public ListRowEventArgs (int row)
		{
			Row = row;
		}
		
		public int Row {
			get;
			private set;
		}
	}
	
	public class ListRowOrderEventArgs: ListRowEventArgs
	{
		public ListRowOrderEventArgs (int parentRow, int[] rows): base (parentRow)
		{
			ChildrenOrder = rows;
		}
		
		public int[] ChildrenOrder {
			get;
			private set;
		}
	}

	/// <summary>
	/// Data source for cell views
	/// </summary>
	/// <remarks>Provides the data required for filling the content of a cell</remarks>
	public interface ICellDataSource
	{
		/// <summary>
		/// Gets the value of a field
		/// </summary>
		/// <returns>The value.</returns>
		/// <param name="field">Field.</param>
		object GetValue (IDataField field);
	}
}

