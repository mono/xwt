//
// SpinButton.cs
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

namespace Xwt
{
	[BackendType (typeof(ISpinButtonBackend))]
	public class SpinButton : Widget
	{
		ButtonStyle style;

		protected new class WidgetBackendHost: Widget.WidgetBackendHost, ISpinButtonEventSink
		{
			public void ValueChanged ()
			{
				((SpinButton)Parent).OnValueChanged (EventArgs.Empty);
			}
		}
		
		ISpinButtonBackend Backend {
			get { return (ISpinButtonBackend) BackendHost.Backend; }
		}
		
		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}

		public double ClimbRate {
			get { return Backend.ClimbRate; }
			set { Backend.ClimbRate = value; }
		}

		public int Digits {
			get { return Backend.Digits; }
			set { Backend.Digits = value; }
		}

		public double Value {
			get { return Backend.Value; }
			set { Backend.Value = value; }
		}

		public bool Wrap {
			get { return Backend.Wrap; }
			set { Backend.Wrap = value; }
		}

		public double MinimumValue {
			get { return Backend.MinimumValue; }
			set { Backend.MinimumValue = value; }
		}

		public double MaximumValue {
			get { return Backend.MaximumValue; }
			set { Backend.MaximumValue = value; }
		}

		public double IncrementValue {
			get { return Backend.IncrementValue; }
			set { Backend.IncrementValue = value; }
		}

		public ButtonStyle Style {
			get { return style; }
			set {
				style = value;
				Backend.SetButtonStyle (style);
				OnPreferredSizeChanged ();
			}
		}

		public string IndeterminateMessage {
			get { return Backend.IndeterminateMessage; }
			set { Backend.IndeterminateMessage = value; }
		}

		public bool IsIndeterminate {
			get { return Backend.IsIndeterminate; }
			set { Backend.IsIndeterminate = value; }
		}
		
		protected virtual void OnValueChanged (EventArgs e)
		{
			if (valueChanged != null)
				valueChanged (this, e);
		}

		EventHandler valueChanged;
		
		public event EventHandler ValueChanged {
			add {
				BackendHost.OnBeforeEventAdd (SpinButtonEvent.ValueChanged, valueChanged);
				valueChanged += value;
			}
			remove {
				valueChanged -= value;
				BackendHost.OnAfterEventRemove (SpinButtonEvent.ValueChanged, valueChanged);
			}
		}
	}
}

