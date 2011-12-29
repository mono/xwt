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
using Xwt.Engine;

namespace Xwt.GtkBackend
{
	public class ButtonBackend: WidgetBackend, IButtonBackend
	{
		public ButtonBackend ()
		{
		}

		public override void Initialize ()
		{
			Widget = new Gtk.Button ();
			Widget.Show ();
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
			Button b = (Button) Frontend;
			
			Gdk.Pixbuf pix = (Gdk.Pixbuf)imageBackend;
			
			Gtk.Widget imageWidget = null;
			
			switch (b.Type) {
			case ButtonType.Normal:
				if (pix != null)
					imageWidget = new Gtk.Image (pix);
				break;
			case ButtonType.DropDown:
				imageWidget = new Gtk.Arrow (Gtk.ArrowType.Down, Gtk.ShadowType.Out);
				break;
			case ButtonType.Disclosure:
				label = null;
				imageWidget = new Gtk.Arrow (Gtk.ArrowType.Down, Gtk.ShadowType.Out);
				break;
			}
			
			if (label != null && imageWidget == null) {
				Widget.Label = label;
			}
			else if (label == null && imageWidget != null) {
				imageWidget.Show ();
				Widget.Image = imageWidget;
			} else if (label != null && imageWidget != null) {
				Gtk.Box box = position == ContentPosition.Left || position == ContentPosition.Right ? (Gtk.Box) new Gtk.HBox (false, 3) : (Gtk.Box) new Gtk.VBox (false, 3);
				var lab = new Gtk.Label (label);
				
				if (position == ContentPosition.Left || position == ContentPosition.Top) {
					box.PackStart (imageWidget, false, false, 0);
					box.PackStart (lab, false, false, 0);
				} else {
					box.PackStart (lab, false, false, 0);
					box.PackStart (imageWidget, false, false, 0);
				}
				
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
		
		public void SetButtonType (ButtonType type)
		{
			Button b = (Button) Frontend;
			SetContent (b.Label, WidgetRegistry.GetBackend (b.Image), b.ImagePosition);
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
			Toolkit.Invoke (delegate {
				EventSink.OnClicked ();
			});
		}
	}
}

