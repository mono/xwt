using System;
using Xwt;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace MacTest
{
	class MainClass
	{
		static void Main (string [] args)
		{
			var list = new List<string> (args);
			list.Add ("-domain=None");
			list.Add ("-noshadow");
			list.Add ("-nothread");
			//			if (!list.Contains (typeof (MainClass).Assembly.Location))
			//	list.Add (typeof (MainClass).Assembly.Location);
			NUnit.ConsoleRunner.Runner.Main (list.ToArray ());
			ReferenceImageManager.ShowImageVerifier ();
		}
	}
}	

