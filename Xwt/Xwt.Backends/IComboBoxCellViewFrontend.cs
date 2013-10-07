//
// IComboBoxCellViewFrontend.cs
//
// Author:
//       Steffen Winkler <steffen-winkler@gmx.de>
//
// Copyright (c) 2013 
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
	public interface IComboBoxCellViewFrontend: ICellViewFrontend
	{
		bool Editable { get; }
		bool AllowMixed { get; }
		bool RaiseIndexChanged ();
		CellViewCollection View { get; }
		int VisibleColumn{ get; }
		int SelectedRow{ get; }
		IListDataSource Source { get; }
		IBackend SourceBackend { get; }
		IDataField<CellViewCollection> ViewField { get; }
		IDataField<int> SelectedRowField{get;}
		IDataField<IListDataSource> SourceField { get; }
		IDataField<IBackend> SourceBackendField { get; }
		IDataField<int> VisibleColumnField{ get; }
		IDataField<bool> ActiveField{ get; }
	}
}

