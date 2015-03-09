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
using System.Windows.Controls;
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
			Widget.SelectedDatesChanged += (object sender, SelectionChangedEventArgs e) =>  HandleValueChanged(sender, e);
			Widget.DisplayMode = CalendarMode.Month;
			Widget.IsTodayHighlighted = true;
			Widget.SelectionMode = CalendarSelectionMode.SingleDate;
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

		DateTime minDate;
		public DateTime MinDate {
			get {
				return minDate;
			}
			set {
				minDate = value;
				NoMonthChange = noMonthChange;
			}
		}

		DateTime maxDate;
		public DateTime MaxDate {
			get {
				return maxDate;
			}
			set {
				maxDate = value;
				NoMonthChange = noMonthChange;
			}
		}

		bool noMonthChange;
		public bool NoMonthChange {
			get {
				return noMonthChange;
			}
			set {
				noMonthChange = value;
				if (noMonthChange) {
					var dateStart = new DateTime (Calendar.DisplayDate.Year, Calendar.DisplayDate.Month, 1);
					var dateEnd = new DateTime (Calendar.DisplayDate.Year, Calendar.DisplayDate.Month, 1).AddMonths (1).AddDays (-1);
					if (dateStart < MinDate)
						dateStart = MinDate;
					if (dateEnd > MaxDate)
						dateEnd = MaxDate;
					if (Calendar.SelectedDate < dateStart)
						Calendar.SelectedDate = dateStart;
					if (Calendar.SelectedDate > dateEnd)
						Calendar.SelectedDate = dateEnd;
					Calendar.DisplayDateStart = dateStart;
					Calendar.DisplayDateEnd = dateEnd ;
				} else {
					Calendar.DisplayDateStart = MinDate;
					Calendar.DisplayDateEnd = MaxDate;
				}
			}
		}

		void HandleValueChanged (object sender, EventArgs e)
		{
			if (Date < MinDate) {
				Date = MinDate;
				return;
			}
			if (Date > MaxDate) {
				Date = MaxDate;
				return;
			}
			Context.InvokeUserCode (delegate {
				EventSink.OnValueChanged ();
			});
		}
	}
}

