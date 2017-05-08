//
// RadioButtonCellViewBackend.cs
//
// Author:
//       Vsevolod Kukol <sevoku@microsoft.com>
//
// Copyright (c) 2016 (c) Vsevolod Kukol
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
using SWC = System.Windows.Controls;
using Xwt.Backends;

namespace Xwt.WPFBackend
{
	class RadioButtonCellViewBackend : CellViewBackend
	{
		public override void OnInitialize(CellView cellView, FrameworkElementFactory factory)
		{
			factory.AddHandler (UngroupedRadioButton.ToggleEvent, new RoutedEventHandler (HandleToggled));
			base.OnInitialize(cellView, factory);
		}

		void HandleToggled(object sender, RoutedEventArgs e)
		{
			var view = (IRadioButtonCellViewFrontend)CellView;
			Load(sender as FrameworkElement);
			SetCurrentEventRow();
			e.Handled = view.RaiseToggled();
		}
	}

	class UngroupedRadioButton : SWC.RadioButton
	{
		public UngroupedRadioButton()
		{
			GroupName = Guid.NewGuid().ToString();
		}

		public static readonly RoutedEvent ToggleEvent = EventManager.RegisterRoutedEvent("Toggle", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UngroupedRadioButton));

		public event RoutedEventHandler Toggle
		{
			add { AddHandler(ToggleEvent, value); }
			remove { RemoveHandler(ToggleEvent, value); }
		}

		protected virtual void OnToggle(RoutedEventArgs e)
		{
			RaiseEvent(e);
		}

		protected override void OnToggle()
		{
			var args = new RoutedEventArgs(ToggleEvent);
			OnToggle(args);
			if (!args.Handled)
				base.OnToggle();
		}
	}
}
