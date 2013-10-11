using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xwt.Backends;

namespace Xwt.WPFBackend
{
	class WpfKeyboardHandler: KeyboardHandler
	{
		public override ModifierKeys CurrentModifiers
		{
			get { return KeyboardUtil.GetModifiers (); }
		}
	}
}
