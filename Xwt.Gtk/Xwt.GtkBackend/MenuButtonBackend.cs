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
using Xwt.Engine;

namespace Xwt.GtkBackend
{
	public class MenuButtonBackend: ButtonBackend, IMenuButtonBackend
	{
		public MenuButtonBackend ()
		{
		}

		public override void Initialize ()
		{
			Widget = new InternalMenuButton ();
			Widget.ShowAll ();
			Widget.MenuCreator = CreateMenu;
		}
		
		new InternalMenuButton Widget {
			get { return (InternalMenuButton)base.Widget; }
			set { base.Widget = value; }
		}
		
		new IMenuButtonEventSink EventSink {
			get { return (IMenuButtonEventSink)base.EventSink; }
		}
		
		Gtk.Menu CreateMenu (InternalMenuButton mb)
		{
			MenuBackend m = null;
			Toolkit.Invoke (delegate {
				m = (MenuBackend) EventSink.OnCreateMenu ();
			});
			return m != null ? m.Menu : null;
		}
	}
	
	class InternalMenuButton : Gtk.Button
	{
		MenuCreator creator;
		bool isOpen;
		
		public InternalMenuButton ()
			: base ()
		{
		}
		
		protected InternalMenuButton (IntPtr raw)
			: base (raw)
		{
			
		}
		
		public MenuCreator MenuCreator {
			get { return creator; }
			set { creator = value; }
		}
		
		protected override void OnClicked ()
		{
			base.OnClicked ();
			
			if (creator != null) {
				Gtk.Menu menu = creator (this);
				
				if (menu != null) {
					isOpen = true;
					
					//make sure the button looks depressed
					Gtk.ReliefStyle oldRelief = this.Relief;
					this.Relief = Gtk.ReliefStyle.Normal;
					
					//clean up after the menu's done
					menu.Hidden += delegate {
						this.Relief = oldRelief ;
						isOpen = false;
						this.State = Gtk.StateType.Normal;
						
						//FIXME: for some reason the menu's children don't get activated if we destroy 
						//directly here, so use a timeout to delay it
						GLib.Timeout.Add (100, delegate {
							//menu.Destroy ();
							return false;
						});
					};
					menu.Popup (null, null, PositionFunc, 0, Gtk.Global.CurrentEventTime);
				}
			}
			
		}
		
		protected override void OnStateChanged (Gtk.StateType previous_state)
		{
			base.OnStateChanged (previous_state);
			
			//while the menu's open, make sure the button looks depressed
			if (isOpen && this.State != Gtk.StateType.Active)
				this.State = Gtk.StateType.Active;
		}
		
		void PositionFunc (Gtk.Menu mn, out int x, out int y, out bool push_in)
		{
			this.GdkWindow.GetOrigin (out x, out y);
			Gdk.Rectangle rect = this.Allocation;
			x += rect.X;
			y += rect.Y + rect.Height;
			
			//if the menu would be off the bottom of the screen, "drop" it upwards
			if (y + mn.Requisition.Height > this.Screen.Height) {
				y -= mn.Requisition.Height;
				y -= rect.Height;
			}
			
			//let GTK reposition the button if it still doesn't fit on the screen
			push_in = true;
		}
		
		protected override void OnDestroyed ()
		{
			creator = null;
			base.OnDestroyed ();
		}
	}
	
	delegate Gtk.Menu MenuCreator (InternalMenuButton button);
}

