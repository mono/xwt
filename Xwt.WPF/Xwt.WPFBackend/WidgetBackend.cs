//
// WidgetBackend.cs
//
// Authors:
//       Carlos Alberto Cortez <calberto.cortez@gmail.com>
//       Eric Maupin <ermau@xamarin.com>
//
// Copyright (c) 2011 Carlos Alberto Cortez
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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SWM = System.Windows.Media;
using SWC = System.Windows.Controls; // When we need to resolve ambigituies.

using Xwt.Backends;
using Xwt.Engine;
using Color = Xwt.Drawing.Color;

namespace Xwt.WPFBackend
{
	public abstract class WidgetBackend
		: Backend, IWidgetBackend, IWpfWidgetBackend
	{
		IWidgetEventSink eventSink;
		WidgetEvent enabledEvents;
		DragDropEffects currentDragEffect;
		FrameworkElement widget;

		const WidgetEvent dragDropEvents = WidgetEvent.DragDropCheck | WidgetEvent.DragDrop | WidgetEvent.DragOver | WidgetEvent.DragOverCheck;

		// Set to true when measuring a natural size for this widget
		bool gettingNaturalSize;

		// Set to true when calculating the default preferred size of the widget
		bool calculatingPreferredSize;

		void IWidgetBackend.Initialize (IWidgetEventSink eventSink)
		{
			this.eventSink = eventSink;
			Initialize ();
		}

		protected virtual void Initialize ()
		{
		}
		
		~WidgetBackend ()
		{
			Dispose (false);
		}
		
		public void Dispose ()
		{
			GC.SuppressFinalize (this);
			Dispose (true);
		}
		
		protected virtual void Dispose (bool disposing)
		{
		}

		public IWidgetEventSink EventSink {
			get { return eventSink; }
		}

		public object NativeWidget {
			get { return Widget; }
		}

		public FrameworkElement Widget {
			get { return widget; }
			set
			{
				widget = value;
				if (widget is IWpfWidget)
					((IWpfWidget)widget).Backend = this;
				widget.InvalidateMeasure ();
			}
		}

		Color? customBackgroundColor;

		public virtual Color BackgroundColor {
			get {
				if (customBackgroundColor.HasValue)
					return customBackgroundColor.Value;

				return DataConverter.ToXwtColor (GetWidgetColor ());
			}
			set {
				customBackgroundColor = value;
				SetWidgetColor (value);
			}
		}

		SWM.Color GetWidgetColor ()
		{
			if (Widget is Control) {
				var control = (Control)Widget;
				if (control.Background != null)
					return ((SWM.SolidColorBrush)control.Background).Color;
			} else if (Widget is SWC.Panel) {
				var panel = (SWC.Panel)Widget;
				if (panel.Background != null)
					return ((SWM.SolidColorBrush)panel.Background).Color;
			}

			return SystemColors.ControlColor;
		}

		void SetWidgetColor (Color value)
		{
			if ((Widget is Control))
				((Control)Widget).Background = ResPool.GetSolidBrush (value);
			if ((Widget is System.Windows.Controls.Panel))
				((SWC.Panel)Widget).Background = ResPool.GetSolidBrush (value);
		}

		public bool UsingCustomBackgroundColor {
			get { return customBackgroundColor.HasValue; }
		}

		public virtual object Font {
			get { return GetWidgetFont (); }
			set {
				SetWidgetFont ((FontData)value);
			}
		}

		FontData GetWidgetFont ()
		{
			if (!(Widget is Control))
				return FontData.SystemDefault;

			return FontData.FromControl ((Control)Widget);
		}

		void SetWidgetFont (FontData font)
		{
			if (!(Widget is Control))
				return;

			var control = (Control)Widget;
			control.FontFamily = font.Family;
			control.FontSize = font.Size;
			control.FontStyle = font.Style;
			control.FontWeight = font.Weight;
			control.FontStretch = font.Stretch;
		}

		public bool CanGetFocus {
			get { return Widget.Focusable; }
			set { Widget.Focusable = value; }
		}

		public bool HasFocus {
			get { return Widget.IsFocused; }
		}

		public void SetFocus ()
		{
			Widget.Focus ();
		}

		public virtual bool Sensitive {
			get { return Widget.IsEnabled; }
			set { Widget.IsEnabled = value; }
		}

		public Size Size {
			get { return new Size (Widget.ActualWidth, Widget.ActualHeight); }
		}

		public virtual bool Visible {
			get { return Widget.Visibility == Visibility.Visible; }
			set { Widget.Visibility = value ? Visibility.Visible : Visibility.Hidden; }
		}

		public string TooltipText {
			get { return Widget.ToolTip.ToString (); }
			set { Widget.ToolTip = value; }
		}

		public static FrameworkElement GetFrameworkElement (IWidgetBackend backend)
		{
			return backend == null ? null : (FrameworkElement)backend.NativeWidget;
		}

		public Point ConvertToScreenCoordinates (Point widgetCoordinates)
		{
			double wratio = WidthPixelRatio;
			double hratio = HeightPixelRatio;

			var p = Widget.PointToScreen (new System.Windows.Point (
				widgetCoordinates.X / wratio, widgetCoordinates.Y / hratio));

			return new Point (p.X * wratio, p.Y * hratio);
		}

		System.Windows.Size GetWidgetDesiredSize (double availableWidth, double availableHeight)
		{
			// Calculates the desired size of widget.

			if (!Widget.IsMeasureValid) {
				Widget.UpdateLayout ();
				try {
					calculatingPreferredSize = true;
					Widget.Measure (new System.Windows.Size (availableWidth, availableHeight));
				}
				finally {
					calculatingPreferredSize = false;
				}
			}

			return Widget.DesiredSize;
		}

		System.Windows.Size GetWidgetNaturalSize (double availableWidth, double availableHeight)
		{
			// Calculates the natural size of widget
			// Since WPF doesn't have the concept of natural size, we use the
			// normal size calculation, but we set gettingNaturalSize to true.
			// The flag is checked when the size is measured in MeasureOverride
			try {
				gettingNaturalSize = true;
				return GetWidgetDesiredSize (availableWidth, availableHeight);
			}
			finally {
				gettingNaturalSize = false;
			}
		}

		// The GetPreferred* methods are called when the corresponding OnGetPreferred* methods in the
		// XWT widget are not overriden, or if they are overriden and the new implementation calls
		// base.OnGetPreferred*. For this reason, we have to ensure that the widget's MeasureOverride
		// method doesn't end calling the frontend OnGetPreferred* methods. To avoid it we set
		// the calculatingPreferredSize flag to true, and we check this flag in MeasureOverride

		public virtual WidgetSize GetPreferredWidth ()
		{
			var size = GetWidgetDesiredSize (Double.PositiveInfinity, Double.PositiveInfinity);
			var naturalSize = GetWidgetNaturalSize (Double.PositiveInfinity, Double.PositiveInfinity);
			return new WidgetSize (size.Width * WidthPixelRatio, naturalSize.Width * WidthPixelRatio);
		}

		public virtual WidgetSize GetPreferredHeight ()
		{
			var size = GetWidgetDesiredSize (Double.PositiveInfinity, Double.PositiveInfinity);
			var naturalSize = GetWidgetNaturalSize (Double.PositiveInfinity, Double.PositiveInfinity);
			return new WidgetSize (size.Height * WidthPixelRatio, naturalSize.Height * HeightPixelRatio);
		}

		public virtual WidgetSize GetPreferredWidthForHeight (double height)
		{
			var size = GetWidgetDesiredSize (Double.PositiveInfinity, height);
			var naturalSize = GetWidgetNaturalSize (Double.PositiveInfinity, height);
			return new WidgetSize (size.Width * WidthPixelRatio, naturalSize.Width * WidthPixelRatio);
		}

		public virtual WidgetSize GetPreferredHeightForWidth (double width)
		{
			var size = GetWidgetDesiredSize (width, Double.PositiveInfinity);
			var naturalSize = GetWidgetNaturalSize (width, Double.PositiveInfinity);
			return new WidgetSize (size.Height * HeightPixelRatio, naturalSize.Height * HeightPixelRatio);
		}

		/// <summary>
		/// A default implementation of MeasureOverride to be used by all WPF widgets
		/// </summary>
		/// <param name="constraint">Size constraints</param>
		/// <param name="wpfMeasure">Size returned by the base MeasureOverride</param>
		/// <returns></returns>
		public System.Windows.Size MeasureOverride (System.Windows.Size constraint, System.Windows.Size wpfMeasure)
		{
			// Calculate the natural size, if that's what is being measured

			if (gettingNaturalSize) {
				var defNaturalSize = eventSink.GetDefaultNaturalSize ();

				// -2 means use the WPF default, -1 use the XWT default, any other other value is used as custom natural size
				var nw = DefaultNaturalWidth;
				if (nw == -2)
					nw = wpfMeasure.Width;
				else if (nw == -1) {
					nw = defNaturalSize.Width;
					if (nw == 0)
						nw = wpfMeasure.Width;
				}

				var nh = DefaultNaturalHeight;
				if (nh == -2)
					nh = wpfMeasure.Height;
				else if (nh == -1) {
					nh = defNaturalSize.Height;
					if (nh == 0)
						nh = wpfMeasure.Height;
				}
			}

			// If we are calculating the default preferred size of the widget we end here.
			// See note above about when GetPreferred* methods are called.
			if (calculatingPreferredSize)
				return wpfMeasure;

			Toolkit.Invoke (delegate
			{
				if (eventSink.GetSizeRequestMode () == SizeRequestMode.HeightForWidth) {
					// Calculate the preferred width through the frontend, if there is an overriden OnGetPreferredWidth
					if ((enabledEvents & WidgetEvent.PreferredWidthCheck) != 0) {
						var ws = eventSink.OnGetPreferredWidth ();
						wpfMeasure.Width = gettingNaturalSize ? ws.NaturalSize : ws.MinSize;
					}

					// Now calculate the preferred height for that width, also using the override if available
					if ((enabledEvents & WidgetEvent.PreferredHeightForWidthCheck) != 0) {
						var ws = eventSink.OnGetPreferredHeightForWidth (wpfMeasure.Width);
						wpfMeasure.Height = gettingNaturalSize ? ws.NaturalSize : ws.MinSize;
					}
				}
				else {
					// Calculate the preferred height through the frontend, if there is an overriden OnGetPreferredHeight
					if ((enabledEvents & WidgetEvent.PreferredHeightCheck) != 0) {
						var ws = eventSink.OnGetPreferredHeight ();
						wpfMeasure.Height = gettingNaturalSize ? ws.NaturalSize : ws.MinSize;
					}

					// Now calculate the preferred width for that height, also using the override if available
					if ((enabledEvents & WidgetEvent.PreferredWidthForHeightCheck) != 0) {
						var ws = eventSink.OnGetPreferredWidthForHeight (wpfMeasure.Height);
						wpfMeasure.Width = gettingNaturalSize ? ws.NaturalSize : ws.MinSize;
					}
				}
			});
			return wpfMeasure;
		}

		/// <summary>
		/// Natural width for the widget. It can be any arbitrary custom value, 
		/// or -1 if the XWT defined default has to be used, 
		/// or -2 if the WPF desired size has to be used (this is the default)
		/// </summary>
		protected virtual double DefaultNaturalWidth {
			get { return -2; }
		}

		/// <summary>
		/// Natural width for the widget. It can be any arbitrary custom value, 
		/// or -1 if the XWT defined default has to be used, 
		/// or -2 if the WPF desired size has to be used (this is the default)
		/// </summary>
		protected virtual double DefaultNaturalHeight
		{
			get { return -2; }
		}

		public void SetMinSize (double width, double height)
		{
			if (width == -1)
				Widget.ClearValue (FrameworkElement.MinWidthProperty);
			else
				Widget.MinWidth = width / WidthPixelRatio;

			if (height == -1)
				Widget.ClearValue (FrameworkElement.MinHeightProperty);
			else
				Widget.MinHeight = height / HeightPixelRatio;
		}

		public void SetNaturalSize (double width, double height)
		{
			if (width == -1)
				Widget.ClearValue (FrameworkElement.WidthProperty);
			else
				Widget.Width = width / WidthPixelRatio;

			if (height == -1)
				Widget.ClearValue (FrameworkElement.HeightProperty);
			else
				Widget.Height = height / HeightPixelRatio;
		}

		public void SetCursor (CursorType cursor)
		{
			// TODO
		}
		
		public virtual void UpdateLayout ()
		{
		}

		public override void EnableEvent (object eventId)
		{
			if (eventId is WidgetEvent) {
				var ev = (WidgetEvent)eventId;
				switch (ev) {
					case WidgetEvent.DragLeave:
						Widget.DragLeave += WidgetDragLeaveHandler;
						break;
					case WidgetEvent.KeyPressed:
						Widget.KeyDown += WidgetKeyDownHandler;
						break;
					case WidgetEvent.KeyReleased:
						Widget.KeyDown += WidgetKeyUpHandler;
						break;
					case WidgetEvent.ButtonPressed:
						Widget.MouseDown += WidgetMouseDownHandler;
						break;
					case WidgetEvent.ButtonReleased:
						Widget.MouseUp += WidgetMouseUpHandler;
						break;
					case WidgetEvent.GotFocus:
						Widget.GotFocus += WidgetGotFocusHandler;
						break;
					case WidgetEvent.LostFocus:
						Widget.LostFocus += WidgetLostFocusHandler;
						break;
					case WidgetEvent.MouseEntered:
						Widget.MouseEnter += WidgetMouseEnteredHandler;
						break;
					case WidgetEvent.MouseExited:
						Widget.MouseLeave += WidgetMouseExitedHandler;
						break;
					case WidgetEvent.MouseMoved:
						Widget.MouseMove += WidgetMouseMoveHandler;
						break;
					case WidgetEvent.BoundsChanged:
						Widget.SizeChanged += WidgetOnSizeChanged;
						break;
				}

				if ((ev & dragDropEvents) != 0 && (enabledEvents & dragDropEvents) == 0) {
					// Enabling a drag&drop event for the first time
					Widget.DragOver += WidgetDragOverHandler;
					Widget.Drop += WidgetDropHandler;
				}

				enabledEvents |= ev;
			}
		}

		public override void DisableEvent (object eventId)
		{
			if (eventId is WidgetEvent) {
				var ev = (WidgetEvent)eventId;
				switch (ev) {
					case WidgetEvent.DragLeave:
						Widget.DragLeave -= WidgetDragLeaveHandler;
						break;
					case WidgetEvent.KeyPressed:
						Widget.KeyDown -= WidgetKeyDownHandler;
						break;
					case WidgetEvent.KeyReleased:
						Widget.KeyUp -= WidgetKeyUpHandler;
						break;
					case WidgetEvent.ButtonPressed:
						Widget.MouseDown -= WidgetMouseDownHandler;
						break;
					case WidgetEvent.ButtonReleased:
						Widget.MouseUp -= WidgetMouseUpHandler;
						break;
					case WidgetEvent.MouseEntered:
						Widget.MouseEnter -= WidgetMouseEnteredHandler;
						break;
					case WidgetEvent.MouseExited:
						Widget.MouseLeave -= WidgetMouseExitedHandler;
						break;
					case WidgetEvent.MouseMoved:
						Widget.MouseMove -= WidgetMouseMoveHandler;
						break;
					case WidgetEvent.BoundsChanged:
						Widget.SizeChanged -= WidgetOnSizeChanged;
						break;
				}

				enabledEvents &= ~ev;

				if ((ev & dragDropEvents) != 0 && (enabledEvents & dragDropEvents) == 0) {
					// All drag&drop events have been disabled
					Widget.DragOver -= WidgetDragOverHandler;
					Widget.Drop -= WidgetDropHandler;
				}
			}
		}

		protected double WidthPixelRatio
		{
			get
			{
				PresentationSource source = PresentationSource.FromVisual (Widget);
				if (source == null)
					return 1;

				Matrix m = source.CompositionTarget.TransformToDevice;
				return m.M11;
			}
		}

		protected double HeightPixelRatio
		{
			get
			{
				PresentationSource source = PresentationSource.FromVisual (Widget);
				if (source == null)
					return 1;

				Matrix m = source.CompositionTarget.TransformToDevice;
				return m.M22;
			}
		}

		void WidgetKeyDownHandler (object sender, System.Windows.Input.KeyEventArgs e)
		{
			KeyEventArgs args;
			if (MapToXwtKeyArgs (e, out args))
				Toolkit.Invoke (delegate {
					eventSink.OnKeyPressed (args);
				});
		}

		void WidgetKeyUpHandler (object sender, System.Windows.Input.KeyEventArgs e)
		{
			KeyEventArgs args;
			if (MapToXwtKeyArgs (e, out args))
				Toolkit.Invoke (delegate {
					eventSink.OnKeyReleased (args);
				});
		}

		bool MapToXwtKeyArgs (System.Windows.Input.KeyEventArgs e, out KeyEventArgs result)
		{
			result = null;

			var key = KeyboardUtil.TranslateToXwtKey (e.Key);
			if ((int)key == 0)
				return false;

			result = new KeyEventArgs (key, KeyboardUtil.GetModifiers (), e.IsRepeat, e.Timestamp);
			return true;
		}

		void WidgetMouseDownHandler (object o, MouseButtonEventArgs e)
		{
			Toolkit.Invoke (delegate () {
				eventSink.OnButtonPressed (ToXwtButtonArgs (e));
			});
		}

		void WidgetMouseUpHandler (object o, MouseButtonEventArgs e)
		{
			var args = ToXwtButtonArgs (e);
			Toolkit.Invoke (delegate () {
				eventSink.OnButtonReleased (ToXwtButtonArgs (e));
			});
		}

		ButtonEventArgs ToXwtButtonArgs (MouseButtonEventArgs e)
		{
			var pos = e.GetPosition (Widget);
			return new ButtonEventArgs () {
				X = pos.X * WidthPixelRatio,
				Y = pos.Y * HeightPixelRatio,
				MultiplePress = e.ClickCount,
				Button = e.ChangedButton.ToXwtButton ()
			};
		}

		void WidgetGotFocusHandler (object o, RoutedEventArgs e)
		{
			Toolkit.Invoke (delegate {
				eventSink.OnGotFocus ();
			});
		}

		void WidgetLostFocusHandler (object o, RoutedEventArgs e)
		{
			Toolkit.Invoke (delegate {
				eventSink.OnLostFocus ();
			});
		}

		public void DragStart (DragStartData data)
		{
			if (data.Data == null)
				throw new ArgumentNullException ("data");

			var dataObj = CreateDataObject (data.Data);
			DragDrop.DoDragDrop (Widget, dataObj, data.DragAction.ToWpfDropEffect ());
		}

		static DataObject CreateDataObject (TransferDataSource data)
		{
			var retval = new DataObject ();
			foreach (var type in data.DataTypes) {
				if (type == TransferDataType.Text)
					retval.SetText ((string)data.GetValue (type));
			}

			return retval;
		}

		public void SetDragTarget (TransferDataType [] types, DragDropAction dragAction)
		{
			Widget.AllowDrop = true;
		}

		public void SetDragSource (TransferDataType [] types, DragDropAction dragAction)
		{
		}

		static DragDropAction DetectDragAction (DragDropKeyStates keys)
		{
			if ((keys & DragDropKeyStates.ControlKey) == DragDropKeyStates.ControlKey) {
				if ((keys & DragDropKeyStates.ShiftKey) == DragDropKeyStates.ShiftKey)
					return DragDropAction.Link;
				else
					return DragDropAction.Copy;
			}

			return DragDropAction.Move;
		}

		static void FillDataStore (TransferDataStore store, IDataObject data)
		{
			foreach (string format in data.GetFormats ()) {
				if (format == DataFormats.UnicodeText)
					store.AddText ((string)data.GetData (format));
			}
		}

		void WidgetDragOverHandler (object sender, System.Windows.DragEventArgs e)
		{
			var types = e.Data.GetFormats ().Select (t => t.ToXwtDragType ()).ToArray ();
			var pos = e.GetPosition (Widget).ToXwtPoint ();
			var proposedAction = DetectDragAction (e.KeyStates);

			e.Handled = true; // Prevent default handlers from being used.

			if ((enabledEvents & WidgetEvent.DragOverCheck) > 0) {
				var checkArgs = new DragOverCheckEventArgs (pos, types, proposedAction);
				Toolkit.Invoke (delegate {
					eventSink.OnDragOverCheck (checkArgs);
				});
				if (checkArgs.AllowedAction == DragDropAction.None) {
					e.Effects = currentDragEffect = DragDropEffects.None;
					return;
				}
				if (checkArgs.AllowedAction != DragDropAction.Default) {
					e.Effects = currentDragEffect = checkArgs.AllowedAction.ToWpfDropEffect ();
					return;
				}
			}

			if ((enabledEvents & WidgetEvent.DragOver) > 0) {
				var store = new TransferDataStore ();
				FillDataStore (store, e.Data);

				var args = new DragOverEventArgs (pos, store, proposedAction);
				Toolkit.Invoke (delegate {
					eventSink.OnDragOver (args);
				});
				if (args.AllowedAction == DragDropAction.None) {
					e.Effects = currentDragEffect = DragDropEffects.None;
					return;
				}
				if (args.AllowedAction != DragDropAction.Default) {
					e.Effects = currentDragEffect = args.AllowedAction.ToWpfDropEffect ();
					return;
				}
			}

			e.Effects = currentDragEffect = proposedAction.ToWpfDropEffect ();
		}

		void WidgetDropHandler (object sender, System.Windows.DragEventArgs e)
		{
			var types = e.Data.GetFormats ().Select (t => t.ToXwtDragType ()).ToArray ();
			var pos = e.GetPosition (Widget).ToXwtPoint ();
			var actualEffect = currentDragEffect;

			e.Handled = true; // Prevent default handlers from being used.

			if ((enabledEvents & WidgetEvent.DragDropCheck) > 0) {
				var checkArgs = new DragCheckEventArgs (pos, types, actualEffect.ToXwtDropAction ());
				bool res = Toolkit.Invoke (delegate {
					eventSink.OnDragDropCheck (checkArgs);
				});
				if (checkArgs.Result == DragDropResult.Canceled || !res) {
					e.Effects = DragDropEffects.None;
					return;
				}
			}

			if ((enabledEvents & WidgetEvent.DragDrop) > 0) {
				var store = new TransferDataStore ();
				FillDataStore (store, e.Data);

				var args = new DragEventArgs (pos, store, actualEffect.ToXwtDropAction ());
				Toolkit.Invoke (delegate {
					eventSink.OnDragDrop (args);
				});

				e.Effects = args.Success ? actualEffect : DragDropEffects.None;
			}

			// No DrapDropCheck/DragDrop event enabled.
			e.Effects = DragDropEffects.None;
		}

		void WidgetDragLeaveHandler (object sender, System.Windows.DragEventArgs e)
		{
			Toolkit.Invoke (delegate {
				eventSink.OnDragLeave (EventArgs.Empty);
			});
		}

		private void WidgetMouseEnteredHandler (object sender, MouseEventArgs e)
		{
			Toolkit.Invoke (eventSink.OnMouseEntered);
		}

		private void WidgetMouseExitedHandler (object sender, MouseEventArgs e)
		{
			Toolkit.Invoke (eventSink.OnMouseExited);
		}

		private void WidgetMouseMoveHandler (object sender, MouseEventArgs e)
		{
			Toolkit.Invoke (() => {
				var p = e.GetPosition (Widget);
				eventSink.OnMouseMoved (new MouseMovedEventArgs (
					e.Timestamp, p.X * WidthPixelRatio, p.Y * HeightPixelRatio));
			});
		}

		private void WidgetOnSizeChanged (object sender, SizeChangedEventArgs e)
		{
			if (Widget.IsVisible)
				Toolkit.Invoke (this.eventSink.OnBoundsChanged);
		}
	}

	public interface IWpfWidgetBackend
	{
		FrameworkElement Widget { get; }
	}

	public interface IWpfWidget
	{
		WidgetBackend Backend { get; set; }
	}
}
