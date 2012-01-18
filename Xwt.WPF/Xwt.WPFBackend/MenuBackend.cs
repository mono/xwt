// 
// MenuBackend.cs
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

using Xwt.Backends;

namespace Xwt.WPFBackend
{
	public class MenuBackend : IMenuBackend
	{
		List<MenuItemBackend> items;

		public void Initialize (object frontend)
		{
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
			if (ParentItem != null)
				ParentItem.MenuItem.Items.Insert (index, itemBackend.MenuItem);
			else if (ParentWindow != null)
				ParentWindow.mainMenu.Items.Insert (index, itemBackend.MenuItem);
		}

		public void RemoveItem (IMenuItemBackend item)
		{
			var itemBackend = (MenuItemBackend)item;
			items.Remove (itemBackend);
			if (ParentItem != null)
				ParentItem.MenuItem.Items.Remove (itemBackend.MenuItem);
			else if (ParentWindow != null)
				ParentWindow.mainMenu.Items.Remove (itemBackend.MenuItem);
		}

		public void RemoveFromParentItem ()
		{
			if (ParentItem == null)
				return;

			ParentItem.MenuItem.Items.Clear ();
			ParentItem = null;
		}

		public void EnableEvent (object eventId)
		{
		}

		public void DisableEvent (object eventId)
		{
		}
	}
}
