//
// WidgetBackendGtk2.cs
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
using Xwt.Backends;
using Xwt.Drawing;

namespace Xwt.GtkBackend
{
	public partial class WidgetBackend
	{
		bool gettingPreferredSize;

		protected Gtk.Editable EditableWidget 
		{
			get { return Widget as Gtk.Editable; }
		}

		protected virtual void OnSetBackgroundColor (Color color)
		{
			EventsRootWidget.SetBackgroundColor (color);
		}

		public virtual Size GetPreferredSize (SizeConstraint widthConstraint, SizeConstraint heightConstraint)
		{
			try {
				SetSizeConstraints (widthConstraint, heightConstraint);
				gettingPreferredSize = true;
				var sr = Widget.SizeRequest ();
				return new Size (sr.Width, sr.Height);
			} finally {
				gettingPreferredSize = false;
			}
		}

		void EnableSizeCheckEvents ()
		{
			if ((enabledEvents & WidgetEvent.PreferredSizeCheck) == 0 && !minSizeSet) {
				// Enabling a size request event for the first time
				Widget.SizeRequested += HandleWidgetSizeRequested;
			}
		}

		void DisableSizeCheckEvents ()
		{
			if ((enabledEvents & WidgetEvent.PreferredSizeCheck) == 0 && !minSizeSet) {
				// All size request events have been disabled
				Widget.SizeRequested -= HandleWidgetSizeRequested;
			}
		}

		void HandleWidgetSizeRequested (object o, Gtk.SizeRequestedArgs args)
		{
			var req = args.Requisition;

			if (!gettingPreferredSize && (enabledEvents & WidgetEvent.PreferredSizeCheck) != 0) {
				SizeConstraint wc = SizeConstraint.Unconstrained, hc = SizeConstraint.Unconstrained;
				var cp = Widget.Parent as IConstraintProvider;
				if (cp != null)
					cp.GetConstraints (Widget, out wc, out hc);

				ApplicationContext.InvokeUserCode (delegate {
					var w = eventSink.GetPreferredSize (wc, hc);
					req.Width = (int) w.Width;
					req.Height = (int) w.Height;
				});
			}

			if (Frontend.MinWidth != -1 && Frontend.MinWidth > req.Width)
				req.Width = (int) Frontend.MinWidth;
			if (Frontend.MinHeight != -1 && Frontend.MinHeight > req.Height)
				req.Height = (int) Frontend.MinHeight;

			args.Requisition = req;
		}

		public void SetMinSize (double width, double height)
		{
			if (width != -1 || height != -1) {
				EnableSizeCheckEvents ();
				minSizeSet = true;
				Widget.QueueResize ();
			}
			else {
				minSizeSet = false;
				DisableSizeCheckEvents ();
				Widget.QueueResize ();
			}
		}

		public void SetSizeRequest (double width, double height)
		{
			Widget.WidthRequest = (int)width;
			Widget.HeightRequest = (int)height;
		}

		double opacity = 1d;
		public double Opacity {
			get { return opacity; }
			set { opacity = value; }
		}
	}
}

