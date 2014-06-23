using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xwt
{
	public class PreviewTextInputEventArgs : EventArgs
	{
		public bool Handled { get; set; }
		public string Text { get; private set; }

		public PreviewTextInputEventArgs(string text)
		{
			Text = text;
		}
	}
}
