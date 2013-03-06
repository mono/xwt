using System;
using Xwt;
using System.IO;

namespace MacTest
{
	class MainClass
	{
		static void Main (string [] args)
		{
			Xwt.Application.Initialize (Xwt.ToolkitType.Cocoa);

			var baseDir = Path.GetDirectoryName (System.Reflection.Assembly.GetEntryAssembly ().Location);
			ReferenceImageManager.ProjectCustomReferenceImageDir = baseDir + "/../../../../../ReferenceImages";

			ConsoleTestRunner t = new ConsoleTestRunner ();
			t.Run (new string[0]);
		}
	}
}	

