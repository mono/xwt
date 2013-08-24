//
// Scrollbar.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc.
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
	[BackendType (typeof(IScrollbarBackend))]
	public class Scrollbar: Widget
	{
		Orientation orientation;
		ScrollAdjustment adjustment;

		internal Scrollbar (Orientation orientation)
		{
			this.orientation = orientation;
		}

		protected new class WidgetBackendHost: Widget.WidgetBackendHost<Scrollbar,IScrollbarBackend>
		{
			protected override void OnBackendCreated ()
			{
				Backend.Initialize (Parent.orientation);
				base.OnBackendCreated ();
			}
		}
		
		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}

		IScrollbarBackend Backend {
			get { return (IScrollbarBackend) BackendHost.Backend; }
		}

		public ScrollAdjustment ScrollAdjustment {
			get {
				if (adjustment == null)
					adjustment = new ScrollAdjustment (Backend.CreateAdjustment ());
				return adjustment;
			}
		}

		/// <summary>
		/// Updates the adjustment value to ensure that a range is in the current page
		/// </summary>
		/// <param name='lower'>
		/// The lower value
		/// </param>
		/// <param name='upper'>
		/// The upper value
		/// </param>
		/// <remarks>
		/// Updates the adjustment value to ensure that the range between lower and upper is in the current page
		/// (i.e. between Value and Value + PageSize). If the range is larger than the page size, then only the
		/// start of it will be in the current page. A "Changed" event will be raised if the value is changed.
		/// </remarks>
		public void ClampPage (double lower, double upper)
		{
			if (upper - lower >= PageSize || upper <= lower) {
				Value = lower;
				return;
			}
			if (lower >= Value && upper < Value + PageSize)
				return;
			if (lower < Value)
				Value = lower;
			else
				Value = upper - PageSize;
		}

		public double Value {
			get { return ScrollAdjustment.Value; }
			set { ScrollAdjustment.Value = value; }
		}

		public double LowerValue {
			get { return ScrollAdjustment.LowerValue; }
			set { ScrollAdjustment.LowerValue = value; OnAdjustmentChanged (); }
		}

		public double UpperValue {
			get { return ScrollAdjustment.UpperValue; }
			set { ScrollAdjustment.UpperValue = value; OnAdjustmentChanged (); }
		}

		public double PageIncrement {
			get { return ScrollAdjustment.PageIncrement; }
			set { ScrollAdjustment.PageIncrement = value; OnAdjustmentChanged (); }
		}

		public double StepIncrement {
			get { return ScrollAdjustment.StepIncrement; }
			set { ScrollAdjustment.StepIncrement = value; OnAdjustmentChanged (); }
		}

		public double PageSize {
			get { return ScrollAdjustment.PageSize; }
			set { ScrollAdjustment.PageSize = value; OnAdjustmentChanged (); }
		}
		
		EventHandler valueChanged;

		public event EventHandler ValueChanged {
			add {
				if (valueChanged == null)
					ScrollAdjustment.ValueChanged += HandleValueChanged;
				valueChanged += value;
			}
			remove {
				valueChanged -= value;
				if (valueChanged == null)
					ScrollAdjustment.ValueChanged -= HandleValueChanged;
			}
		}

		void HandleValueChanged (object sender, EventArgs e)
		{
			OnValueChanged (e);
		}

		protected virtual void OnValueChanged (EventArgs e)
		{
			if (valueChanged != null)
				valueChanged (this, e);
		}

		/// <summary>
		/// </summary>
		/// <remarks>
		/// This method is called when one of the properties of the adjustment changes.
		/// It is not called if the Value changes. You can override OnValueChanged for
		/// this use case.
		/// </remarks>
		protected virtual void OnAdjustmentChanged ()
		{
		}
	}
}

