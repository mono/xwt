using Xwt;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;

namespace MacTest
{
	class MainClass
	{
		static int Main (string [] args)
		{
			var assemblyName = Path.GetFileName(typeof(MainClass).Assembly.Location);
			var list = new List<string> (args.Where (arg => !arg.StartsWith ("-psn_", System.StringComparison.Ordinal) && !arg.EndsWith(assemblyName, System.StringComparison.Ordinal)));
			list.Add ("-domain=None");
			list.Add ("-noshadow");
			list.Add ("-nothread");

			if (!list.Contains (typeof (MainClass).Assembly.Location))
				list.Add (typeof (MainClass).Assembly.Location);

			bool skipImageVerification = list.Remove ("-no-image-verify");

			var res = NUnit.ConsoleRunner.Runner.Main (list.ToArray ());

			if (!skipImageVerification)
				ReferenceImageManager.ShowImageVerifier ();

			return res;
		}
	}
}	

