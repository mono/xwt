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

        public override void EnableEvent(object eventId)
        {
            base.EnableEvent(eventId);
            if (eventId is ScrollAdjustmentEvent)
            {
                if (((ScrollAdjustmentEvent)eventId) == ScrollAdjustmentEvent.ValueChanged)
                    Scrollbar.Scroll += Scrollbar_Scroll;
            }
        }

        public override void DisableEvent(object eventId)
        {
            base.DisableEvent(eventId);
            if (eventId is ScrollAdjustmentEvent)
            {
                if (((ScrollAdjustmentEvent)eventId) == ScrollAdjustmentEvent.ValueChanged)
                    Scrollbar.Scroll -= Scrollbar_Scroll;
            }
        }

        void Scrollbar_Scroll(object sender, SWC.Primitives.ScrollEventArgs e)
        {
            this.Context.InvokeUserCode(delegate
            {
                eventSink.OnValueChanged();
            });
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
            this.eventSink = eventSink;
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
