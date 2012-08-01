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

		public event EventHandler Closed;

		public Popover (WindowFrame parent, Widget child, Position arrowPosition)
		{
			this.arrowPosition = arrowPosition;
			backend = WidgetRegistry.CreateBackend<IPopoverBackend> (GetType ());
			backend.Init ((IWindowFrameBackend) WidgetRegistry.GetBackend (parent),
			              (IWidgetBackend) WidgetRegistry.GetBackend (child), arrowPosition);
			backend.Closed += (sender, e) => {
				if (Closed != null)
					Closed (this, EventArgs.Empty);
			};
		}

		public void Run (Widget referenceWidget)
		{
			if (backend == null)
				throw new InvalidOperationException ("The Popover was disposed");
			backend.Run ((IWidgetBackend) WidgetRegistry.GetBackend (referenceWidget));
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

