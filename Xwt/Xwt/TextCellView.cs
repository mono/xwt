// 
// TextCellView.cs
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
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using Xwt.Backends;

namespace Xwt
{
	public sealed class TextCellView: CellView, ITextCellViewFrontend
	{
		string text;
		string markup;
		bool editable;
		EllipsizeMode ellipsize;

		public IDataField TextField { get; set; }
		public IDataField<string> MarkupField { get; set; }
		public IDataField<bool> EditableField { get; set; }
		public IDataField<EllipsizeMode> EllipsizeField { get; set; }

		public TextCellView ()
		{
		}
		
		public TextCellView (IDataField textField)
		{
			TextField = textField;
		}
		
		public TextCellView (string text)
		{
			this.text = text;
		}
		
		[DefaultValue (null)]
		public string Text {
			get {
				if (TextField != null && DataSource != null)
					return Convert.ToString (DataSource.GetValue (TextField));
				else
					return text;
			}
			set {
				text = value;
			}
		}

		[DefaultValue (null)]
		public string Markup {
			get {
				return GetValue (MarkupField, markup);
			}
			set {
				markup = value;
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

		[DefaultValue (EllipsizeMode.None)]
		public EllipsizeMode Ellipsize {
			get {
				return GetValue (EllipsizeField, ellipsize);
			}
			set {
				ellipsize = value;
			}
		}

		/// <summary>
		/// Occurs when the text of the cell is modified.
		/// </summary>
		public event EventHandler<TextChangedEventArgs> TextChanged;

		bool ITextCellViewFrontend.RaiseTextChanged (string newText)
		{
			if (TextChanged != null) {
				var args = new TextChangedEventArgs (newText);
				TextChanged (this, args);
				return args.Handled;
			}
			return false;
		}
	}
}
