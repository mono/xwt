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
using Xwt.Engine;

namespace Xwt.GtkBackend
{
	public class ProgressBarBackend: WidgetBackend, IProgressBarBackend
	{
		Timer timer = new Timer (100);

		public ProgressBarBackend ()
		{
		}

		public override void Initialize ()
		{
			var progressBar = new Gtk.ProgressBar ();
			Widget = progressBar;
			progressBar.Pulse ();
			timer.Elapsed += Pulse;
			Widget.Show ();
			timer.Start ();
		}

		private void Pulse (object sender, ElapsedEventArgs args)
		{
			Application.Invoke (() => Widget.Pulse ());
		}
		
		protected new Gtk.ProgressBar Widget {
			get { return (Gtk.ProgressBar)base.Widget; }
			set { base.Widget = value; }
		}

		public void SetFraction (double? fraction)
		{
			if (fraction == null)
			{
				timer.Start ();
				Widget.Fraction = 0.1;
			} else {
				timer.Stop ();
				Widget.Fraction = fraction.Value;
			}
		}
	}
}

