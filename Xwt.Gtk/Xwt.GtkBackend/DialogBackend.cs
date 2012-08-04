// 
// DialogBackend.cs
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
using System.Collections.Generic;
using System.Linq;
using Xwt.Engine;

namespace Xwt.GtkBackend
{
	public class DialogBackend: WindowBackend, IDialogBackend
	{
		DialogButton[] dialogButtons;
		Gtk.Button[] buttons;
		
		public DialogBackend ()
		{
		}
		
		public override void Initialize ()
		{
			Window = new Gtk.Dialog ();
			Window.VBox.PackStart (CreateMainLayout ());
		}
		
		new Gtk.Dialog Window {
			get { return (Gtk.Dialog) base.Window; }
			set { base.Window = value; }
		}
		
		new IDialogEventSink EventSink {
			get { return (IDialogEventSink) base.EventSink; }
		}
		
		public void SetButtons (IEnumerable<DialogButton> newButtons)
		{
			if (buttons != null) {
				foreach (var b in buttons) {
					((Gtk.Container)b.Parent).Remove (b);
					b.Destroy ();
				}
			}
			dialogButtons = newButtons.ToArray ();
			buttons = new Gtk.Button [dialogButtons.Length];
			
			for (int n=0; n<dialogButtons.Length; n++) {
				var db = dialogButtons[n];
				Gtk.Button b = (Gtk.Button) Window.AddButton (db.Label, Gtk.ResponseType.None);
				UpdateButton (db, b);
				buttons[n] = b;
				buttons[n].Clicked += HandleButtonClicked;
			}
		}
		
		void UpdateButton (DialogButton btn, Gtk.Button b)
		{
			if (!string.IsNullOrEmpty (btn.Label) && btn.Image == null) {
				b.Label = btn.Label;
			} else if (string.IsNullOrEmpty (btn.Label) && btn.Image != null) {
				var pix = (Gdk.Pixbuf) WidgetRegistry.GetBackend (btn.Image);
				b.Image = new Gtk.Image (pix);
			} else if (!string.IsNullOrEmpty (btn.Label)) {
				Gtk.Box box = new Gtk.HBox (false, 3);
				var pix = (Gdk.Pixbuf) WidgetRegistry.GetBackend (btn.Image);
				box.PackStart (new Gtk.Image (pix), false, false, 0);
				box.PackStart (new Gtk.Label (btn.Label), true, true, 0);
				b.Image = box;
			}
			if (btn.Visible)
				b.ShowAll ();
			else
				b.Hide ();
			b.Sensitive = btn.Sensitive;
		}
		
		void HandleButtonClicked (object o, EventArgs a)
		{
			int i = Array.IndexOf (buttons, (Gtk.Button) o);
			Toolkit.Invoke (delegate {
				EventSink.OnDialogButtonClicked (dialogButtons[i]);
			});
		}

		public void UpdateButton (DialogButton btn)
		{
			int i = Array.IndexOf (dialogButtons, btn);
			UpdateButton (btn, buttons[i]);
		}

		public void RunLoop (IWindowFrameBackend parent)
		{
			var p = (WindowFrameBackend) parent;
			MessageService.RunCustomDialog (Window, p != null ? p.Window : null);
		}

		public void EndLoop ()
		{
			Window.Respond (Gtk.ResponseType.Ok);
		}
	}
}

