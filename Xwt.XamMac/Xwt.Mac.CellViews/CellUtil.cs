// 
// CellUtil.cs
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
using System.Collections.Generic;
using System.Linq;
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
	static class CellUtil
	{
		public static NSCell CreateCell (ApplicationContext context, NSTableView table, ICellSource source, ICollection<CellView> cells, int column)
		{
			CompositeCell c = new CompositeCell (context, Orientation.Horizontal, source);
			foreach (var cell in cells)
				c.AddCell ((ICellRenderer) CreateCell (table, c, cell, column));
			return c;
		}
		
		static NSCell CreateCell (NSTableView table, CompositeCell source, CellView cell, int column)
		{
			ICellRenderer cr = null;

			if (cell is ITextCellViewFrontend)
				cr = new TextTableCell ();
			else if (cell is IImageCellViewFrontend)
				cr = new ImageTableCell ();
			else if (cell is ICanvasCellViewFrontend)
				cr = new CanvasTableCell ();
			else if (cell is ICheckBoxCellViewFrontend)
				cr = new CheckBoxTableCell ();
			else
				throw new NotImplementedException ();
			cr.Backend = new CellViewBackend (table, column);
			ICellViewFrontend fr = cell;
			fr.AttachBackend (null, cr.Backend);
			return (NSCell)cr;
		}
	}
}

