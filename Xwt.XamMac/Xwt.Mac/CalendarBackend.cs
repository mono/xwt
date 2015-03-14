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

		// NSDate timezone workaround: cache and restore the DateTimeKind of the
		// users DateTime object, since all conversions between DateTime and NSDate
		// are in UTC (see https://github.com/mono/maccore/blob/master/src/Foundation/NSDate.cs).
		bool userTimeIsUTC;

		public DateTime Date {
			get {
				if (userTimeIsUTC)
					return ((DateTime)Widget.DateValue).ToUniversalTime ();
				else
					return ((DateTime)Widget.DateValue).ToLocalTime ();
			}
			set {

				if (value.Kind == DateTimeKind.Local) {
					userTimeIsUTC = false;
					Widget.TimeZone = NSTimeZone.LocalTimeZone;
				} else {
					userTimeIsUTC = true;
					Widget.TimeZone = NSTimeZone.FromName ("UTC");
				}
				var date = (NSDate)value.ToUniversalTime ();
				Widget.DateValue = date;
			}
		}

		public DateTime MinimumDate {
			get {
				if (userTimeIsUTC)
					return ((DateTime)Widget.MinDate).ToUniversalTime ();
				else
					return ((DateTime)Widget.MinDate).ToLocalTime ();
			}
			set {
				var minDate = (NSDate)value.ToUniversalTime ();
				Widget.MinDate = minDate;
			}
		}

		public DateTime MaximumDate {
			get {
				if (userTimeIsUTC)
					return ((DateTime)Widget.MaxDate).ToUniversalTime ();
				else
					return ((DateTime)Widget.MaxDate).ToLocalTime ();
			}
			set {
				var maxDate = (NSDate)value.ToUniversalTime ();
				Widget.MaxDate = maxDate;
			}
		}

		void HandleValueChanged (object sender, EventArgs e)
		{
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnValueChanged ();
			});
		}
	}

	class MacCalendar: NSDatePicker, IViewObject
	{
		public ViewBackend Backend { get; set; }

		public NSView View { get { return this; } }

		public MacCalendar ()
		{
			DatePickerStyle = NSDatePickerStyle.ClockAndCalendar;
			DatePickerMode = NSDatePickerMode.Single;
			Bordered = true;
			DatePickerElements = NSDatePickerElementFlags.YearMonthDateDay;
			DrawsBackground = true;
			MinDate = DateTime.MinValue;
			MaxDate = DateTime.MaxValue;
		}
	}
}

