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
			var list = new List<string> (args);
			list.Add ("-domain=None");
			list.Add ("-noshadow");
			list.Add ("-nothread");
			if (!list.Contains (typeof (Program).Assembly.Location))
				list.Add (typeof (Program).Assembly.Location);
			NUnit.ConsoleRunner.Runner.Main (list.ToArray ());
			ReferenceImageManager.ShowImageVerifier ();
		}
	}
}
