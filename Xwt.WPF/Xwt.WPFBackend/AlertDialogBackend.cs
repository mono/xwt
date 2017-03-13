// 
// AlertDialogBackend.cs
//  
// Author:
//       Thomas Ziegler <ziegler.thomas@web.de>
// 
// Copyright (c) 2012 Thomas Ziegler
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
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using Xwt.Backends;


namespace Xwt.WPFBackend
{
	public class AlertDialogBackend : IAlertDialogBackend
	{
		MessageBoxResult dialogResult;
		MessageBoxButton buttons;
		MessageBoxImage icon;
		MessageBoxOptions options;
		MessageBoxResult defaultResult;
		ApplicationContext context;

		public AlertDialogBackend()
		{
			this.buttons = MessageBoxButton.OKCancel;
			this.icon = MessageBoxImage.None;
			this.options = MessageBoxOptions.None;
			this.defaultResult = MessageBoxResult.Cancel;
		}

		public void Initialize (ApplicationContext actx)
		{
			context = actx;
		}

		public Command Run (WindowFrame transientFor, MessageDescription message)
		{
			this.icon = GetIcon (message.Icon);
			if (ConvertButtons (message.Buttons, out buttons) && message.Options.Count == 0) {
				// Use a system message box
				if (message.SecondaryText == null)
					message.SecondaryText = String.Empty;
				else {
					message.Text = message.Text + "\r\n\r\n" + message.SecondaryText;
					message.SecondaryText = String.Empty;
				}
				var parent =  context.Toolkit.GetNativeWindow(transientFor) as System.Windows.Window;
				if (parent != null) {
					this.dialogResult = MessageBox.Show (parent, message.Text, message.SecondaryText,
														this.buttons, this.icon, this.defaultResult, this.options);
				}
				else {
					this.dialogResult = MessageBox.Show (message.Text, message.SecondaryText, this.buttons,
														this.icon, this.defaultResult, this.options);
				}
				return ConvertResultToCommand (this.dialogResult);
			}
			else {
				// Custom message box required
				Dialog dlg = new Dialog ();
				dlg.Resizable = false;
				dlg.Padding = 0;
				HBox mainBox = new HBox { Margin = 25 };

				if (message.Icon != null) {
					var image = new ImageView (message.Icon.WithSize (32,32));
					mainBox.PackStart (image, vpos: WidgetPlacement.Start);
				}
				VBox box = new VBox () { Margin = 3, MarginLeft = 8, Spacing = 15 };
				mainBox.PackStart (box, true);
				var text = new Label {
					Text = message.Text ?? ""
				};
				Label stext = null;
				box.PackStart (text);
				if (!string.IsNullOrEmpty (message.SecondaryText)) {
					stext = new Label {
						Text = message.SecondaryText
					};
					box.PackStart (stext);
				}
				foreach (var option in message.Options) {
					var check = new CheckBox (option.Text);
					check.Active = option.Value;
					box.PackStart(check);
					check.Toggled += (sender, e) => message.SetOptionValue(option.Id, check.Active);
				}
				dlg.Buttons.Add (message.Buttons.ToArray ());
				if (message.DefaultButton >= 0 && message.DefaultButton < message.Buttons.Count)
					dlg.DefaultCommand = message.Buttons[message.DefaultButton];
				if (mainBox.Surface.GetPreferredSize (true).Width > 480) {
					text.Wrap = WrapMode.Word;
					if (stext != null)
						stext.Wrap = WrapMode.Word;
					mainBox.WidthRequest = 480;
				}
				var s = mainBox.Surface.GetPreferredSize (true);

				dlg.Content = mainBox;
				return dlg.Run ();
			}
		}

		public bool ApplyToAll
		{
			get;
			set;
		}

		MessageBoxImage GetIcon (Xwt.Drawing.Image icon)
		{
			if (icon == Xwt.StockIcons.Error)
				return MessageBoxImage.Error;
			if (icon == Xwt.StockIcons.Warning)
				return MessageBoxImage.Warning;
			if (icon == Xwt.StockIcons.Information)
				return MessageBoxImage.Information;
			if (icon == Xwt.StockIcons.Question)
				return MessageBoxImage.Question;
			return MessageBoxImage.None;
		}

		Command ConvertResultToCommand (MessageBoxResult dialogResult)
		{
			switch (dialogResult) {
			case MessageBoxResult.None:
				return Command.No;
			case MessageBoxResult.Cancel:
				return Command.Cancel;
			case MessageBoxResult.No:
				return Command.No;
			case MessageBoxResult.Yes:
				return Command.Yes;
			case MessageBoxResult.OK:
				return Command.Ok;
			default:
				return Command.Cancel;
			}
		}

		bool ConvertButtons (IList<Command> buttons, out MessageBoxButton result)
		{
			switch (buttons.Count){
			case 1:
					if (buttons.Contains (Command.Ok)) {
						result = MessageBoxButton.OK;
						return true;
					}
					break;
			case 2:
				if (buttons.Contains (Command.Ok) && buttons.Contains (Command.Cancel)) {
					result = MessageBoxButton.OKCancel;
					return true;
				} else if (buttons.Contains (Command.Yes) && buttons.Contains (Command.No)) {
					result = MessageBoxButton.YesNo;
					return true;
				}
				break;
			case 3:
				if (buttons.Contains (Command.Yes) && buttons.Contains (Command.No) && buttons.Contains (Command.Cancel)) {
					result = MessageBoxButton.YesNoCancel;
					return true;
				}
				break;
			}
			result = MessageBoxButton.OK;
			return false;
		}

		public void Dispose ()
		{
		}
	}
}