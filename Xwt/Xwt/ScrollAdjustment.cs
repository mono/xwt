// 
// Adjustment.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
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
using Xwt.Backends;

namespace Xwt
{
	[BackendType (typeof(IScrollAdjustmentBackend))]
	public class ScrollAdjustment: XwtComponent
	{
		EventHandler valueChanged;
		
		class ScrollAdjustmentBackendHost: BackendHost<ScrollAdjustment,IScrollAdjustmentBackend>, IScrollAdjustmentEventSink
		{
			protected override IBackend OnCreateBackend ()
			{
				// When creating a standalone ScrollAdjustment (not bound to any widget) we always use
				// a default platform-agnostic implementation
				return new DefaultScrollAdjustmentBackend ();
			}

			protected override void OnBackendCreated ()
			{
				base.OnBackendCreated ();
				Backend.Initialize (this);
			}
	
			public void OnValueChanged ()
			{
				Parent.OnValueChanged (EventArgs.Empty);
			}
		}
		
		protected override Xwt.Backends.BackendHost CreateBackendHost ()
		{
			return new ScrollAdjustmentBackendHost ();
		}
		
		static ScrollAdjustment ()
		{
			MapEvent (ScrollAdjustmentEvent.ValueChanged, typeof(ScrollAdjustment), "OnValueChanged");
		}

		public ScrollAdjustment ()
		{
			LowerValue = 0;
			UpperValue = 100;
			Value = 0;
			PageSize = 10;
			StepIncrement = 1;
			PageIncrement = 10;
		}
		
		internal ScrollAdjustment (IBackend backend)
		{
			BackendHost.SetCustomBackend (backend);
		}
		
		IScrollAdjustmentBackend Backend {
			get { return (IScrollAdjustmentBackend) BackendHost.Backend; }
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
			get { return Backend.Value; }
			set { Backend.Value = value; }
		}
		
		public double LowerValue {
			get { return Backend.LowerValue; }
			set { Backend.LowerValue = value; OnAdjustmentChanged (); }
		}
		
		public double UpperValue {
			get { return Backend.UpperValue; }
			set { Backend.UpperValue = value; OnAdjustmentChanged (); }
		}
		
		public double PageIncrement {
			get { return Backend.PageIncrement; }
			set { Backend.PageIncrement = value; OnAdjustmentChanged (); }
		}
		
		public double StepIncrement {
			get { return Backend.StepIncrement; }
			set { Backend.StepIncrement = value; OnAdjustmentChanged (); }
		}
		
		public double PageSize {
			get { return Backend.PageSize; }
			set { Backend.PageSize = value; OnAdjustmentChanged (); }
		}
		
		protected virtual void OnValueChanged (EventArgs e)
		{
			if (valueChanged != null)
				valueChanged (this, e);
		}
		
		public event EventHandler ValueChanged {
			add {
				BackendHost.OnBeforeEventAdd (ScrollAdjustmentEvent.ValueChanged, valueChanged);
				valueChanged += value;
			}
			remove {
				valueChanged -= value;
				BackendHost.OnAfterEventRemove (ScrollAdjustmentEvent.ValueChanged, valueChanged);
			}
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

		class DefaultScrollAdjustmentBackend: IScrollAdjustmentBackend
		{
			IScrollAdjustmentEventSink eventSink;
			double currentValue;

			public void Initialize (IScrollAdjustmentEventSink eventSink)
			{
				this.eventSink = eventSink;
			}

			public double Value {
				get { return currentValue; }
				set { currentValue = value; eventSink.OnValueChanged (); }
			}

			public double LowerValue { get; set; }

			public double UpperValue { get; set; }

			public double PageIncrement { get; set; }

			public double StepIncrement { get; set; }

			public double PageSize { get; set; }

			public void InitializeBackend (object frontend, ApplicationContext context)
			{
			}

			public void EnableEvent (object eventId)
			{
			}

			public void DisableEvent (object eventId)
			{
			}
		}
	}
}

