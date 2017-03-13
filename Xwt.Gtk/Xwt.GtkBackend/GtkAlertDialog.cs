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
	internal class GtkAlertDialog : Gtk.MessageDialog
	{
		ApplicationContext actx;
		Command resultButton = null;
		Command[] buttons;
		
		ImageBox image;
		
		public Command ResultButton {
			get {
				return resultButton;
			}
		}
		
		public bool ApplyToAll { get; set; }
		
		void Init ()
		{
			image = new ImageBox (actx);

			this.Title        = "";
			this.Resizable    = false;
		}
		
		public GtkAlertDialog (ApplicationContext actx, MessageDescription message)
		{
			this.actx = actx;
			Init ();
			this.buttons = message.Buttons.ToArray ();

			string primaryText = String.Empty;
			string secondaryText = String.Empty;

			if (string.IsNullOrEmpty (message.Text)) {
				if (!string.IsNullOrEmpty (message.SecondaryText))
					primaryText = message.SecondaryText;
			} else {
				primaryText = message.Text;
				secondaryText = message.SecondaryText;
			}

			if (message.Icon == StockIcons.Information)
				base.MessageType = MessageType.Info;
			else if (message.Icon == StockIcons.Question)
				base.MessageType = MessageType.Question;
			else if (message.Icon == StockIcons.Warning)
				base.MessageType = MessageType.Warning;
			else if (message.Icon == StockIcons.Error)
				base.MessageType = MessageType.Error;
			else {
				var icon = message.Icon.ToImageDescription (actx);
				image.Image = icon.WithDefaultSize (Gtk.IconSize.Dialog);
				base.Image = image;
			}

			StringBuilder markup = new StringBuilder (@"<span weight=""bold"" size=""larger"">");
			markup.Append (GLib.Markup.EscapeText (primaryText));
			markup.Append ("</span>");

			base.Markup = markup.ToString ();
			if (!String.IsNullOrEmpty (secondaryText)) {
				base.SecondaryText = GLib.Markup.EscapeText (secondaryText);
				base.SecondaryUseMarkup = true;
			}
			
			foreach (Command button in message.Buttons) {
				Gtk.Button newButton = (Gtk.Button)base.AddButton (button.Label, button.ToResponseType());
				newButton.UseUnderline = true;
				newButton.UseStock     = button.IsStockButton;
				if (button.Icon != null) {
					var icon = button.Icon.ToImageDescription (actx);
					newButton.Image = new ImageBox (actx, icon.WithDefaultSize (Gtk.IconSize.Button));
				}
				newButton.Clicked += ButtonClicked;
			}
			
			foreach (var op in message.Options) {
				Gtk.CheckButton check = new Gtk.CheckButton (op.Text);
				check.Active = op.Value;
				this.AddContent (check, false, false, 0);
				check.Toggled += delegate {
					message.SetOptionValue (op.Id, check.Active);
				};
			}
			
			if (message.AllowApplyToAll) {
				Gtk.CheckButton check = new Gtk.CheckButton (Xwt.Application.TranslationCatalog.GetString("Apply to all"));
				this.AddContent (check, false, false, 0);
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
