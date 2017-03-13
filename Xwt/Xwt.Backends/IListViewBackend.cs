// 
// IListViewBackend.cs
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
	public interface IListViewBackend: ITableViewBackend
	{
		void SetSource (IListDataSource source, IBackend sourceBackend);
		int[] SelectedRows { get; }
		int FocusedRow { get; set; }
		void SelectRow (int pos);
		void UnselectRow (int pos);
		void ScrollToRow (int row);
		void StartEditingCell (int row, CellView cell);

		bool BorderVisible { get; set; }
		GridLines GridLinesVisible { get; set; }
		bool HeadersVisible { get; set; }

		int GetRowAtPosition (Point p);
		Rectangle GetCellBounds (int row, CellView cell, bool includeMargin);
		Rectangle GetRowBounds (int row, bool includeMargin);
		int CurrentEventRow { get; }
	}
	
	public enum ListViewEvent
	{
		RowActivated
	}

	public interface IListViewEventSink: ITableViewEventSink
	{
		void OnRowActivated (int rowIndex);
	}
}

