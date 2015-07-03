//
// DatePickerSample.cs
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
using Xwt;

namespace Samples
{
	public class DatePickerSample: VBox
	{
		public DatePickerSample ()
		{
			var dtp = new DatePicker (DateTime.Now);
			PackStart (dtp);

			Label la1 = new Label ("Initial Value: " + dtp.DateTime);
			PackStart (la1);
			dtp.ValueChanged += delegate {
				la1.Text = "Value changed: " + dtp.DateTime;
			};

			var dp = new DatePicker (DatePickerStyle.Date);
			PackStart (dp);

			Label la2 = new Label ("Initial Value: " + dp.DateTime);
			PackStart (la2);
			dp.ValueChanged += delegate {
				la2.Text = "Value changed: " + dp.DateTime;
			};

			var tp = new DatePicker (DatePickerStyle.Time, DateTime.MinValue);
			PackStart (tp);

			Label la3 = new Label ("Initial Value: " + tp.DateTime);
			PackStart (la3);
			tp.ValueChanged += delegate {
				la3.Text = "Value changed: " + tp.DateTime;
			};
		}
	}
}

