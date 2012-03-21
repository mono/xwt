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
using MonoMac.AppKit;
using MonoMac.Foundation;
using Xwt.Backends;
using Xwt.Drawing;

namespace Xwt.Mac
{
	public class AlertDialogBackend : NSAlert, IAlertDialogBackend
	{
		public AlertDialogBackend ()
		{
		}
		
		public AlertDialogBackend (System.IntPtr intptr)
		{
		}
		
		#region IAlertDialogBackend implementation
		public Command Run (WindowFrame transientFor, MessageDescription message)
		{
			this.MessageText = (message.Text != null) ? message.Text : String.Empty;
			this.InformativeText = (message.SecondaryText != null) ? message.SecondaryText : String.Empty;
			//TODO Set Icon
			//TODO Sort Buttons to have the default button first
			foreach (Command cmd in message.Buttons) {
				this.AddButton (cmd.Label);
			}

			return message.Buttons [this.RunModal () - 1000];
		}

		public bool ApplyToAll { get; set; }
		#endregion
	}
}

