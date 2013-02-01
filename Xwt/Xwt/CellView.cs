// 
// CellView.cs
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
using Xwt.Drawing;

namespace Xwt
{
	public class CellView
	{
		public static CellView GetDefaultCellView (IDataField field)
		{
			if (field.Index == -1)
				throw new InvalidOperationException ("Field must be bound to a data source");
			if (field.FieldType == typeof(bool))
				return new CheckBoxCellView ((IDataField<bool>)field);
			else if (field.FieldType == typeof(Image))
				return new ImageCellView ((IDataField<Image>)field);
			return new TextCellView (field);
		}

		protected ICellDataSource DataSource { get; private set; }

		public void Initialize (ICellDataSource source)
		{
			DataSource = source;
			OnDataChanged ();
		}

		protected T GetValue<T> (IDataField<T> field, T defaultValue = default(T))
		{
			return DataSource != null && field != null ? (T) DataSource.GetValue (field) : defaultValue;
		}

		protected virtual void OnDataChanged ()
		{
		}
	}
}
