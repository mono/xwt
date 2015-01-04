// 
// ITreeViewBackend.cs
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
using Xwt;

namespace Xwt.Backends
{
	public interface ITreeViewBackend: ITableViewBackend
	{
		void SetSource (ITreeDataSource source, IBackend sourceBackend);
		TreePosition[] SelectedRows { get; }
		void SelectRow (TreePosition pos);
		void UnselectRow (TreePosition pos);
		TreePosition FocusedRow { get; set; }
		
		bool IsRowSelected (TreePosition pos);
		bool IsRowExpanded (TreePosition pos);
		void ExpandRow (TreePosition pos, bool expandChildren);
		void CollapseRow (TreePosition pos);
		void ScrollToRow (TreePosition pos);
		void ExpandToRow (TreePosition pos);
		
		bool HeadersVisible { get; set; }
		GridLines GridLinesVisible { get; set; }
		
		bool GetDropTargetRow (double x, double y, out RowDropPosition pos, out TreePosition nodePosition);

		TreePosition GetRowAtPosition (Point p);
		Rectangle GetCellBounds (TreePosition pos, CellView cell, bool includeMargin);
		Rectangle GetRowBounds (TreePosition pos, bool includeMargin);
		TreePosition CurrentEventRow { get; }
	}
	
	public enum ListViewColumnChange
	{
		Title,
		Cells,
		CanResize,
		SortDirection,
		SortDataField,
		SortIndicatorVisible,
		Alignment
	}
	
	public enum TreeViewEvent
	{
		RowActivated,
		RowExpanding,
		RowExpanded,
		RowCollapsing,
		RowCollapsed
	}

	public interface ITreeViewEventSink: ITableViewEventSink
	{
		void OnRowActivated (TreePosition position);
		void OnRowExpanding (TreePosition position);
		void OnRowExpanded (TreePosition position);
		void OnRowCollapsing (TreePosition position);
		void OnRowCollapsed (TreePosition position);
	}
}

