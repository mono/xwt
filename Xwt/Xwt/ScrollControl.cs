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
	[BackendType (typeof(IScrollControlBackend))]
	public class ScrollControl: XwtComponent
	{
		EventHandler valueChanged;
		
		class ScrollAdjustmentBackendHost: BackendHost<ScrollControl,IScrollControlBackend>, IScrollControlEventSink
		{
			public void OnValueChanged ()
			{
				Parent.OnValueChanged (EventArgs.Empty);
			}
		}
		
		protected override Xwt.Backends.BackendHost CreateBackendHost ()
		{
			return new ScrollAdjustmentBackendHost ();
		}
		
		static ScrollControl ()
		{
			MapEvent (ScrollAdjustmentEvent.ValueChanged, typeof(ScrollAdjustment), "OnValueChanged");
		}

		internal ScrollControl (IScrollControlBackend backend)
		{
			BackendHost.SetCustomBackend (backend);
			backend.Initialize ((ScrollAdjustmentBackendHost)BackendHost);
		}
		
		IScrollControlBackend Backend {
			get { return (IScrollControlBackend) BackendHost.Backend; }
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

		/// <summary>
		/// Gets or sets the lowest value of the scroll range.
		/// </summary>
		/// <value>
		/// The lower value.
		/// </value>
		/// <remarks>It must be &lt;= UpperValue</remarks>
		public double LowerValue {
			get { return Backend.LowerValue; }
		}

		/// <summary>
		/// Gets or sets the highest value of the scroll range
		/// </summary>
		/// <value>
		/// The upper value.
		/// </value>
		/// <remarks>It must be >= LowerValue</remarks>
		public double UpperValue {
			get { return Backend.UpperValue; }
		}

		/// <summary>
		/// How much Value will be incremented when you click on the scrollbar to move
		/// to the next page (when the scrollbar supports it)
		/// </summary>
		/// <value>
		/// The page increment.
		/// </value>
		public double PageIncrement {
			get { return Backend.PageIncrement; }
		}

		/// <summary>
		/// How much the Value is incremented/decremented when you click on the down/up button in the scrollbar
		/// </summary>
		/// <value>
		/// The step increment.
		/// </value>
		public double StepIncrement {
			get { return Backend.StepIncrement; }
		}

		/// <summary>
		/// Size of the visible range
		/// </summary>
		/// <remarks>
		/// For example, if LowerValue=0, UpperValue=100, Value=25 and PageSize=50, the visible range will be 25 to 75
		/// </remarks>
		public double PageSize {
			get { return Backend.PageSize; }
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
	}
}

