﻿//
// DropDownButton.cs
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
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using SWC = System.Windows.Controls;

namespace Xwt.WPFBackend
{
	public class DropDownButton
		: SWC.Primitives.ToggleButton, IWpfWidget
	{
		public DropDownButton()
		{
			Checked += OnChecked;
		}

		public event EventHandler<MenuOpeningEventArgs> MenuOpening;

		public WidgetBackend Backend
		{
			get;
			set;
		}

		protected override System.Windows.Size MeasureOverride (System.Windows.Size constraint)
		{
			var s = base.MeasureOverride (constraint);
			return Backend.MeasureOverride (constraint, s);
		}

		private void OnChecked (object sender, RoutedEventArgs routedEventArgs)
		{
			if (!IsChecked.HasValue || !IsChecked.Value)
				return;

			var args = new MenuOpeningEventArgs ();

			var opening = this.MenuOpening;
			if (opening != null)
				opening (this, args);

			var menu = args.ContextMenu;
			if (menu == null) {
				IsChecked = false;
				return;
			}
			
			string text = Content as string;
			if (!String.IsNullOrWhiteSpace (text)) {
				SWC.MenuItem selected = menu.Items.OfType<SWC.MenuItem>().FirstOrDefault (i => i.Header as string == text);
				if (selected != null)
					selected.IsChecked = true;
			}

			menu.Closed += OnMenuClosed;

			menu.PlacementTarget = this;
			menu.Placement = PlacementMode.Bottom;
			menu.IsOpen = true;
		}

		private void OnMenuClosed (object sender, RoutedEventArgs e)
		{
			var menu = sender as SWC.ContextMenu;
			if (menu != null)
				menu.Closed -= OnMenuClosed;

			IsChecked = false;
		}

		public class MenuOpeningEventArgs
			: EventArgs
		{
			public SWC.ContextMenu ContextMenu
			{
				get;
				set;
			}
		}
	}
}