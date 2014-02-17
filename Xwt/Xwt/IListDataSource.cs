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

