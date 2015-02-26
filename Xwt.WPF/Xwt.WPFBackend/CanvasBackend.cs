
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
			Canvas = new CustomPanel ();
			Canvas.RenderAction = Render;
		}

		private CustomPanel Canvas
		{
			get { return (CustomPanel)Widget; }
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
            ctx.Context.PushClip(new RectangleGeometry(new Rect(0, 0, Widget.ActualWidth, Widget.ActualHeight)));
            CanvasEventSink.OnDraw(ctx, new Rectangle(0, 0, Widget.ActualWidth, Widget.ActualHeight));
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

		List<IWidgetBackend> children = new List<IWidgetBackend> ();
		List<Rectangle> childrenBounds = new List<Rectangle> ();

		public void SetChildBounds (IWidgetBackend widget, Rectangle bounds)
		{
			int i = children.IndexOf (widget);
			if (i == -1) {
				children.Add (widget);
				childrenBounds.Add (bounds);
			}
			else {
				childrenBounds[i] = bounds;
			}
			Canvas.SetAllocation (children.ToArray (), childrenBounds.ToArray ());
		}

		public void RemoveChild (IWidgetBackend widget)
		{
			UIElement element = widget.NativeWidget as UIElement;
			if (element == null)
				throw new ArgumentException ();

			Canvas.Children.Remove (element);
			int i = children.IndexOf (widget);
			if (i != -1) {
				children.RemoveAt (i);
				childrenBounds.RemoveAt (i);
			}
		}

		#endregion
	}
}
