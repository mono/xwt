// 
// MenuBackend.cs
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

namespace Xwt.Mac
{
	public class MenuBackend: NSMenu, IMenuBackend
	{
		public void InsertItem (int index, IMenuItemBackend menuItem)
		{
			InsertItematIndex ((NSMenuItem)menuItem, index);
		}

		public void RemoveItem (IMenuItemBackend menuItem)
		{
			RemoveItem ((NSMenuItem)menuItem);
		}

		public void Initialize (object frontend)
		{
		}

		public void EnableEvent (object eventId)
		{
		}

		public void DisableEvent (object eventId)
		{
		}
		
		public void Popup ()
		{
			throw new System.NotImplementedException ();
		}
		
		public void Popup (IWidgetBackend widget, double x, double y)
		{
			throw new System.NotImplementedException ();
		}
	}
}

