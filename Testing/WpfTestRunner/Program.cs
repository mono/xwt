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
			ReferenceImageManager.Init ("GtkTestRunner");

			var list = new List<string> (args);
			list.Add ("-domain=None");
			list.Add ("-noshadow");
			list.Add ("-nothread");
			list.Add ("-xml=result.xml");
//			list.Add ("-fixture=Xwt.WindowTests");
//			list.Add ("-run=Xwt.WindowTests.DefaultSize");
			list.Add (typeof (Program).Assembly.Location);
			NUnit.ConsoleRunner.Runner.Main (list.ToArray ());
			ReferenceImageManager.ShowImageVerifier ();
		}
	}
}
