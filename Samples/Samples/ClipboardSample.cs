// 
// Clipboard.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2012 Xamarin Inc
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
	public class ClipboardSample: VBox
	{
		public ClipboardSample ()
		{
			HBox box = new HBox ();
			var source = new TextEntry ();
			box.PackStart (source);
			Button b = new Button ("Copy");
			box.PackStart (b);
			b.Clicked += delegate {
				Clipboard.SetText (source.Text);
			};
			PackStart (box);
			
			box = new HBox ();
			var dest = new TextEntry ();
			box.PackStart (dest);
			b = new Button ("Paste");
			box.PackStart (b);
			b.Clicked += delegate {
				dest.Text = Clipboard.GetText ();
			};
			PackStart (box);
			PackStart (new HSeparator ());
			
			box = new HBox ();
			b = new Button ("Copy complex object");
			box.PackStart (b);
			int n = 0;
			b.Clicked += delegate {
				Clipboard.SetData (new ComplexObject () { Data = "Hello world " + (++n) });
			};
			PackStart (box);
			
			box = new HBox ();
			var destComplex = new TextEntry ();
			box.PackStart (destComplex);
			b = new Button ("Paste complex object");
			box.PackStart (b);
			b.Clicked += delegate {
				ComplexObject ob = Clipboard.GetData<ComplexObject> ();
				if (ob != null)
					destComplex.Text = ob.Data;
				else
					destComplex.Text = "Data not found";
			};
			PackStart (box);
		}
	}
	
	[Serializable]
	class ComplexObject
	{
		public string Data;
	}
}

