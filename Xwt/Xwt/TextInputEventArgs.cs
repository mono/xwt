using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xwt
{
	public class TextInputEventArgs : EventArgs
	{
		public bool Handled { get; set; }
		public string Text { get; private set; }

		public TextInputEventArgs(string text)
		{
			Text = text;
		}
	}
}
