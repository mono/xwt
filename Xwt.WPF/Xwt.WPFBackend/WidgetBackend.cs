﻿//
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

		const WidgetEvent dragDropEvents = WidgetEvent.DragDropCheck | WidgetEvent.DragDrop | WidgetEvent.DragOver | WidgetEvent.DragOverCheck;

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

		public FrameworkElement Widget { get; set; }

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

		System.Windows.Size GetWidgetDesiredSize ()
		{
			if (!Widget.IsMeasureValid) {
				Widget.UpdateLayout ();
				Widget.Measure (new System.Windows.Size (Double.PositiveInfinity, Double.PositiveInfinity));
			}

			return Widget.DesiredSize;
		}

		public virtual WidgetSize GetPreferredWidth ()
		{
			var size = GetWidgetDesiredSize ();
			return new WidgetSize (size.Width * WidthPixelRatio);
		}

		public virtual WidgetSize GetPreferredHeight ()
		{
			var size = GetWidgetDesiredSize ();
			return new WidgetSize (size.Height * WidthPixelRatio);
		}

		public virtual WidgetSize GetPreferredWidthForHeight (double height)
		{
			var size = GetWidgetDesiredSize ();
			return new WidgetSize (size.Width * WidthPixelRatio);
		}

		public virtual WidgetSize GetPreferredHeightForWidth (double width)
		{
			var size = GetWidgetDesiredSize ();
			return new WidgetSize (size.Height * HeightPixelRatio);
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
}
