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
			ReferenceImageManager.Init ("WpfTestRunner");

			var list = new List<string> (args);
			list.Add ("-domain=None");
			list.Add ("-noshadow");
			list.Add ("-nothread");
			list.Add ("-xml=result.xml");
//			list.Add ("/run=Xwt.NinePatchTests");
			list.Add (typeof (Program).Assembly.Location);
			NUnit.ConsoleRunner.Runner.Main (list.ToArray ());
			ReferenceImageManager.ShowImageVerifier ();
		}
	}
}
