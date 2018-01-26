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
using AppKit;
using Xwt.Backends;

namespace Xwt.Mac
{
	public class ListBoxBackend: ListViewBackend, IListBoxBackend
	{
		readonly ListViewColumn column = new ListViewColumn { Expands = true };
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
	}
}

