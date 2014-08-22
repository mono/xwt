//
// WidgetBackendGtk3.cs
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
		protected Gtk.IEditable EditableWidget 
		{
			get { return Widget as Gtk.IEditable; }
		}

		protected virtual void OnSetBackgroundColor (Color color)
		{
			Widget.SetBackgroundColor (color);
		}

		public virtual Size GetPreferredSize (SizeConstraint widthConstraint, SizeConstraint heightConstraint)
		{
			SetSizeConstraints (widthConstraint, heightConstraint);

			int min_width = 0;
			int min_height = 0;

			IWidgetBackend xwt_backend = Widget as IWidgetBackend;
			if (xwt_backend != null) {
				var size = xwt_backend.GetPreferredSize (widthConstraint, heightConstraint);
				min_width = (int)size.Width;
				min_height = (int)size.Height;
			} else {
				int nat_width, nat_height;

				if (Widget.RequestMode == Gtk.SizeRequestMode.WidthForHeight) {
					if (widthConstraint.IsConstrained) {
						Widget.GetPreferredHeightForWidth ((int)widthConstraint.AvailableSize, out min_height, out nat_height);
					} else if (heightConstraint.IsConstrained) {
						Widget.GetPreferredWidthForHeight ((int)heightConstraint.AvailableSize, out min_width, out nat_width);
					} else if ((heightConstraint.IsConstrained) && (widthConstraint.IsConstrained)) {
					} else {
						Widget.GetPreferredHeight (out min_height, out nat_height);
						Widget.GetPreferredWidthForHeight (min_height, out min_width, out nat_width);
					}
				} else if (Widget.RequestMode == Gtk.SizeRequestMode.HeightForWidth) {
					if (heightConstraint.IsConstrained) {
						Widget.GetPreferredWidthForHeight ((int)heightConstraint.AvailableSize, out min_width, out nat_width);
					} else if (widthConstraint.IsConstrained) {
						Widget.GetPreferredHeightForWidth ((int)widthConstraint.AvailableSize, out min_height, out nat_height);
					} else if ((heightConstraint.IsConstrained) && (widthConstraint.IsConstrained)) {
					} else {
						Widget.GetPreferredWidth (out min_width, out nat_width);
						Widget.GetPreferredHeightForWidth (min_width, out min_height, out nat_height);
					}
				} else {
					Widget.GetPreferredWidth (out min_width, out nat_width);
					Widget.GetPreferredHeightForWidth (min_width, out min_height, out nat_height);
				}
			}

			if ((enabledEvents & WidgetEvent.PreferredSizeCheck) != 0) {
				SizeConstraint wc = SizeConstraint.Unconstrained, hc = SizeConstraint.Unconstrained;
				var cp = Widget.Parent as IConstraintProvider;
				if (cp != null)
					cp.GetConstraints (Widget, out wc, out hc);

				ApplicationContext.InvokeUserCode (delegate {
					var w = eventSink.GetPreferredSize (wc, hc);
					min_width = (int) w.Width;
					min_height = (int) w.Height;
				});
			}

			if (Widget.WidthRequest > min_width)
				min_width = Widget.WidthRequest;
			if (Widget.HeightRequest > min_height)
				min_height = Widget.HeightRequest;

			if (Frontend.MinWidth > 0 && Frontend.MinWidth > min_width)
				min_width = (int) Frontend.MinWidth;

			if (Frontend.MinHeight > 0 && Frontend.MinHeight > min_height)
				min_height = (int) Frontend.MinHeight;

			return new Size(min_width, min_height);
		}

		void UpdateSizeRequests ()
		{
			if (minSizeSet) {
				Widget.WidthRequest = (int)Math.Max (Frontend.MinWidth, Frontend.WidthRequest);
				Widget.HeightRequest = (int)Math.Max (Frontend.MinHeight, Frontend.HeightRequest);
			} else {
				Widget.WidthRequest = (int)Frontend.WidthRequest;
				Widget.HeightRequest = (int)Frontend.HeightRequest;
			}
		}

		public void SetMinSize (double width, double height)
		{
			if (width != -1 || height != -1) {
				minSizeSet = true;
				UpdateSizeRequests ();
				Widget.QueueResize ();
			}
			else {
				minSizeSet = false;
				UpdateSizeRequests ();
				Widget.QueueResize ();
			}
		}

		public void SetSizeRequest (double width, double height)
		{
			UpdateSizeRequests ();
		}

		void EnableSizeCheckEvents ()
		{
		}

		void DisableSizeCheckEvents ()
		{
		}

		public double Opacity {
			get {
				using (GLib.Value property = Widget.GetProperty ("opacity")) {
					double result = (double)property;
					return result;
				}
			}
			set {
				using (GLib.Value val = new GLib.Value (value)) {
					Widget.SetProperty ("opacity", val);
				}
			}
		}
	}
}

