// 
// NotebookBackend.cs
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
using MonoMac.AppKit;
using Xwt.Backends;
using System.Collections.Generic;

namespace Xwt.Mac
{
	public class NotebookBackend: ViewBackend<NSTabView,IWidgetEventSink>, INotebookBackend
	{
		public NotebookBackend ()
		{
			ViewObject = new TabView ();
			Widget.AutoresizesSubviews = true;
		}

		#region INotebookBackend implementation
		public void Add (IWidgetBackend widget, NotebookTab tab)
		{
			NSTabViewItem item = new NSTabViewItem ();
			item.Label = tab.Label;
			item.View = GetWidget (widget);
			Widget.Add (item);
		}

		public void Remove (IWidgetBackend widget)
		{
			var v = GetWidget (widget);
			foreach (var t in Widget.Items) {
				if (t.View == v) {
					Widget.Remove (t);
					return;
				}
			}
		}
		#endregion
	}
	
	class TabView: NSTabView, IViewObject<NSTabView>
	{
		public Widget Frontend { get; set; }
		public NSTabView View {
			get { return this; }
		}
	}
}

