// 
// ButtonBackend.cs
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
	public class ButtonBackend: WidgetBackend, IButtonBackend
	{
		protected bool ignoreClickEvents;
		
		public ButtonBackend ()
		{
		}

		public override void Initialize ()
		{
			Widget = new Gtk.Button ();
			base.Widget.Show ();
			
		}
		
		protected new Gtk.Button Widget {
			get { return (Gtk.Button)base.Widget; }
			set { base.Widget = value; }
		}
		
		protected new IButtonEventSink EventSink {
			get { return (IButtonEventSink)base.EventSink; }
		}
		
		public void SetContent (string label, object imageBackend, ContentPosition position)
		{
			if (label != null && label.Length == 0)
				label = null;
			
			Button b = (Button) Frontend;
			if (label != null && imageBackend == null && b.Type == ButtonType.Normal) {
				Widget.Label = label;
				return;
			}
			
			if (b.Type == ButtonType.Disclosure) {
				Widget.Label = null;
				Widget.Image = new Gtk.Arrow (Gtk.ArrowType.Down, Gtk.ShadowType.Out);
				Widget.Image.ShowAll ();
				return;
			}
			
			Gtk.Widget contentWidget = null;
			
			Gtk.Widget imageWidget = null;
			if (imageBackend != null)
				imageWidget = new Gtk.Image ((Gdk.Pixbuf)imageBackend);
			
			if (label != null && imageWidget == null) {
				contentWidget = new Gtk.Label (label); 
			}
			else if (label == null && imageWidget != null) {
				contentWidget = imageWidget;
			}
			else if (label != null && imageWidget != null) {
				Gtk.Box box = position == ContentPosition.Left || position == ContentPosition.Right ? (Gtk.Box) new Gtk.HBox (false, 3) : (Gtk.Box) new Gtk.VBox (false, 3);
				var lab = new Gtk.Label (label);
				
				if (position == ContentPosition.Left || position == ContentPosition.Top) {
					box.PackStart (imageWidget, false, false, 0);
					box.PackStart (lab, false, false, 0);
				} else {
					box.PackStart (lab, false, false, 0);
					box.PackStart (imageWidget, false, false, 0);
				}
				
				contentWidget = box;
			}
			if (b.Type == ButtonType.DropDown) {
				if (contentWidget != null) {
					Gtk.HBox box = new Gtk.HBox (false, 3);
					box.PackStart (contentWidget, true, true, 3);
					box.PackStart (new Gtk.VSeparator (), true, true, 0);
					box.PackStart (new Gtk.Arrow (Gtk.ArrowType.Down, Gtk.ShadowType.Out), false, false, 0);
					contentWidget = box;
				} else
					contentWidget = new Gtk.Arrow (Gtk.ArrowType.Down, Gtk.ShadowType.Out);
			}
			if (contentWidget != null) {
				contentWidget.ShowAll ();
				Widget.Label = null;
				Widget.Image = contentWidget;
			} else
				Widget.Label = null;
		}
		
		public void SetButtonStyle (ButtonStyle style)
		{
			switch (style) {
			case ButtonStyle.Normal:
				SetMiniMode (false);
				Widget.Relief = Gtk.ReliefStyle.Normal;
				break;
			case ButtonStyle.Flat:
				SetMiniMode (false);
				Widget.Relief = Gtk.ReliefStyle.None;
				break;
			case ButtonStyle.Borderless:
				SetMiniMode (true);
				Widget.Relief = Gtk.ReliefStyle.None;
				break;
			}
		}
		
		public void SetButtonType (ButtonType type)
		{
			Button b = (Button) Frontend;
			SetContent (b.Label, Toolkit.GetBackend (b.Image), b.ImagePosition);
		}
		
		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is ButtonEvent) {
				switch ((ButtonEvent)eventId) {
				case ButtonEvent.Clicked: Widget.Clicked += HandleWidgetClicked; break;
				}
			}
		}
		
		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is ButtonEvent) {
				switch ((ButtonEvent)eventId) {
				case ButtonEvent.Clicked: Widget.Clicked -= HandleWidgetClicked; break;
				}
			}
		}

		void HandleWidgetClicked (object sender, EventArgs e)
		{
			if (!ignoreClickEvents) {
				ApplicationContext.InvokeUserCode (delegate {
					EventSink.OnClicked ();
				});
			}
		}
		
		bool miniMode;
		
		protected void SetMiniMode (bool miniMode)
		{
//			Gtk.Rc.ParseString ("style \"Xwt.GtkBackend.CustomButton\" {\n GtkButton::inner-border = {0,0,0,0} GtkButton::child-displacement-x = {0} GtkButton::child-displacement-y = {0}\n }\n");
//			Gtk.Rc.ParseString ("widget \"*.Xwt.GtkBackend.CustomButton\" style  \"Xwt.GtkBackend.CustomButton\"\n");
//			Name = "Xwt.GtkBackend.CustomButton";
			
			if (this.miniMode == miniMode)
				return;
			this.miniMode = miniMode;
			if (miniMode) {
				Widget.ExposeEvent += HandleExposeEvent;
				Widget.SizeAllocated += HandleSizeAllocated;
				Widget.SizeRequested += HandleSizeRequested;
			}
			Widget.QueueResize ();
		}

		void HandleSizeRequested (object o, Gtk.SizeRequestedArgs args)
		{
			args.Requisition = Widget.Child.SizeRequest ();
		}

		[GLib.ConnectBefore]
		void HandleSizeAllocated (object o, Gtk.SizeAllocatedArgs args)
		{
			Widget.Child.SizeAllocate (args.Allocation);
			args.RetVal = true;
		}

		[GLib.ConnectBefore]
		void HandleExposeEvent (object o, Gtk.ExposeEventArgs args)
		{
			var gc = Widget.Style.BackgroundGC (Widget.State);
			Widget.GdkWindow.DrawRectangle (gc, true, Widget.Allocation);
			Widget.PropagateExpose (Widget.Child, args.Event);
			args.RetVal = true;
		}
	}
}

