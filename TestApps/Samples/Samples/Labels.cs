// 
// Labels.cs
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
	public class Labels: VBox
	{
		public Labels ()
		{
			Label la = new Label ("Simple label");
			PackStart (la);

			la = new Label ("Centered Text, Centered Text, Centered Text, Centered Text, Centered Text, Centered Text, Centered Text");
			la.Wrap = WrapMode.Word;
			la.TextAlignment = Alignment.Center;
			PackStart (la);

			la = new Label ("Centered Ellipsized Text, Centered Ellipsized Text, Centered Ellipsized Text, Centered Ellipsized Text, Centered Ellipsized Text");
			la.TextAlignment = Alignment.Center;
			la.Ellipsize = EllipsizeMode.End;
			PackStart (la);

			la = new Label ("Left Aligned, Left Aligned, Left Aligned, Left Aligned, Left Aligned, Left Aligned, Left Aligned, Left Aligned");
			la.Wrap = WrapMode.Word;
			la.TextAlignment = Alignment.Start;
			PackStart (la);

			la = new Label ("Right Aligned, Right Aligned, Right Aligned, Right Aligned, Right Aligned, Right Aligned, Right Aligned");
			la.Wrap = WrapMode.Word;
			la.TextAlignment = Alignment.End;
			PackStart (la);

			la = new Label ("Label with red background") {
				BackgroundColor = new Xwt.Drawing.Color (1, 0, 0),
				HeightRequest = 40
			};
			PackStart (la);

			la = new Label ("Label with red background and blue foreground") {
				BackgroundColor = new Xwt.Drawing.Color (1, 0, 0),
				TextColor = new Xwt.Drawing.Color (0, 0, 1)
			};
			PackStart (la);

			la = new Label ("A crazy long label text with a lots of content and information in it but fortunately it should appear wrapped");
			la.Wrap = WrapMode.Word;
			PackStart (la);

			la = new Label ("Another Label with red background") {
				BackgroundColor = new Xwt.Drawing.Color (1, 0, 0),
				TextColor = new Xwt.Drawing.Color (0, 0, 1)
			};
			PackStart (la);

			la = new Label () { Markup = "Label with <b>bold</b> and <span size=\"xx-small\" font=\"Arial\" color='#ff0000'>red</span> <span size=\"xx-large\" font=\"Tahoma\" color='#00ff00'>green</span> <span size=\"7.3\" color='#0000ff'>blue PT</span> <span size=\"10720\" font=\"Tahoma\" color='#ff0000'>Red Pango</span> text" };
			PackStart (la);
			
			la = new Label () { Markup = "Label with a <a href='http://xamarin.com'>link</a> to a web site." };
			PackStart (la);
		}
	}
}

