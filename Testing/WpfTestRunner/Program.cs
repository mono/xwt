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
		static int Main (string[] args)
		{
			var list = new List<string> (args);
			list.Add ("-domain=None");
			list.Add ("-noshadow");
			list.Add ("-nothread");
			if (!list.Contains (typeof (Program).Assembly.Location))
				list.Add (typeof (Program).Assembly.Location);
			
			bool skipImageVerification = list.Remove ("-no-image-verify");

			var res = NUnit.ConsoleRunner.Runner.Main (list.ToArray ());

			if (!skipImageVerification)
				ReferenceImageManager.ShowImageVerifier ();

			return res;
		}
	}
}
