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
			Window.SizeChanged += new SizeChangedEventHandler (CheckSizeChange);
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

		public override Rectangle Bounds
		{
			get
			{
				var w = double.IsNaN (rootPanel.Width) ? rootPanel.ActualWidth : rootPanel.Width;
				var h = double.IsNaN (rootPanel.Height) ? rootPanel.ActualHeight : rootPanel.Height;
				return new Rectangle (Window.Left, Window.Top, w, h);
			}
			set
			{
				SetBounds (value, false);
			}
		}

		void SetBounds (Rectangle value, bool setMinSize)
		{
			if (setMinSize) {
				// We are changing in min size of the window. We know the min size of the content,
				// but not the min size of the frame. This will be calculated later on.
				// For now just reset any existing min size, so that the window can get the new min size.
				((WpfWindow)Window).SettingMinSize = true;
				Window.MinHeight = Window.MinHeight = 0;
			}
			var r = ToNonClientRect (new Rectangle (value.X, value.Y, 1, 1));
			Window.Top = r.Top;
			Window.Left = r.Left;

			// We don't use the size returned by ToNonClientRect because those values are not reliable (see comment in ToNonClientRect)
			// Instead we set the size of the content and we ask the window to adapt to it
			rootPanel.Width = value.Width;
			rootPanel.Height = value.Height;
			Window.SizeToContent = System.Windows.SizeToContent.WidthAndHeight;

			Context.InvokeUserCode (delegate
			{
				EventSink.OnBoundsChanged (Bounds);
			});
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
			// The provided size is the min size of the content, not the size of the frame, so we set that value to the root container
			rootPanel.MinWidth = s.Width;
			rootPanel.MinHeight = s.Height;
			var r = Bounds;
			if (r.Width < s.Width)
				r.Width = s.Width;
			if (r.Height < s.Height)
				r.Height = s.Height;
			if (r != Bounds)
				SetBounds (r, true);
		}

		void CheckSizeChange (object sender, SizeChangedEventArgs e)
		{
			// The window doesn't respect the min size of the content, so we have to check
			// that the window is big enough every time the window size changes

			var r = Bounds;
			if (r.Width < rootPanel.MinWidth)
				r.Width = rootPanel.MinWidth;
			if (r.Height < rootPanel.MinHeight)
				r.Height = rootPanel.MinHeight;

			// We reset the min size of the window if the content is too small. We also do it if the content has exactly the min size
			// because the window won't reduce the widget beyond that value (but it will still reduce the window frame, that's why we
			// have to set a min size to the frame)
			if (r != Bounds || r.Width == rootPanel.MinWidth || r.Height == rootPanel.MinHeight)
				SetBounds (r, true);
		}

		public virtual void GetMetrics (out Size minSize, out Size decorationSize)
		{
			minSize = decorationSize = Size.Zero;
			if (mainMenu != null) {
				mainMenu.InvalidateMeasure ();
				mainMenu.Measure (new System.Windows.Size (double.PositiveInfinity, double.PositiveInfinity));
				var h = mainMenu.DesiredSize.Height;
				decorationSize.Height = h;
			}
		}
	}

	class WpfWindow : System.Windows.Window
	{
		public Window Frontend;
		public bool SettingMinSize { get; set; }

		public WpfWindow ()
		{
			SizeToContent = System.Windows.SizeToContent.WidthAndHeight;
		}

		protected override System.Windows.Size ArrangeOverride (System.Windows.Size arrangeBounds)
		{
			var s = base.ArrangeOverride (arrangeBounds);
			if (Frontend.Content != null)
				Frontend.Content.Surface.Reallocate ();
			if (SizeToContent == System.Windows.SizeToContent.WidthAndHeight) {
				WPFEngine.Instance.InvokeAsync (delegate
				{
					// We were resizing the window to fit the size of the content.
					// That's now done and we can go back to Manual resize mode.
					// From now on, the content has to adapt to the size of the window.
					SizeToContent = System.Windows.SizeToContent.Manual;
					var c = (FrameworkElement)Content;
					c.Width = double.NaN;
					c.Height = double.NaN;
					if (SettingMinSize) {
						// We were setting the min size of the window. We now have a valid frame size
						// for the min size of the content, so we can set the min size of the window.
						SettingMinSize = false;
						if (c.ActualWidth <= c.MinWidth)
							MinWidth = Width;
						if (c.ActualHeight <= c.MinHeight)
							MinHeight = Height;
					}
				});
			}
			return s;
		}
	}
}
