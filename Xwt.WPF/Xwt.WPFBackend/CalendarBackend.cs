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
	public class CalendarBackend: WidgetBackend, ICalendarBackend
	{
		public CalendarBackend ()
		{
			Widget = new WindowsCalendar ();
			Widget.DisplayMode = CalendarMode.Month;
			Widget.IsTodayHighlighted = true;
			Widget.SelectionMode = CalendarSelectionMode.SingleDate;
		}

		protected virtual WindowsCalendar Calendar {
			get { return (WindowsCalendar)base.Widget; }
		}

		protected new WindowsCalendar Widget {
			get { return Calendar; }
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
					Widget.SelectedDatesChanged += HandleValueChanged;
			}
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is CalendarEvent) {
				if ((CalendarEvent)eventId == CalendarEvent.ValueChanged)
					Widget.SelectedDatesChanged -= HandleValueChanged;
			}
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

		public DateTime MinimumDate {
			get {
				return Widget.DisplayDateStart ?? DateTime.MinValue;
			}
			set {
				if (Widget.SelectedDate < value) {
					Widget.SelectedDate = value;
				}
				Widget.DisplayDateStart = value;
			}
		}

		public DateTime MaximumDate {
			get {
				return Widget.DisplayDateEnd ?? DateTime.MaxValue;
			}
			set {
				if (Widget.SelectedDate > value) {
					Widget.SelectedDate = value;
				}
				Widget.DisplayDateEnd = value;
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

