using System;
using System.Linq;
using System.Collections.Generic;
using Xwt.Backends;
using Xwt.Engine;

namespace Xwt.GtkBackend
{
	public class DatePickerBackend : WidgetBackend, IDatePickerBackend
	{
		public override void Initialize ()
		{
			Widget = new GtkDatePickerEntry ();
			Widget.ValueChanged += HandleValueChanged;
			Widget.ShowAll ();
		}
		
		new GtkDatePickerEntry Widget {
			get { return (GtkDatePickerEntry)base.Widget; }
			set { base.Widget = value; }
		}
		
		protected new ISpinButtonEventSink EventSink {
			get { return (ISpinButtonEventSink)base.EventSink; }
		}
		
		public DateTime DateTime {
			get {
				return Widget.CurrentValue;
			}
			set {
				Widget.CurrentValue = value;
			}
		}
		
		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is SpinButtonEvent) {
				if ((SpinButtonEvent)eventId == SpinButtonEvent.ValueChanged)
					Widget.ChangeValue += HandleValueChanged;
			}
		}
		
		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is SpinButtonEvent) {
				if ((SpinButtonEvent)eventId == SpinButtonEvent.ValueChanged)
					Widget.ChangeValue -= HandleValueChanged;
			}
		}

		void HandleValueChanged (object sender, EventArgs e)
		{
			Toolkit.Invoke (delegate {
				EventSink.ValueChanged ();
			});
		}
		
		public class GtkDatePickerEntry : Gtk.SpinButton
		{
			enum Component {
				None = 0,
				Month,
				Day,
				Year,
				Hour,
				Minute,
				Second
			}
			
			public new EventHandler ValueChanged;
	
			// We use the format of the invariant culture which is american biased apparently
			const string DateTimeFormat = "MM/dd/yyyy HH:mm:ss";
			Component selectedComponent;
			double oldValue = -1;
	
			int startPos = -1, endPos = -1;
			int currentDigitInsert;
	
			public GtkDatePickerEntry () : base (DateTime.MinValue.Ticks,
			                                     DateTime.MaxValue.Ticks,
			                                     TimeSpan.TicksPerSecond)
			{
				Adjustment.PageIncrement = TimeSpan.TicksPerDay;
				IsEditable = true;
				HasFrame = false;
				CurrentValue = DateTime.MinValue;
				Adjustment.ValueChanged += HandleValueChanged;
			}
	
			// Hack to supply the right tick value
			void HandleValueChanged (object sender, EventArgs e)
			{
				// Prevent reentrant call
				if (oldValue < 0)
					return;
				double old = oldValue;
				oldValue = -1;

				var value = Adjustment.Value;
				Adjustment.Value = old;
				if (value > old)
					GoUp ();
				else if (value < old)
					GoDown ();
				RaiseChangedEvent ();
	
				oldValue = Adjustment.Value;
			}
			
			protected override int OnOutput ()
			{
				DateTime dateTime = CurrentValue;
				Text = dateTime.ToString (DateTimeFormat);
				
				return 1;
			}
			
			protected override int OnInput (out double newValue)
			{
				newValue = Adjustment.Value;
				return 1;
			}
	
			void GoDown ()
			{
				switch (selectedComponent) {
				case Component.Second:
					Adjustment.Value -= TimeSpan.TicksPerSecond;
					break;
				case Component.Minute:
					Adjustment.Value -= TimeSpan.TicksPerMinute;
					break;
				case Component.Hour:
					Adjustment.Value -= TimeSpan.TicksPerHour;
					break;
				case Component.Day:
					Adjustment.Value = CurrentValue.AddDays (-1).Ticks;
					break;
				case Component.Month:
					Adjustment.Value = CurrentValue.AddMonths (-1).Ticks;
					break;
				case Component.Year:
					Adjustment.Value = CurrentValue.AddYears (-1).Ticks;
					break;
				}
			}
	
			void GoUp ()
			{
				switch (selectedComponent) {
				case Component.Second:
					Adjustment.Value += TimeSpan.TicksPerSecond;
					break;
				case Component.Minute:
					Adjustment.Value += TimeSpan.TicksPerMinute;
					break;
				case Component.Hour:
					Adjustment.Value += TimeSpan.TicksPerHour;
					break;
				case Component.Day:
					Adjustment.Value = CurrentValue.AddDays (1).Ticks;
					break;
				case Component.Month:
					Adjustment.Value = CurrentValue.AddMonths (1).Ticks;
					break;
				case Component.Year:
					Adjustment.Value = CurrentValue.AddYears (1).Ticks;
					break;
				}
			}
			
			protected override bool OnButtonPressEvent (Gdk.EventButton evnt)
			{
				int posX = (int)evnt.X, posY = (int)evnt.Y;
				//GetPointer (out posX, out posY);
				int layoutX, layoutY;
				GetLayoutOffsets (out layoutX, out layoutY);
				int index, trailing;
				
				bool result = Layout.XyToIndex (Pango.Units.FromPixels (posX - layoutX),
				                                Pango.Units.FromPixels (posY - layoutY),
				                                out index,
				                                out trailing);
	
				// Hacky. Since it's entry that maintain the text GdkWindow it's normally always
				// the last children of the widget GdkWindow because the other children are created
				// by the spin button
				if (evnt.Window == GdkWindow.Children.Last () && result) {
					index = TextIndexToLayoutIndex (index);
					UpdateSelectedComponent (index);
					currentDigitInsert = 0;
					GrabFocus ();
					return false;
				} else {
					return base.OnButtonPressEvent (evnt);
				}
			}
	
			// Override default GTK behavior which is to select the whole entry
			protected override void OnFocusGrabbed ()
			{
				base.OnFocusGrabbed ();
				SelectRegion (startPos, endPos);
			}
	
			void UpdateSelectedComponent (int index)
			{
				int componentSelected = -1;
				string txt = Text;
				List<int> stops = Enumerable.Range (0, txt.Length)
					.Where (i => char.IsPunctuation (txt, i) || char.IsSeparator (txt, i))
					.ToList ();
				stops.Insert (0, -1);
				stops.Add (txt.Length);
				
				for (int i = 0; i < stops.Count - 1 && stops[i] <= index; i++) {
					componentSelected = i;
					startPos = stops[i] + 1;
					endPos = stops[i + 1];
				}
				selectedComponent = (Component)componentSelected + 1;
			}
			
			protected override void OnChanged ()
			{
				base.OnChanged ();
				if (startPos == -1 || endPos == -1)
					SelectRegion (0, 0);
				else
					SelectRegion (startPos, endPos);
			}
	
			protected override bool OnKeyReleaseEvent (Gdk.EventKey evnt)
			{
				char pressedKey = (char)Gdk.Keyval.ToUnicode (evnt.KeyValue);
				if (char.IsDigit (pressedKey) && selectedComponent != Component.None) {
					try {
						int value = (int)pressedKey - (int)'0';
						DateTime current = CurrentValue;
						switch (selectedComponent) {
						case Component.Month:
							current = FromCopy (current, month: ValueFromDigitInsert (current.Month, value));
							break;
						case Component.Day:
							current = FromCopy (current, day: ValueFromDigitInsert (current.Day, value));
							break;
						case Component.Year:
							current = FromCopy (current, year: ValueFromDigitInsert (current.Year, value));
							break;
						case Component.Hour:
							current = FromCopy (current, hour: ValueFromDigitInsert (current.Hour, value));
							break;
						case Component.Minute:
							current = FromCopy (current, minute: ValueFromDigitInsert (current.Minute, value));
							break;
						case Component.Second:
							current = FromCopy (current, second: ValueFromDigitInsert (current.Second, value));
							break;
						}
						currentDigitInsert++;
						CurrentValue = current;
					} catch (ArgumentOutOfRangeException) {
						// In case date wasn't representable we redo the call with an updated digit insert
						currentDigitInsert = 0;
						return OnKeyReleaseEvent (evnt);
					}
				}
				return base.OnKeyReleaseEvent (evnt);
			}
	
			protected override bool OnKeyPressEvent (Gdk.EventKey evnt)
			{
				// We only allow the keypress to proceed to the normal handler
				// if it doesn't involve adding an actual character
				// (i.e. navigation keys)
				uint value = Gdk.Keyval.ToUnicode (evnt.KeyValue);
				if (value == 0)
					return base.OnKeyPressEvent (evnt);
				else
					return true;
			}
	
			int ValueFromDigitInsert (int baseValue, int newValue)
			{
				return currentDigitInsert == 0 ? newValue : newValue + baseValue * 10;
			}
	
			DateTime FromCopy (DateTime source,
			                   int year = -1,
			                   int month = -1,
			                   int day = -1,
			                   int hour = -1,
			                   int minute = -1,
			                   int second = -1)
			{
				return new DateTime (year == -1 ? source.Year : year,
				                     month == -1 ? source.Month : month,
				                     day == -1 ? source.Day : day,
				                     hour == -1 ? source.Hour : hour,
				                     minute == -1 ? source.Minute : minute,
				                     second == -1 ? source.Second : second);
			}
			
			void RaiseChangedEvent ()
			{
				var tmp = ValueChanged;
				if (tmp != null)
					tmp (this, EventArgs.Empty);
			}
			
			public DateTime CurrentValue {
				get {
					return new DateTime ((long)Adjustment.Value);
				}
				set {
					// Inhibit our custom handler for specific set
					oldValue = -1;
					Adjustment.Value = value.Ticks;
					oldValue = value.Ticks;
					RaiseChangedEvent ();
				}
			}
		}
	}
}

