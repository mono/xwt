//
// SpinButtonBackend.cs
//
// Author:
//       Jérémie Laval <jeremie.laval@xamarin.com>
//
// Copyright (c) 2012 Xamarin Inc
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
using AppKit;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using Xwt.Backends;

namespace Xwt.Mac
{
	public class SpinButtonBackend : ViewBackend<MacSpinButton, ISpinButtonEventSink>, ISpinButtonBackend
	{
		public override void Initialize ()
		{
			ViewObject = new MacSpinButton (EventSink, ApplicationContext);
		}
		
		public override void EnableEvent (object eventId)
		{
			Widget.EnableEvent (eventId);
		}
		
		public override void DisableEvent (object eventId)
		{
			Widget.DisableEvent (eventId);
		}

		public double ClimbRate {
			get { return Widget.ClimbRate; }
			set { Widget.ClimbRate = value; }
		}

		public int Digits {
			get { return Widget.Digits; }
			set { Widget.Digits = value; }
		}

		public double Value {
			get { return Widget.Value; }
			set { Widget.Value = value; }
		}

		public bool Wrap {
			get { return Widget.Wrap; }
			set { Widget.Wrap = value; }
		}

		public double MinimumValue {
			get { return Widget.MinimumValue; }
			set { Widget.MinimumValue = value; }
		}

		public double MaximumValue {
			get { return Widget.MaximumValue; }
			set { Widget.MaximumValue = value; }
		}

		public double IncrementValue {
			get { return Widget.IncrementValue; }
			set { Widget.IncrementValue = value; }
		}

		public void SetButtonStyle (ButtonStyle style)
		{
			Widget.SetButtonStyle (style);
		}

		public string IndeterminateMessage {
			get { return Widget.IndeterminateMessage; }
			set { Widget.IndeterminateMessage = value; }
		}

		public bool IsIndeterminate {
			get { return Widget.IsIndeterminate; }
			set { Widget.IsIndeterminate = value; }
		}

		protected override void OnSizeToFit ()
		{
			Widget.SizeToFit ();
		}

		bool canGetFocus;
		public override bool CanGetFocus
		{
			get { return canGetFocus; }
			set { canGetFocus = value; }
		}

		public override void SetFocus()
		{
			if (Widget.Input.Window != null && CanGetFocus)
				Widget.Input.Window.MakeFirstResponder(Widget.Input);
		}

		public override bool HasFocus
		{
			get
			{
				return Widget.Window != null && (Widget.Window.FirstResponder == Widget.Input || Widget.Window.FirstResponder == Widget.Stepper);
			}
		}
	}

	public sealed class MacSpinButton : WidgetView
	{
		NSStepper stepper;
		NSTextField input;
		NSNumberFormatter formater;

		internal NSTextField Input { get { return input; } }
		internal NSStepper Stepper { get { return stepper; } }

		ISpinButtonEventSink eventSink;

		public MacSpinButton (ISpinButtonEventSink eventSink, ApplicationContext context) : base (eventSink, context)
		{
			this.eventSink = eventSink;
			formater = new NSNumberFormatter ();
			stepper = new VibrancyStepper ();
			input = new NSTextField ();
			input.Formatter = formater;
			input.DoubleValue = 0;
			input.Alignment = NSTextAlignment.Right;
			formater.NumberStyle = NSNumberFormatterStyle.Decimal;
			stepper.Activated += HandleStepperChanged;
			input.Changed += HandleTextChanged;
			input.DoCommandBySelector = DoCommandBySelector;

			AutoresizesSubviews = true;

			AddSubview (input);
			AddSubview (stepper);
		}

		public void SizeToFit()
		{
			stepper.SizeToFit ();
			input.SizeToFit ();

			var minHeight = (nfloat)Math.Max (stepper.Frame.Height, input.Frame.Height);
			var minWidth = input.Frame.Width + stepper.Frame.Width;
			minWidth = (nfloat)Math.Max (minWidth, 60);
			SetFrameSize (new CGSize (minWidth, minHeight));
		}

		public override void ResizeSubviewsWithOldSize (CGSize oldSize)
		{
			stepper.SizeToFit ();
			var stepperX = Frame.Width - (stepper.Frame.Width);
			var stepperY = (Frame.Height - stepper.Frame.Height) / 2;
			stepper.Frame = new CGRect (stepperX, stepperY, stepper.Frame.Width, stepper.Frame.Height);

			var inputX = 0;
			var inputY = (Frame.Height - input.Frame.Height) / 2;
			input.Frame = new CGRect (inputX, inputY, Frame.Width - stepper.Frame.Width, input.Frame.Height);
		}

		public override void ScrollWheel (NSEvent theEvent)
		{
			double minDelta = 0;
			if (theEvent.DeltaY > 0)
				minDelta = 1;
			if (theEvent.DeltaY < 0)
				minDelta = -1;

			Value += IncrementValue * (Math.Abs (theEvent.DeltaY) < 1 ? minDelta : Math.Round(theEvent.DeltaY));
			base.ScrollWheel (theEvent);
		}

		void HandleStepperChanged (object sender, EventArgs e)
		{
			isIndeterminate = false;
			var alignedStepperValue = Math.Round (stepper.DoubleValue, Digits);
			if (Math.Abs (stepper.DoubleValue - alignedStepperValue) > double.Epsilon)
				stepper.DoubleValue = alignedStepperValue;
			
			input.DoubleValue = stepper.DoubleValue;
			if (enableValueChangedEvent) {
				Backend.ApplicationContext.InvokeUserCode (eventSink.ValueChanged);
			}
		}

		void HandleTextChanged (object sender, EventArgs e)
		{
			isIndeterminate = false;
			stepper.DoubleValue = input.DoubleValue;
			if (enableValueChangedEvent) {
				Backend.ApplicationContext.InvokeUserCode (eventSink.ValueChanged);
			}
		}

		bool DoCommandBySelector (
			NSControl control, 
			NSTextView textView, 
			Selector commandSelector)
		{
			switch (commandSelector.Name) {
			case "moveUp:":
				Value += IncrementValue;
				return true;
			case "moveDown:":
				Value -= IncrementValue;
				return true;
			case "scrollPageUp:":
				Value += IncrementValue;
				return true;
			case "scrollPageDown:":
				Value -= IncrementValue;
				return true;
			}
			return false;
		}

		//TODO: implement key climb rate
		public double ClimbRate { get; set; }

		public int Digits {
			get { return (int)formater.MaximumFractionDigits; }
			set { formater.MaximumFractionDigits = formater.MinimumFractionDigits = value; }
		}

		public double Value {
			get { return stepper.DoubleValue; }
			set {
				isIndeterminate = false;
				if (value < MinimumValue)
					value = MinimumValue;
				if (value > MaximumValue)
					value = MaximumValue;

				stepper.DoubleValue = value;
				input.DoubleValue = value;
				if (enableValueChangedEvent) {
					Backend.ApplicationContext.InvokeUserCode (eventSink.ValueChanged);
				}
			}
		}

		public bool Wrap {
			get { return stepper.ValueWraps; }
			set { stepper.ValueWraps = value; }
		}

		public double MinimumValue {
			get { return stepper.MinValue; }
			set {
				stepper.MinValue = value;
				formater.Minimum = new NSNumber (value);
				input.DoubleValue = Value; // update text field
			}
		}

		public double MaximumValue {
			get { return stepper.MaxValue; }
			set {
				stepper.MaxValue = value;
				formater.Maximum = new NSNumber (value);
				input.DoubleValue = Value; // update text field
			}
		}

		public double IncrementValue {
			get { return stepper.Increment; }
			set { stepper.Increment = value; }
		}

		string indeterminateMessage;
		public string IndeterminateMessage {
			get {
				return indeterminateMessage;
			}
			set {
				indeterminateMessage = value;
				if (IsIndeterminate)
					input.StringValue = value ?? String.Empty;
			}
		}

		bool isIndeterminate;

		public bool IsIndeterminate {
			get {
				return isIndeterminate;
			}
			set {
				isIndeterminate = value;
				if (value) {
					input.StringValue = IndeterminateMessage ?? String.Empty;
				} else {
					input.DoubleValue = stepper.DoubleValue;
				}
			}
		}

		public void SetButtonStyle (ButtonStyle style)
		{
			switch (style) {
			case ButtonStyle.Borderless:
			case ButtonStyle.Flat:
				input.Bordered = false;
				break;
			default:
				input.Bordered = true;
				break;
			}
		}

		bool enableValueChangedEvent;

		public void EnableEvent (object eventId)
		{
			if (eventId is SpinButtonEvent) {
				switch ((SpinButtonEvent)eventId) {
				case SpinButtonEvent.ValueChanged: enableValueChangedEvent = true; break;
				}
			}
		}

		public void DisableEvent (object eventId)
		{
			if (eventId is SpinButtonEvent) {
				switch ((SpinButtonEvent)eventId) {
				case SpinButtonEvent.ValueChanged: enableValueChangedEvent = false; break;
				}
			}
		}

		public override bool AcceptsFirstResponder()
		{
			return false;
		}

		public override string AccessibilityLabel
		{
			get
			{
				return base.AccessibilityLabel;
			}

			set
			{
				base.AccessibilityLabel = value;
				input.AccessibilityLabel = value;
			}
		}

		public override NSObject AccessibilityTitleUIElement
		{
			get
			{
				return base.AccessibilityTitleUIElement;
			}

			set
			{
				base.AccessibilityTitleUIElement = value;
				input.AccessibilityTitleUIElement = value;
				stepper.AccessibilityTitleUIElement = value;
			}
		}

		class VibrancyStepper : NSStepper
		{
			public override bool AllowsVibrancy {
				get {
					// we don't support vibrancy
					if (EffectiveAppearance.AllowsVibrancy)
						return false;
					return base.AllowsVibrancy;
				}
			}
		}
	}
}

