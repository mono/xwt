//
// Calendar.cs
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

namespace Xwt
{
	[BackendType (typeof(ICalendarBackend))]
	public class Calendar : Widget
	{
		EventHandler valueChanged, doubleClick;

		static Calendar ()
		{
			MapEvent (CalendarEvent.ValueChanged, typeof(Calendar), "OnValueChanged");
		}

		protected new class WidgetBackendHost: Widget.WidgetBackendHost, ICalendarEventSink
		{
			public void OnValueChanged ()
			{
				((Calendar)Parent).OnValueChanged (EventArgs.Empty);
			}
		}

		public Calendar ()
		{
			MinimumDate = DateTime.MinValue;
			MaximumDate = DateTime.MaxValue;
			Date = DateTime.Now;
		}

		ICalendarBackend Backend {
			get { return (ICalendarBackend)BackendHost.Backend; }
		}

		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}

		public DateTime Date {
			get {
				return Backend.Date;
			}
			set {
				Backend.Date = value;
			}
		}

		public DateTime MinimumDate {
			get {
				return Backend.MinimumDate;
			}
			set {
				Backend.MinimumDate = value;
				if (MinimumDate > MaximumDate)
					MaximumDate = MinimumDate;
				if (Date < MinimumDate)
					Date = MinimumDate;
			}
		}

		public DateTime MaximumDate {
			get {
				return Backend.MaximumDate;
			}
			set {
				Backend.MaximumDate = value;
				if (MaximumDate < MinimumDate)
					MinimumDate = MaximumDate;
				if (Date > MaximumDate)
					Date = MaximumDate;
			}
		}

		protected virtual void OnValueChanged (EventArgs e)
		{
			if (valueChanged != null)
				valueChanged (this, e);
		}

		public event EventHandler ValueChanged {
			add {
				BackendHost.OnBeforeEventAdd (CalendarEvent.ValueChanged, valueChanged);
				valueChanged += value;
			}
			remove {
				valueChanged -= value;
				BackendHost.OnAfterEventRemove (CalendarEvent.ValueChanged, valueChanged);
			}
		}
	}
}

