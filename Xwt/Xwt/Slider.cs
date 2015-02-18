//
// Slider.cs
//
// Author:
//       Jérémie Laval <jeremie.laval@xamarin.com>
//
// Copyright (c) 2013 Xamarin, Inc.
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
	[BackendType (typeof(ISliderBackend))]
	public abstract class Slider : Widget
	{
		Orientation orientation;

		internal Slider (Orientation orientation)
		{
			this.orientation = orientation;
		}

		protected new class WidgetBackendHost: Widget.WidgetBackendHost<Slider,ISliderBackend>, ISliderEventSink
		{
			protected override void OnBackendCreated ()
			{
				Backend.Initialize (Parent.orientation);
				base.OnBackendCreated ();
			}

			public void ValueChanged ()
			{
				((Slider)Parent).OnValueChanged (EventArgs.Empty);
			}
		}

		ISliderBackend Backend {
			get { return (ISliderBackend) BackendHost.Backend; }
		}
		
		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}

		public double Value {
			get { return Backend.Value; }
			set { Backend.Value = value; }
		}

		public double MinimumValue {
			get { return Backend.MinimumValue; }
			set { Backend.MinimumValue = value; }
		}

		public double MaximumValue {
			get { return Backend.MaximumValue; }
			set { Backend.MaximumValue = value; }
		}

		public double StepIncrement {
			get { return Backend.StepIncrement; }
			set { Backend.StepIncrement = value; }
		}

		public bool SnapToTicks {
			get { return Backend.SnapToTicks; }
			set { Backend.SnapToTicks = value; }
		}

		public double SliderPosition {
			get { return Backend.SliderPosition; }
		}

		protected virtual void OnValueChanged (EventArgs e)
		{
			if (valueChanged != null)
				valueChanged (this, e);
		}
		
		EventHandler valueChanged;
		
		public event EventHandler ValueChanged {
			add {
				BackendHost.OnBeforeEventAdd (SliderEvent.ValueChanged, valueChanged);
				valueChanged += value;
			}
			remove {
				valueChanged -= value;
				BackendHost.OnAfterEventRemove (SliderEvent.ValueChanged, valueChanged);
			}
		}
	}
}
