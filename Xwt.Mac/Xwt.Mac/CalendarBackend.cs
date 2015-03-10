//
// CalendarBackend.cs
//
// Author:
//       Ezequiel Taranto <ezequiel89@gmail.com>
//		 Claudio Rodrigo Pereyra Diaz <claudiorodrigo@pereyeradiaz.com.ar> 
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

#if MONOMAC
using nint = System.Int32;
using nfloat = System.Single;
using MonoMac.Foundation;
using MonoMac.AppKit;
#else
using Foundation;
using AppKit;
#endif

namespace Xwt.Mac
{
	public class CalendarBackend: ViewBackend<NSDatePicker,ICalendarEventSink>, ICalendarBackend
	{
		public CalendarBackend ()
		{
		}

		public override void Initialize ()
		{
			base.Initialize ();
			ViewObject = new MacCalendar ();
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is CalendarEvent)
				Widget.Activated += HandleValueChanged;
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is CalendarEvent)
				Widget.Activated -= HandleValueChanged;
		}

		void HandleActivated (object sender, EventArgs e)
		{
			ApplicationContext.InvokeUserCode (((ICalendarEventSink)EventSink).OnValueChanged);
		}


		public DateTime Date {
			get {
				return (DateTime)Widget.DateValue;
			}
			set {
				var currentDate = (NSDate)value;
				Widget.DateValue = currentDate;
			}
		}
		DateTime minDate;
		public DateTime MinDate {
			get {
				return minDate;
			}
			set {
				minDate = value;
				if (!NoMonthChange) {
					var date = (NSDate)minDate;
					Widget.MinDate = date;
				}
			}
		}
		DateTime maxDate;
		public DateTime MaxDate {
			get {
				return maxDate;
			}
			set {
				maxDate = value;
				if (!NoMonthChange) {
					var date = (NSDate)maxDate;
					Widget.MaxDate = date;
				}
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
					var dateStart = new DateTime (Date.Year, Date.Month, 1);
					var dateEnd = new DateTime (Date.Year, Date.Month, 1).AddMonths (1).AddDays (-1);
					if (dateStart < MinDate)
						dateStart = MinDate;
					if (dateEnd > MaxDate)
						dateEnd = MaxDate;
					Widget.MinDate = (NSDate)dateStart;
					Widget.MaxDate = (NSDate)dateEnd ;
				} else {
					Widget.MinDate = (NSDate)MinDate;
					Widget.MaxDate = (NSDate)MaxDate;
				}
			}
		}

		void HandleValueChanged (object sender, EventArgs e)
		{
			if (Date < MinDate) {
				Date = MinDate;
			}
			if (Date > MaxDate) {
				Date = MaxDate;
			}
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnValueChanged ();
			});
		}

	}

	class MacCalendar: NSDatePicker, IViewObject
	{

		public ViewBackend Backend { get; set; }

		public NSView View { get { return this; } }

		public MacCalendar()
		{
			DatePickerStyle = NSDatePickerStyle.ClockAndCalendar;
			DatePickerMode = NSDatePickerMode.Single;
			Bordered = true;
			DatePickerElements = NSDatePickerElementFlags.YearMonthDateDay;
			DrawsBackground = true;

		}
	}

}

