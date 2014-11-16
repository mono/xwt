// 
// TextEntries.cs
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
using Xwt;

namespace Samples
{
	public class TextEntries: VBox
	{
		public TextEntries ()
		{
			TextEntry te1 = new TextEntry ();
			PackStart (te1);
			te1.BackgroundColor = Xwt.Drawing.Colors.Red;
			
			Label la = new Label ();
			PackStart (la);
			te1.Changed += delegate {
				la.Text = "Text: " + te1.Text;
			};

			HBox selBox = new HBox ();

			Label las = new Label ("Selection:");
			selBox.PackStart (las);
			Button selReplace = new Button ("Replace");
			selReplace.Clicked += delegate {
				te1.SelectedText = "[TEST]";
			};
			selBox.PackEnd (selReplace);
			Button selAll = new Button ("Select all");
			selAll.Clicked += delegate {
				te1.SelectionStart = 0;
				te1.SelectionLength = te1.Text.Length;
			};
			selBox.PackEnd (selAll);
			Button selPlus = new Button ("+");
			selPlus.Clicked += delegate {
				te1.SelectionLength++;
			};
			selBox.PackEnd (selPlus);
			Button selRight = new Button (">");
			selRight.Clicked += delegate {
				te1.SelectionStart++;
			};
			selBox.PackEnd (selRight);
			PackStart (selBox);

			te1.SelectionChanged += delegate {
				las.Text = "Selection: (" + te1.CursorPosition + " <-> " + te1.SelectionStart + " + " + te1.SelectionLength + ") " + te1.SelectedText;
			};
			
			PackStart (new Label ("Entry with small font"));
			TextEntry te2 = new TextEntry ();
			te2.Font = te2.Font.WithScaledSize (0.5);
			te2.PlaceholderText = "Placeholder text";
			PackStart (te2);

			PackStart (new TextEntry { Text = "Entry with custom height", MinHeight = 50 });

			PackStart (new TextEntry { Text = "Readonly text", ReadOnly = true });
			
			PackStart (new Label ("Entry with placeholder text"));
			TextEntry te3 = new TextEntry ();
			te3.PlaceholderText = "Placeholder text";
			PackStart (te3);

			PackStart (new Label ("Entry with no frame"));
			TextEntry te4 = new TextEntry();
			te4.ShowFrame = false;
			PackStart (te4);

			PackStart (new Label ("Entry with custom frame"));
			FrameBox teFrame = new FrameBox ();
			teFrame.BorderColor = Xwt.Drawing.Colors.Red;
			teFrame.BorderWidth = 1;
			teFrame.Content = new TextEntry () { ShowFrame = false };
			PackStart (teFrame);

			TextEntry te5 = new TextEntry ();
			te5.Text = "I should be centered!";
			te5.TextAlignment = Alignment.Center;
			te5.PlaceholderText = "Placeholder text";
			PackStart (te5);

			TextEntry te6 = new TextEntry ();
			te6.Text = "I should have" + Environment.NewLine + "multiple lines!";
			te6.PlaceholderText = "Placeholder text";
			te6.MultiLine = true;
			te6.MinHeight = 40;
			PackStart (te6);

			try {
				SearchTextEntry te7 = new SearchTextEntry ();
				te7.PlaceholderText = "Type to search ...";
				PackStart (te7);

				SearchTextEntry te8 = new SearchTextEntry ();
				te8.PlaceholderText = "I should have no frame";
				te8.ShowFrame = false;
				PackStart (te8);
			} catch (InvalidOperationException ex) {
				Console.WriteLine (ex);
			}
		}
	}
}

