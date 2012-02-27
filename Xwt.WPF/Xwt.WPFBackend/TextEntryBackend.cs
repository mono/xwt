// 
// TextEntryBackend.cs
//  
// Author:
//       Eric Maupin <ermau@xamarin.com>
// 
// Copyright (c) 2012 Xamarin, Inc.
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Xwt.Backends;

namespace Xwt.WPFBackend
{
	public class TextEntryBackend
		: WidgetBackend, ITextEntryBackend
	{
		public TextEntryBackend()
		{
			Widget = new TextBox();
			Widget.GotFocus += OnGotFocus;
			Widget.LostFocus += OnLostFocus;
		}

		public TextBox TextBox
		{
			get { return (TextBox) Widget; }
		}

		public string Text
		{
			get { return TextBox.Text; }
			set { TextBox.Text = value; }
		}

		private string placeholderText;
		public string PlaceholderText
		{
			get { return this.placeholderText; }
			set
			{
				if (this.placeholderText == value)
					return;

				UpdatePlaceholder (value);
			}
		}

		public bool ReadOnly
		{
			get { return TextBox.IsReadOnly; }
			set { TextBox.IsReadOnly = true; }
		}

		private bool showFrame = true;
		public bool ShowFrame
		{
			get { return this.showFrame; }
			set
			{
				if (this.showFrame == value)
					return;

				if (value)
					TextBox.ClearValue (Control.BorderBrushProperty);
				else
					TextBox.BorderBrush = null;

				this.showFrame = value;
			}
		}

		private void UpdatePlaceholder (string newPlaceholder = null)
		{
			if (TextBox.Text == this.placeholderText)
				TextBox.Text = newPlaceholder;

			this.placeholderText = newPlaceholder;

			if (TextBox.IsFocused && TextBox.Text == PlaceholderText)
			{
				TextBox.Text = null;
				TextBox.ClearValue (Control.ForegroundProperty);
			}
			else if (!TextBox.IsFocused && String.IsNullOrEmpty (TextBox.Text))
			{
				TextBox.Text = PlaceholderText;
				TextBox.Foreground = Brushes.LightGray;
			}
		}

		private void OnGotFocus (object sender, RoutedEventArgs e)
		{
			UpdatePlaceholder();
		}

		private void OnLostFocus(object sender, RoutedEventArgs e)
		{
			UpdatePlaceholder();
		}
	}
}