// 
// ScrollWindowSample.cs
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
	public class ScrollWindowSample: VBox
	{
		public ScrollWindowSample ()
		{
			ScrollView v1 = new ScrollView ();
			VBox b1 = new VBox ();
			for (int n=0; n<30; n++)
				b1.PackStart (new Label ("Line " + n), BoxMode.None);
			Button u = new Button ("Click to remove");
			u.Clicked += delegate {
				b1.Remove (u);
			};
			b1.PackStart (u);
			
			v1.Content = b1;
			v1.VerticalScrollPolicy = ScrollPolicy.Always;
			PackStart (v1, BoxMode.FillAndExpand);
			
			ScrollView v2 = new ScrollView ();
			VBox b2 = new VBox ();
			for (int n=0; n<10; n++)
				b2.PackStart (new Label ("Line " + n), BoxMode.None);
			v2.Content = b2;
			v2.VerticalScrollPolicy = ScrollPolicy.Never;
			PackStart (v2, BoxMode.FillAndExpand);
			
			ScrollView v3 = new ScrollView ();
			VBox b3 = new VBox ();
			Button b = new Button ("Click to add items");
			b.Clicked += delegate {
				for (int n=0; n<10; n++)
					b3.PackStart (new Label ("Line " + n), BoxMode.None);
			};
			b3.PackStart (b);
			v3.Content = b3;
			v3.VerticalScrollPolicy = ScrollPolicy.Automatic;
			PackStart (v3, BoxMode.FillAndExpand);
			
			ScrollView v4 = new ScrollView ();
			PackStart (v4, BoxMode.FillAndExpand);
			SimpleBox sb = new SimpleBox (1000);
			v4.Content = sb;
			v4.VerticalScrollPolicy = ScrollPolicy.Always;
		}
	}
}

