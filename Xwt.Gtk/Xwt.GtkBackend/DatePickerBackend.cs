//
// DatePickerBackend.cs
//
// Author:
//       Jérémie Laval <jeremie.laval@xamarin.com>
//       Vsevolod Kukol <sevo@sevo.org>
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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xwt.Backends;


namespace Xwt.GtkBackend
{
	public class DatePickerBackend : WidgetBackend, IDatePickerBackend
	{
		public override void Initialize ()
		{
			Widget = new GtkDatePickerEntry ();
			Widget.ShowAll ();
		}
		
		new GtkDatePickerEntry Widget {
			get { return (GtkDatePickerEntry)base.Widget; }
			set { base.Widget = value; }
		}
		
		protected new IDatePickerEventSink EventSink {
			get { return (IDatePickerEventSink)base.EventSink; }
		}
		
		public DateTime DateTime {
			get {
				return Widget.DateTime;
			}
			set {
				Widget.DateTime = value;
			}
		}

		public DateTime MinimumDateTime {
			get {
				return Widget.MinimumDateTime;
			}
			set {
				Widget.MinimumDateTime = value;
			}
		}

		public DateTime MaximumDateTime {
			get {
				return Widget.MaximumDateTime;
			}
			set {
				Widget.MaximumDateTime = value;
			}
		}

		public DatePickerStyle Style {
			get {
				return Widget.DatePickerStyle;
			}
			set {
				Widget.DatePickerStyle = value;
			}
		}
		
		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is DatePickerEvent) {
				if ((DatePickerEvent)eventId == DatePickerEvent.ValueChanged)
					Widget.ValueChanged += HandleValueChanged;
			}
		}
		
		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is DatePickerEvent) {
				if ((DatePickerEvent)eventId == DatePickerEvent.ValueChanged)
					Widget.ValueChanged -= HandleValueChanged;
			}
		}

		void HandleValueChanged (object sender, EventArgs e)
		{
			ApplicationContext.InvokeUserCode (EventSink.ValueChanged);
		}
		
		public class GtkDatePickerEntry : Gtk.SpinButton
		{
			static string[] styleFormats = new string[3];

			static GtkDatePickerEntry ()
			{
				styleFormats[(int)DatePickerStyle.Date] = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
				// we use a custom static long time pattern, since we do not support 12/24 formats
				string timeSeparator = CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator;
				styleFormats[(int)DatePickerStyle.Time] = "HH" + timeSeparator + "mm" + timeSeparator + "ss";
				styleFormats[(int)DatePickerStyle.DateTime] = styleFormats[(int)DatePickerStyle.Date] + " " + styleFormats[(int)DatePickerStyle.Time];
			}

			Dictionary<DateTimeComponent, int> componentPosition = new Dictionary<DateTimeComponent, int>();
			Dictionary<DateTimeComponent, int> componentLength = new Dictionary<DateTimeComponent, int>();
			List<DateTimeComponent> componentsSorted = new List<DateTimeComponent>();
			
			public new EventHandler ValueChanged;

			DateTimeComponent selectedComponent = DateTimeComponent.None;

			GtkClipboardBackend clipboard = new GtkClipboardBackend();

			DateTime currentValue = DateTime.MinValue;

			public DateTime DateTime {
				get {
					return currentValue;
				}
				set {
					currentValue = value;
					Adjustment.Value = value.Ticks;
					RaiseChangedEvent ();
				}
			}

			public DateTime MinimumDateTime {
				get {
					return new DateTime ((long)Adjustment.Lower);
				}
				set {
					Adjustment.Lower = value.Ticks;
					if (DateTime < value)
						DateTime = value;
				}
			}

			public DateTime MaximumDateTime {
				get {
					return new DateTime ((long)Adjustment.Upper);
				}
				set {
					Adjustment.Upper = value.Ticks;
					if (DateTime > value)
						DateTime = value;
				}
			}

			DatePickerStyle style;
			public DatePickerStyle DatePickerStyle {
				get {
					return style;
				}
				set {
					style = value;
					string format = styleFormats [(int)value];

					componentPosition.Clear ();
					componentLength.Clear ();

					if (value == DatePickerStyle.DateTime ||
					    value == DatePickerStyle.Date) {
						componentPosition.Add (DateTimeComponent.Day, format.IndexOf ('d'));
						componentPosition.Add (DateTimeComponent.Month, format.IndexOf ('M'));
						componentPosition.Add (DateTimeComponent.Year, format.IndexOf ('y'));
						componentLength.Add (DateTimeComponent.Day, 2);
						componentLength.Add (DateTimeComponent.Month, 2);
						componentLength.Add (DateTimeComponent.Year, 4);
					}
					if (value == DatePickerStyle.DateTime ||
					    value == DatePickerStyle.Time) {
						componentPosition.Add (DateTimeComponent.Hour, format.IndexOfAny (new char[] { 'H', 'h' }));
						componentPosition.Add (DateTimeComponent.Minute, format.IndexOf ('m'));
						componentPosition.Add (DateTimeComponent.Second, format.IndexOf ('s'));
						componentLength.Add (DateTimeComponent.Hour, 2);
						componentLength.Add (DateTimeComponent.Minute, 2);
						componentLength.Add (DateTimeComponent.Second, 2);
					}

					componentsSorted = componentPosition.OrderBy (k => k.Value).Select (k => k.Key).ToList ();

					WidthChars = format.Length;
				}
			}

			public GtkDatePickerEntry () : this (DatePickerStyle.DateTime)
			{
			}
	
			public GtkDatePickerEntry (DatePickerStyle style) : base (DateTime.MinValue.Ticks,
			                                                          DateTime.MaxValue.Ticks,
			                                                          TimeSpan.TicksPerSecond)
			{
				DatePickerStyle = style;

				Adjustment.PageIncrement = TimeSpan.TicksPerDay;
				IsEditable = true;
				HasFrame = true;
				DateTime = DateTime.Now;
				Adjustment.ValueChanged += HandleValueChanged;
			}

			void HandleValueChanged (object sender, EventArgs e)
			{
				if (Math.Abs (Adjustment.Value - currentValue.Ticks) < 1)
					return;

				if (selectedComponent == DateTimeComponent.None)
					SelectComponent (componentsSorted.Last ());

				if (Adjustment.Value > currentValue.Ticks)
					DateTime = currentValue.AddComponent (selectedComponent, 1);
				else if (Adjustment.Value < currentValue.Ticks)
					DateTime = currentValue.AddComponent (selectedComponent, -1);
			}

			protected override void OnDestroyed()
			{
				Adjustment.ValueChanged -= HandleValueChanged;
				base.OnDestroyed();
			}

			protected override int OnOutput ()
			{
				DateTime dateTime = new DateTime ((long)Adjustment.Value);
				string format = styleFormats [(int)DatePickerStyle];

				Text = dateTime.ToString (format);
				return 1;
			}
			
			protected override int OnInput (out double new_value)
			{
				new_value = Adjustment.Value;
				return 1;
			}

			protected override void OnFocusGrabbed ()
			{
				base.OnFocusGrabbed ();
				// Override default GTK behavior which is to select the whole entry
				SelectComponent (selectedComponent);
			}

			protected override bool OnFocusOutEvent (Gdk.EventFocus evnt)
			{
				SelectComponent (DateTimeComponent.None);
				return base.OnFocusOutEvent (evnt);
			}

			protected override void OnChanged ()
			{
				base.OnChanged ();
				SelectComponent (selectedComponent);
			}

			protected override void OnClipboardPasted ()
			{
				if (clipboard.IsTypeAvailable(TransferDataType.Text)) {
					var newText = clipboard.GetData(TransferDataType.Text) as string;
					DateTime newDateTime;
					if (DateTime.TryParse (newText, out newDateTime))
						DateTime = newDateTime;
					else  if (componentLength.ContainsKey (selectedComponent) 
					          && componentLength[selectedComponent] == newText.Length) {

						try {
							var value = int.Parse (newText);
							DateTime = DateTime.SetComponent (selectedComponent, value);
						} catch {
							return;
						}
					}
				}
			}

			Gdk.Window entryWindow;
			Gdk.Window EntryWindow {
				get {
					if (entryWindow == null) {
						#if XWT_GTK3
						// Problem: base.GdkWindow is the parent window and this widgets windows
						// (3: entry + two spin buttons) are children of the parent, which may hold
						// other windows of its children (e.g. container).
						// 
						// Workaround: find child windows inside own allocation and select the first
						// window from the left, which is the window holding the text entry of the
						// spin button.
						entryWindow = GdkWindow.Children
							// get child window geometry
							.Select(childw => {
									int x, y, w, h;
									childw.GetGeometry (out x, out y, out w, out h);
									return new KeyValuePair<Gdk.Window, Gdk.Rectangle> (childw, new Gdk.Rectangle (x, y, w, h));
								})
							// select windows inside own allocation
							.Where(k => Allocation.Contains (k.Value))
							// order by X location from the left
							.OrderBy (k => k.Value.X)
							// select first window or null if failed
							.FirstOrDefault ().Key;
						#else
						// Hacky. Since it's the entry that maintains the text GdkWindow it's normally
						// the last child of the widget's GdkWindow because the other children are created
						// by the spin button
						entryWindow = GdkWindow.Children.LastOrDefault ();
						#endif
					}
					return entryWindow;
				}
			}
			
			protected override bool OnButtonPressEvent (Gdk.EventButton evnt)
			{
				// handle left click only
				if (evnt.Button != 1)
					return base.OnButtonPressEvent (evnt);

				// a spin button has 3 windows (entry + 2 buttons), handle this event only for the window
				// containing the text entry widget
				if (evnt.Window != EntryWindow)
					return base.OnButtonPressEvent (evnt);

				int layoutX, layoutY;
				GetLayoutOffsets (out layoutX, out layoutY);
				int index, trailing;
				bool insideLayout = Layout.XyToIndex (Pango.Units.FromPixels ((int)evnt.X),
				                                      Pango.Units.FromPixels ((int)evnt.Y),
				                                      out index,
				                                      out trailing);

				if (insideLayout) {
					SelectComponentAtPosition (TextIndexToLayoutIndex (index));
					GrabFocus ();
					return false;
				}
				SelectComponent (DateTimeComponent.None);
				return base.OnButtonPressEvent (evnt);
			}

			int currentDigitInsert;
			protected override bool OnKeyReleaseEvent (Gdk.EventKey evnt)
			{
				char pressedKey = (char)Gdk.Keyval.ToUnicode (evnt.KeyValue);

				if (char.IsWhiteSpace(pressedKey) || char.IsPunctuation (pressedKey) || char.IsSeparator (pressedKey)) {
					if (pressedKey != '\t') // exclude tab
						SelectNextComponent ();
				}

				if (char.IsDigit (pressedKey) && selectedComponent != DateTimeComponent.None && pressedKey >= '0') {
					try {
						DateTime current = DateTime;
						current = current.SetComponent (selectedComponent, AddDigitToValue (current.GetComponent (selectedComponent), pressedKey));
						if (currentDigitInsert < componentLength[selectedComponent] - 1)
							currentDigitInsert++;
						else
							currentDigitInsert = 0;

						DateTime = current;
					} catch (ArgumentOutOfRangeException) {
						if (pressedKey == '0')
							return true;
						if (currentDigitInsert != 0) {
							// In case date wasn't representable we redo the call with an updated digit insert
							currentDigitInsert = 0;
							return OnKeyReleaseEvent (evnt);
						}
						return true;
					}
				}
				return base.OnKeyReleaseEvent (evnt);
			}

			int AddDigitToValue (int baseValue, char newValue)
			{
				if (currentDigitInsert == 0 || currentDigitInsert > componentLength [selectedComponent])
					return int.Parse (newValue.ToString ());
				return int.Parse (baseValue.ToString () + newValue);
			}
	
			protected override bool OnKeyPressEvent (Gdk.EventKey evnt)
			{

				if (evnt.Key == Gdk.Key.Left || evnt.Key == Gdk.Key.KP_Left) {
					SelectPrevComponent ();
					return true;
				}

				if (evnt.Key == Gdk.Key.Right || evnt.Key == Gdk.Key.KP_Right) {
					SelectNextComponent ();
					return true;
				}

				// We only allow the keypress to proceed to the normal handler
				// if it doesn't involve adding an actual character
				// (i.e. navigation keys)
				uint value = Gdk.Keyval.ToUnicode (evnt.KeyValue);
				var xwtModifiers = evnt.State.ToXwtValue ();
				if (value == 0 || value == '\t' || xwtModifiers.HasFlag(ModifierKeys.Control) || xwtModifiers.HasFlag (ModifierKeys.Alt))
					return base.OnKeyPressEvent (evnt);
				return true;
			}

			void SelectComponentAtPosition (int characterIndex)
			{
				foreach (var entry in componentPosition) {
					if (characterIndex >= entry.Value  && characterIndex <= entry.Value + componentLength [entry.Key]) {
						SelectComponent (entry.Key);
						return;
					}
				}
				SelectComponent (DateTimeComponent.None);
			}

			void SelectComponent (DateTimeComponent component)
			{
				int startPos, endPos;
				if (componentPosition.ContainsKey (component) && componentLength.ContainsKey (component)) {
					startPos = componentPosition[component];
					endPos = startPos + componentLength[component];
				} else {
					startPos = CursorPosition;
					endPos = CursorPosition;
				}
				SelectRegion (startPos, endPos);

				if (selectedComponent != component) {
					selectedComponent = component;
					currentDigitInsert = 0;
				}
			}

			void SelectNextComponent ()
			{
				if (selectedComponent == DateTimeComponent.None ||
				        componentsSorted.IndexOf (selectedComponent) == componentsSorted.Count - 1)
					selectedComponent = componentsSorted [0];
				else
					selectedComponent = componentsSorted [componentsSorted.IndexOf (selectedComponent) + 1];

				int startPos = componentPosition[selectedComponent];
				int endPos = startPos + componentLength[selectedComponent];
				SelectRegion (startPos, endPos);
				currentDigitInsert = 0;
			}

			void SelectPrevComponent ()
			{
				if (selectedComponent == DateTimeComponent.None ||
				        componentsSorted.IndexOf (selectedComponent) == 0)
					selectedComponent = componentsSorted [componentsSorted.Count - 1];
				else
					selectedComponent = componentsSorted [componentsSorted.IndexOf (selectedComponent) - 1];

				int startPos = componentPosition[selectedComponent];
				int endPos = startPos + componentLength[selectedComponent];
				SelectRegion (startPos, endPos);
				currentDigitInsert = 0;
			}
			
			void RaiseChangedEvent ()
			{
				var tmp = ValueChanged;
				if (tmp != null)
					tmp (this, EventArgs.Empty);
			}

			[Obsolete("Use DateTime property instead.")]
			public DateTime CurrentValue {
				get {
					return DateTime;
				}
				set {
					DateTime = value;
				}
			}
		}
	}

	enum DateTimeComponent {
		None = 0,
		Month,
		Day,
		Year,
		Hour,
		Minute,
		Second
	}

	static class DateTimeExtensions
	{
		public static DateTime AddComponent(this DateTime dateTime, DateTimeComponent component, int value)
		{
			try {
				switch (component) {
					case DateTimeComponent.Second:
						return dateTime.AddSeconds (value);
					case DateTimeComponent.Minute:
						return dateTime.AddMinutes (value);
					case DateTimeComponent.Hour:
						return dateTime.AddHours (value);
					case DateTimeComponent.Day:
						return dateTime.AddDays (value);
					case DateTimeComponent.Month:
						return dateTime.AddMonths (value);
					case DateTimeComponent.Year:
						return dateTime.AddYears (value);
					default:
						return dateTime.AddSeconds (value);
				}
			} catch (ArgumentOutOfRangeException) {
				return dateTime;
			}
		}

		public static int GetComponent(this DateTime dateTime, DateTimeComponent component)
		{
			switch (component) {
				case DateTimeComponent.Second:
					return dateTime.Second;
				case DateTimeComponent.Minute:
					return dateTime.Minute;
				case DateTimeComponent.Hour:
					return dateTime.Hour;
				case DateTimeComponent.Day:
					return dateTime.Day;
				case DateTimeComponent.Month:
					return dateTime.Month;
				case DateTimeComponent.Year:
					return dateTime.Year;
				default:
					return 0;
			}
		}

		public static DateTime SetComponent (this DateTime source, DateTimeComponent component, int newValue)
		{
			int year = source.Year;
			int month = source.Month;
			int day = source.Day;
			int hour = source.Hour;
			int minute = source.Minute;
			int second = source.Second;
			switch (component) {
				case DateTimeComponent.Year:
					return new DateTime (newValue, month, day, hour, minute, second);
				case DateTimeComponent.Month:
					return new DateTime (year, newValue, day, hour, minute, second);
				case DateTimeComponent.Day:
					return new DateTime (year, month, newValue, hour, minute, second);
				case DateTimeComponent.Hour:
					return new DateTime (year, month, day, newValue, minute, second);
				case DateTimeComponent.Minute:
					return new DateTime (year, month, day, hour, newValue, second);
				case DateTimeComponent.Second:
					return new DateTime (year, month, day, hour, minute, newValue);
				default:
					return source;
			}
		}
	}
}

