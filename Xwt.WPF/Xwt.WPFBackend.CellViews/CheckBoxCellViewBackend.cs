//
// CheckBoxCellViewBackend.cs
//
// Author:
//       Vsevolod Kukol <sevo@sevo.org>
//
// Copyright (c) 2015 Vsevolod Kukol
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
using Xwt.Backends;
using System;
using System.Windows.Data;
using System.Globalization;
using System.Windows;
using SWC = System.Windows.Controls;

namespace Xwt.WPFBackend
{
	class CheckBoxCellViewBackend: CellViewBackend
	{
		public override void OnInitialize (CellView cellView, FrameworkElementFactory factory)
		{
			factory.AddHandler (CheckBoxCell.ToggleEvent, new RoutedEventHandler (HandleToggled));
			base.OnInitialize (cellView, factory);
		}

		void HandleToggled(object sender, RoutedEventArgs e)
		{
			var view = (ICheckBoxCellViewFrontend) CellView;
			Load(sender as FrameworkElement);
			SetCurrentEventRow ();
			e.Handled = view.RaiseToggled ();
		}
	}

	class CheckBoxCell : SWC.CheckBox
	{
		public static readonly RoutedEvent ToggleEvent = EventManager.RegisterRoutedEvent ("Toggle", RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (ToggleButton));

		public event RoutedEventHandler Toggle {
			add { AddHandler (ToggleEvent, value); }
			remove { RemoveHandler (ToggleEvent, value); }
		}

		protected virtual void OnToggle (RoutedEventArgs e)
		{
			RaiseEvent (e);
		}

		protected override void OnToggle ()
		{
			var args = new RoutedEventArgs (ToggleEvent);
			OnToggle (args);
			if (!args.Handled)
				base.OnToggle ();
		}
	}

	class CheckBoxStateToBoolConverter : IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			var svalue = value as CheckBoxState?;
			switch (svalue) {
			case CheckBoxState.On:
				return true;
			case CheckBoxState.Off:
				return false;
			}
			return null;
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			var bvalue = value as bool?;
			if (!bvalue.HasValue)
				return CheckBoxState.Mixed;
			return bvalue.Value ? CheckBoxState.On : CheckBoxState.Off;
		}
	}
}

