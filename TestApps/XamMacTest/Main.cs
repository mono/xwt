using Xwt;
using Samples;

namespace MacTest
{
	class MainClass
	{
		static void Main (string [] args)
		{
			//FIXME: remove this once mmp summorts xammac
			ObjCRuntime.Dlfcn.dlopen ("/Library/Frameworks/Xamarin.Mac.framework/Versions/Current/lib/libxammac.dylib", 0);

			App.Run (ToolkitType.XamMac);
		}
	}
}	

