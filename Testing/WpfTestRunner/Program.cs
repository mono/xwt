using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xwt;

namespace WpfTestRunner
{
	class Program
	{
		[STAThread]
		static void Main (string[] args)
		{
			Xwt.Application.Initialize (Xwt.ToolkitType.Wpf);

			ConsoleTestRunner t = new ConsoleTestRunner ();
			t.Run (args);
		}
	}
}
