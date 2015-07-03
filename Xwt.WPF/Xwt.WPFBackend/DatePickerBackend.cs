// 
// DatePickerBackend.cs
//  
// Author:
//       David Karlaš <david.karlas@gmail.com>
// 
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xwt.WPFBackend;
using Xwt.Backends;
using WindowsDatePicker = System.Windows.Controls.DatePicker;

namespace Xwt.WPFBackend
{
	public class DatePickerBackend : WidgetBackend, IDatePickerBackend
	{
		public DatePickerBackend()
		{
			Widget = new WindowsDatePicker();			
		}

		protected WindowsDatePicker DatePicker
		{
			get { return (WindowsDatePicker)Widget; }
		}

		protected new IDatePickerEventSink EventSink
		{
			get { return (IDatePickerEventSink)base.EventSink; }
		}

		public DatePickerStyle Style { get; set; }
		
		public DateTime DateTime
		{
			get
			{
				return DatePicker.SelectedDate ?? new DateTime(0);
			}
			set
			{
				DatePicker.SelectedDate = value;
			}
		}

		public DateTime MinimumDateTime {
			get {
				return DatePicker.DisplayDateStart ?? DateTime.MinValue;
			}
			set {
				DatePicker.DisplayDateStart = value;
				if (DateTime < value)
					DateTime = value;
			}
		}

		public DateTime MaximumDateTime {
			get {
				return DatePicker.DisplayDateEnd ?? DateTime.MaxValue;
			}
			set {
				DatePicker.DisplayDateEnd = value;
				if (DateTime > value)
					DateTime = value;
			}
		}

		public override void EnableEvent(object eventId)
		{
			base.EnableEvent(eventId);
			if (eventId is DatePickerEvent)
			{
				if ((DatePickerEvent)eventId == DatePickerEvent.ValueChanged)
					DatePicker.SelectedDateChanged += HandleValueChanged;
			}
		}

		public override void DisableEvent(object eventId)
		{
			base.DisableEvent(eventId);
			if (eventId is DatePickerEvent)
			{
				if ((DatePickerEvent)eventId == DatePickerEvent.ValueChanged)
					DatePicker.SelectedDateChanged -= HandleValueChanged;
			}
		}

		void HandleValueChanged(object sender, EventArgs e)
		{
			Context.InvokeUserCode(delegate
			{
				EventSink.ValueChanged();
			});
		}		
	}
}
