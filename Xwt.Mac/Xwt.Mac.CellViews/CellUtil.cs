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
using MonoMac.AppKit;
using Xwt.Backends;

namespace Xwt.Mac
{
	static class CellUtil
	{
		public static NSCell CreateCell (ICellSource source, ICollection<CellView> cells)
		{
//			if (cells.Count > 1) {
				CompositeCell c = new CompositeCell (Orientation.Horizontal, source);
				foreach (var cell in cells)
					c.AddCell ((ICellRenderer) CreateCell (source, cell));
				return c;
//			} else
//				return CreateCell (source, cells.First ());
		}
		
		public static NSCell CreateCell (ICellSource source, CellView cell)
		{
			if (cell is TextCellView)
				return new TextTableCell ((TextCellView) cell);
			if (cell is ImageCellView)
				return new ImageTableCell ((ImageCellView) cell);
			if (cell is CanvasCellView)
				return new CanvasTableCell ((CanvasCellView) cell);
			throw new NotImplementedException ();
		}
	}
}

