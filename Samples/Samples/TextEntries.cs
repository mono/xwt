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
			TextEntry te = new TextEntry ();
			PackStart (te);
			
			Label la = new Label ();
			PackStart (la);
			te.Changed += delegate {
				la.Text = "Text: " + te.Text;
			};
			
			PackStart (new Label ("Entry with small font"));
			te = new TextEntry ();
			te.Font = te.Font.WithSize (te.Font.Size / 2);
			PackStart (te);
			
			PackStart (new Label ("Entry with placeholder text"));
			te = new TextEntry ();
			te.PlaceholderText = "Placeholder text";
			PackStart (te);

			PackStart (new Label ("Entry with no frame"));
			te = new TextEntry();
			te.ShowFrame = false;
			PackStart (te);
		}
	}
}

