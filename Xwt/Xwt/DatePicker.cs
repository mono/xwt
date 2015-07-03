//
// DatePicker.cs
//
// Author:
//       Jérémie Laval <jeremie.laval@xamarin.com>
//
// Copyright (c) 2012 Xamarin, Inc.
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
	public enum DatePickerStyle {
		Time,
		Date,
		DateTime
	}
	
	[BackendType (typeof(IDatePickerBackend))]
	public class DatePicker : Widget
	{
		public DatePicker () : this (DatePickerStyle.DateTime, DateTime.Now)
		{
		}

		public DatePicker (DateTime initialDateTime) : this (DatePickerStyle.DateTime, initialDateTime)
		{
		}

		public DatePicker (DatePickerStyle style) : this (style, DateTime.Now)
		{
		}

		public DatePicker (DatePickerStyle style, DateTime initialDateTime)
		{
			VerifyConstructorCall (this);
			Style = style;
			DateTime = initialDateTime;
		}

		static DatePicker ()
		{
			MapEvent (DatePickerEvent.ValueChanged, typeof (Label), "OnValueChanged");
		}

		protected new class WidgetBackendHost: Widget.WidgetBackendHost, IDatePickerEventSink
		{
			public void ValueChanged ()
			{
				((DatePicker)Parent).OnValueChanged (EventArgs.Empty);
			}
		}
		
		IDatePickerBackend Backend {
			get { return (IDatePickerBackend) BackendHost.Backend; }
		}
		
		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}
		
		public DateTime DateTime {
			get {
				return Backend.DateTime;
			}
			set {
				Backend.DateTime = value;
			}
		}

		public DateTime MinimumDateTime {
			get {
				return Backend.MinimumDateTime;
			}
			set {
				Backend.MinimumDateTime = value;
			}
		}

		public DateTime MaximumDateTime {
			get {
				return Backend.MaximumDateTime;
			}
			set {
				Backend.MaximumDateTime = value;
			}
		}
		
		public DatePickerStyle Style {
			get {
				return Backend.Style;
			}
			set {
				Backend.Style = value;
			}
		}
		
		protected virtual void OnValueChanged (EventArgs e)
		{
			if (valueChanged != null)
				valueChanged (this, e);
		}

		EventHandler valueChanged;
		
		public event EventHandler ValueChanged {
			add {
				BackendHost.OnBeforeEventAdd (DatePickerEvent.ValueChanged, valueChanged);
				valueChanged += value;
			}
			remove {
				valueChanged -= value;
				BackendHost.OnAfterEventRemove (DatePickerEvent.ValueChanged, valueChanged);
			}
		}
	}
}

