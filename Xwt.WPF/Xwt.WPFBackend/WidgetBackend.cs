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
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using SWM = System.Windows.Media;
using SWC = System.Windows.Controls; // When we need to resolve ambigituies.
using SW = System.Windows; // When we need to resolve ambigituies.

using Xwt.Backends;

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

		class DragDropData
		{
			// Source
			public bool AutodetectDrag;
			public Rect DragRect;
			// Target
			public TransferDataType [] TargetTypes = new TransferDataType [0];
		}

		DragDropData dragDropInfo;

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

		public new Widget Frontend {
			get { return (Widget) base.Frontend; }
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
			if (!(Widget is Control)) {
				double size = WpfFontBackendHandler.GetPointsFromDeviceUnits (SystemFonts.MessageFontSize);

				return new FontData (SystemFonts.MessageFontFamily, size) {
					Style = SystemFonts.MessageFontStyle,
					Weight = SystemFonts.MessageFontWeight
				};
			}

			return FontData.FromControl ((Control)Widget);
		}

		void SetWidgetFont (FontData font)
		{
			if (!(Widget is Control))
				return;

			var control = (Control)Widget;
			control.FontFamily = font.Family;
			control.FontSize = font.GetDeviceIndependentPixelSize (control);
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
			set { Widget.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
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
			var p = Widget.PointToScreen (new System.Windows.Point (
				widgetCoordinates.X, widgetCoordinates.Y));

			return new Point (p.X, p.Y);
		}

		SW.Size lastNaturalSize;

		void GetWidgetDesiredSize (double availableWidth, double availableHeight, out SW.Size minSize, out SW.Size naturalSize)
		{
			// Calculates the desired size of widget.

			if (!Widget.IsMeasureValid) {
				try {
					calculatingPreferredSize = true;
					gettingNaturalSize = true;
					Widget.Measure (new System.Windows.Size (availableWidth, availableHeight));
					lastNaturalSize = Widget.DesiredSize;
					gettingNaturalSize = false;

					Widget.InvalidateMeasure ();
					Widget.Measure (new System.Windows.Size (availableWidth, availableHeight));
				}
				finally {
					calculatingPreferredSize = false;
					gettingNaturalSize = false;
				}
			}
			minSize = Widget.DesiredSize;
			naturalSize = lastNaturalSize;
		}

		// The GetPreferred* methods are called when the corresponding OnGetPreferred* methods in the
		// XWT widget are not overriden, or if they are overriden and the new implementation calls
		// base.OnGetPreferred*. For this reason, we have to ensure that the widget's MeasureOverride
		// method doesn't end calling the frontend OnGetPreferred* methods. To avoid it we set
		// the calculatingPreferredSize flag to true, and we check this flag in MeasureOverride

		public virtual WidgetSize GetPreferredWidth ()
		{
			SW.Size minSize, natSize;
			Widget.InvalidateMeasure ();
			GetWidgetDesiredSize (Double.PositiveInfinity, Double.PositiveInfinity, out minSize, out natSize);
			return new WidgetSize (minSize.Width - Frontend.Margin.HorizontalSpacing, natSize.Width - Frontend.Margin.HorizontalSpacing);
		}

		public virtual WidgetSize GetPreferredHeight ()
		{
			SW.Size minSize, natSize;
			Widget.InvalidateMeasure ();
			GetWidgetDesiredSize (Double.PositiveInfinity, Double.PositiveInfinity, out minSize, out natSize);
			return new WidgetSize (minSize.Height - Frontend.Margin.VerticalSpacing, natSize.Height - Frontend.Margin.VerticalSpacing);
		}

		public virtual WidgetSize GetPreferredWidthForHeight (double height)
		{
			SW.Size minSize, natSize;
			Widget.InvalidateMeasure ();
			GetWidgetDesiredSize (Double.PositiveInfinity, height, out minSize, out natSize);
			return new WidgetSize (minSize.Width - Frontend.Margin.HorizontalSpacing, natSize.Width - Frontend.Margin.HorizontalSpacing);
		}

		public virtual WidgetSize GetPreferredHeightForWidth (double width)
		{
			SW.Size minSize, natSize;
			Widget.InvalidateMeasure ();
			GetWidgetDesiredSize (width, Double.PositiveInfinity, out minSize, out natSize);
			return new WidgetSize (minSize.Height - Frontend.Margin.VerticalSpacing, natSize.Height - Frontend.Margin.VerticalSpacing);
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
				if (nw == -1) {
					nw = defNaturalSize.Width;
					if (nw == 0)
						nw = wpfMeasure.Width;
					wpfMeasure.Width = nw;
				}
				else if (nw != -2)
					wpfMeasure.Width = nw;

				var nh = DefaultNaturalHeight;
				if (nh == -1) {
					nh = defNaturalSize.Height;
					if (nh == 0)
						nh = wpfMeasure.Height;
					wpfMeasure.Height = nh;
				}
				else if (nh != -2)
					wpfMeasure.Height = nh;
			}

			// If we are calculating the default preferred size of the widget we end here.
			// See note above about when GetPreferred* methods are called.
			if (calculatingPreferredSize)
				return wpfMeasure;

			Context.InvokeUserCode (delegate
			{
				if (eventSink.GetSizeRequestMode () == SizeRequestMode.HeightForWidth) {
					// Calculate the preferred width through the frontend if there is an overriden OnGetPreferredWidth, but only do it
					// if we are not given a constraint. If there is a width constraint, we'll use that constraint value for calculating the height 
					if ((enabledEvents & WidgetEvent.PreferredWidthCheck) != 0 && constraint.Width == Double.PositiveInfinity) {
						var ws = eventSink.OnGetPreferredWidth ();
						wpfMeasure.Width = constraint.Width = gettingNaturalSize ? ws.NaturalSize : ws.MinSize;
					}

					// Now calculate the preferred height for that width, also using the override if available
					if ((enabledEvents & WidgetEvent.PreferredHeightForWidthCheck) != 0) {
						var ws = eventSink.OnGetPreferredHeightForWidth (constraint.Width);
						wpfMeasure.Height = gettingNaturalSize ? ws.NaturalSize : ws.MinSize;
					}
				}
				else {
					// Calculate the preferred height through the frontend, if there is an overriden OnGetPreferredHeight
					if ((enabledEvents & WidgetEvent.PreferredHeightCheck) != 0 && constraint.Height == Double.PositiveInfinity) {
						var ws = eventSink.OnGetPreferredHeight ();
						wpfMeasure.Height = constraint.Height = gettingNaturalSize ? ws.NaturalSize : ws.MinSize;
					}

					// Now calculate the preferred width for that height, also using the override if available
					if ((enabledEvents & WidgetEvent.PreferredWidthForHeightCheck) != 0) {
						var ws = eventSink.OnGetPreferredWidthForHeight (constraint.Height);
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
				Widget.MinWidth = width;

			if (height == -1)
				Widget.ClearValue (FrameworkElement.MinHeightProperty);
			else
				Widget.MinHeight = height;
		}

		public void SetNaturalSize (double width, double height)
		{
			if (width == -1)
				Widget.ClearValue (FrameworkElement.WidthProperty);
			else
				Widget.Width = width;

			if (height == -1)
				Widget.ClearValue (FrameworkElement.HeightProperty);
			else
				Widget.Height = height;
		}

		public void SetCursor (CursorType cursor)
		{
			if (cursor == CursorType.Arrow)
				Widget.Cursor = Cursors.Arrow;
			else if (cursor == CursorType.Crosshair)
				Widget.Cursor = Cursors.Cross;
			else if (cursor == CursorType.Hand)
				Widget.Cursor = Cursors.Hand;
			else if (cursor == CursorType.IBeam)
				Widget.Cursor = Cursors.IBeam;
			else if (cursor == CursorType.ResizeDown)
				Widget.Cursor = Cursors.SizeNS;
			else if (cursor == CursorType.ResizeUp)
				Widget.Cursor = Cursors.SizeNS;
			else if (cursor == CursorType.ResizeUpDown)
				Widget.Cursor = Cursors.SizeNS;
			else if (cursor == CursorType.ResizeLeft)
				Widget.Cursor = Cursors.SizeWE;
			else if (cursor == CursorType.ResizeRight)
				Widget.Cursor = Cursors.SizeWE;
			else if (cursor == CursorType.ResizeLeftRight)
				widget.Cursor = Cursors.SizeWE;
		}
		
		public virtual void UpdateLayout ()
		{
			Xwt.Widget frontend = (Xwt.Widget)Frontend;
			widget.Margin = new Thickness (frontend.Margin.Left, frontend.Margin.Top, frontend.Margin.Right, frontend.Margin.Bottom);
		}

		public override void EnableEvent (object eventId)
		{
			if (eventId is WidgetEvent) {
				var ev = (WidgetEvent)eventId;
				switch (ev) {
					case WidgetEvent.KeyPressed:
						Widget.KeyDown += WidgetKeyDownHandler;
						break;
					case WidgetEvent.KeyReleased:
						Widget.KeyUp += WidgetKeyUpHandler;
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
					case WidgetEvent.MouseScrolled:
						Widget.MouseWheel += WidgetMouseWheelHandler;
						break;
				}

				if ((ev & dragDropEvents) != 0 && (enabledEvents & dragDropEvents) == 0) {
					// Enabling a drag&drop event for the first time
					Widget.DragOver += WidgetDragOverHandler;
					Widget.Drop += WidgetDropHandler;
					widget.DragLeave += WidgetDragLeaveHandler;
				}

				enabledEvents |= ev;
			}
		}

		public override void DisableEvent (object eventId)
		{
			if (eventId is WidgetEvent) {
				var ev = (WidgetEvent)eventId;
				switch (ev) {
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
					case WidgetEvent.MouseScrolled:
						Widget.MouseWheel -= WidgetMouseWheelHandler;
						break;
				}

				enabledEvents &= ~ev;

				if ((ev & dragDropEvents) != 0 && (enabledEvents & dragDropEvents) == 0) {
					// All drag&drop events have been disabled
					Widget.DragOver -= WidgetDragOverHandler;
					Widget.Drop -= WidgetDropHandler;
					Widget.DragLeave -= WidgetDragLeaveHandler;
				}
			}
		}

		public double aWidthPixelRatio
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

		public double aHeightPixelRatio
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
			if (MapToXwtKeyArgs (e, out args)) {
				Context.InvokeUserCode (delegate {
					eventSink.OnKeyPressed (args);
				});
				if (args.Handled)
					e.Handled = true;
			}
		}

		void WidgetKeyUpHandler (object sender, System.Windows.Input.KeyEventArgs e)
		{
			KeyEventArgs args;
			if (MapToXwtKeyArgs (e, out args)) {
				Context.InvokeUserCode (delegate
				{
					eventSink.OnKeyReleased (args);
				});
				if (args.Handled)
					e.Handled = true;
			}
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
			var args = ToXwtButtonArgs (e);
			Context.InvokeUserCode (delegate () {
				eventSink.OnButtonPressed (args);
			});
			if (args.Handled)
				e.Handled = true;
		}

		void WidgetMouseUpHandler (object o, MouseButtonEventArgs e)
		{
			var args = ToXwtButtonArgs (e);
			Context.InvokeUserCode (delegate ()
			{
				eventSink.OnButtonReleased (args);
			});
			if (args.Handled)
				e.Handled = true;
		}

		ButtonEventArgs ToXwtButtonArgs (MouseButtonEventArgs e)
		{
			var pos = e.GetPosition (Widget);
			return new ButtonEventArgs () {
				X = pos.X,
				Y = pos.Y,
				MultiplePress = e.ClickCount,
				Button = e.ChangedButton.ToXwtButton ()
			};
		}

		void WidgetGotFocusHandler (object o, RoutedEventArgs e)
		{
			Context.InvokeUserCode (this.eventSink.OnGotFocus);
		}

		void WidgetLostFocusHandler (object o, RoutedEventArgs e)
		{
			Context.InvokeUserCode (eventSink.OnLostFocus);
		}

		DragDropData DragDropInfo {
			get {
				if (dragDropInfo == null)
					dragDropInfo = new DragDropData ();

				return dragDropInfo;
			}
		}

		private static ImageAdorner Adorner;
		private static AdornerLayer AdornedLayer;
		private static System.Windows.Window AdornedWindow;

		private SW.Window GetParentWindow()
		{
			FrameworkElement current = Widget;
			while (current != null) {
				if (current is SW.Window)
					return (SW.Window)current;

				current = VisualTreeHelper.GetParent (current) as FrameworkElement;
			}

			return null;
		}

		public void DragStart (DragStartData data)
		{
			if (data.Data == null)
				throw new ArgumentNullException ("data");
			
			DataObject dataObj = data.Data.ToDataObject();

			if (data.ImageBackend != null) {
				AdornedWindow = GetParentWindow ();
				AdornedWindow.AllowDrop = true;

				var e = (UIElement)AdornedWindow.Content;

				Adorner = new ImageAdorner (e, data.ImageBackend);

				AdornedLayer = AdornerLayer.GetAdornerLayer (e);
				AdornedLayer.Add (Adorner);

				AdornedWindow.DragOver += AdornedWindowOnDragOver;
			}

			Widget.Dispatcher.BeginInvoke ((Action)(() => {
				var effect = DragDrop.DoDragDrop (Widget, dataObj, data.DragAction.ToWpfDropEffect ());

				OnDragFinished (this, new DragFinishedEventArgs (effect == DragDropEffects.Move));

				if (Adorner != null) {
					AdornedLayer.Remove (Adorner);
					AdornedLayer = null;
					Adorner = null;

					AdornedWindow.AllowDrop = false;
					AdornedWindow.DragOver -= AdornedWindowOnDragOver;
					AdornedWindow = null;
				}
			}));
		}

		private void AdornedWindowOnDragOver (object sender, System.Windows.DragEventArgs e)
		{
			WidgetDragOverHandler (sender, e);
		}

		public void SetDragTarget (TransferDataType [] types, DragDropAction dragAction)
		{
			DragDropInfo.TargetTypes = types == null ? new TransferDataType [0] : types;
			Widget.AllowDrop = true;
		}

		public void SetDragSource (TransferDataType [] types, DragDropAction dragAction)
		{
			if (DragDropInfo.AutodetectDrag)
				return; // Drag auto detect has been already activated.

			DragDropInfo.AutodetectDrag = true;
			Widget.MouseUp += WidgetMouseUpForDragHandler;
			Widget.MouseMove += WidgetMouseMoveForDragHandler;
		}

		private void SetupDragRect (MouseEventArgs e)
		{
			var width = SystemParameters.MinimumHorizontalDragDistance;
			var height = SystemParameters.MinimumVerticalDragDistance;
			var loc = e.GetPosition (Widget);
			DragDropInfo.DragRect = new Rect (loc.X - width / 2, loc.Y - height / 2, width, height);
		}

		void WidgetMouseUpForDragHandler (object o, EventArgs e)
		{
			DragDropInfo.DragRect = Rect.Empty;
		}

		void WidgetMouseMoveForDragHandler (object o, MouseEventArgs e)
		{
			if ((enabledEvents & WidgetEvent.DragStarted) == 0)
				return;
			if (e.LeftButton != MouseButtonState.Pressed)
				return;

			if (DragDropInfo.DragRect.IsEmpty)
				SetupDragRect (e);

			if (DragDropInfo.DragRect.Contains (e.GetPosition (Widget)))
				return;

			DragStartData dragData = null;
			Context.InvokeUserCode (delegate {
				dragData = eventSink.OnDragStarted ();
			});

			if (dragData != null)
				DragStart (dragData);

			DragDropInfo.DragRect = Rect.Empty;
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

		static void FillDataStore (TransferDataStore store, IDataObject data, TransferDataType [] types)
		{
			foreach (var type in types) {
				string format = type.ToWpfDataFormat ();
				if (!data.GetDataPresent (format)) {
					// This is a workaround to support type names which don't include the assembly name.
					// It eases integration with Windows DND.
					format = NormalizeTypeName (format);
					if (!data.GetDataPresent (format))
						continue;
				}

				var value = data.GetData (format);
				if (type == TransferDataType.Text)
					store.AddText ((string)value);
				else if (type == TransferDataType.Uri) {
					var uris = ((string [])value).Select (f => new Uri (f)).ToArray ();
					store.AddUris (uris);
				} else if (value is byte[])
					store.AddValue (type, (byte[]) value);
				else
					store.AddValue (type, value);
			}
		}

		static string NormalizeTypeName (string dataType)
		{
			// If the string is a fully qualified type name, strip the assembly name
			int i = dataType.IndexOf (',');
			if (i == -1)
				return dataType;
			string asmName = dataType.Substring (i + 1).Trim ();
			try {
				new System.Reflection.AssemblyName (asmName);
			}
			catch {
				return dataType;
			}
			return dataType.Substring (0, i).Trim ();
		}

		protected virtual void OnDragFinished (object sender, DragFinishedEventArgs e)
		{
			Context.InvokeUserCode (delegate {
				this.eventSink.OnDragFinished (e);
			});
		}

		protected virtual void OnDragOver (object sender, DragOverEventArgs e)
		{
			Context.InvokeUserCode (delegate {
				eventSink.OnDragOver (e);
			});
		}

		protected virtual void OnDragLeave (object sender, EventArgs e)
		{
			Context.InvokeUserCode (delegate {
				eventSink.OnDragLeave (e);
			});
		}

		void WidgetDragOverHandler (object sender, System.Windows.DragEventArgs e)
		{
			var types = e.Data.GetFormats ().Select (t => t.ToXwtTransferType ()).ToArray ();
			var pos = e.GetPosition (Widget).ToXwtPoint ();
			var proposedAction = DetectDragAction (e.KeyStates);

			e.Handled = true; // Prevent default handlers from being used.

			if (Adorner != null) {
				var w = GetParentWindow ();
				var v = (UIElement)w.Content;

				if (w != AdornedWindow) {
					AdornedLayer.Remove (Adorner);

					AdornedWindow.AllowDrop = false;
					AdornedWindow.DragOver -= AdornedWindowOnDragOver;

					AdornedWindow = w;
					AdornedWindow.AllowDrop = true;
					AdornedWindow.DragOver += AdornedWindowOnDragOver;

					AdornedLayer = AdornerLayer.GetAdornerLayer (v);
					AdornedLayer.Add (Adorner);
				}

				Adorner.Offset = e.GetPosition (v);
			}

			if ((enabledEvents & WidgetEvent.DragOverCheck) > 0) {
				var checkArgs = new DragOverCheckEventArgs (pos, types, proposedAction);
				Context.InvokeUserCode (delegate {
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
				FillDataStore (store, e.Data, DragDropInfo.TargetTypes);

				var args = new DragOverEventArgs (pos, store, proposedAction);
				OnDragOver (sender, args);
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
			WidgetDragLeaveHandler (sender, e);

			var types = e.Data.GetFormats ().Select (t => t.ToXwtTransferType ()).ToArray ();
			var pos = e.GetPosition (Widget).ToXwtPoint ();
			var actualEffect = currentDragEffect;

			e.Handled = true; // Prevent default handlers from being used.
			e.Effects = DragDropEffects.None;

			if ((enabledEvents & WidgetEvent.DragDropCheck) > 0) {
				var checkArgs = new DragCheckEventArgs (pos, types, actualEffect.ToXwtDropAction ());
				bool res = Context.InvokeUserCode (delegate {
					eventSink.OnDragDropCheck (checkArgs);
				});

				if (checkArgs.Result == DragDropResult.Canceled || !res) {
					e.Effects = DragDropEffects.None;
					return;
				}
			}

			if ((enabledEvents & WidgetEvent.DragDrop) > 0) {
				var store = new TransferDataStore ();
				FillDataStore (store, e.Data, DragDropInfo.TargetTypes);

				var args = new DragEventArgs (pos, store, actualEffect.ToXwtDropAction ());
				Context.InvokeUserCode (delegate {
					eventSink.OnDragDrop (args);
				});

				if (args.Success)
					e.Effects = actualEffect;
			}
		}

		void WidgetDragLeaveHandler (object sender, System.Windows.DragEventArgs e)
		{
			OnDragLeave (sender, e);
		}

		private void WidgetMouseEnteredHandler (object sender, MouseEventArgs e)
		{
			Context.InvokeUserCode (eventSink.OnMouseEntered);
		}

		private void WidgetMouseExitedHandler (object sender, MouseEventArgs e)
		{
			Context.InvokeUserCode (eventSink.OnMouseExited);
		}

		private void WidgetMouseMoveHandler (object sender, MouseEventArgs e)
		{
			var p = e.GetPosition (Widget);
			var a = new MouseMovedEventArgs(e.Timestamp, p.X, p.Y);
			Context.InvokeUserCode (() => {
				eventSink.OnMouseMoved(a);
			});
			if (a.Handled)
				e.Handled = true;
		}

		private int mouseScrollCumulation = 0;

		private void WidgetMouseWheelHandler (object sender, MouseWheelEventArgs e)
		{
			mouseScrollCumulation += e.Delta;
			int jumps = mouseScrollCumulation / 120;
			mouseScrollCumulation %= 120;
			var p = e.GetPosition(Widget);
			Context.InvokeUserCode (delegate {
				for (int i = 0; i < jumps; i++) {
					var a = new MouseScrolledEventArgs(e.Timestamp, p.X, p.Y, ScrollDirection.Up);
					eventSink.OnMouseScrolled(a);
					if (a.Handled)
						e.Handled = true;
				}
				for (int i = 0; i > jumps; i--) {
					var a = new MouseScrolledEventArgs(e.Timestamp, p.X, p.Y, ScrollDirection.Down);
					eventSink.OnMouseScrolled(a);
					if (a.Handled)
						e.Handled = true;
				}
			});
		}

		private void WidgetOnSizeChanged (object sender, SizeChangedEventArgs e)
		{
			if (Widget.IsVisible)
				Context.InvokeUserCode (this.eventSink.OnBoundsChanged);
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
