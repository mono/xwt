//
// CalendarBackend.cs
//
// Author:
//       Claudio Rodrigo Pereyra Diaz <claudiorodrigo@pereyradiaz.com.ar>
//
// Copyright (c) 2015 Hamekoz
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

namespace Xwt.GtkBackend
{
	public class CalendarBackend: WidgetBackend, ICalendarBackend
	{
		public override void Initialize ()
		{
			Widget = new Gtk.Calendar ();
			Widget.DaySelected += CheckBetweenMinMax;
			Widget.Show ();
		}

		void CheckBetweenMinMax (object sender, EventArgs e)
		{
			if (Date < MinimumDate)
				Date = MinimumDate;
			if (Date > MaximumDate)
				Date = MaximumDate;
		}

		protected new Gtk.Calendar Widget {
			get { return (Gtk.Calendar)base.Widget; }
			set { base.Widget = value; }
		}

		protected new ICalendarEventSink EventSink {
			get { return (ICalendarEventSink)base.EventSink; }
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is CalendarEvent) {
				if ((CalendarEvent)eventId == CalendarEvent.ValueChanged)
					Widget.DaySelected += HandleValueChanged;
			}
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is CalendarEvent) {
				if ((CalendarEvent)eventId == CalendarEvent.ValueChanged)
					Widget.DaySelected -= HandleValueChanged;
			}
		}

		public DateTime Date {
			get {
				return Widget.Date;
			}
			set {
				Widget.Date = value;
			}
		}

		DateTime minimumDate;

		public DateTime MinimumDate {
			get {
				return minimumDate;
			}
			set {
				if (Widget.Date < value) {
					Widget.Date = value;
				}
				minimumDate = value;
			}
		}

		DateTime maximumDate;

		public DateTime MaximumDate {
			get {
				return maximumDate;
			}
			set {
				if (Widget.Date > value) {
					Widget.Date = value;
				}
				maximumDate = value;
			}
		}

		void HandleValueChanged (object sender, EventArgs e)
		{
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnValueChanged ();
			});
		}
	}
}

