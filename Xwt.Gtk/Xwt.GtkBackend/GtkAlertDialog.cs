//
// AlertDialog.cs
//
// Author:
//   Mike Krüger <mkrueger@novell.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Text;
using Gtk;
using System.Linq;
using Xwt.Backends;

namespace Xwt.GtkBackend
{
	/// <summary>
	/// A Gnome HIG compliant alert dialog.
	/// </summary>
	internal class GtkAlertDialog : Gtk.Dialog
	{
		ApplicationContext actx;
		Command resultButton = null;
		Command[] buttons;
		
		Gtk.HBox  hbox  = new Gtk.HBox ();
		ImageBox image;
		Gtk.Label label = new Gtk.Label ();
		Gtk.VBox labelsBox = new Gtk.VBox (false, 6);
		
		public Command ResultButton {
			get {
				return resultButton;
			}
		}
		
		public bool ApplyToAll { get; set; }
		
		void Init ()
		{
			image = new ImageBox (actx);
			#if XWT_GTK3
			ContentArea.Add (hbox);
			#else
			VBox.PackStart (hbox);
			#endif
			hbox.PackStart (image, false, false, 0);
			hbox.PackStart (labelsBox, true, true, 0);
			labelsBox.PackStart (label, true, true, 0);
				
			// Table 3.1
			this.Title        = "";
			this.BorderWidth  = 6;
			//this.Type         = WindowType.Toplevel;
			this.Resizable    = false;
			#if !XWT_GTK3
			this.HasSeparator = false;
			#endif
			
			// Table 3.2
			#if XWT_GTK3
			this.ContentArea.Spacing = 12;
			#else
			this.VBox.Spacing = 12;
			#endif
			
			// Table 3.3
			this.hbox.Spacing     = 12;
			this.hbox.BorderWidth = 6;
			
			// Table 3.4
			this.image.Yalign   = 0.00f;
			//this.image.IconSize = Gtk.IconSize.Dialog;
			
			// Table 3.5
			this.label.UseMarkup = true;
			this.label.Wrap      = true;
			this.label.Yalign    = 0.00f;
			this.label.Xalign    = 0.00f;
		}
		
		public GtkAlertDialog (ApplicationContext actx, MessageDescription message)
		{
			this.actx = actx;
			Init ();
			this.buttons = message.Buttons.ToArray ();
			
			string primaryText;
			string secondaryText;
			
			if (string.IsNullOrEmpty (message.SecondaryText)) {
				secondaryText = message.Text;
				primaryText = null;
			} else {
				primaryText = message.Text;
				secondaryText = message.SecondaryText;
			}

			var icon = message.Icon.ToImageDescription ();
			image.Image = icon.WithDefaultSize (Gtk.IconSize.Dialog);
			
			StringBuilder markup = new StringBuilder (@"<span weight=""bold"" size=""larger"">");
			markup.Append (GLib.Markup.EscapeText (primaryText));
			markup.Append ("</span>");
			if (!String.IsNullOrEmpty (secondaryText)) {
				if (!String.IsNullOrEmpty (primaryText)) {
					markup.AppendLine ();
					markup.AppendLine ();
				}
				markup.Append (GLib.Markup.EscapeText (secondaryText));
			}
			label.Markup = markup.ToString ();
			label.Selectable = true;
			label.CanFocus = false;
			
			foreach (Command button in message.Buttons) {
				Gtk.Button newButton = new Gtk.Button ();
				newButton.Label        = button.Label;
				newButton.UseUnderline = true;
				newButton.UseStock     = button.IsStockButton;
				if (button.Icon != null) {
					icon = button.Icon.ToImageDescription ();
					newButton.Image = new ImageBox (actx, icon.WithDefaultSize (Gtk.IconSize.Button));
				}
				newButton.Clicked += ButtonClicked;
				ActionArea.Add (newButton);
			}
			
			foreach (var op in message.Options) {
				CheckButton check = new CheckButton (op.Text);
				check.Active = op.Value;
				labelsBox.PackStart (check, false, false, 0);
				check.Toggled += delegate {
					message.SetOptionValue (op.Id, check.Active);
				};
			}
			
			if (message.AllowApplyToAll) {
				CheckButton check = new CheckButton ("Apply to all");
				labelsBox.PackStart (check, false, false, 0);
				check.Toggled += delegate {
					ApplyToAll = check.Active;
				};
			}
			
			//don't show this yet, let the consumer decide when
			this.Child.ShowAll ();
		}
		
		public void FocusButton (int buttonNumber)
		{
			if (buttonNumber == -1)
				buttonNumber = ActionArea.Children.Length - 1;
			ActionArea.Children[buttonNumber].GrabFocus ();
		}
			
		
		void ButtonClicked (object sender, EventArgs e) 
		{
			Gtk.Button clickButton = (Gtk.Button)sender;
			foreach (Command alertButton in buttons) {
				if (clickButton.Label == alertButton.Label) {
					resultButton = alertButton;
					break;
				}
			}
			this.Destroy ();
		}
	}
}
