// 
// Designer.cs
//  
// Author:
//       lluis <${AuthorEmail}>
// 
// Copyright (c) 2011 lluis
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
using Xwt.Design;

namespace Samples
{
	public class Designer: VBox
	{
		public Designer ()
		{
			VBox box = new VBox ();
			Button b = new Button ("Hi there");
			box.PackStart (b);
			Label la = new Label ("Some label");
			box.PackStart (la);
			HBox hb = new HBox ();
			hb.PackStart (new Label ("Text"));
			var cb = new ComboBox ();
			cb.Items.Add ("One");
			cb.Items.Add ("Two");
			cb.SelectedIndex = 0;
			hb.PackStart (cb);
			box.PackStart (hb);
			
			DesignerSurface ds = new DesignerSurface ();
			ds.Load (box);
			PackStart (ds, true);
		}
	}
}

