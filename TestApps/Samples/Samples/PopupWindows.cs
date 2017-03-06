//
// PopupWindows.cs
//
// Author:
//       Vsevolod Kukol <sevoku@microsoft.com>
//
// Copyright (c) 2017 (c) Microsoft Corporation
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
using Xwt;
using Xwt.Drawing;

namespace Samples
{
	public class PopupWindows : VBox
	{
		public PopupWindows ()
		{
			var btn1 = new Button ("Show Utility Window");
			UtilityWindow utility = null;
			btn1.Clicked += (sender, e) => {
				if (utility == null) {
					utility = new UtilityWindow ();
					utility.TransientFor = ParentWindow;
					utility.InitialLocation = WindowLocation.CenterParent;
					utility.Title = "Utility";
					var content = new VBox ();
					content.PackStart (new Label ("Utility Window"));
					var btn = new Button ("Close");
					btn.Clicked += delegate { utility.Close (); };
					content.PackStart (btn);
					utility.Content = content;
				}
				utility.Show ();
			};

			PackStart (btn1);


			var btn2 = new Button ("Show custom Tooltip Window");
			PopupWindow popup2 = null;
			btn2.Clicked += (sender, e) => {
				if (popup2 == null) {
					popup2 = new PopupWindow (PopupWindow.PopupType.Tooltip);
					popup2.TransientFor = ParentWindow;
					popup2.Decorated = false;
					popup2.Title = "Tooltip";
					popup2.BackgroundColor = Colors.Blue.WithAlpha (0.7);
					var l = new Label ("Tooltip Window");
					l.Margin = 5;
					l.TextColor = Colors.White;
					l.MouseExited += (sl, el) => popup2.Hide ();
					l.VerticalPlacement = l.HorizontalPlacement = WidgetPlacement.Center;
					l.Margin = new WidgetSpacing (5, 4, 5, 4);

					popup2.Content = l;
				}
				if (!popup2.Visible) {
					btn2.Label = "Hide custom Tooltip Window";
					popup2.Show ();
				} else {
					btn2.Label = "Show custom Tooltip Window";
					popup2.Hide ();
				}
			};

			PackStart (btn2);
		}
	}
}
