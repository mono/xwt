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
using AppKit;
using Foundation;
using Xwt.Backends;

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
		// users DateTime objects, since all conversions between DateTime and NSDate
		// are in UTC (see https://github.com/mono/maccore/blob/master/src/Foundation/NSDate.cs).
		DateTimeKind userDateKind = DateTimeKind.Unspecified;
		DateTimeKind userMinDateKind = DateTimeKind.Unspecified;
		DateTimeKind userMaxDateKind = DateTimeKind.Unspecified;

		public DateTime Date {
			get {
				if (userDateKind == DateTimeKind.Utc)
					return ((DateTime)Widget.DateValue).ToUniversalTime ();
				// handle DateTimeKind.Unspecified and DateTimeKind.Local the same way
				// like its done by System.TimeZone.ToUniversalTime when setting the Date.
				return new DateTime(((DateTime)Widget.DateValue).ToLocalTime ().Ticks, userDateKind);
			}
			set {
				userDateKind = value.Kind;
				Widget.DateValue = (NSDate)value.ToUniversalTime ();
			}
		}

		public DateTime MinimumDate {
			get {
				if (userMinDateKind == DateTimeKind.Utc)
					return ((DateTime)Widget.MinDate).ToUniversalTime ();
				return new DateTime(((DateTime)Widget.MinDate).ToLocalTime ().Ticks, userMinDateKind);
			}
			set {
				userMinDateKind = value.Kind;
				Widget.MinDate = (NSDate)value.ToUniversalTime ();
			}
		}

		public DateTime MaximumDate {
			get {
				if (userMaxDateKind == DateTimeKind.Utc)
					return ((DateTime)Widget.MaxDate).ToUniversalTime ();
				return new DateTime(((DateTime)Widget.MaxDate).ToLocalTime ().Ticks, userMaxDateKind);
			}
			set {
				userMaxDateKind = value.Kind;
				Widget.MaxDate = (NSDate)value.ToUniversalTime ();
			}
		}

		void HandleValueChanged (object sender, EventArgs e)
		{
			ApplicationContext.InvokeUserCode (EventSink.OnValueChanged);
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
			MinDate = (NSDate)DateTime.MinValue.ToUniversalTime ();
			MaxDate = (NSDate)DateTime.MaxValue.ToUniversalTime ();
		}
	}
}

