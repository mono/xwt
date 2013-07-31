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
			Button b1 = new Button ("_Click me");
			b1.Clicked += delegate {
				b1.Label = "_Clicked!";
			};
			PackStart (b1);
			
			Button b2 = new Button ("C_lick me");
			b2.Style = ButtonStyle.Flat;
			b2.Clicked += delegate {
				b2.Label = "C_licked!";
			};
			PackStart (b2);
			
			PackStart (new Button (StockIcons.ZoomIn.WithSize (22)));
			PackStart (new Button (new CustomImage ().WithSize (22)));

			MenuButton mb = new MenuButton ("This is a Menu Button");
			Menu men = new Menu ();
			men.Items.Add (new MenuItem ("_First"));
			men.Items.Add (new MenuItem ("_Second"));
			men.Items.Add (new MenuItem ("_Third"));
			men.Items.Add (new SeparatorMenuItem ());
			men.Items.Add (new CheckBoxMenuItem ("C_heck") { Checked = true });
			men.Items.Add (new RadioButtonMenuItem ("_Radio") { Checked = true });
			men.Items.Add (new MenuItem ("With _image") { Image = Image.FromResource (typeof(App), "class.png") });

			mb.Menu = men;
			PackStart (mb);
			foreach (var mi in men.Items) {
				var cmi = mi;
				mi.Clicked += delegate {
					mb.Label = cmi.Label + " Clicked";
				};
			}
			
			ToggleButton tb = new ToggleButton ("To_ggle me");
			PackStart (tb);
			
			var b = new Button ("_Mini button");
			b.Style = ButtonStyle.Borderless;
			PackStart (b);
			
			tb = new ToggleButton ("Mi_ni toggle");
			tb.Style = ButtonStyle.Borderless;
			PackStart (tb);


			var child = new VBox ();
			var container = new MyWidget { Content = child };

			var button = new Xwt.Button ("Clic_k to add a child");
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

