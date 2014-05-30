// 
// MenuButtonBackend.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//       Michael Hutchinson <mhutch@xamarin.com>
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
	public class MenuButtonBackend: ButtonBackend, IMenuButtonBackend
	{
		bool isOpen;
		
		public MenuButtonBackend ()
		{
		}

		public override void Initialize ()
		{
			base.Initialize ();
			Widget.ButtonPressEvent += HandlePressed;
			Widget.Clicked += HandleClicked;
			Widget.StateChanged += HandleStateChanged;
		}
		
		new IMenuButtonEventSink EventSink {
			get { return (IMenuButtonEventSink)base.EventSink; }
		}

		[GLib.ConnectBefore]
		void HandlePressed (object sender, Gtk.ButtonPressEventArgs e)
		{
			if (e.Event.Button != 1)
				return;
			ShowMenu ();
		}

		[GLib.ConnectBefore]
		void HandleClicked (object sender, EventArgs e)
		{
			if (!isOpen)
				ShowMenu ();
		}

		void ShowMenu ()
		{
			Gtk.Menu menu = CreateMenu ();
			
			if (menu != null) {
				isOpen = true;
				
				//make sure the button looks depressed
				Gtk.ReliefStyle oldRelief = Widget.Relief;
				Widget.Relief = Gtk.ReliefStyle.Normal;
				Widget.SetStateActive();
				
				//clean up after the menu's done
				menu.Hidden += delegate {
					Widget.Relief = oldRelief ;
					isOpen = false;
					Widget.SetStateNormal ();
					
					//FIXME: for some reason the menu's children don't get activated if we destroy 
					//directly here, so use a timeout to delay it
					GLib.Timeout.Add (100, delegate {
						//menu.Destroy ();
						return false;
					});
				};
				menu.Popup (null, null, PositionFunc, 1, Gtk.Global.CurrentEventTime);
			}
		}

		void HandleStateChanged (object o, Gtk.StateChangedArgs args)
		{
			//while the menu's open, make sure the button looks depressed
			if (isOpen && Widget.State != Gtk.StateType.Active)
				Widget.SetStateActive ();
		}

		void PositionFunc (Gtk.Menu mn, out int x, out int y, out bool push_in)
		{
			Gtk.Widget w = (Gtk.Widget)Widget;
			w.GdkWindow.GetOrigin (out x, out y);
			Gdk.Rectangle rect = w.Allocation;
			x += rect.X;
			y += rect.Y + rect.Height;
			
			//if the menu would be off the bottom of the screen, "drop" it upwards
			if (y + mn.Requisition.Height > w.Screen.Height) {
				y -= mn.Requisition.Height;
				y -= rect.Height;
			}
			
			//let GTK reposition the button if it still doesn't fit on the screen
			push_in = true;
		}
		
		Gtk.Menu CreateMenu ()
		{
			MenuBackend m = null;
			ApplicationContext.InvokeUserCode (delegate {
				m = (MenuBackend) EventSink.OnCreateMenu ();
			});
			return m != null ? m.Menu : null;
		}
	}
}

