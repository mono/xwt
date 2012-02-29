// 
// ScrollAdjustmentBackend.cs
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
using Xwt.Engine;

namespace Xwt.GtkBackend
{
	public class ScrollAdjustmentBackend: IScrollAdjustmentBackend
	{
		Gtk.Adjustment adjustment;
		IScrollAdjustmentEventSink eventSink;
		
		public Gtk.Adjustment Adjustment {
			get { return adjustment; }
		}
		
		public ScrollAdjustmentBackend ()
		{
		}
		
		public ScrollAdjustmentBackend (Gtk.Adjustment adjustment)
		{
			this.adjustment = adjustment;
		}
		
		#region IBackend implementation
		public void Initialize (object frontend)
		{
			if (adjustment == null)
				adjustment = new Gtk.Adjustment (0, 0, 0, 0, 0, 0);
		}

		public void Initialize (IScrollAdjustmentEventSink eventSink)
		{
			this.eventSink = eventSink;
		}

		public void EnableEvent (object eventId)
		{
			ScrollAdjustmentEvent ev = (ScrollAdjustmentEvent) eventId;
			if (ev == ScrollAdjustmentEvent.ValueChanged) {
				adjustment.ValueChanged += HandleValueChanged;
			}
		}

		public void DisableEvent (object eventId)
		{
			ScrollAdjustmentEvent ev = (ScrollAdjustmentEvent) eventId;
			if (ev == ScrollAdjustmentEvent.ValueChanged) {
				adjustment.ValueChanged -= HandleValueChanged;
			}
		}

		void HandleValueChanged (object sender, EventArgs e)
		{
			Toolkit.Invoke (delegate {
				eventSink.OnValueChanged ();
			});
		}
		#endregion

		#region IScrollAdjustmentBackend implementation
		public double Value {
			get {
				return adjustment.Value;
			}
			set {
				adjustment.Value = value;
			}
		}

		public double LowerValue {
			get {
				return adjustment.Lower;
			}
			set {
				adjustment.Lower = value;
			}
		}

		public double UpperValue {
			get {
				return adjustment.Upper;
			}
			set {
				adjustment.Upper = value;
			}
		}

		public double PageIncrement {
			get {
				return adjustment.PageIncrement;
			}
			set {
				adjustment.PageIncrement = value;
			}
		}

		public double StepIncrement {
			get {
				return adjustment.StepIncrement;
			}
			set {
				adjustment.StepIncrement = value;
			}
		}

		public double PageSize {
			get {
				return adjustment.PageSize;
			}
			set {
				adjustment.PageSize = value;
			}
		}
		#endregion
	}
}

