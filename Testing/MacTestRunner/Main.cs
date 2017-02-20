using Xwt;
using System.Collections.Generic;

namespace MacTest
{
	class MainClass
	{
		static int Main (string [] args)
		{
			//FIXME: remove this once mmp summorts xammac
			ObjCRuntime.Dlfcn.dlopen ("/Library/Frameworks/Xamarin.Mac.framework/Versions/Current/lib/libxammac.dylib", 0);

			var list = new List<string> (args);
			list.Add ("-domain=None");
			list.Add ("-noshadow");
			list.Add ("-nothread");
			//			if (!list.Contains (typeof (MainClass).Assembly.Location))
			//	list.Add (typeof (MainClass).Assembly.Location);

			bool skipImageVerification = list.Remove ("-no-image-verify");

			var res = NUnit.ConsoleRunner.Runner.Main (list.ToArray ());

			if (!skipImageVerification)
				ReferenceImageManager.ShowImageVerifier ();

			return res;
		}
	}
}	

