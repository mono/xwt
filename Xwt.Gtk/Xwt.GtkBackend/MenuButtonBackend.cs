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
	public class MenuButtonBackend: WidgetBackend, IMenuButtonBackend
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
			MenuBackend m = (MenuBackend) EventSink.OnCreateMenu ();
			return m != null ? m.Menu : null;
		}

		#region IMenuButtonBackend implementation

		public void SetContent (string label, object imageBackend)
		{
			Gdk.Pixbuf pix = (Gdk.Pixbuf)imageBackend;
			if (label != null && imageBackend == null)
				Widget.Label = label;
			else if (label == null && imageBackend != null) {
				var img = new Gtk.Image (pix);
				img.Show ();
				Widget.Image = img;
			} else if (label != null && imageBackend != null) {
				Gtk.HBox box = new Gtk.HBox (false, 3);
				var img = new Gtk.Image (pix);
				box.PackStart (img, false, false, 0);
				var lab = new Gtk.Label (label);
				box.PackStart (lab, false, false, 0);
				box.ShowAll ();
				Widget.Image = box;
			}
		}
		
		public void SetButtonStyle (ButtonStyle style)
		{
			switch (style) {
			case ButtonStyle.Normal:
				Widget.Relief = Gtk.ReliefStyle.Normal;
				break;
			case ButtonStyle.Flat:
				Widget.Relief = Gtk.ReliefStyle.None;
				break;
			}
		}
		#endregion

	}
	
	class InternalMenuButton : Gtk.Button
	{
		MenuCreator creator;
		Gtk.Label label;
		Gtk.Image image;
		Gtk.Arrow arrow;
		bool isOpen;
		
		public InternalMenuButton ()
			: base ()
		{
			Gtk.HBox box = new Gtk.HBox();
			box.Spacing = 6;
			Add (box);
			
			image = new Gtk.Image ();
			box.PackStart (image, false, false, 0);
			label = new Gtk.Label ();
			box.PackStart (label, false, false, 0);
			ArrowType = Gtk.ArrowType.Down;
			base.Label = null;
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
		
		public Gtk.ArrowType? ArrowType {
			get { return arrow == null? (Gtk.ArrowType?) null : arrow.ArrowType; }
			set {
				if (value == null) {
					if (arrow != null) {
						((Gtk.HBox)arrow.Parent).Remove (arrow);
						arrow.Destroy ();
						arrow = null;
					}
				} else {
					if (arrow == null ) {
						arrow = new Gtk.Arrow (Gtk.ArrowType.Down, Gtk.ShadowType.Out);
						arrow.Show ();
						((Gtk.HBox)label.Parent).PackEnd (arrow, false, false, 0);
					}
					arrow.ArrowType = value?? Gtk.ArrowType.Down;
				}
			}
		}
		
		protected override void OnDestroyed ()
		{
			creator = null;
			base.OnDestroyed ();
		}

		public new string Label {
			get { return label.Text; }
			set { label.Text = value; }
		}
		
		public new bool UseUnderline {
			get { return label.UseUnderline; }
			set { label.UseUnderline = value; }
		}
		
		public string StockImage {
			set { image.Pixbuf = RenderIcon (value, Gtk.IconSize.Button, null); }
		}
		
		public bool UseMarkup
		{
			get { return label.UseMarkup; }
			set { label.UseMarkup = value; }
		}
		
		public string Markup {
			set { label.Markup = value; }
		}
	}
	
	delegate Gtk.Menu MenuCreator (InternalMenuButton button);
}

