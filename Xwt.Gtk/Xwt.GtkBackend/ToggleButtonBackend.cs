// 
// ToggleButtonBackend.cs
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

// 
// ToggleButtonBackend.cs
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
	public class ToggleButtonBackend: ButtonBackend, IToggleButtonBackend
	{
		public ToggleButtonBackend ()
		{
		}
		
		public override void Initialize ()
		{
			Widget = new Gtk.ToggleButton ();
			Widget.Show ();
		}
		
		protected new Gtk.ToggleButton Widget {
			get { return (Gtk.ToggleButton)base.Widget; }
			set { base.Widget = value; }
		}
		
		protected new IToggleButtonEventSink EventSink {
			get { return (IToggleButtonEventSink)base.EventSink; }
		}
		
		public bool Active {
			get {
				return Widget.Active;
			}
			set {
				ignoreClickEvents = true;
				if (value != Widget.Active)
					ChangeImageIfDisclosure ();
				Widget.Active = value;
				ignoreClickEvents = false;
			}
		}
		
		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is ToggleButtonEvent) {
				switch ((ToggleButtonEvent)eventId) {
				case ToggleButtonEvent.Toggled: Widget.Toggled += HandleToggled; break;
				}
			}
		}
		
		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is ToggleButtonEvent) {
				switch ((ToggleButtonEvent)eventId) {
				case ToggleButtonEvent.Toggled: Widget.Toggled -= HandleToggled; break;
				}
			}
		}

		void HandleToggled (object sender, EventArgs e)
		{
			ChangeImageIfDisclosure ();
			ApplicationContext.InvokeUserCode (EventSink.OnToggled);
		}
		
		void ChangeImageIfDisclosure ()
		{
			if (((Button)Frontend).Type != ButtonType.Disclosure)
				return;
			
			Gtk.ArrowType newArrowType = Gtk.ArrowType.None;
			switch (((Gtk.Arrow)Widget.Image).ArrowType) {
			case Gtk.ArrowType.Down:
				newArrowType = Gtk.ArrowType.Up;
				break;
			case Gtk.ArrowType.Up:
				newArrowType = Gtk.ArrowType.Down;
				break;
			case Gtk.ArrowType.Left:
				newArrowType = Gtk.ArrowType.Right;
				break;
			case Gtk.ArrowType.Right:
				newArrowType = Gtk.ArrowType.Left;
				break;
			}
			Widget.Image = new Gtk.Arrow (newArrowType, Gtk.ShadowType.Out);
			Widget.Image.ShowAll ();
		}
	}
}

