//
// Popover.cs
//
// Author:
//       Jérémie Laval <jeremie.laval@xamarin.com>
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
using Xwt.Drawing;
using Xwt.Engine;
using Xwt.Backends;

namespace Xwt
{
	public sealed class Popover : IDisposable
	{
		public enum Position {
			Top,
			Bottom,
			/*Left,
				Right*/
		}

		IPopoverBackend backend;
		Position arrowPosition;
		WindowFrame parent;

		public event EventHandler Closed;

		public Popover (WindowFrame parent, Position arrowPosition)
		{
			this.arrowPosition = arrowPosition;
			this.parent = parent;
			backend = WidgetRegistry.MainRegistry.CreateBackend<IPopoverBackend> (GetType ());

			backend.Closed += (sender, e) => {
				if (Closed != null)
					Closed (this, EventArgs.Empty);
			};
		}

		public Func<Widget> ChildSource {
			get;
			set;
		}

		public void Run (Widget referenceWidget)
		{
			if (backend == null)
				throw new InvalidOperationException ("The Popover was disposed");
			if (ChildSource == null)
				throw new InvalidOperationException ("A child widget source must be set before running the Popover");
			backend.Run (parent,
			             arrowPosition,
			             ChildSource,
			             referenceWidget);
		}

		public void Dispose ()
		{
			if (backend != null) {
				backend.Dispose ();
				backend = null;
			}
		}
	}
}

