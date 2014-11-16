// 
// ButtonSample.cs
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
using System.Linq;

using Xwt;
using Xwt.Drawing;

namespace Samples
{
	public class ButtonSample: VBox
	{
		public class MyWidget : Widget
		{
			public new Widget Content
			{
				get { return base.Content; }
				set { base.Content = value; }
			}
		}

		public ButtonSample ()
		{
			Button b1 = new Button ("Click me");
			b1.Clicked += delegate {
				b1.Label = "Clicked!";
			};
			PackStart (b1, false, false);
			
			Button b2 = new Button ("Click me");
			b2.Style = ButtonStyle.Flat;
			b2.Clicked += delegate {
				b2.Label = "Clicked!";
			};
			PackStart (b2);

			PackStart (new Button (StockIcons.ZoomIn.WithSize (22)));
			PackStart (new Button (StockIcons.ZoomIn.WithSize (32), "Custom Size") { WidthRequest = 110, MinHeight = 50 });

			var hbox = new HBox();
			hbox.PackStart (new Button (StockIcons.ZoomIn.WithSize (22), "Zoom In") { ImagePosition = ContentPosition.Top });
			hbox.PackStart (new Button (StockIcons.ZoomOut.WithSize (22), "Zoom Out") { ImagePosition = ContentPosition.Bottom });
			hbox.PackStart (new Button (StockIcons.Information.WithSize (48), "Info") { ImagePosition = ContentPosition.Top }, true);
			hbox.PackEnd (new Button ("Custom" + Environment.NewLine + "Width") { MinWidth = 110 });
			PackStart (hbox);

			
			PackStart (new Button (new CustomImage ().WithSize (22), "with red background") { BackgroundColor = Colors.Red });

			MenuButton mb = new MenuButton ("This is a Menu Button");
			Menu men = new Menu ();
			men.Items.Add (new MenuItem ("First"));
			men.Items.Add (new MenuItem ("Second"));
			men.Items.Add (new MenuItem ("Third"));
			men.Items.Add (new SeparatorMenuItem ());
			men.Items.Add (new CheckBoxMenuItem ("Check") { Checked = true });
			men.Items.Add (new RadioButtonMenuItem ("Radio") { Checked = true });
			men.Items.Add (new MenuItem ("With image") { Image = Image.FromResource (typeof(App), "class.png") });

			mb.Menu = men;
			PackStart (mb);
			foreach (var mi in men.Items) {
				var cmi = mi;
				mi.Clicked += delegate {
					mb.Label = cmi.Label + " Clicked";
				};
			}

			ToggleButton tb1 = new ToggleButton ("Toggle me");
			ToggleButton tb2 = new ToggleButton ("Mini toggle");
			tb1.Toggled += delegate(object sender, EventArgs e) {
				if (tb1.Active)
					tb1.Label = "Toggled";
				else
					tb1.Label = "Untoggled";
				tb2.Active = tb1.Active;
			};
			PackStart (tb1);

			var b = new Button ("Mini button");
			b.Style = ButtonStyle.Borderless;
			PackStart (b);

			tb2.Style = ButtonStyle.Borderless;
			tb2.Toggled += delegate(object sender, EventArgs e) {
				if (tb2.Active)
					tb2.Label = "Toggled";
				else
					tb2.Label = "Untoggled";
				tb1.Active = tb2.Active;
			};
			PackStart (tb2);

			b = new Button (" ? ");
			b.Type = ButtonType.Help;
			PackStart (b);


			var child = new VBox ();
			var container = new MyWidget { Content = child };

			var button = new Xwt.Button ("Click to add a child");
			button.Clicked += delegate {
				child.PackStart (new Xwt.Label ("Child" + child.Children.Count ()));
			};

			var content = new Xwt.VBox ();
			content.PackStart (button);
			content.PackStart (container);

			PackStart (content);
		}
	}
}

