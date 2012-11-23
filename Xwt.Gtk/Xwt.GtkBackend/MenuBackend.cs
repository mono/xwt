// 
// MenuBackend.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2011 Xamarin Inc
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
using Xwt.Backends;
using System.Collections.Generic;


namespace Xwt.GtkBackend
{
	public class MenuBackend: IMenuBackend
	{
		Gtk.MenuShell menu;
		
		public void InitializeBackend (object frontend, ApplicationContext context)
		{
			menu = new Gtk.Menu ();
			menu.Visible = true;
		}

		public Gtk.Menu Menu {
			get {
				if (menu is Gtk.Menu)
					return (Gtk.Menu) menu;
				Gtk.Menu newMenu = new Gtk.Menu ();
				TransferProps (menu, newMenu);
				foreach (var it in menu.Children) {
					menu.Remove (it);
					newMenu.Add (it);
				}
				menu = newMenu;
				return newMenu;
			}
		}
		
		public Gtk.MenuBar MenuBar {
			get {
				if (menu is Gtk.MenuBar)
					return (Gtk.MenuBar) menu;
				Gtk.MenuBar bar = new Gtk.MenuBar ();
				TransferProps (menu, bar);
				foreach (var it in menu.Children) {
					menu.Remove (it);
					bar.Insert (it, -1);
				}
				menu = bar;
				return bar;
			}
		}
		
		void TransferProps (Gtk.MenuShell oldMenu, Gtk.MenuShell newMenu)
		{
			if (oldMenu.Visible)
				newMenu.Show ();
			if (!oldMenu.Sensitive)
				newMenu.Sensitive = false;
		}

		public void InsertItem (int index, IMenuItemBackend menuItem)
		{
			Gtk.MenuItem item = ((MenuItemBackend)menuItem).MenuItem;
			menu.Insert (item, index);
		}

		public void RemoveItem (IMenuItemBackend menuItem)
		{
			Gtk.MenuItem item = ((MenuItemBackend)menuItem).MenuItem;
			menu.Remove (item);
		}

		public void EnableEvent (object eventId)
		{
		}

		public void DisableEvent (object eventId)
		{
		}
		
		public void Popup ()
		{
			GtkWorkarounds.ShowContextMenu (Menu, null, Gdk.Rectangle.Zero);
		}
		
		public void Popup (IWidgetBackend widget, double x, double y)
		{
			GtkWorkarounds.ShowContextMenu (Menu, ((WidgetBackend)widget).Widget, new Gdk.Rectangle ((int)x, (int)y, 0, 0));
		}
	}
}

