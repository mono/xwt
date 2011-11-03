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
	class ButtonBackend<T,S>: WidgetBackend<T,S>, IButtonBackend where T:Gtk.Button where S:IButtonEventSink
	{
		public ButtonBackend ()
		{
		}

		public override void Initialize ()
		{
			Widget = (T) new Gtk.Button ();
			Widget.Show ();
		}

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
			EventSink.OnClicked ();
		}
	}
}

