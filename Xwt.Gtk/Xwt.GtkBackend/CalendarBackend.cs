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
	public partial class CalendarBackend: WidgetBackend, ICalendarBackend
	{
		public override void Initialize ()
		{
			Widget = new Gtk.Calendar ();
			Widget.Show ();
		}

		protected virtual Gtk.Calendar Calendar {
			get { return (Gtk.Calendar)base.Widget; }
		}

		protected new Gtk.Calendar Widget {
			get { return Calendar; }
			set { base.Widget = value; }
		}

		protected new ICalendarEventSink EventSink {
			get { return (ICalendarEventSink)base.EventSink; }
		}

		public DateTime Date {
			get {
				return Widget.Date;
			}
			set {
				Widget.Date = value;
			}
		}

		public bool NoMonthChange {
			get {
				return Widget.NoMonthChange;
			}
			set {
				Widget.NoMonthChange = value;
			}
		}

		public bool ShowDayNames {
			get {
				return Widget.ShowDayNames;
			}
			set {
				Widget.ShowDayNames = value;
			}
		}

		public bool ShowHeading {
			get {
				return Widget.ShowHeading;
			}
			set {
				Widget.ShowHeading = value;
			}
		}

		public bool ShowWeekNumbers {
			get {
				return Widget.ShowWeekNumbers;
			}
			set {
				Widget.ShowWeekNumbers = value;
			}
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is CalendarEvent) {
				switch ((CalendarEvent)eventId) {
				case CalendarEvent.ValueChanged:
					Widget.DaySelected += HandleValueChanged;
					Widget.DaySelectedDoubleClick += HandleValueChanged;
					Widget.MonthChanged += HandleValueChanged;
					break;
				}
			}
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is CalendarEvent) {
				switch ((CalendarEvent)eventId) {
				case CalendarEvent.ValueChanged:
					Widget.DaySelected -= HandleValueChanged;
					Widget.DaySelectedDoubleClick -= HandleValueChanged;
					Widget.MonthChanged -= HandleValueChanged;
					break;
				}
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

