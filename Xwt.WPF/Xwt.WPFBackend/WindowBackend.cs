// 
// WindowBackend.cs
//  
// Author:
//       Carlos Alberto Cortez <calberto.cortez@gmail.com>
//       Luís Reis <luiscubal@gmail.com>
// 
// Copyright (c) 2011 Carlos Alberto Cortez
// Copyright (c) 2012 Luís Reis
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using Xwt.Backends;

namespace Xwt.WPFBackend
{
	public class WindowBackend : WindowFrameBackend, IWindowBackend
	{
		protected Grid rootPanel;
		public System.Windows.Controls.Menu mainMenu;
		MenuBackend mainMenuBackend;
		FrameworkElement widget;
		Thickness padding;

		public WindowBackend ()
		{
			Window = new System.Windows.Window ();
			Window.UseLayoutRounding = true;
			rootPanel = CreateMainGrid ();

			Window.Content = rootPanel;
		}

		// A Grid with a single column, and two rows (menu and child control).
		static Grid CreateMainGrid ()
		{
			var grid = new Grid ();

			grid.ColumnDefinitions.Add (new ColumnDefinition ());
			
			var menuRow = new RowDefinition () { Height = GridLength.Auto }; // Only take the menu requested space.
			var contentRow = new RowDefinition (); // Take all the remaining space (default).

			grid.RowDefinitions.Add (menuRow);
			grid.RowDefinitions.Add (contentRow);

			return grid;
		}

		public override bool HasMenu {
			get { return mainMenu != null; }
		}

		public void SetChild (IWidgetBackend child)
		{
			if (widget != null)
				rootPanel.Children.Remove(widget);
			widget = ((IWpfWidgetBackend)child).Widget;

			widget.Margin = padding;
			Grid.SetColumn (widget, 0);
			Grid.SetRow (widget, 1);

			rootPanel.Children.Add (widget);
		}

		public void SetMainMenu (IMenuBackend menu)
		{
			if (mainMenu != null) {
				mainMenuBackend.ParentWindow = null;
				rootPanel.Children.Remove (mainMenu);
			}
		
			if (menu == null) {
				mainMenu = null;
				mainMenuBackend = null;
				return;
			}

			var menuBackend = (MenuBackend)menu;

			var m = new System.Windows.Controls.Menu ();
			foreach (var item in menuBackend.Items)
				m.Items.Add (item.Item);

			Grid.SetColumn (m, 0);
			Grid.SetRow (m, 0);
			rootPanel.Children.Add (m);

			mainMenu = m;
			mainMenuBackend = menuBackend;
			mainMenuBackend.ParentWindow = this;
		}

		public void SetPadding (double left, double top, double right, double bottom)
		{
			padding = new Thickness (left, top, right, bottom);
			if (widget != null)
				widget.Margin = padding;
		}

		public virtual void SetMinSize (Size s)
		{
			var r = ToNonClientRect (new Rectangle (0, 0, s.Width, s.Height));
			Window.MinHeight = r.Height;
			Window.MinWidth = r.Width;
		}

		public virtual Size ImplicitMinSize {
			get { return new Size (0,0); }
		}
	}
}
