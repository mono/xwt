//
// Gtk3Container.cs
//
// Author:
//       Vsevolod Kukol <v.kukol@rubologic.de>
//
// Copyright (c) 2014 Vsevolod Kukol
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

namespace Xwt.GtkBackend
{
	partial class BoxBackend
	{
		protected override void OnSetBackgroundColor (Xwt.Drawing.Color color)
		{
			if (EventsRootWidget != null)
				EventsRootWidget.SetBackgroundColor (color);
			base.OnSetBackgroundColor (color);
		}
	}

	partial class CustomContainer
	{
		public void QueueResizeIfRequired()
		{
			// since we have no SizeRequest event, we must always queue up for resize
			QueueResize ();
		}

		protected override void OnUnrealized ()
		{
			// force reallocation on next realization, since allocation may be lost
			IsReallocating = false;
			base.OnUnrealized ();
		}

		protected override void OnRealized ()
		{
			// force reallocation, if unrealized previously
			if (!IsReallocating) {
				try {
					OnReallocate ();
				} catch {
					IsReallocating = false;
				}
			}
			base.OnRealized ();
		}

		protected override Gtk.SizeRequestMode OnGetRequestMode ()
		{
			// dirty fix: unwrapped labels report fixed sizes, forcing parents to fixed mode
			//            -> report always width_for_height, since we don't support angles
			return Gtk.SizeRequestMode.WidthForHeight;
		}

		protected override void OnGetPreferredHeight (out int minimum_height, out int natural_height)
		{
			// containers need initial width in heigt_for_width mode
			// dirty fix: do not constrain width on first allocation 
			var force_width = SizeConstraint.Unconstrained;
			if (IsReallocating)
				force_width = SizeConstraint.WithSize (Allocation.Width);
			var size = OnGetRequisition (force_width, SizeConstraint.Unconstrained);
			if (size.Height < HeightRequest)
				minimum_height = natural_height = HeightRequest;
			else
				minimum_height = natural_height = size.Height;
		}

		protected override void OnGetPreferredWidth (out int minimum_width, out int natural_width)
		{
			// containers need initial height in width_for_height mode
			// dirty fix: do not constrain height on first allocation
			var force_height = SizeConstraint.Unconstrained;
			if (IsReallocating)
				force_height = SizeConstraint.WithSize (Allocation.Width);
			var size = OnGetRequisition (SizeConstraint.Unconstrained, force_height);
			if (size.Height < WidthRequest)
				minimum_width = natural_width = WidthRequest;
			else
				minimum_width = natural_width = size.Width;
		}

		protected override void OnGetPreferredHeightForWidth (int width, out int minimum_height, out int natural_height)
		{
			var size = OnGetRequisition (SizeConstraint.WithSize (width), SizeConstraint.Unconstrained);
			if (size.Height < HeightRequest)
				minimum_height = natural_height = HeightRequest;
			else
				minimum_height = natural_height = size.Height;
		}

		protected override void OnGetPreferredWidthForHeight (int height, out int minimum_width, out int natural_width)
		{
			var size = OnGetRequisition (SizeConstraint.Unconstrained, SizeConstraint.WithSize (height));
			if (size.Height < WidthRequest)
				minimum_width = natural_width = WidthRequest;
			else
				minimum_width = natural_width = size.Width;
		}
	}
}

