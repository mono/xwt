// 
// MenuBackend.cs
//  
// Author:
//       Carlos Alberto Cortez <calberto.cortez@gmail.com>
//       Luís Reis <luiscubal@gmail.com>
//       Eric Maupin <ermau@xamarin.com>
// 
// Copyright (c) 2011 Carlos Alberto Cortez
// Copyright (c) 2012 Luís Reis
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

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Xwt.Backends;

namespace Xwt.WPFBackend
{
	public class MenuBackend : Backend, IMenuBackend
	{
		List<MenuItemBackend> items;

		public override void InitializeBackend (object frontend, ApplicationContext context)
		{
			base.InitializeBackend (frontend, context);
			items = new List<MenuItemBackend> ();
		}

		public IList<MenuItemBackend> Items {
			get {
				return items;
			}
		}

		public MenuItemBackend ParentItem {
			get;
			set;
		}

		public WindowBackend ParentWindow {
			get;
			set;
		}

		public void InsertItem (int index, IMenuItemBackend item)
		{
			var itemBackend = (MenuItemBackend)item;
			items.Insert (index, itemBackend);
			if (ParentItem != null && ParentItem.MenuItem != null)
				ParentItem.MenuItem.Items.Insert (index, itemBackend.Item);
			else if (ParentWindow != null)
				ParentWindow.mainMenu.Items.Insert (index, itemBackend.Item);
			else if (this.menu != null)
				this.menu.Items.Insert (index, itemBackend.Item);
		}

		public void RemoveItem (IMenuItemBackend item)
		{
			var itemBackend = (MenuItemBackend)item;
			items.Remove (itemBackend);
			if (ParentItem != null)
				ParentItem.MenuItem.Items.Remove (itemBackend.Item);
			else if (ParentWindow != null)
				ParentWindow.mainMenu.Items.Remove (itemBackend.Item);
			else if (this.menu != null)
				this.menu.Items.Remove (itemBackend.Item);
		}

		public void RemoveFromParentItem ()
		{
			if (ParentItem == null)
				return;

			ParentItem.MenuItem.Items.Clear ();
			ParentItem = null;
		}

		public void Popup ()
		{
			var menu = CreateContextMenu ();
			menu.Placement = PlacementMode.MousePoint;
			menu.IsOpen = true;
		}

		public void Popup (IWidgetBackend widget, double x, double y)
		{
			var menu = CreateContextMenu ();
			menu.PlacementTarget = (UIElement) widget.NativeWidget;
			menu.Placement = PlacementMode.Relative;

			double hratio = 1;
			double vratio = 1;
			PresentationSource source = PresentationSource.FromVisual ((Visual)widget.NativeWidget);
			if (source != null) {
				Matrix m = source.CompositionTarget.TransformToDevice;
				hratio = m.M11;
				vratio = m.M22;
			}

			menu.HorizontalOffset = x * hratio;
			menu.VerticalOffset = y * vratio;
			menu.IsOpen = true;
		}

		private ContextMenu menu;
		internal ContextMenu CreateContextMenu()
		{
			if (this.menu == null) {
				this.menu = new ContextMenu ();
				foreach (var item in Items)
					this.menu.Items.Add (item.Item);
			}

			return menu;
		}
	}
}
