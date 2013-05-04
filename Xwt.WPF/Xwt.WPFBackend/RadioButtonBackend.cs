// 
// RadioButtonBackend.cs
//  
// Author:
//       David Karlaš <david.karlas@gmail.com>
// 
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

using WindowsRadioButton = System.Windows.Controls.RadioButton;
using System.Collections.Generic;

namespace Xwt.WPFBackend
{
	public class RadioButtonBackend
		: WidgetBackend, IRadioButtonBackend
	{
		public RadioButtonBackend()
		{
			Widget = new WindowsRadioButton();
			RadioButton.GroupName = Guid.NewGuid().ToString();//Prevent WPF from managing RadioButton groups
			RadioButton.IsChecked = true;//GTK has default true
		}

		public void SetContent(IWidgetBackend widget)
		{
			if (widget == null)
				RadioButton.Content = null;
			else
				RadioButton.Content = widget.NativeWidget;
		}

		public void SetContent(string label)
		{
			RadioButton.Content = new TextBlock { Text = label };
		}

		public bool Active
		{
			get { return RadioButton.IsChecked.HasValue && RadioButton.IsChecked.Value; }
			set { RadioButton.IsChecked = value; }
		}

		public override void EnableEvent(object eventId)
		{
			base.EnableEvent(eventId);
			if (eventId is RadioButtonEvent)
			{
				switch ((RadioButtonEvent)eventId)
				{
					case RadioButtonEvent.Clicked:
						RadioButton.Click += OnClicked;
						break;
					case RadioButtonEvent.ActiveChanged:
						RadioButton.Checked += OnChecked;
						RadioButton.Unchecked += OnChecked;
						break;
				}
			}
		}

		public override void DisableEvent(object eventId)
		{
			base.DisableEvent(eventId);
			if (eventId is RadioButtonEvent)
			{
				switch ((RadioButtonEvent)eventId)
				{
					case RadioButtonEvent.Clicked:
						RadioButton.Click -= OnClicked;
						break;
					case RadioButtonEvent.ActiveChanged:
						RadioButton.Checked -= OnChecked;
						RadioButton.Unchecked -= OnChecked;
						break;
				}
			}
		}

		private void OnChecked(object sender, RoutedEventArgs routedEventArgs)
		{
			Context.InvokeUserCode(RadioButtonEventSink.OnToggled);
		}

		private void OnClicked(object sender, RoutedEventArgs e)
		{
			Context.InvokeUserCode(RadioButtonEventSink.OnClicked);
		}

		protected IRadioButtonEventSink RadioButtonEventSink
		{
			get { return (IRadioButtonEventSink)EventSink; }
		}

		protected WindowsRadioButton RadioButton
		{
			get { return (WindowsRadioButton)Widget; }
		}

		public class WindowsRadioButtonGroup
		{
			List<WindowsRadioButton> items = new List<WindowsRadioButton>();
			WindowsRadioButton activeRadioButton;

			internal void Add(WindowsRadioButton r)
			{
				items.Add(r);
				if (activeRadioButton == null && (r.IsChecked ?? false))
				{
					activeRadioButton = r;
				}
				else if (r.IsChecked ?? false)
				{
					r.IsChecked = false;
				}
				r.Checked += new RoutedEventHandler(r_Checked);
			}

			void r_Checked(object sender, RoutedEventArgs e)
			{
				var r = (WindowsRadioButton)sender;
				if (r == activeRadioButton)
				{
					if (!(r.IsChecked ?? false))
						activeRadioButton = null;
				}
				else
				{
					if (r.IsChecked ?? false)
						if (activeRadioButton == null)
						{
							activeRadioButton = r;
						}
						else
						{
							activeRadioButton.IsChecked = false;
							activeRadioButton = r;
						}
				}
			}

			internal void Remove(WindowsRadioButton r)
			{
				if (r == activeRadioButton)
					activeRadioButton = null;
				items.Remove(r);
				r.Checked -= new RoutedEventHandler(r_Checked);
			}
		}

		private WindowsRadioButtonGroup radioGroup = null;
		public object Group
		{
			get
			{
				if (radioGroup == null)
				{
					radioGroup = new WindowsRadioButtonGroup();
					radioGroup.Add(RadioButton);
				}
				return radioGroup;
			}
			set
			{
				if (radioGroup != null)
					radioGroup.Remove(RadioButton);
				radioGroup = (WindowsRadioButtonGroup)value;
				if (radioGroup != null)
					radioGroup.Add(RadioButton);
			}
		}
	}
}
