//
// NSApplicationInitializer.cs
//
// Author:
//       Lluis Sanchez Gual <lluis@xamarin.com>
//
// Copyright (c) 2016 Xamarin, Inc (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;

using AppKit;
using Foundation;
using ObjCRuntime;

namespace Xwt.Mac
{
	static class NSApplicationInitializer
	{
		static readonly object lockObject = new object();
		public static void Initialize ()
		{
			var ds = System.Threading.Thread.GetNamedDataSlot ("NSApplication.Initialized");
			if (System.Threading.Thread.GetData (ds) == null) {
				System.Threading.Thread.SetData (ds, true);

#if !NET7_0_OR_GREATER
				NSApplication.IgnoreMissingAssembliesDuringRegistration = true;
#endif

				// Setup a registration handler that does not let Xamarin.Mac register assemblies by default.
				Runtime.AssemblyRegistration += Runtime_AssemblyRegistration;

				NSApplication.Init ();

				// Register a callback when an assembly is loaded so it's registered in the xammac registrar.
				AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;

				// Manually register all the currently loaded assemblies.
				foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					Runtime.RegisterAssembly(assembly);
				}
			}
		}

		private static void Runtime_AssemblyRegistration(object sender, AssemblyRegistrationEventArgs args)
		{
			// If we don't do this, Xamarin.Mac will forcefully load all the assemblies referenced from the app startup assembly.
			args.Register = false;
		}

		private static void CurrentDomain_AssemblyLoad (object sender, AssemblyLoadEventArgs args)
		{
			try
			{
				if (args.LoadedAssembly.ReflectionOnly)
				{
					// Ignore. Code cannot be executed from reflection only assemblies. They will also fail
					// to be loaded by Xamarin.Mac's DynamicRegistrar when it calls assembly.GetTypes() without an
					// AppDomain.ReflectionOnlyAssemblyResolve event handler resolve the dependencies.
					return;
				}
				Runtime.RegisterAssembly(args.LoadedAssembly);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine("Error during static registrar initialization load {0}", e);
			}
		}
	}
}

