using AppKit;
using Xwt.Backends;

namespace Xwt.Mac
{
	public class EmbedNativeWidgetBackend : ViewBackend, IEmbeddedWidgetBackend
	{
		NSView innerView;
		bool reparent;

		public EmbedNativeWidgetBackend ()
		{

		}

		public NSView EmbeddedView {
			get { return innerView; }
		}

		public override void Initialize ()
		{
			ViewObject = new WidgetView (EventSink, ApplicationContext);
			if (innerView != null) {
				var aView = innerView;
				innerView = null;

				SetNativeView (aView);
			}
		}

		public void SetContent (object nativeWidget, bool reparent)
		{
			if (nativeWidget is NSView) {
				this.reparent = reparent;
				if (ViewObject == null)
					innerView = (NSView)nativeWidget;
				else
					SetNativeView ((NSView)nativeWidget);
			}
		}

		void SetNativeView (NSView aView)
		{
			if (innerView != null && reparent)
				innerView.RemoveFromSuperview ();
			
			innerView = aView;
			if (!reparent)
				return;
			
			innerView.Frame = Widget.Bounds;

			innerView.AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable;
			innerView.TranslatesAutoresizingMaskIntoConstraints = true;
			Widget.AutoresizesSubviews = true;

			Widget.AddSubview (innerView);
		}
	}
}

