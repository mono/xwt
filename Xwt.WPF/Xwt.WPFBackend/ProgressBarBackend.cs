// 
// CheckBoxbackend.cs
//  
// Author:
//       Andres G. Aragoneses
// 
// Copyright (c) 2012 Andres G. Aragoneses
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
using Xwt.Backends;
using Xwt.Engine;
using WindowsCheckBox = System.Windows.Controls.CheckBox;

namespace Xwt.WPFBackend
{
	public class ProgressBarBackend: WidgetBackend, IProgressBarBackend
	{
		public ProgressBarBackend()
		{
			var progressBar = new System.Windows.Controls.ProgressBar();
			Widget = progressBar;
			progressBar.Maximum = 1.0;
			progressBar.Minimum = 0.0;
			progressBar.Height = 40;
			progressBar.Width = 80;
			progressBar.IsIndeterminate = true;
		}

		public void SetFraction(double? fraction)
		{
			var widget = (System.Windows.Controls.ProgressBar) Widget;
			if (fraction.HasValue)
			{
				widget.IsIndeterminate = false;
				widget.Value = fraction.Value;
			} else {
				widget.IsIndeterminate = true;
			}
		}

		public void SetContent(string label)
		{
			CheckBox.Content = new TextBlock { Text = label };
		}

		public bool Active
		{
			get { return CheckBox.IsChecked.HasValue && CheckBox.IsChecked.Value; }
			set { CheckBox.IsChecked = value; }
		}

		public bool Mixed
		{
			get { return !CheckBox.IsChecked.HasValue; }
			set
			{
				if (value)
					CheckBox.IsChecked = null;
				else
					CheckBox.IsChecked = false;
			}
		}

		public bool AllowMixed
		{
			get { return CheckBox.IsThreeState; }
			set { CheckBox.IsThreeState = value; }
		}

		public override void EnableEvent(object eventId)
		{
			base.EnableEvent(eventId);
			if (eventId is CheckBoxEvent)
			{
				switch ((CheckBoxEvent)eventId)
				{
					case CheckBoxEvent.Clicked:
						CheckBox.Click += OnClicked;
						break;

					case CheckBoxEvent.Toggled:
						CheckBox.Checked += OnChecked;
						break;
				}
			}
		}

		public override void DisableEvent(object eventId)
		{
			base.DisableEvent(eventId);
			if (eventId is CheckBoxEvent)
			{
				switch ((CheckBoxEvent)eventId)
				{
					case CheckBoxEvent.Clicked:
						CheckBox.Click -= OnClicked;
						break;

					case CheckBoxEvent.Toggled:
						CheckBox.Checked -= OnChecked;
						break;
				}
			}
		}

		private void OnChecked(object sender, RoutedEventArgs routedEventArgs)
		{
			Toolkit.Invoke(CheckBoxEventSink.OnToggled);
		}

		private void OnClicked(object sender, RoutedEventArgs e)
		{
			Toolkit.Invoke(CheckBoxEventSink.OnClicked);
		}

		protected ICheckBoxEventSink CheckBoxEventSink
		{
			get { return (ICheckBoxEventSink)EventSink; }
		}

		protected WindowsCheckBox CheckBox
		{
			get { return (WindowsCheckBox)Widget; }
		}

		public void SetButtonStyle(ButtonStyle style)
		{
			//throw new NotImplementedException();
		}

		public void SetButtonType(ButtonType type)
		{
			//throw new NotImplementedException();
		}

		public void SetContent(string label, ContentPosition position)
		{
			CheckBox.Content = new TextBlock { Text = label };
		}
	}
}
