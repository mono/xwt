using System;
using Xwt.Drawing;
using Xwt.Engine;
using Xwt.Backends;

namespace Xwt
{
	public sealed class Popover : IDisposable
	{
		public enum Position {
			Top,
			Bottom,
			/*Left,
				Right*/
		}

		IPopoverBackend backend;
		Position arrowPosition;

		public event EventHandler Closed;

		public Popover (WindowFrame parent, Widget child, Position arrowPosition)
		{
			this.arrowPosition = arrowPosition;
			backend = WidgetRegistry.CreateBackend<IPopoverBackend> (GetType ());
			backend.Init ((IWindowFrameBackend) WidgetRegistry.GetBackend (parent),
			              (IWidgetBackend) WidgetRegistry.GetBackend (child), arrowPosition);
			backend.Closed += (sender, e) => {
				if (Closed != null)
					Closed (this, EventArgs.Empty);
			};
		}

		public void Run (Widget referenceWidget)
		{
			if (backend == null)
				throw new InvalidOperationException ("The Popover was disposed");
			var location = new Point (referenceWidget.ScreenBounds.Center.X, arrowPosition == Position.Top ? referenceWidget.ScreenBounds.Bottom : referenceWidget.ScreenBounds.Top);
			backend.Run (location);
		}

		public void Dispose ()
		{
			if (backend != null) {
				backend.Dispose ();
				backend = null;
			}
		}
	}
}

