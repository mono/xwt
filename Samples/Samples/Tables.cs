// 
// Tables.cs
//  
// Author:
//       Lluis Sanchez Gual <lluis@xamarin.com>
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
	public class Tables: VBox
	{
		public Tables ()
		{
			Table t = new Table ();
			
			SimpleBox b = new SimpleBox (200, 20);
			t.Attach (b, 0, 1, 0, 1);
			
			b = new SimpleBox (5, 20);
			t.Attach (b, 1, 2, 0, 1);
			
			b = new SimpleBox (250, 20);
			t.Attach (b, 0, 2, 1, 2, AttachOptions.Expand, AttachOptions.Expand);
			
			b = new SimpleBox (300, 20);
			t.Attach (b, 1, 3, 2, 3);
			
			b = new SimpleBox (100, 20);
			t.Attach (b, 2, 3, 3, 4);
			
			b = new SimpleBox (450, 20);
			t.Attach (b, 0, 3, 4, 5);
			
			PackStart (t);
			
			HBox box = new HBox ();
			PackStart (box);
			t = new Table ();
			t.Attach (new Label ("One:"), 0, 1, 0, 1);
			t.Attach (new TextEntry (), 1, 2, 0, 1);
			t.Attach (new Label ("Two:"), 0, 1, 1, 2);
			t.Attach (new TextEntry (), 1, 2, 1, 2);
			t.Attach (new Label ("Three:"), 0, 1, 2, 3);
			t.Attach (new TextEntry (), 1, 2, 2, 3);
			box.PackStart (t);
		}
	}
}

