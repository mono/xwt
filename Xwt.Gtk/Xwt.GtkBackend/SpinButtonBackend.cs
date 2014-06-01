//
// SpinButtonBackend.cs
//
// Author:
//       Jérémie Laval <jeremie.laval@xamarin.com>
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
using Xwt.Backends;


using Gtk;

namespace Xwt.GtkBackend
{
	public partial class SpinButtonBackend : WidgetBackend, ISpinButtonBackend
	{
		string indeterminateMessage;

		public SpinButtonBackend ()
		{
		}

		public override void Initialize ()
		{
			Widget = (Gtk.SpinButton) CreateWidget ();
			Widget.Numeric = true;
			Widget.Alignment = 1.0f;
			InitializeGtk ();
			Widget.Show ();
		}
		
		protected virtual Gtk.Widget CreateWidget ()
		{
			return new Gtk.SpinButton (0, 1, .1);
		}
		
		protected new Gtk.SpinButton Widget {
			get { return (Gtk.SpinButton)base.Widget; }
			set { base.Widget = value; }
		}
		
		protected new ISpinButtonEventSink EventSink {
			get { return (ISpinButtonEventSink)base.EventSink; }
		}
		
		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is SpinButtonEvent) {
				if ((SpinButtonEvent)eventId == SpinButtonEvent.ValueChanged)
					Widget.ValueChanged += HandleValueChanged;
			}
		}
		
		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is SpinButtonEvent) {
				if ((SpinButtonEvent)eventId == SpinButtonEvent.ValueChanged)
					Widget.ValueChanged -= HandleValueChanged;
			}
		}

		void HandleValueChanged (object sender, EventArgs e)
		{
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.ValueChanged ();
			});
		}

		public double ClimbRate {
			get { return Widget.ClimbRate; }
			set { Widget.ClimbRate = value; }
		}

		public int Digits {
			get { return (int)Widget.Digits; }
			set { Widget.Digits = (uint)value; }
		}

		public double Value {
			get { return Widget.Value; }
			set {
				Widget.Numeric = true;
				Widget.Value = value;
			}
		}

		public bool Wrap {
			get { return Widget.Wrap; }
			set { Widget.Wrap = value; }
		}

		public double MinimumValue {
			get { return Widget.Adjustment.Lower; }
			set { Widget.Adjustment.Lower = value; }
		}

		public double MaximumValue {
			get { return Widget.Adjustment.Upper; }
			set { Widget.Adjustment.Upper = value; }
		}

		public double IncrementValue {
			get { return Widget.Adjustment.StepIncrement; }
			set { Widget.Adjustment.StepIncrement = value; }
		}

		public void SetButtonStyle (ButtonStyle style)
		{
			switch (style) {
			case ButtonStyle.Borderless:
			case ButtonStyle.Flat:
				Widget.HasFrame = false;
				break;
			default:
				Widget.HasFrame = true;
				break;
			}
		}

		public bool IsIndeterminate {
			get { return !Widget.Numeric; }
			set {
				Widget.Numeric = !value;
				if (value)
					Widget.Text = indeterminateMessage ?? string.Empty;
				else
					Widget.Value = MinimumValue;
			}
		}

		public string IndeterminateMessage {
			get {
				return indeterminateMessage;
			}
			set {
				indeterminateMessage = value;
				if (IsIndeterminate)
					Widget.Text = indeterminateMessage ?? string.Empty;
			}
		}
	}
}

