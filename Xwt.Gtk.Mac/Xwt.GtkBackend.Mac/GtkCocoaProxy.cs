using System;

using Gtk;
using MonoMac.AppKit;

namespace Xwt.GtkBackend.Mac
{
	public class GtkCocoaProxy : Gtk.Widget
	{
		public GtkCocoaProxy (NSView view) : base (GtkMacInterop.NSViewToGtkWidgetPtr (view))
		{
			OriginalView = view;
		}

		public NSView OriginalView {
			get;
			private set;
		}
	}
}

