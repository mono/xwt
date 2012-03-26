// 
// MenuItemBackend.cs
//  
// Author:
//       Carlos Alberto Cortez <calberto.cortez@gmail.com>
//       Eric Maupin <ermau@xamarin.com>
// 
// Copyright (c) 2011 Carlos Alberto Cortez
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using SWC = System.Windows.Controls;
using SWMI = System.Windows.Media.Imaging;
using Xwt.Backends;
using Xwt.Engine;

namespace Xwt.WPFBackend
{
	public class MenuItemBackend : IMenuItemBackend
	{
		object item;
		SWC.MenuItem menuItem;
		MenuBackend subMenu;
		MenuItemType type;
		IMenuItemEventSink eventSink;

		public MenuItemBackend ()
			: this (new SWC.MenuItem())
		{
		}

		protected MenuItemBackend (object item)
		{
			this.item = item;
			this.menuItem = item as SWC.MenuItem;
		}

		public void Initialize (IMenuItemEventSink eventSink)
		{
			this.eventSink = eventSink;
		}

		public void Initialize (object frontend)
		{
		}

		public object Item {
			get { return this.item; }
		}

		public SWC.MenuItem MenuItem {
			get { return this.menuItem; }
		}

		public IMenuItemEventSink EventSink {
			get { return eventSink; }
		}

		public bool Checked {
			get { return this.menuItem.IsCheckable && this.menuItem.IsChecked; }
			set {
				if (!this.menuItem.IsCheckable)
					return;
				this.menuItem.IsChecked = value;
			}
		}

		public string Label {
			get { return this.menuItem.Header.ToString (); }
			set { this.menuItem.Header = value; }
		}

		public bool Sensitive {
			get { return this.menuItem.IsEnabled; }
			set { this.menuItem.IsEnabled = value; }
		}

		public bool Visible {
			get { return this.menuItem.IsVisible; }
			set { this.menuItem.Visibility = (value) ? Visibility.Visible : Visibility.Collapsed; }
		}

		public void SetImage (object imageBackend)
		{
			if (imageBackend == null)
				this.menuItem.Icon = null;
			else
			{
				var img = (SWMI.BitmapSource) imageBackend;
				this.menuItem.Icon = new System.Windows.Controls.Image
				{
					Source = img,
					Width = img.Width,
					Height = img.Height
				};
			}
		}

		public void SetSubmenu (IMenuBackend menu)
		{
			if (menu == null) {
				this.menuItem.Items.Clear ();
				if (subMenu != null) {
					subMenu.RemoveFromParentItem ();
					subMenu = null;
				}

				return;
			}

			var menuBackend = (MenuBackend)menu;
			menuBackend.RemoveFromParentItem ();

			foreach (var itemBackend in menuBackend.Items)
				this.menuItem.Items.Add (itemBackend.Item);

			menuBackend.ParentItem = this;
			subMenu = menuBackend;
		}

		public void SetType (MenuItemType type)
		{
			switch (type) {
				case MenuItemType.RadioButton:
				case MenuItemType.CheckBox:
					this.menuItem.IsCheckable = true;
					break;
				case MenuItemType.Normal:
					this.menuItem.IsCheckable = false;
					break;
			}

			this.type = type;
		}

		public void EnableEvent (object eventId)
		{
			if (eventId is MenuItemEvent) {
				switch ((MenuItemEvent)eventId) {
					case MenuItemEvent.Clicked:
						this.menuItem.Click += MenuItemClickHandler;
						break;
				}
			}
		}

		public void DisableEvent (object eventId)
		{
			if (eventId is MenuItemEvent) {
				switch ((MenuItemEvent)eventId) {
					case MenuItemEvent.Clicked:
						this.menuItem.Click -= MenuItemClickHandler;
						break;
				}
			}
		}

		void MenuItemClickHandler (object sender, EventArgs args)
		{
			Toolkit.Invoke (eventSink.OnClicked);
		}
	}
}
