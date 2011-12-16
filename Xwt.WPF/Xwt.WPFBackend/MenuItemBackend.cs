// 
// MenuItemBackend.cs
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

using Xwt.Backends;

namespace Xwt.WPFBackend
{
	public class MenuItemBackend : IMenuItemBackend
	{
		System.Windows.Controls.MenuItem item;
		MenuBackend subMenu;
		MenuItemType type;
		IMenuItemEventSink eventSink;

		public MenuItemBackend ()
		{
			item = new System.Windows.Controls.MenuItem ();
		}

		public void Initialize (IMenuItemEventSink eventSink)
		{
			this.eventSink = eventSink;
		}

		public void Initialize (object frontend)
		{
		}

		public System.Windows.Controls.MenuItem MenuItem {
			get { return item; }
		}

		public IMenuItemEventSink EventSink {
			get { return eventSink; }
		}

		public bool Checked {
			get { return item.IsCheckable && item.IsChecked; }
			set {
				if (!item.IsCheckable)
					return;
				item.IsChecked = value;
			}
		}

		public string Label {
			get { return item.Header.ToString (); }
			set { item.Header = value; }
		}

		public bool Sensitive {
			get { return item.IsEnabled; }
			set { item.IsEnabled = value; }
		}

		public bool Visible {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public void SetImage (object imageBackend)
		{
			throw new NotImplementedException ();
		}

		public void SetSeparator ()
		{
			throw new NotImplementedException ();
		}

		public void SetSubmenu (IMenuBackend menu)
		{
			if (menu == null) {
				item.Items.Clear ();
				if (subMenu != null) {
					subMenu.RemoveFromParentItem ();
					subMenu = null;
				}

				return;
			}

			var menuBackend = (MenuBackend)menu;
			menuBackend.RemoveFromParentItem ();

			foreach (var itemBackend in menuBackend.Items)
				item.Items.Add (itemBackend.MenuItem);

			menuBackend.ParentItem = this;
			subMenu = menuBackend;
		}

		public void SetType (MenuItemType type)
		{
			switch (type) {
				case MenuItemType.CheckBox:
					item.IsCheckable = true;
					break;
				case MenuItemType.Normal:
					item.IsCheckable = false;
					break;
				case MenuItemType.RadioButton:
					throw new NotImplementedException ("RadioButton type is not implemented for WPF");
			}

			this.type = type;
		}

		public void EnableEvent (object eventId)
		{
			if (eventId is MenuItemEvent) {
				switch ((MenuItemEvent)eventId) {
					case MenuItemEvent.Clicked:
						item.Click += MenuItemClickHandler;
						break;
				}
			}
		}

		public void DisableEvent (object eventId)
		{
			if (eventId is MenuItemEvent) {
				switch ((MenuItemEvent)eventId) {
					case MenuItemEvent.Clicked:
						item.Click -= MenuItemClickHandler;
						break;
				}
			}
		}

		void MenuItemClickHandler (object sender, EventArgs args)
		{
			eventSink.OnClicked ();
		}
	}
}
