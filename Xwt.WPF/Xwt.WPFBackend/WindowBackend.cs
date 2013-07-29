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
		DockPanel contentBox;

		public WindowBackend ()
		{
			Window = new WpfWindow ();
			Window.UseLayoutRounding = true;
			rootPanel = CreateMainGrid ();
			contentBox = new DockPanel ();

			Window.Content = rootPanel;
			Grid.SetColumn (contentBox, 0);
			Grid.SetRow (contentBox, 1);
			rootPanel.Children.Add (contentBox);
		}

		public override void Initialize ()
		{
			base.Initialize ();
			((WpfWindow)Window).Frontend = (Window) Frontend;
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
			if (widget != null) {
				contentBox.Children.Remove (widget);
				widget.SizeChanged -= ChildSizeChanged;
			}
			widget = ((IWpfWidgetBackend)child).Widget;
			contentBox.Children.Add (widget);

			// This event is subscribed to ensure that the content of the
			// widget is reallocated when the widget gets a new size. This
			// is not a problem when setting the child before showing the
			// window, but it may be a problem if the window is already visible.
			widget.SizeChanged += ChildSizeChanged;

			if (child != null)
				UpdateChildPlacement (child);
		}

		public virtual void UpdateChildPlacement (IWidgetBackend childBackend)
		{
			WidgetBackend.SetChildPlacement (childBackend);
		}

		void ChildSizeChanged (object o, SizeChangedEventArgs args)
		{
			((Window)Frontend).Content.Surface.Reallocate ();
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
			contentBox.Margin = new Thickness (left, top, right, bottom);
		}

		public virtual void SetMinSize (Size s)
		{
			var r = ToNonClientRect (new Rectangle (0, 0, s.Width, s.Height));
			Window.MinHeight = r.Height;
			Window.MinWidth = r.Width;
		}

		public virtual void GetMetrics (out Size minSize, out Size decorationSize)
		{
			minSize = decorationSize = Size.Zero;
		}
	}

	class WpfWindow : System.Windows.Window
	{
		public Window Frontend;

		protected override System.Windows.Size ArrangeOverride (System.Windows.Size arrangeBounds)
		{
			var s = base.ArrangeOverride (arrangeBounds);
			if (Frontend.Content != null)
				Frontend.Content.Surface.Reallocate ();
			return s;
		}
	}
}
