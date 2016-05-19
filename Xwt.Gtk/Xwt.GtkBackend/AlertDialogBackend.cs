// 
// AlertDialogBackend.cs
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
// AlertDialogBackend.cs
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
using System.Collections.Generic;
using Xwt.Backends;
using System.Linq;


namespace Xwt.GtkBackend
{
	public class AlertDialogBackend: IAlertDialogBackend
	{
		ApplicationContext context;

		#region IAlertDialogBackend implementation
		
		public void Dispose ()
		{
		}

		public void Initialize (ApplicationContext context)
		{
			this.context = context;
		}

		public Command Run (WindowFrame transientFor, MessageDescription message)
		{			
			GtkAlertDialog alertDialog = new GtkAlertDialog (context, message);
			alertDialog.FocusButton (message.DefaultButton);
			var win = Toolkit.CurrentEngine.GetNativeWindow (transientFor) as Gtk.Window;
			MessageService.ShowCustomDialog (alertDialog, win);
			if (alertDialog.ApplyToAll)
				ApplyToAll = true;
			var res = alertDialog.ResultButton;
			
			if (res == null) {
				// If the dialog is closed clicking the close window button we may have no result.
				// In that case, try to find a cancelling button
				if (message.Buttons.Contains (Command.Cancel))
					return Command.Cancel;
				else if (message.Buttons.Contains (Command.No))
					return Command.No;
				else if (message.Buttons.Contains (Command.Close))
					return Command.Close;
			}
			return res;
		}

		public bool ApplyToAll { get; set; }
		
		#endregion
	}
}

