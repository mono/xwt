using System;
using System.Globalization;

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

		public string Text {
			get { return Widget.Text; }
			set { Widget.Text = value; }
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

		string indeterminateMessage = String.Empty;
		public string IndeterminateMessage {
			get { return indeterminateMessage; }
			set {
				indeterminateMessage = value;
				if (IsIndeterminate)
					Text = indeterminateMessage;
			}
		}

		bool isIndeterminate = false;
		public bool IsIndeterminate {
			get { return isIndeterminate; }
			set {
				isIndeterminate = value;
				if (value)
					Text = indeterminateMessage;
			}
		}
	}

	public class MacSpinButton : NSView, IViewObject
	{
		NSStepper stepper;
		NSTextField input;

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
				SetFrameSize (new CGSize (reference.Frame.Left - 6, Frame.Size.Height));
			}
		}

		public MacSpinButton (ISpinButtonEventSink eventSink)
		{
			stepper = new NSStepper ();
			input = new RelativeTextField (stepper);
			input.StringValue = stepper.DoubleValue.ToString("N");
			input.Alignment = NSTextAlignment.Right;
			stepper.Activated += HandleValueOutput;
			input.Changed += HandleValueInput;


			SetFrameSize (new CGSize (55, 22));
			stepper.Frame = new System.Drawing.RectangleF (new System.Drawing.PointF (36, 0), new System.Drawing.SizeF (19, 22));
			input.Frame = new System.Drawing.RectangleF (new System.Drawing.PointF (4, 0), new System.Drawing.SizeF (26, 22));

			AutoresizesSubviews = true;
			stepper.AutoresizingMask = NSViewResizingMask.MinXMargin | NSViewResizingMask.MinYMargin;
			input.AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.MaxXMargin | NSViewResizingMask.MaxYMargin;

			AddSubview (input);
			AddSubview (stepper);
		}

		void HandleValueOutput (object sender, EventArgs e)
		{
			HandleValueOutput ();
			Backend.ApplicationContext.InvokeUserCode (delegate {
				((SpinButtonBackend)Backend).EventSink.ValueChanged();
			});
		}

		void HandleValueOutput ()
		{
			var outputArgs = new WidgetEventArgs ();
			Backend.ApplicationContext.InvokeUserCode (delegate {
				((SpinButtonBackend)Backend).EventSink.ValueOutput(outputArgs);
			});
			if (!outputArgs.Handled)
				input.StringValue = stepper.DoubleValue.ToString("N" + Digits);
		}

		void HandleValueInput (object sender, EventArgs e)
		{
			var argsInput = new SpinButtonInputEventArgs ();
			Backend.ApplicationContext.InvokeUserCode (delegate {
				((SpinButtonBackend)Backend).EventSink.ValueInput(argsInput);
			});

			double new_val = double.NaN;
			if (argsInput.Handled)
				new_val = argsInput.NewValue;
			else {
				var numberStyle = NumberStyles.Float;
				if (Digits == 0)
					numberStyle = NumberStyles.Integer;
				if (!double.TryParse (input.StringValue, numberStyle, CultureInfo.CurrentCulture, out new_val))
					new_val = double.NaN;
			}

			if (stepper.DoubleValue == new_val)
				return;

			if (double.IsNaN (new_val)) { // reset to previous input
				HandleValueOutput ();
				return;
			}

			stepper.DoubleValue = new_val;
			if (!argsInput.Handled)
				input.StringValue = stepper.DoubleValue.ToString ("N" + Digits);

			Backend.ApplicationContext.InvokeUserCode (delegate {
				((SpinButtonBackend)Backend).EventSink.ValueChanged ();
			});
		}

		public double ClimbRate {
			get { return stepper.Increment; }
			set { stepper.Increment = value; }
		}

		int digits;
		public int Digits {
			get { return digits; }
			set {
				digits = value;
				HandleValueOutput ();
			}
		}

		public double Value {
			get { return stepper.DoubleValue; }
			set {
				stepper.DoubleValue = value;
				HandleValueOutput ();
			}
		}

		public string Text {
			get { return input.StringValue; }
			set { input.StringValue = value; }
		}

		public bool Wrap {
			get { return stepper.ValueWraps; }
			set { stepper.ValueWraps = value; }
		}

		public double MinimumValue {
			get { return stepper.MinValue; }
			set {
				stepper.MinValue = value;
				HandleValueOutput ();
			}
		}

		public double MaximumValue {
			get { return stepper.MaxValue; }
			set {
				stepper.MaxValue = value;
				HandleValueOutput ();
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

