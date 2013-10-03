using System;

namespace Xwt.Backends
{
	public abstract class KeyboardHandler
	{
		public abstract ModifierKeys CurrentModifiers { get; }
	}
}

