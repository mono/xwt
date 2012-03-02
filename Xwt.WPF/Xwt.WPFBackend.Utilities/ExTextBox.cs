// 
// ExTextBox.cs
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
using System.Windows.Media;

namespace Xwt.WPFBackend.Utilities
{
	public class ExTextBox
		: System.Windows.Controls.TextBox
	{
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

		private bool showFrame = true;
		public bool ShowFrame
		{
			get { return this.showFrame; }
			set
			{
				if (this.showFrame == value)
					return;

				if (value)
					ClearValue (Control.BorderBrushProperty);
				else
					BorderBrush = null;

				this.showFrame = value;
			}
		}

		private string placeholderText;

		private void UpdatePlaceholder (string newPlaceholder)
		{
			if (Text == this.placeholderText)
				Text = newPlaceholder;

			this.placeholderText = newPlaceholder;

			if (IsFocused && Text == PlaceholderText)
			{
				Text = null;
				ClearValue (Control.ForegroundProperty);
			}
			else if (!IsFocused && String.IsNullOrEmpty (Text))
			{
				Text = PlaceholderText;
				Foreground = Brushes.LightGray;
			}
		}

		protected override void OnGotFocus (RoutedEventArgs e)
		{
			base.OnGotFocus (e);
			UpdatePlaceholder (this.placeholderText);
		}

		protected override void OnLostFocus (RoutedEventArgs e)
		{
			base.OnLostFocus (e);
			UpdatePlaceholder (this.placeholderText);
		}
	}
}
