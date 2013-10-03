using System;

namespace Xwt
{
	public static class Keyboard
	{
		public static ModifierKeys CurrentModifiers {
			get { return Toolkit.CurrentEngine.KeyboardHandler.CurrentModifiers; }
		}
	}
}

