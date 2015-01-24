using System;

using Xwt.Backends;


#if MONOMAC
using nint = System.Int32;
using nfloat = System.Single;
using CGRect = System.Drawing.RectangleF;
using CGPoint = System.Drawing.PointF;
using CGSize = System.Drawing.SizeF;
using MonoMac.Foundation;
using MonoMac.AppKit;
#else
using Foundation;
using AppKit;
using CoreGraphics;
#endif

namespace Xwt.Mac
{
	public class SpinButtonBackend : ViewBackend<MacSpinButton, ISpinButtonEventSink>, ISpinButtonBackend
	{
		public override void Initialize ()
		{
			ViewObject = new MacSpinButton (EventSink);
		}
		
		protected new ISpinButtonEventSink EventSink {
			get { return (ISpinButtonEventSink)base.EventSink; }
		}
		
		public override void EnableEvent (object eventId)
		{
		}
		
		public override void DisableEvent (object eventId)
		{
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
			get;
			set;
		}

		public bool IsIndeterminate {
			get;
			set;
		}

		protected override void OnSizeToFit ()
		{
			Widget.SizeToFit ();
		}
	}

	public class MacSpinButton : NSView, IViewObject
	{
		NSStepper stepper;
		NSTextField input;
		NSNumberFormatter formater;

		class RelativeTextField : NSTextField
		{
			NSView reference;

			public RelativeTextField (NSView reference)
			{
				this.reference = reference;
			}

			public override void ResizeWithOldSuperviewSize (CGSize oldSize)
			{
				base.ResizeWithOldSuperviewSize (oldSize);
				SetFrameSize (new CGSize (reference.Frame.Left, Frame.Size.Height));
			}
		}

		public MacSpinButton (ISpinButtonEventSink eventSink)
		{
			formater = new NSNumberFormatter ();
			stepper = new NSStepper ();
			input = new RelativeTextField (stepper);
			input.Formatter = formater;
			input.Alignment = NSTextAlignment.Right;
			formater.NumberStyle = NSNumberFormatterStyle.Decimal;
			stepper.Activated += (sender, e) => input.DoubleValue = stepper.DoubleValue;

			AutoresizesSubviews = true;
			stepper.AutoresizingMask = NSViewResizingMask.MinXMargin;
			input.AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.MaxXMargin;

			AddSubview (input);
			AddSubview (stepper);
		}

		public void SizeToFit()
		{
			stepper.SizeToFit ();
			input.SizeToFit ();

			var minHeight = (nfloat)Math.Max (stepper.Frame.Height, input.Frame.Height);
			var minWidth = input.Frame.Width + stepper.Frame.Width;
			minWidth = (nfloat)Math.Max (minWidth, 55);
			var stepperX = minWidth - (stepper.Frame.Width);
			var stepperY = (minHeight - stepper.Frame.Height) / 2;
			var inputX = 0;
			var inputY = (minHeight - input.Frame.Height) / 2;

			SetFrameSize (new CGSize (minWidth, minHeight));
			stepper.Frame = new CGRect (stepperX, stepperY, stepper.Frame.Width, stepper.Frame.Height);
			input.Frame = new CGRect (inputX, inputY, minWidth - (stepper.Frame.Width), input.Frame.Height);
		}

		public double ClimbRate {
			get { return stepper.Increment; }
			set { stepper.Increment = value; }
		}

		public int Digits {
			get { return (int)formater.MaximumSignificantDigits; }
			set { formater.MaximumSignificantDigits = (uint)value; }
		}

		public double Value {
			get { return input.DoubleValue; }
			set {
				stepper.DoubleValue = value;
				input.DoubleValue = value;
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
			}
		}

		public double MaximumValue {
			get { return stepper.MaxValue; }
			set {
				stepper.MaxValue = value;
				formater.Maximum = new NSNumber (value);
			}
		}

		public double IncrementValue {
			get { return stepper.Increment; }
			set { stepper.Increment = value; }
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

		public ViewBackend Backend { get; set; }
		
		public NSView View {
			get { return this; }
		}
		
		public void EnableEvent (Xwt.Backends.ButtonEvent ev)
		{
		}

		public void DisableEvent (Xwt.Backends.ButtonEvent ev)
		{
		}
	}
}

