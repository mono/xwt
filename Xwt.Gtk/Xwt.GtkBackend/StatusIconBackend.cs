// 
// StatusIconBackend.cs
//  
// Author:
//       Antony Denyer <antonydenyer@gmail.com>
// 
// Copyright (c) 2012 7digital Media Ltd
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
using Gdk;

namespace Xwt.GtkBackend
{
	public class StatusIconBackend : IStatusIconBackend
	{
		Gtk.StatusIcon statusItem;
		MenuBackend menu;
			
		public void SetImage (object imageBackend) 
		{
			if (imageBackend == null) {
				throw new ArgumentNullException ("imageBackend");
			}
			statusItem.Pixbuf = (Pixbuf)imageBackend;
		}
		
		public void InitializeBackend (object frontend)
		{
			statusItem = new Gtk.StatusIcon();
		}
		
		public void SetMenu (object menuBackend) 
		{
			if (menuBackend == null) {
				throw new ArgumentNullException ("menuBackend");
			}
			if (menu != null) {
				statusItem.PopupMenu -= HandleStatusItemPopupMenu;
			}
			menu = (MenuBackend)menuBackend;
			statusItem.PopupMenu += HandleStatusItemPopupMenu;
		}
		
		void HandleStatusItemPopupMenu (object o, Gtk.PopupMenuArgs args)
		{
			menu.Popup();
		}
		
		public void Dispose () 
		{
			if (statusItem != null) {
				statusItem.Dispose();
			}
			statusItem = null;
			menu = null;
		}
		
		public void EnableEvent (object eventId) { throw new NotImplementedException (); }
		public void DisableEvent (object eventId) { throw new NotImplementedException (); }
		
	}
}

