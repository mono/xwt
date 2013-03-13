// 
// PasswordEntryBackend.cs
//  
// Author:
//       Henrique Esteves <henriquemotaesteves@gmail.com>
// 
// Copyright (c) 2012 Henrique Esteves
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
using Xwt.Drawing;


namespace Xwt.GtkBackend
{
	public class PasswordEntryBackend: WidgetBackend, IPasswordEntryBackend
	{
		public override void Initialize ()
		{
			Widget = new Gtk.Entry ();
			Widget.Show ();
			Widget.Visibility = false;
		}
		
		protected new Gtk.Entry Widget {
			get { return (Gtk.Entry)base.Widget; }
			set { base.Widget = value; }
		}

		protected new IPasswordEntryEventSink EventSink {
			get { return (IPasswordEntryEventSink)base.EventSink; }
		}

		public string Password {
			get {
				return Widget.Text;
			}
			set {
				Widget.Text = value;
			}
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is PasswordEntryEvent) {
				switch ((PasswordEntryEvent)eventId) {
				case PasswordEntryEvent.Changed: Widget.Changed += HandleChanged; break;
				}
			}
		}
		
		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is PasswordEntryEvent) {
				switch ((PasswordEntryEvent)eventId) {
				case PasswordEntryEvent.Changed: Widget.Changed -= HandleChanged; break;
				}
			}
		}

		void HandleChanged (object sender, EventArgs e)
		{
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnChanged ();
			});
		}
	}
}

