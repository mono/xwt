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
using System.Windows.Controls;
using SWC = System.Windows.Controls;
using SWMI = System.Windows.Media.Imaging;
using Xwt.Backends;
using Xwt.WPFBackend.Utilities;
using WindowsOrientation = System.Windows.Controls.Orientation;

namespace Xwt.WPFBackend
{
	public class MenuItemBackend : WidgetBackend, IMenuItemBackend
	{
		UIElement item;
		SWC.MenuItem menuItem;
		MenuBackend subMenu;
		MenuItemType type;
		IMenuItemEventSink eventSink;
		string label;
		bool useMnemonic;

		public MenuItemBackend ()
			: this (new SWC.MenuItem ())
		{
		}

		protected MenuItemBackend (UIElement item)
		{
			this.item = item;
			this.menuItem = item as SWC.MenuItem;
			useMnemonic = true;
		}

		public void Initialize (IMenuItemEventSink eventSink)
		{
			this.eventSink = eventSink;
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
			get {
				if (this.menuItem == null)
					return false;
				return this.menuItem.IsCheckable && this.menuItem.IsChecked;
			}
			set {
				if (this.menuItem == null || !this.menuItem.IsCheckable)
					return;
				this.menuItem.IsChecked = value;
			}
		}

		public string Label {
			get { return label; }
			set {
				label = value;
				if (this.menuItem != null && label != null) {
					var mnemonicValue = UseMnemonic ? value : value.Replace ("_", "__");
					var formattedText = FormattedText.FromMarkup (value);
					if (mnemonicValue.CompareTo (formattedText.Text) != 0) {
						var formattedLabel = new Label (mnemonicValue) { Markup = mnemonicValue };
						this.menuItem.Header = ((Widget)formattedLabel).GetBackend ()?.NativeWidget;
					} else {
						this.menuItem.Header = mnemonicValue;
					}
				}
			}
		}

		public new string TooltipText {
			get { return menuItem.ToolTip == null ? null : ((ToolTip)menuItem.ToolTip).Content.ToString (); }
			set {
				var tp = menuItem.ToolTip as ToolTip;
				if (tp == null)
					menuItem.ToolTip = tp = new ToolTip ();
				tp.Content = value ?? string.Empty;
				ToolTipService.SetIsEnabled (menuItem, value != null);
				if (tp.IsOpen && value == null)
					tp.IsOpen = false;
			}
		}

		public bool UseMnemonic {
			get { return useMnemonic; }
			set {
				useMnemonic = value;
				Label = label;
			}
		}

		public new bool Sensitive {
			get { return this.item.IsEnabled; }
			set { this.item.IsEnabled = value; }
		}

		public new bool Visible {
			get { return this.item.IsVisible; }
			set { this.item.Visibility = (value) ? Visibility.Visible : Visibility.Collapsed; }
		}

		public void SetImage (ImageDescription imageBackend)
		{
			if (imageBackend.IsNull)
				this.menuItem.Icon = null;
			else
				this.menuItem.Icon = new ImageBox (Context) { ImageSource = imageBackend };
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

		internal void SetFont (FontData font)
		{
			MenuItem.FontFamily = font.Family;
			MenuItem.FontSize = font.GetDeviceIndependentPixelSize (MenuItem);
			MenuItem.FontStyle = font.Style;
			MenuItem.FontWeight = font.Weight;
			MenuItem.FontStretch = font.Stretch;
		}

		public override void EnableEvent (object eventId)
		{
			if (menuItem == null)
				return;

			if (eventId is MenuItemEvent) {
				switch ((MenuItemEvent)eventId) {
				case MenuItemEvent.Clicked:
					this.menuItem.Click += MenuItemClickHandler;
					break;
				}
			}
		}

		public override void DisableEvent (object eventId)
		{
			if (menuItem == null)
				return;

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
			Context.InvokeUserCode (eventSink.OnClicked);
		}

	}
}
