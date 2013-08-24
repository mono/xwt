// 
// ButtonSample.cs
//  
// Author:
//       David Lechner <david@lechnology.com>
// 
// Copyright (c) 2013 Xamarin Inc
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

namespace Samples
{
	using System;
	using Xwt;

	/// <summary>
	/// Sample class for demonstrating use of mnemonics
	/// </summary>
	public class Mnemonics : VBox
	{
		public class MyWidget : Widget
		{
			public new Widget Content
			{
				get { return base.Content; }
				set { base.Content = value; }
			}
		}

		public Mnemonics ()
		{
			var windowsNoteLabel = new Label ()
			{
				Text = "Notes:\n" +
					"\t1) You must press the ALT key to make the mnemonics visible when using WPF backend.\n" +
					"\t2) Mnemonics are not supported on OS X.",
				Wrap = WrapMode.Word
			};
			PackStart (windowsNoteLabel);
			var withMnemonicsFrame = new Frame (GenerateFrameContents(true))
			{
				Label = "UseMnemonics == true"
			};
			PackStart (withMnemonicsFrame, true, true);
			var withoutMnemonicsFrame = new Frame (GenerateFrameContents(false))
			{
					Label = "UseMnemonics == false"
			};
			PackStart (withoutMnemonicsFrame, true, true);
		}

		VBox GenerateFrameContents (bool useMnemonics)
		{
			var statusText = useMnemonics ? "with mnemonic" : "without mnemonic";
			var vbox = new VBox ();

			var button = new Button ("_Button");
			button.UseMnemonic = useMnemonics;
			button.Clicked += (sender, e) => MessageDialog.ShowMessage (string.Format ("Button {0} clicked.", statusText));
			vbox.PackStart (button);

			var toggleButton = new ToggleButton ("_Toggle Button");
			toggleButton.UseMnemonic = useMnemonics;
			toggleButton.Clicked += (sender, e) => MessageDialog.ShowMessage (string.Format ("Toggle Button {0} clicked.", statusText));
			vbox.PackStart (toggleButton);

			var menuButton = new MenuButton ("_Menu Button");
			menuButton.UseMnemonic = useMnemonics;
			menuButton.Label = "_Menu Button";
			var firstMenuItem = new MenuItem ("_First");
			firstMenuItem.UseMnemonic = useMnemonics;
			firstMenuItem.Clicked += (sender, e) => MessageDialog.ShowMessage (string.Format ("First Menu Item {0} clicked.", statusText));
			var secondMenuItem = new MenuItem ("_Second");
			secondMenuItem.UseMnemonic = useMnemonics;
			secondMenuItem.Clicked += (sender, e) => MessageDialog.ShowMessage (string.Format ("Second Menu Item {0} clicked.", statusText));
			var menu = new Menu ();
			menu.Items.Add (firstMenuItem);
			menu.Items.Add (secondMenuItem);
			menuButton.Menu = menu;
			vbox.PackStart (menuButton);

			return vbox;
		}
	}
}
