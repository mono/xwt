using System;
using Xwt;
using System.IO;
using System.Collections.Generic;

namespace MacTest
{
	class MainClass
	{
		static void Main (string [] args)
		{
			Xwt.Application.Initialize (Xwt.ToolkitType.Cocoa);
			ReferenceImageManager.Init ("MacTestRunner");

			var list = new List<string> (args);
			list.Add ("-domain=None");
			list.Add ("-noshadow");
			list.Add ("-nothread");
			NUnit.ConsoleRunner.Runner.Main (list.ToArray ());
			ReferenceImageManager.ShowImageVerifier ();
		}
	}
}	

