using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xwt.Backends;
using SWC = System.Windows.Controls;

namespace Xwt.WPFBackend
{
	class ScrollbarBackend : WidgetBackend, IScrollbarBackend, IScrollAdjustmentBackend
	{
		IScrollAdjustmentEventSink eventSink;

		#region IScrollbarBackend Members

		public void Initialize (Orientation dir)
		{
			Widget = new System.Windows.Controls.Primitives.ScrollBar () {
				Orientation = dir == Orientation.Horizontal ? SWC.Orientation.Horizontal : SWC.Orientation.Vertical
			};
		}

		public IScrollAdjustmentBackend CreateAdjustment ()
		{
			return this;
		}

		protected SWC.Primitives.ScrollBar Scrollbar
		{
			get { return (SWC.Primitives.ScrollBar)Widget; }
		}

		#endregion

		#region IScrollAdjustmentBackend Members

		public void Initialize (IScrollAdjustmentEventSink eventSink)
		{
		}

		public double Value
		{
			get
			{
				return Scrollbar.Value;
			}
			set
			{
				Scrollbar.Value = value;
			}
		}

		public void SetRange (double lowerValue, double upperValue, double pageSize, double pageIncrement, double stepIncrement, double value)
		{
			Scrollbar.Minimum = lowerValue;
			Scrollbar.Maximum = upperValue;
			Scrollbar.ViewportSize = pageSize;
		}

		#endregion
	}
}
