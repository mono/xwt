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
using Xwt;

namespace Samples
{
	public class CalendarSample : VBox
	{
		public CalendarSample ()
		{
			var label = new Label ();
			var calendar = new Calendar () {
				NoMonthChange = false,
				ShowDayNames = true,
				ShowHeading = true,
				ShowWeekNumbers = false,
				ExpandHorizontal = false,
			};
			var entry = new TextEntry () {
				PlaceholderText = "Enter a date to change calendar",
			};
			calendar.ValueChanged += delegate {
				label.Text = string.Format ("Selected date: {0}", calendar.Date.ToShortDateString ());
				entry.Text = calendar.Date.ToShortDateString ();
			};
			label.Text = string.Format ("Selected date: {0} (Event not working, help to fix it)", calendar.Date.ToShortDateString ());

			entry.Activated += delegate {
				var date = DateTime.Parse (entry.Text);
				calendar.Date = date;
			};
			var showDayNames = new CheckBox () {
				Label = "Show day names",
				Active = calendar.ShowDayNames,
			};
			showDayNames.Clicked += delegate {
				calendar.ShowDayNames = showDayNames.Active;
			};
			var showHeading = new CheckBox () {
				Label = "Show heading",
				Active = calendar.ShowHeading,
			};
			showHeading.Clicked += delegate {
				calendar.ShowHeading = showHeading.Active;
			};
			var showWeekNumbers = new CheckBox () {
				Label = "Show Week Numbers",
				Active = calendar.ShowWeekNumbers,
			};
			showWeekNumbers.Clicked += delegate {
				calendar.ShowWeekNumbers = showWeekNumbers.Active;
			};
			var noMonthChange = new CheckBox () {
				Label = "Disable month change",
				Active = calendar.NoMonthChange,
			};
			noMonthChange.Clicked += delegate {
				calendar.NoMonthChange = noMonthChange.Active;
			};
			PackStart (calendar);
			PackStart (noMonthChange);
			PackStart (showDayNames);
			PackStart (showWeekNumbers);
			PackStart (showHeading);
			PackStart (entry);
			PackStart (label);
		}
	}
}

