// 
// WidgetBackend.cs
//  
// Author:
//       Carlos Alberto Cortez <calberto.cortez@gmail.com>
// 
// Copyright (c) 2011 Carlos Alberto Cortez
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

		void IWidgetBackend.Initialize (IWidgetEventSink eventSink)
		{
			this.eventSink = eventSink;
			Initialize ();
		}

		protected virtual void Initialize ()
		{
		}

		public virtual void Dispose (bool disposing)
		{
		}

		public IWidgetEventSink EventSink {
			get { return eventSink; }
		}

		public Widget Frontend {
			get { return frontend; }
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
				return ((SWM.SolidColorBrush)control.Background).Color;
			}
			if (Widget is SWC.Panel) {
				var panel = (SWC.Panel)Widget;
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
			var p = Widget.PointToScreen (new System.Windows.Point (widgetCoordinates.X, widgetCoordinates.Y));
			return new Point (p.X, p.Y);
		}

		System.Windows.Size GetWidgetDesiredSize ()
		{
			if (!Widget.IsMeasureValid)
				Widget.Measure (new System.Windows.Size (Double.PositiveInfinity, Double.PositiveInfinity));

			return Widget.DesiredSize;
		}

		public virtual WidgetSize GetPreferredWidth ()
		{
			var size = GetWidgetDesiredSize ();
			return new WidgetSize (size.Width) + frontend.Margin.HorizontalSpacing;
		}

		public virtual WidgetSize GetPreferredHeight ()
		{
			var size = GetWidgetDesiredSize ();
			return new WidgetSize (size.Height) + frontend.Margin.VerticalSpacing;
		}

		public virtual WidgetSize GetPreferredWidthForHeight (double height)
		{
			var size = GetWidgetDesiredSize ();
			return new WidgetSize (size.Width) + frontend.Margin.HorizontalSpacing;
		}

		public virtual WidgetSize GetPreferredHeightForWidth (double width)
		{
			var size = GetWidgetDesiredSize ();
			return new WidgetSize (size.Height) + frontend.Margin.VerticalSpacing;
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
			Widget.Width = width / WidthPixelRatio;
			Widget.Height = height / HeightPixelRatio;
		}

		public virtual void UpdateLayout ()
		{
		}

		public override void EnableEvent (object eventId)
		{
			if (eventId is WidgetEvent) {
				switch ((WidgetEvent)eventId) {
					case WidgetEvent.DragDropCheck:
						break;
					case WidgetEvent.DragDrop:
						break;
					case WidgetEvent.DragOverCheck:
						break;
					case WidgetEvent.DragOver:
						break;
					case WidgetEvent.DragLeave:
						Widget.DragLeave += WidgetDragLeaveHandler;
						break;
					case WidgetEvent.KeyPressed:
						Widget.KeyDown += WidgetKeyDownHandler;
						break;
					case WidgetEvent.KeyReleased:
						Widget.KeyDown += WidgetKeyUpHandler;
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
				}
			}
		}

		public override void DisableEvent (object eventId)
		{
			if (eventId is WidgetEvent) {
				switch ((WidgetEvent)eventId) {
					case WidgetEvent.DragDropCheck:
						break;
					case WidgetEvent.DragDrop:
						break;
					case WidgetEvent.DragOverCheck:
						break;
					case WidgetEvent.DragOver:
						break;
					case WidgetEvent.DragLeave:
						Widget.DragLeave -= WidgetDragLeaveHandler;
						break;
					case WidgetEvent.KeyPressed:
						Widget.KeyDown -= WidgetKeyDownHandler;
						break;
					case WidgetEvent.KeyReleased:
						Widget.KeyUp -= WidgetKeyUpHandler;
						break;
					case WidgetEvent.MouseEntered:
						Widget.MouseEnter -= WidgetMouseEnteredHandler;
						break;
					case WidgetEvent.MouseExited:
						Widget.MouseLeave -= WidgetMouseExitedHandler;
						break;
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
			throw new NotImplementedException ();
		}

		public void SetDragTarget (TransferDataType [] types, DragDropAction dragAction)
		{
			throw new NotImplementedException ();
		}

		public void SetDragSource (TransferDataType [] types, DragDropAction dragAction)
		{
			throw new NotImplementedException ();
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
	}

	public interface IWpfWidgetBackend
	{
		FrameworkElement Widget { get; }
	}
}
