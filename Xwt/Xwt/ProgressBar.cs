// 
// ProgressBar.cs
//  
// Author:
//       Andres G. Aragoneses <knocte@gmail.com>
// 
// Copyright (c) 2012 Anrdres G. Aragoneses
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
using Xwt.Drawing;
using System.ComponentModel;

namespace Xwt
{
	[BackendType (typeof(IProgressBarBackend))]
	public class ProgressBar : Widget
	{
		double fraction = 0.0;
		bool indeterminate = false;
		
		public ProgressBar ()
		{
		}
		
		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}
		
		IProgressBarBackend Backend {
			get { return (IProgressBarBackend) BackendHost.Backend; }
		}

		[DefaultValue (0d)]
		public double Fraction {
			get { return fraction; }
			set {
				// TODO: allow any value, by implementing MinValue and MaxValue properties
				// and then adjusting the fraction to a [0.0..1.0] range only in the Gtk backend
				if (value < 0.0 || value > 1.0)
					throw new NotSupportedException ("Fraction value can only be in the [0.0..1.0] range");

				fraction = value;
				Backend.SetFraction (fraction);
			}
		}

		[DefaultValue (false)]
		public bool Indeterminate {
			get { return indeterminate; }
			set {
				if (indeterminate != value) {
					indeterminate = value;
					Backend.SetIndeterminate (value);
				}
			}
		}
	}
}

