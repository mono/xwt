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
using System.Timers;
using Xwt.Backends;


namespace Xwt.GtkBackend
{
	public class ProgressBarBackend: WidgetBackend, IProgressBarBackend
	{
		public ProgressBarBackend ()
		{
		}

		uint? timerId = null;

		public override void Initialize ()
		{
			var progressBar = new Gtk.ProgressBar ();
			Widget = progressBar;
			Widget.Show ();
		}

		private bool Pulse ()
		{
			Widget.Pulse ();
			return true;
		}
		
		protected new Gtk.ProgressBar Widget {
			get { return (Gtk.ProgressBar)base.Widget; }
			set { base.Widget = value; }
		}

		public void SetIndeterminate (bool value) {
			if (value) {
				if (timerId != null)
					return;

				timerId = GLib.Timeout.Add (100, Pulse);

			} else {

				if (timerId == null)
					return;

				DisposeTimeout ();
			}
		}

		public void SetFraction (double fraction)
		{
			Widget.Fraction = fraction;
		}

		protected void DisposeTimeout ()
		{
			if (timerId != null) {
				GLib.Source.Remove (timerId.Value);
				timerId = null;
			}
		}

		protected override void Dispose (bool disposing)
		{
			DisposeTimeout ();
			base.Dispose(disposing);
		}
	}
}

