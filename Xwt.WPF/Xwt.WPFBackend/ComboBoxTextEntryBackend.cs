// 
// ComboBoxTextEntryBackend.cs
//  
// Author:
//	   Eric Maupin <ermau@xamarin.com>
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
using Xwt.Backends;

namespace Xwt.WPFBackend
{
	public class ComboBoxTextEntryBackend
		: WidgetBackend, ITextEntryBackend
	{
		public ComboBoxTextEntryBackend (ExComboBox combobox)
		{
			if (combobox == null)
				throw new ArgumentNullException ("combobox");

			this.combobox = combobox;
			this.combobox.GotFocus += OnGotFocus;
			this.combobox.LostFocus += OnLostFocus;
		}

		public string Text
		{
			get { return this.combobox.Text; }
			set { this.combobox.Text = value; }
		}

		public string PlaceholderText
		{
			get { return this.placeholderText; }
			set
			{
				if (this.placeholderText == value)
					return;

				UpdatePlaceholder (value, focused: this.combobox.IsFocused);
			}
		}

		public bool ReadOnly
		{
			get { return this.combobox.IsReadOnly; }
			set { this.combobox.IsReadOnly = value; }
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
					this.combobox.ClearValue (Control.BorderBrushProperty);
				else
					this.combobox.BorderBrush = null;

				this.showFrame = value;
			}
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is TextEntryEvent) {
				switch ((TextEntryEvent)eventId) {
				case TextEntryEvent.Changed:
					this.combobox.TextChanged += OnTextChanged;
					break;
				}
			}
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is TextEntryEvent) {
				switch ((TextEntryEvent)eventId) {
				case TextEntryEvent.Changed:
					this.combobox.TextChanged -= OnTextChanged;
					break;
				}
			}
		}

		private readonly ExComboBox combobox;
		private string placeholderText;

		protected ITextEntryEventSink TextEntryEventSink {
			get { return (ITextEntryEventSink) EventSink; }
		}

		private void OnTextChanged (object sender, EventArgs e)
		{
			TextEntryEventSink.OnChanged();
		}

		private void UpdatePlaceholder (string newPlaceholder, bool focused)
		{
			if (Text == this.placeholderText)
				Text = newPlaceholder;

			this.placeholderText = newPlaceholder;

			if (focused && Text == PlaceholderText)
				this.combobox.ClearValue (Control.ForegroundProperty);
			else if (!focused && String.IsNullOrEmpty (Text))
			{
				Text = PlaceholderText;
				this.combobox.Foreground = Brushes.LightGray;
			}
		}

		protected void OnGotFocus (object sender, RoutedEventArgs e)
		{
			UpdatePlaceholder (this.placeholderText, focused: true);
		}

		protected void OnLostFocus (object sender, RoutedEventArgs e)
		{
			UpdatePlaceholder (this.placeholderText, focused: false);
		}
	}
}