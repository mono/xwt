// 
// WindowBackend.cs
//  
// Author:
//       Carlos Alberto Cortez <calberto.cortez@gmail.com>
// 
// Copyright (c) 2011 Carlos Alberto Cortez
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
		DockPanel rootPanel;
		System.Windows.Controls.Menu mainMenu;
		MenuBackend mainMenuBackend;

		public WindowBackend ()
		{
			Window = new System.Windows.Window ();
			rootPanel = new DockPanel ();

			Window.Content = rootPanel;
		}

		public void SetChild (IWidgetBackend child)
		{
			var widget = ((IWpfWidgetBackend)child).Widget;

			DockPanel.SetDock (widget, Dock.Bottom);
			Window.Content = widget;
		}

		public void SetMainMenu (IMenuBackend menu)
		{
			if (menu == null) {
				if (mainMenu != null) {
					rootPanel.Children.Remove (mainMenu);
					mainMenu = null;
					mainMenuBackend = null;
				}
				return;
			}

			var menuBackend = (MenuBackend)menu;

			var m = new System.Windows.Controls.Menu ();
			foreach (var item in menuBackend.Items)
				m.Items.Add (item.MenuItem);

			DockPanel.SetDock (m, Dock.Top);
			rootPanel.Children.Add (m);

			mainMenu = m;
			mainMenuBackend = menuBackend;
		}

		public void SetPadding (double left, double top, double right, double bottom)
		{
			Window.Padding = new Thickness (left, top, right, bottom);
		}
	}
}
