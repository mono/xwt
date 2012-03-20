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
using Xwt.Engine;

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

		#region IAlertDialogBackend implementation
		public Command Run (WindowFrame transientFor, MessageDescription message)
		{
			this.icon = getIcon (message.Icon);
			this.buttons = convertButtons(message.Buttons);
			if (message.SecondaryText == null)
			{
				message.SecondaryText = "";
			} else {
				message.Text = message.Text + "\r\n\r\n" + message.SecondaryText;
				message.SecondaryText = "";
			}
			var wb = (WindowFrameBackend)WidgetRegistry.GetBackend (transientFor);
			if (wb != null){
				this.dialogResult = MessageBox.Show (wb.Window, message.Text,message.SecondaryText,
				                                this.buttons,this.icon,this.defaultResult,this.options);
			} else {
				this.dialogResult = MessageBox.Show (message.Text, message.SecondaryText, this.buttons, 
				                                this.icon, this.defaultResult, this.options);
			}
			return convertResultToCommand(this.dialogResult);
		}

		public bool ApplyToAll {
			get;
			set;
		}
		#endregion

		MessageBoxImage getIcon(string iconText)
		{
			MessageBoxImage result = MessageBoxImage.None;

			switch (iconText) {
			case StockIcons.Error:
				result = MessageBoxImage.Error;
				break;
			case StockIcons.Warning:
				result = MessageBoxImage.Warning;
				break;
			case StockIcons.Information:
				result = MessageBoxImage.Information;
				break;
			case StockIcons.Question:
				result = MessageBoxImage.Question;
				break;
			default:
				break;
			}
			return result;
		}

		Command convertResultToCommand(MessageBoxResult dialogResult)
		{
			Command command = Command.Cancel;
			switch (dialogResult) {
			case MessageBoxResult.None:
				command = Command.No;
				break;
			case MessageBoxResult.Cancel:
				command = Command.Cancel;
				break;
			case MessageBoxResult.No:
				command = Command.No;
				break;
			case MessageBoxResult.Yes:
				command = Command.Yes;
				break;
			case MessageBoxResult.OK:
				command = Command.Ok;
				break;
			default:
				command = Command.Cancel;
				break;
			}
			return command;
		}

		MessageBoxButton convertButtons(IList<Command> buttons)
		{
			MessageBoxButton result;

			switch (buttons.Count){
			case 1:
				if (buttons.Contains(Command.Ok)){
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
				if (buttons.Contains (Command.Yes) && buttons.Contains (Command.No) && buttons.Contains (Command.Cancel)){
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

		#region IDisposable implementation
		public void Dispose ()
		{
		}
		#endregion

	}
}