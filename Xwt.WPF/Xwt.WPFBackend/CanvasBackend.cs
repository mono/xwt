
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xwt.Backends;
using System.Windows;
using SWC = System.Windows.Controls;
using System.Windows.Media;

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

		protected override void SetWidgetColor (Drawing.Color value)
		{
			// Do nothing
		}

		private void Render (System.Windows.Media.DrawingContext dc)
		{
			if (BackgroundColorSet) {
				SolidColorBrush mySolidColorBrush = new SolidColorBrush ();
				mySolidColorBrush.Color = BackgroundColor.ToWpfColor ();
				Rect myRect = new Rect (0, 0, Widget.ActualWidth, Widget.ActualHeight);
				dc.DrawRectangle (mySolidColorBrush, null, myRect);
			}
			
			var ctx = new Xwt.WPFBackend.DrawingContext (dc, Widget.GetScaleFactor ());
			CanvasEventSink.OnDraw (ctx, new Rectangle (0, 0, Widget.ActualWidth, Widget.ActualHeight));
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

			var h = bounds.Height;
			var w = bounds.Width;

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
