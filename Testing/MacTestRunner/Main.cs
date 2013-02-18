using System;
using Xwt;

namespace MacTest
{
	class MainClass
	{
		static void Main (string [] args)
		{
			Xwt.Application.Initialize (Xwt.ToolkitType.Cocoa);
			
			ConsoleTestRunner t = new ConsoleTestRunner ();
			t.Run (new string[0]);
		}
	}
}	

