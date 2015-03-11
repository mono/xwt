//
// DatePickerBackend.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc.
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
	public class DatePickerBackend: ViewBackend<NSDatePicker,IDatePickerEventSink>, IDatePickerBackend
	{
		public DatePickerBackend ()
		{
		}

		public override void Initialize ()
		{
			base.Initialize ();
			ViewObject = new MacDatePicker ();
			Widget.DatePickerMode = NSDatePickerMode.Single;
			Widget.DatePickerStyle = NSDatePickerStyle.TextFieldAndStepper;
			Widget.DatePickerElements = DatePickerStyle.DateTime.ToMacValue();
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is DatePickerEvent)
				Widget.Activated += HandleActivated;
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is DatePickerEvent)
				Widget.Activated -= HandleActivated;
		}

		void HandleActivated (object sender, EventArgs e)
		{
			ApplicationContext.InvokeUserCode (((IDatePickerEventSink)EventSink).ValueChanged);
		}

		#region IDatePickerBackend implementation

		// NSDate timezone workaround: cache and restore the DateTimeKind of the
		// users DateTime object, since all conversions between DateTime and NSDate
		// are in UTC (see https://github.com/mono/maccore/blob/master/src/Foundation/NSDate.cs).
		bool userTimeIsUTC;

		public DateTime DateTime {
			get {
				if (userTimeIsUTC)
					return ((DateTime)Widget.DateValue).ToUniversalTime();
				else
					return ((DateTime)Widget.DateValue).ToLocalTime();
			}
			set {

				if (value.Kind == DateTimeKind.Local) {
					userTimeIsUTC = false;
					Widget.TimeZone = NSTimeZone.LocalTimeZone;
				} else {
					userTimeIsUTC = true;
					Widget.TimeZone = NSTimeZone.FromName("UTC");
				}

				Widget.DateValue = (NSDate)value.ToUniversalTime ();
			}
		}

		public DateTime MinDateTime {
			get {
				if (userTimeIsUTC)
					return ((DateTime)Widget.MinDate).ToUniversalTime();
				else
					return ((DateTime)Widget.MinDate).ToLocalTime();
			}
			set {
				Widget.MinDate = (NSDate)value.ToUniversalTime ();
			}
		}

		public DateTime MaxDateTime {
			get {
				if (userTimeIsUTC)
					return ((DateTime)Widget.MaxDate).ToUniversalTime();
				else
					return ((DateTime)Widget.MaxDate).ToLocalTime();
			}
			set {
				Widget.MaxDate = (NSDate)value.ToUniversalTime ();
			}
		}

		public DatePickerStyle Style {
			get {
				return Widget.DatePickerElements.ToXwtValue();
			}
			set {
				Widget.DatePickerElements = value.ToMacValue ();
			}
		}

		#endregion
	}

	class MacDatePicker: NSDatePicker, IViewObject
	{
		public NSView View { get { return this; } }
		public ViewBackend Backend { get; set; }

		public override void ResetCursorRects ()
		{
			base.ResetCursorRects ();
			if (Backend.Cursor != null)
				AddCursorRect (Bounds, Backend.Cursor);
		}
	}
}

