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
using Xwt.Backends;
using System.ComponentModel;

namespace Xwt
{
	public class CellView: ICellViewFrontend
	{
		/// <summary>
		/// Gets the default cell view for the provided field type
		/// </summary>
		/// <returns>The default cell view.</returns>
		/// <param name="field">Field.</param>
		public static CellView GetDefaultCellView (IDataField field)
		{
			if (field.Index == -1)
				throw new InvalidOperationException ("Field must be bound to a data source");
			if (field.FieldType == typeof(bool))
				return new CheckBoxCellView ((IDataField<bool>)field);
			else if (field.FieldType == typeof(CheckBoxState))
				return new CheckBoxCellView ((IDataField<CheckBoxState>)field);
			else if (field.FieldType == typeof(Image))
				return new ImageCellView ((IDataField<Image>)field);
			return new TextCellView (field);
		}

		/// <summary>
		/// Data source object to be used to get the data with which to fill the cell
		/// </summary>
		/// <value>The data source.</value>
		protected ICellDataSource DataSource { get; private set; }

		bool visible = true;

		public IDataField<bool> VisibleField { get; set; }

		[DefaultValue (true)]
		public bool Visible {
			get { return GetValue (VisibleField, visible); }
			set { visible = value; }
		}

		void ICellViewFrontend.Initialize (ICellDataSource source)
		{
			DataSource = source;
			OnDataChanged ();
		}

		/// <summary>
		/// Gets the value of a field
		/// </summary>
		/// <returns>The value.</returns>
		/// <param name="field">Field.</param>
		/// <param name="defaultValue">Default value to be returned if the field has no value</param>
		/// <typeparam name="T">Type of the value</typeparam>
		protected T GetValue<T> (IDataField<T> field, T defaultValue = default(T))
		{
			if (DataSource != null && field != null) {
				var result = DataSource.GetValue (field);
				return result == null || result == DBNull.Value ? defaultValue : (T) result;
			}
			return defaultValue;
		}

		/// <summary>
		/// Invoked when the data source changes
		/// </summary>
		protected virtual void OnDataChanged ()
		{
		}
	}
}
