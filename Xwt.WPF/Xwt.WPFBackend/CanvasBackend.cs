
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xwt.Backends;
using System.Windows;
using SWC = System.Windows.Controls;

namespace Xwt.WPFBackend
{
	class CanvasBackend
		: WidgetBackend, ICanvasBackend
	{
		#region ICanvasBackend Members

		public CanvasBackend ()
		{
			Canvas = new ExCanvas ();
			Canvas.RenderAction = Render;
		}

		private ExCanvas Canvas
		{
			get { return (ExCanvas) Widget; }
			set { Widget = value; }
		}

		private ICanvasEventSink CanvasEventSink
		{
			get { return (ICanvasEventSink) EventSink; }
		}

		private void Render (System.Windows.Media.DrawingContext dc)
		{
			CanvasEventSink.OnDraw (new Xwt.WPFBackend.DrawingContext (dc, Widget.GetScaleFactor ()), new Rectangle (0, 0, Widget.ActualWidth, Widget.ActualHeight));
		}

		public void QueueDraw ()
		{
			Canvas.InvalidateVisual ();
		}

		public void QueueDraw (Rectangle rect)
		{
			Canvas.InvalidateVisual ();
		}

		public void AddChild (IWidgetBackend widget, Rectangle bounds)
		{
			UIElement element = widget.NativeWidget as UIElement;
			if (element == null)
				throw new ArgumentException ();

			if (!Canvas.Children.Contains (element))
				Canvas.Children.Add (element);

			SetChildBounds (widget, bounds);
		}

		public void SetChildBounds (IWidgetBackend widget, Rectangle bounds)
		{
			FrameworkElement element = widget.NativeWidget as FrameworkElement;
			if (element == null)
				throw new ArgumentException ();

			SWC.Canvas.SetTop (element, bounds.Top);
			SWC.Canvas.SetLeft (element, bounds.Left);

			// We substract the widget margin here because the size we are assigning is the actual size, not including the WPF marings
			var h = bounds.Height - ((WidgetBackend)widget).Frontend.Margin.VerticalSpacing;
			var w = bounds.Width - ((WidgetBackend)widget).Frontend.Margin.HorizontalSpacing;

			h = (h > 0) ? h : 0;
			w = (w > 0) ? w : 0;

			// Measure the widget again using the allocation constraints. This is necessary
			// because WPF widgets my cache some measurement information based on the
			// constraints provided in the last Measure call (which when calculating the
			// preferred size is normally set to infinite.
			element.InvalidateMeasure ();
			element.Measure (new System.Windows.Size (w, h));
			element.Height = h;
			element.Width = w;
			element.UpdateLayout ();
		}

		public void RemoveChild (IWidgetBackend widget)
		{
			UIElement element = widget.NativeWidget as UIElement;
			if (element == null)
				throw new ArgumentException ();

			Canvas.Children.Remove (element);
		}

		#endregion
	}
}
