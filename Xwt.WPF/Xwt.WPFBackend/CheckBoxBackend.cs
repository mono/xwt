// 
// CheckBoxBackend.cs
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
using Xwt.Backends;

using WindowsCheckBox = System.Windows.Controls.CheckBox;

namespace Xwt.WPFBackend
{
	public class CheckBoxBackend
		: WidgetBackend, ICheckBoxBackend
	{
		public CheckBoxBackend()
		{
			Widget = new WindowsCheckBox ();
		}

		public void SetContent (IWidgetBackend widget)
		{
			if (widget == null)
				CheckBox.Content = null;
			else
				CheckBox.Content = widget.NativeWidget;
		}

		public void SetContent (string label)
		{
			CheckBox.Content = new TextBlock { Text = label };
		}

		public CheckBoxState State
		{
			get {
				if (!CheckBox.IsChecked.HasValue)
					return CheckBoxState.Mixed;
				else if (CheckBox.IsChecked.Value)
					return CheckBoxState.On;
				else
					return CheckBoxState.Off;
			}
			set {
				if (value == CheckBoxState.Mixed)
					CheckBox.IsChecked = null;
				else
					CheckBox.IsChecked = value == CheckBoxState.On;
			}
		}

		public bool AllowMixed
		{
			get { return CheckBox.IsThreeState; }
			set { CheckBox.IsThreeState = value; }
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is CheckBoxEvent) {
				switch ((CheckBoxEvent)eventId) {
				case CheckBoxEvent.Clicked:
					CheckBox.Click += OnClicked;
					break;

				case CheckBoxEvent.Toggled:
					CheckBox.Checked += OnChecked;
					CheckBox.Unchecked += OnChecked;
					break;
				}
			}
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is CheckBoxEvent) {
				switch ((CheckBoxEvent)eventId) {
				case CheckBoxEvent.Clicked:
					CheckBox.Click -= OnClicked;
					break;

				case CheckBoxEvent.Toggled:
					CheckBox.Checked -= OnChecked;
					CheckBox.Unchecked -= OnChecked;
					break;
				}
			}
		}

		private void OnChecked (object sender, RoutedEventArgs routedEventArgs)
		{
			Context.InvokeUserCode (CheckBoxEventSink.OnToggled);
		}

		private void OnClicked (object sender, RoutedEventArgs e)
		{
			Context.InvokeUserCode (CheckBoxEventSink.OnClicked);
		}

		protected ICheckBoxEventSink CheckBoxEventSink
		{
			get { return (ICheckBoxEventSink) EventSink; }
		}

		protected WindowsCheckBox CheckBox
		{
			get { return (WindowsCheckBox) Widget; }
		}
	}
}
