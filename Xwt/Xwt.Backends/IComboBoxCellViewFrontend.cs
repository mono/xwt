﻿//
// IComboBoxCellViewFrontend.cs
//
// Author:
//       Lluis Sanchez Gual <lluis@xamarin.com>
//
// Copyright (c) 2016 Xamarin, Inc (http://www.xamarin.com)
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
	public interface IEditableCellViewFrontend<T>
	{
		event EventHandler<CellEditingFinishedArgs<T>> EditingFinished;
		void RaiseEditingFinished (CellEditingFinishedArgs<T> args);
	}

	public interface IComboBoxCellViewFrontend: ICellViewFrontend, IEditableCellViewFrontend<string>
	{
		IDataField<string> SelectedTextField { get; }
		IDataField<int> SelectedIndexField { get; }
		IDataField<object> SelectedItemField { get; }
		IDataField<bool> EditableField { get; }
		IDataField<ItemCollection> ItemsField { get; }
		IDataField<IListDataSource> ItemsSourceField { get; }

		string SelectedText { get; }

		bool Editable { get; }

		ItemCollection Items { get; }

		IListDataSource ItemsSource { get; }

		bool RaiseSelectionChanged ();
	}
}
