// 
// CustomWidgetBackend.cs
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
	public class CustomWidgetBackend: WidgetBackend, ICustomWidgetBackend
	{
		public CustomWidgetBackend ()
		{
		}

		public override void Initialize ()
		{
			Widget = new Gtk.EventBox ();
			Widget.Show ();
		}

		WidgetBackend childBackend;
		
		protected new Gtk.EventBox Widget {
			get { return (Gtk.EventBox)base.Widget; }
			set { base.Widget = value; }
		}

		public override Size GetPreferredSize (SizeConstraint widthConstraint, SizeConstraint heightConstraint)
		{
			if (childBackend == null)
				return base.GetPreferredSize (widthConstraint, heightConstraint);
			var size = childBackend.Frontend.Surface.GetPreferredSize (widthConstraint, heightConstraint, true);
			return size;
		}


		public void SetContent (IWidgetBackend widget)
		{
			childBackend = (WidgetBackend)widget;

			var newWidget = GetWidgetWithPlacement (widget);
			var oldWidget = Widget.Child;

			if (oldWidget == null)
				Widget.Child = newWidget;
			else {
				GtkEngine.ReplaceChild (oldWidget, newWidget);
				RemoveChildPlacement (oldWidget);
			}
		}
	}
}

