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

using AppKit;

namespace Xwt.Mac
{
	static class NSApplicationInitializer
	{
		public static void Initialize ()
		{
			var ds = System.Threading.Thread.GetNamedDataSlot ("NSApplication.Initialized");
			if (System.Threading.Thread.GetData (ds) == null) {
				System.Threading.Thread.SetData (ds, true);

				// IgnoreMissingAssembliesDuringRegistration is only avalilable in Xamarin.Mac 3.4+
				// Use reflection to not break builds with older Xamarin.Mac
				var ignoreMissingAssemblies = typeof (NSApplication).GetField ("IgnoreMissingAssembliesDuringRegistration",
				                                                               System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
				ignoreMissingAssemblies?.SetValue (null, true);
				NSApplication.Init ();
			}
		}
	}
}

