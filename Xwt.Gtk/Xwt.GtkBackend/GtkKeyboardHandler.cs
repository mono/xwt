using System;
using Xwt.Backends;

namespace Xwt.GtkBackend
{
	public class GtkKeyboardHandler: KeyboardHandler
	{
		public override ModifierKeys CurrentModifiers {
			get {
				return GtkWorkarounds.GetCurrentKeyModifiers ().ToXwtValue ();
			}
		}
	}
}

