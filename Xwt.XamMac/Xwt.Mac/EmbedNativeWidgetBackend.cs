using AppKit;
using Xwt.Backends;

namespace Xwt.Mac
{
	public class EmbedNativeWidgetBackend : ViewBackend, IEmbeddedWidgetBackend
	{
		NSView innerView;

		public EmbedNativeWidgetBackend ()
		{

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

		public void SetContent (object nativeWidget)
		{
			if (nativeWidget is NSView) {
				if (ViewObject == null)
					innerView = (NSView)nativeWidget;
				else
					SetNativeView ((NSView)nativeWidget);
			}
		}

		void SetNativeView (NSView aView)
		{
			if (innerView != null)
				innerView.RemoveFromSuperview ();
			innerView = aView;
			innerView.Frame = Widget.Bounds;

			innerView.AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable;
			innerView.TranslatesAutoresizingMaskIntoConstraints = true;
			Widget.AutoresizesSubviews = true;

			Widget.AddSubview (innerView);
		}
	}
}

