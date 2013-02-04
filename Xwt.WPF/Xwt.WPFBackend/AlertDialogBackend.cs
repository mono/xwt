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

		public AlertDialogBackend()
		{
			this.buttons = MessageBoxButton.OKCancel;
			this.icon = MessageBoxImage.None;
			this.options = MessageBoxOptions.None;
			this.defaultResult = MessageBoxResult.Cancel;
		}

		public Command Run (WindowFrame transientFor, MessageDescription message)
		{
			this.icon = GetIcon (message.Icon);
			this.buttons = ConvertButtons (message.Buttons);
			if (message.SecondaryText == null)
				message.SecondaryText = String.Empty;
			else {
				message.Text = message.Text + "\r\n\r\n" + message.SecondaryText;
				message.SecondaryText = String.Empty;
			}

			var wb = (WindowFrameBackend)Toolkit.GetBackend (transientFor);
			if (wb != null) {
				this.dialogResult = MessageBox.Show (wb.Window, message.Text,message.SecondaryText,
													this.buttons, this.icon, this.defaultResult, this.options);
			} else {
				this.dialogResult = MessageBox.Show (message.Text, message.SecondaryText, this.buttons, 
													this.icon, this.defaultResult, this.options);
			}

			return ConvertResultToCommand (this.dialogResult);
		}

		public bool ApplyToAll
		{
			get;
			set;
		}

		MessageBoxImage GetIcon (string iconText)
		{
			switch (iconText) {
			case StockIconId.Error:
				return MessageBoxImage.Error;
			case StockIconId.Warning:
				return MessageBoxImage.Warning;
			case StockIconId.Information:
				return MessageBoxImage.Information;
			case StockIconId.Question:
				return MessageBoxImage.Question;
			default:
				return MessageBoxImage.None;
			}
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

		MessageBoxButton ConvertButtons (IList<Command> buttons)
		{
			MessageBoxButton result;

			switch (buttons.Count){
			case 1:
				if (buttons.Contains(Command.Ok)) {
					result = MessageBoxButton.OK;
				} else {
					throw new NotImplementedException ();
				}
				break;
			case 2:
				if (buttons.Contains (Command.Ok) && buttons.Contains (Command.Cancel)) {
					result = MessageBoxButton.OKCancel;
				} else if (buttons.Contains (Command.Yes) && buttons.Contains (Command.No)) {
					result = MessageBoxButton.YesNo;
				} else {
					throw new NotImplementedException ();
				}
				break;
			case 3:
				if (buttons.Contains (Command.Yes) && buttons.Contains (Command.No) && buttons.Contains (Command.Cancel)) {
					result = MessageBoxButton.YesNoCancel;
				} else {
					throw new NotImplementedException ();
				}
				break;
			default:
				throw new NotImplementedException ();
			}

			return result;
		}

		public void Dispose ()
		{
		}
	}
}