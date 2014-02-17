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

		public Binding TextBinding { get; set; }
		public Binding MarkupBinding { get; set; }
		public Binding EditableBinding { get; set; }
		public Binding EllipsizeBinding { get; set; }
		public Binding TextColorBinding { get; set; }

		public TextCellView ()
		{
		}
		
		public TextCellView (Binding textField)
		{
			TextBinding = textField;
		}
		
		public TextCellView (string text)
		{
			this.text = text;
		}
		
		[DefaultValue (null)]
		public string Text {
			get {
				if (TextBinding != null && DataSource != null)
					return TextBinding.GetValue<string> (DataSource);
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
				return GetValue (MarkupBinding, markup);
			}
			set {
				markup = value;
			}
		}

		[DefaultValue (false)]
		public bool Editable {
			get {
				return GetValue (EditableBinding, editable);
			}
			set {
				editable = value;
			}
		}

		[DefaultValue (EllipsizeMode.None)]
		public EllipsizeMode Ellipsize {
			get {
				return GetValue (EllipsizeBinding, ellipsize);
			}
			set {
				ellipsize = value;
			}
		}

		/// <summary>
		/// Occurs when the text of the cell is modified.
		/// </summary>
		public event EventHandler<WidgetEventArgs> TextChanged;

		bool ITextCellViewFrontend.RaiseTextChanged ()
		{
			if (TextChanged != null) {
				var args = new WidgetEventArgs ();
				TextChanged (this, args);
				return args.Handled;
			}
			return false;
		}
	}
}
