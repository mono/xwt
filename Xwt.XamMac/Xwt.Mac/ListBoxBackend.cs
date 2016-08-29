//
// ListBoxBackend.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc.
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

#if MONOMAC
using nint = System.Int32;
using nfloat = System.Single;
using MonoMac.AppKit;
#else
using AppKit;
#endif

namespace Xwt.Mac
{
	public class ListBoxBackend: ListViewBackend, IListBoxBackend
	{
		ListViewColumn column = new ListViewColumn ();
		NSTableColumn columnHandle;

		public ListBoxBackend ()
		{
		}

		public new bool GridLinesVisible {
			get {
				return (base.GridLinesVisible == Xwt.GridLines.Horizontal || base.GridLinesVisible == Xwt.GridLines.Both);
			}
			set {
				base.GridLinesVisible = value ? Xwt.GridLines.Horizontal : Xwt.GridLines.None;
			}
		}

		public override void Initialize ()
		{
			base.Initialize ();
			HeadersVisible = false;
			columnHandle = AddColumn (column);
			VerticalScrollPolicy = ScrollPolicy.Automatic;
			HorizontalScrollPolicy = ScrollPolicy.Automatic;
		}

		public void SetViews (CellViewCollection views)
		{
			column.Views.Clear ();
			foreach (var v in views)
				column.Views.Add (v);
			UpdateColumn (column, columnHandle, ListViewColumnChange.Cells);
		}

		public override void SetSource (IListDataSource source, IBackend sourceBackend)
		{
			base.SetSource (source, sourceBackend);

			source.RowInserted += HandleColumnSizeChanged;
			source.RowDeleted += HandleColumnSizeChanged;
			source.RowChanged += HandleColumnSizeChanged;
			ResetColumnSize (source);
		}

		void HandleColumnSizeChanged (object sender, ListRowEventArgs e)
		{
			var source = (IListDataSource)sender;
			ResetColumnSize (source);
		}

		void ResetColumnSize (IListDataSource source)
		{
			// Calculate size of column
			// This is how Apple implements it; unfortunately, they don't expose this functionality in the API.
			// https://developer.apple.com/library/mac/documentation/Cocoa/Reference/NSTableViewDelegate_Protocol/index.html#//apple_ref/occ/intfm/NSTableViewDelegate/tableView:sizeToFitWidthOfColumn:
			nfloat w = 0;
			for (var row = 0; row < source.RowCount; row++) {
				using (var cell = Table.GetCell (0, row)) {
					var size = cell.CellSize;
					w = (nfloat)Math.Max (w, size.Width);
				}
			}
			columnHandle.MinWidth = (nfloat)Math.Ceiling (w);
			columnHandle.Width = (nfloat)Math.Ceiling (w);
		}
	}
}

