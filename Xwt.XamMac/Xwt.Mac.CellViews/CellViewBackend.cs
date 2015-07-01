//
// CellViewBackend.cs
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

#if MONOMAC
using nint = System.Int32;
using nfloat = System.Single;
using MonoMac.AppKit;
#else
using AppKit;
#endif

namespace Xwt.Mac
{
	public class CellViewBackend: ICellViewBackend
	{
		NSTableView table;
		int column;

		public CellViewBackend (NSTableView table, int column)
		{
			this.table = table;
			this.column = column;
		}

		public ICellViewFrontend Frontend { get; private set; }

		public virtual void InitializeBackend (object frontend, ApplicationContext context)
		{
			Frontend = (ICellViewFrontend)frontend;
		}

		public NSCell CurrentCell { get; set; }

		public int Column {
			get {
				return column;
			}
		}

		public int CurrentRow { get; set; }

		internal ITablePosition CurrentPosition { get; set; }

		public virtual void EnableEvent (object eventId)
		{
		}

		public virtual void DisableEvent (object eventId)
		{
		}

		public Rectangle CellBounds {
			get {
				if (CurrentPosition is TableRow) {
					var r = table.GetCellFrame (column, ((TableRow)CurrentPosition).Row);
					r = ((ICellRenderer)CurrentCell).CellContainer.GetCellRect (r, CurrentCell);
					return new Rectangle (r.X, r.Y, r.Width, r.Height);
				}
				return Rectangle.Zero;
			}
		}

		public Rectangle BackgroundBounds {
			get {
				// TODO
				return CellBounds;
			}
		}

		public bool Selected {
			get {
				if (CurrentPosition is TableRow) {
					return table.IsRowSelected (((TableRow)CurrentPosition).Row);
				}
				// TODO
				return false;
			}
		}

		public bool HasFocus {
			get {
				// TODO
				return false;
			}
		}
	}
}

