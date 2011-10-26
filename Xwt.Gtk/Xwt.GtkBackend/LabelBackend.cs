// 
// LabelBackend.cs
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
using Xwt.Backends;


namespace Xwt.GtkBackend
{
	class LabelBackend<T,S>: WidgetBackend<T,S>, ILabelBackend where T:Gtk.Label where S:IWidgetEventSink
	{
		public LabelBackend ()
		{
			Widget = (T) new Gtk.Label ();
			Widget.Show ();
			Widget.Xalign = 0;
			Widget.Yalign = 0;
		}
		
		public string Text {
			get { return Widget.Text; }
			set { Widget.Text = value; }
		}

		public Alignment HorizontalAlignment {
			get {
				if (Widget.Xalign == 0)
					return Alignment.Start;
				else if (Widget.Xalign == 1)
					return Alignment.End;
				else
					return Alignment.Center;
			}
			set {
				switch (value) {
				case Alignment.Start: Widget.Xalign = 0; break;
				case Alignment.End: Widget.Xalign = 1; break;
				case Alignment.Center: Widget.Xalign = 0.5f; break;
				}
			}
		}
	}
}

