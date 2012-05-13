// 
// ProgressBarBackend.cs
//  
// Author:
//       Andres G. Aragoneses <knocte@gmail.com>
// 
// Copyright (c) 2012 Andres G. Aragoneses
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
using Xwt.Engine;

namespace Xwt.GtkBackend
{
	public class ProgressBarBackend: WidgetBackend, IProgressBarBackend
	{
		bool allowMixed;
		bool internalActiveUpdate;
		bool toggleEventEnabled;
		
		public ProgressBarBackend ()
		{
		}

		public override void Initialize ()
		{
			var progressBar = new Gtk.ProgressBar ();
			Widget = progressBar;
			progressBar.Fraction = 0.4;
			Widget.Show ();

		}
		
		protected new Gtk.ProgressBar Widget {
			get { return (Gtk.ProgressBar)base.Widget; }
			set { base.Widget = value; }
		}
		
		protected new ICheckBoxEventSink EventSink {
			get { return (ICheckBoxEventSink)base.EventSink; }
		}

		public void SetContent (string label, ContentPosition position)
		{
			//Widget.Label = label;
		}
		
		public void SetContent (IWidgetBackend widget)
		{
			throw new NotImplementedException ();
		}
		
		public override void EnableEvent (object eventId)
		{
		}
		
		public override void DisableEvent (object eventId)
		{
		}

		void HandleWidgetClicked (object sender, EventArgs e)
		{
			if (internalActiveUpdate)
				return;
			
			Toolkit.Invoke (delegate {
				EventSink.OnClicked ();
			});
		}

		public void SetButtonStyle (ButtonStyle style)
		{
		}
		
		public void SetButtonType (ButtonType type)
		{
		}
	}
}

