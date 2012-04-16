//
// SelectFolderDialogBackend.cs
//
// Author:
//       Andres G. Aragoneses <knocte@gmail.com>
//
// Copyright (c) 2012 Xamarin, Inc.
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
using System.Drawing;
using Xwt.Backends;

namespace Xwt.WPFBackend
{
	public class StatusIconBackend : IStatusIconBackend
	{
		//TODO: stop using System.Windows.Forms solution to start using native WPF, alternatives:
		// * http://www.codeproject.com/Articles/36468/WPF-NotifyIcon (license is CPOL but author is willing to give as MIT for XWT)
		// * http://wpfcontrib.codeplex.com/ (license is MS-PL, so it is compatible with MIT afaik)
		private System.Windows.Forms.NotifyIcon statusIcon;

		public void InitializeBackend (object frontend)
		{
			statusIcon = new System.Windows.Forms.NotifyIcon ();

			//TODO: expose Visible property in the StatusIcon class
			statusIcon.Visible = true;
		}

		//TODO: expose Tooltip property in the StatusIcon class
		public string ToolTip
		{
			get { return statusIcon.Text; }
			set { statusIcon.Text = value; }
		}

		public void Dispose ()
		{
			this.statusIcon.Visible = false;
			this.statusIcon.Dispose ();
			this.statusIcon = null;
		}

		public void SetMenu (object menuBackend)
		{
			if (menuBackend == null) {
				throw new ArgumentNullException ("menuBackend");
			}
			this.statusIcon.ContextMenu = ((MenuBackend) menuBackend).CreateSwfContextMenu ();
		}

		public void SetImage(object imageBackend)
		{
			if (imageBackend == null) {
				throw new ArgumentNullException ("imageBackend");
			}
			this.statusIcon.Icon = (Icon) imageBackend;
		}

		public void EnableEvent(object eventId) { throw new NotImplementedException (); }

		public void DisableEvent(object eventId) { throw new NotImplementedException (); }
	}
}
