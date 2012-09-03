//
// PopoverSample.cs
//
// Author:
//       Jérémie Laval <jeremie.laval@xamarin.com>
//
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
using Xwt;

namespace Samples
{
	public class PopoverSample : VBox
	{
		Popover popover;

		public PopoverSample ()
		{
			var btn = new Button ("Click me");
			btn.Clicked += HandleClicked;
			PackStart (btn);
		}
	
		void HandleClicked (object sender, EventArgs e)
		{
			if (popover == null) {
				popover = new Popover ();

				var table = new Table () { DefaultColumnSpacing = 20, DefaultRowSpacing = 10 };
//					table.Margin.SetAll (60);
				table.Attach (new Label ("Font") { TextAlignment = Alignment.End }, 0, 0);
				table.Attach (new ComboBox (), 1, 0, AttachOptions.Fill, AttachOptions.Fill | AttachOptions.Expand);

				table.Attach (new Label ("Family")  { TextAlignment = Alignment.End }, 0, 1);
				table.Attach (new ComboBox (), 1, 1, AttachOptions.Fill, AttachOptions.Fill | AttachOptions.Expand);

				table.Attach (new Label ("Style")  { TextAlignment = Alignment.End }, 0, 2);
				table.Attach (new ComboBox (), 1, 2, AttachOptions.Fill, AttachOptions.Fill | AttachOptions.Expand);

				table.Attach (new Label ("Size")  { TextAlignment = Alignment.End }, 0, 3);
				table.Attach (new SpinButton (), 1, 3, AttachOptions.Fill, AttachOptions.Fill | AttachOptions.Expand);

				var b = new Button ("Add more");
				table.Attach (b, 0, 4);
				int next = 5;
				b.Clicked += delegate {
					table.Attach (new Label ("Row " + next), 0, next++);
				};

				table.Margin.SetAll (20);
				popover.Content = table;
			}
//			popover.Padding.SetAll (20);
			popover.Show (Popover.Position.Top, (Button)sender);
		}
	}
}

