// 
// MenuSamples.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2012 Xamarin Inc
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
using Xwt;

namespace Samples
{
	public class MenuSamples: VBox
	{
		Menu menu;
		
		public MenuSamples ()
		{
			Label la = new Label ("Right click here to show the context menu");
			menu = new Menu ();
			menu.Items.Add (new MenuItem ("One"));

			var menuItem = new MenuItem("Two");
			menuItem.Accessible.Label = "Menu Item: Two";
			menu.Items.Add (menuItem);

			menu.Items.Add (new MenuItem ("Three"));
			menu.Items.Add (new SeparatorMenuItem ());

			var rgroup = new RadioButtonMenuItemGroup ();
			menu.Items.Add (new RadioButtonMenuItem ("Opt 1") { Group = rgroup, Sensitive = false });
			menu.Items.Add (new RadioButtonMenuItem ("Opt 2") { Group = rgroup, Checked = true });
			menu.Items.Add (new RadioButtonMenuItem ("Opt 3") { Group = rgroup });

			menu.Items.Add (new SeparatorMenuItem ());

			menu.Items.Add (new CheckBoxMenuItem ("Check 1"));
			menu.Items.Add (new CheckBoxMenuItem ("Check 2") { Checked = true });

			menu.Items.Add (new SeparatorMenuItem ());

			var subMenu = new MenuItem ("Submenu");
			subMenu.SubMenu = new Menu ();
			subMenu.SubMenu.Font = subMenu.SubMenu.Font.WithSize (20).WithWeight (Xwt.Drawing.FontWeight.Bold);
			var subZoomIn = new MenuItem (new Command ("Zoom+", StockIcons.ZoomIn));
			var subZoomOut = new MenuItem (new Command ("Zoom-", StockIcons.ZoomOut));
			subMenu.SubMenu.Items.Add (subZoomIn);
			subMenu.SubMenu.Items.Add (subZoomOut);
			menu.Items.Add (subMenu);

			subZoomIn.Clicked += (sender, e) => MessageDialog.ShowMessage ("'Zoom+' item clicked.");
			subZoomOut.Clicked += (sender, e) => MessageDialog.ShowMessage ("'Zoom-' item clicked.");

			la.ButtonPressed += HandleButtonPressed;
			PackStart (la);
		}

		void HandleButtonPressed (object sender, ButtonEventArgs e)
		{
			if (e.Button == PointerButton.Right) {
				menu.Popup();
				menu.Accessible.Label = "Menu";
				menu.Accessible.Identifier = nameof(menu);
			}
		}
	}
}

