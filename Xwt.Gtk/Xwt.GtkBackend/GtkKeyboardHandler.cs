using System;
using Xwt.Backends;

namespace Xwt.GtkBackend
{
	public class GtkKeyboardHandler: KeyboardHandler
	{
		public override ModifierKeys CurrentModifiers {
			get {
				Gdk.ModifierType mtype;
				Gtk.Global.GetCurrentEventState(out mtype);
				return mtype.ToXwtValue ();
			}
		}
	}
}

