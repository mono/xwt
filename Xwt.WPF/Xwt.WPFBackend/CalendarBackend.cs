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
using Xwt.WPFBackend;
using Xwt.Backends;
using WindowsCalendar = System.Windows.Controls.Calendar;

namespace Xwt.WPFBackend
{
	public partial class CalendarBackend: WidgetBackend, ICalendarBackend
	{
		public CalendarBackend ()
		{
			Widget = new WindowsCalendar ();
		}

		protected virtual WindowsCalendar Calendar
		{
			get { return (WindowsCalendar)base.Widget; }
		}

		protected new WindowsCalendar Widget {
			get { return Calendar; }
			set { base.Widget = value; }
		}

		protected new ICalendarEventSink EventSink {
			get { return (ICalendarEventSink)base.EventSink; }
		}

		public DateTime Date {
			get {
				return Calendar.SelectedDate ?? Calendar.DisplayDate;
			}
			set {
				Calendar.DisplayDate = value;
				Calendar.SelectedDate = value;
			}
		}

		bool noMonthChange;
		public bool NoMonthChange {
			get {
				return noMonthChange;
			}
			set {
				noMonthChange = value;
				if (value) {
					var dateStart = new DateTime (Calendar.DisplayDate.Year, Calendar.DisplayDate.Month, 1);
					var dateEnd = new DateTime (Calendar.DisplayDate.Year, Calendar.DisplayDate.Month, 1).AddMonths (1).AddDays (-1);
					if (Calendar.SelectedDate < dateStart)
						Calendar.SelectedDate = dateStart;
					if (Calendar.SelectedDate > dateEnd)
						Calendar.SelectedDate = dateEnd;
					Calendar.DisplayDateStart = dateStart;
					Calendar.DisplayDateEnd = dateEnd ;
				} else {
					Calendar.DisplayDateStart = DateTime.MinValue;
					Calendar.DisplayDateEnd = DateTime.MaxValue;
				}
			}
		}

		public bool ShowDayNames {
			get;
			set;
		}

		public bool ShowHeading {
			get;
			set;
		}

		public bool ShowWeekNumbers {
			get;
			set;
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is CalendarEvent) {
				switch ((CalendarEvent)eventId) {
				case CalendarEvent.ValueChanged:
					Calendar.DisplayDateChanged += HandleValueChanged;
					Calendar.SelectedDatesChanged += HandleValueChanged;
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
					Calendar.DisplayDateChanged -= HandleValueChanged;
					Calendar.SelectedDatesChanged -= HandleValueChanged;
					break;
				}
			}
		}

		void HandleValueChanged (object sender, EventArgs e)
		{
			Context.InvokeUserCode (delegate {
				EventSink.OnValueChanged ();
			});
		}
	}
}

