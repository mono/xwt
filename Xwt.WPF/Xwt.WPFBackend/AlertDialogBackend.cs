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
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using SWC = System.Windows.Controls;
using SWMI = System.Windows.Media.Imaging;

using Xwt.Backends;
using Xwt.WPFBackend.Utilities;

namespace Xwt.WPFBackend
{
	public class AlertDialogBackend :IAlertDialogBackend
	{
		public AlertDialogBackend ()
		{
		}

		#region IDisposable implementation
		public void Dispose ()
		{
		}
		#endregion

		#region IAlertDialogBackend implementation
		public Command Run (WindowFrame transientFor, MessageDescription message)
		{
			//TODO: Implement all options	
			switch (MessageBox.Show (message.Text)) {
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
				return Command.Ok;
			}
		}

		public bool ApplyToAll {
			get;
			set;
		}
		#endregion
	}
}

