//
// SliderBackend.cs
//
// Author:
//       Jérémie Laval <jeremie.laval@xamarin.com>
//
// Copyright (c) 2013 Xamarin, Inc.
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


using Gtk;

namespace Xwt.GtkBackend
{
	public class SliderBackend : WidgetBackend, ISliderBackend
	{
		public SliderBackend ()
		{
		}
		
		public override void Initialize ()
		{
			Widget = (Gtk.Scale) CreateWidget ();
			Widget.DrawValue = false;
			Widget.Show ();
		}
		
		protected virtual Gtk.Widget CreateWidget ()
		{
			return new Gtk.HScale (0, 1.0, 0.1);
		}
		
		protected new Gtk.Scale Widget {
			get { return (Gtk.Scale)base.Widget; }
			set { base.Widget = value; }
		}
		
		protected new ISliderEventSink EventSink {
			get { return (ISliderEventSink)base.EventSink; }
		}
		
		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is SliderEvent) {
				if ((SliderEvent)eventId == SliderEvent.ValueChanged)
					Widget.ValueChanged += HandleValueChanged;
			}
		}
		
		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is SliderEvent) {
				if ((SliderEvent)eventId == SliderEvent.ValueChanged)
					Widget.ValueChanged -= HandleValueChanged;
			}
		}
		
		void HandleValueChanged (object sender, EventArgs e)
		{
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.ValueChanged ();
			});
		}

		public double Value {
			get { return Widget.Value; }
			set { Widget.Value = value; }
		}

		public double MaximumValue {
			get { return Widget.Adjustment.Upper; }
			set { Widget.SetRange (Math.Min (value - 1, MinimumValue), value); }
		}

		public double MinimumValue {
			get { return Widget.Adjustment.Lower; }
			set { Widget.SetRange (value, Math.Max (value + 1, MaximumValue)); }
		}
	}
}

