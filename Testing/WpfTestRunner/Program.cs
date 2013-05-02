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
            //It seems like WPF MainThread stops when window that was first created closes
            //this is why we create dummyWindows here because otherwise 1st test window would close
            //and every next test would not work
            Window dummyWindow = new Window();
			ConsoleTestRunner t = new ConsoleTestRunner ();
			t.Run (args);
		}
	}
}
