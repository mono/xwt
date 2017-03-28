// 
// ComboBoxCellView.cs
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
using System.ComponentModel;
using Xwt.Backends;

namespace Xwt 
{
	public sealed class ComboBoxCellView : CellView, IComboBoxCellViewFrontend
	{
		bool editable;
		string selectedText;
		ItemCollection items;
		IListDataSource itemsSource;

		public IDataField<string> SelectedTextField { get; set; }
		public IDataField<int> SelectedIndexField { get; set; }
		public IDataField<object> SelectedItemField { get; set; }
		public IDataField<bool> EditableField { get; set; }
		public IDataField<ItemCollection> ItemsField { get; set; }
		public IDataField<IListDataSource> ItemsSourceField { get; set; }

		public ComboBoxCellView ()
		{
		}

		public ComboBoxCellView (IDataField<string> field)
		{
			SelectedTextField = field;
		}

		[DefaultValue ("")]
		public string SelectedText {
			get {
				if (SelectedTextField != null)
					return GetValue (SelectedTextField, selectedText);
				if (SelectedIndexField != null) {
					var s = ItemsSource;
					var row = GetValue (SelectedIndexField, -1);
					if (row != -1)
						return s.GetValue (row, 0).ToString ();
				}
				return selectedText;
			}
			set {
				selectedText = value;
			}
		}

		[DefaultValue (false)]
		public bool Editable {
			get {
				return GetValue (EditableField, editable);
			}
			set {
				editable = value;
			}
		}

		[DefaultValue (null)]
		public ItemCollection Items {
			get {
				if (ItemsField != null)
					return GetValue (ItemsField, null);
				if (items == null)
					items = new ItemCollection ();
				return items;
			}
			set {
				items = value;
			}
		}

		[DefaultValue (null)]
		public IListDataSource ItemsSource {
			get {
				if (items != null || ItemsField != null)
					return GetValue (ItemsField, items).DataSource;
				return GetValue (ItemsSourceField, itemsSource);
			}
			set {
				itemsSource = value;
			}
		}

		public event EventHandler<WidgetEventArgs> SelectionChanged;

		/// <summary>
		/// Raises the SelectionChanged event
		/// </summary>
		/// <returns><c>true</c>, if the event was handled, <c>false</c> otherwise.</returns>
		public bool RaiseSelectionChanged ()
		{
			if (SelectionChanged != null) {
				var args = new WidgetEventArgs ();
				SelectionChanged (this, args);
				return args.Handled;
			}
			return false;
		}
	}
}
