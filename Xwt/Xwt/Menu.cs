// 
// Menu.cs
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
using Xwt.Engine;

namespace Xwt
{
	public class Menu: XwtComponent
	{
		MenuItemCollection items;
		
		public Menu ()
		{
			items = new MenuItemCollection (this);
		}
		
		internal IMenuBackend Backend {
			get { return (IMenuBackend) BackendHost.Backend; }
		}
		
		public MenuItemCollection Items {
			get { return items; }
		}
		
		internal void InsertItem (int n, MenuItem item)
		{
			Backend.InsertItem (n, (IMenuItemBackend)WidgetRegistry.GetBackend (item));
		}
		
		internal void RemoveItem (MenuItem item)
		{
			Backend.RemoveItem ((IMenuItemBackend)WidgetRegistry.GetBackend (item));
		}
		
		public void Popup ()
		{
			Backend.Popup ();
		}
		
		public void Popup (Widget parentWidget, double x, double y)
		{
			Backend.Popup ((IWidgetBackend)WidgetRegistry.GetBackend (parentWidget), x, y);
		}
		
		/// <summary>
		/// Removes all separators of the menu which follow another separator
		/// </summary>
		public void CollapseSeparators ()
		{
			bool wasSeparator = true;
			for (int n=0; n<Items.Count; n++) {
				if (Items[n] is SeparatorMenuItem) {
					if (wasSeparator)
						Items.RemoveAt (n--);
					else
						wasSeparator = true;
				} else
					wasSeparator = false;
			}
			if (Items.Count > 0 && Items[Items.Count - 1] is SeparatorMenuItem)
				Items.RemoveAt (Items.Count - 1);
		}
	}
}

