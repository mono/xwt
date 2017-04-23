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


namespace Xwt.GtkBackend
{
	public class ScrollControltBackend: IScrollControlBackend
	{
		Gtk.Adjustment adjustment;
		IScrollControlEventSink eventSink;
		ApplicationContext context;
		
		public Gtk.Adjustment Adjustment {
			get { return adjustment; }
		}
		
		public ScrollControltBackend ()
		{
		}
		
		public ScrollControltBackend (Gtk.Adjustment adjustment)
		{
			this.adjustment = adjustment;
		}
		
		#region IBackend implementation
		public void InitializeBackend (object frontend, ApplicationContext context)
		{
			this.context = context;
		}

		public void Initialize (IScrollControlEventSink eventSink)
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
			context.InvokeUserCode (eventSink.OnValueChanged);
		}
		#endregion

		#region IScrollControlBackend implementation

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
		}

		public double UpperValue {
			get {
				return adjustment.Upper;
			}
		}

		public double PageSize {
			get {
				return adjustment.PageSize;
			}
		}

		public double PageIncrement {
			get {
				return adjustment.PageIncrement;
			}
		}

		public double StepIncrement {
			get {
				return adjustment.StepIncrement;
			}
		}
		#endregion
	}
}

