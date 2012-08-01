//
// GtkMacEngine.cs
//
// Author:
//       Jérémie Laval <jeremie.laval@xamarin.com>
//
// Copyright (c) 2012 Xamarin, Inc.
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
using Xwt.Engine;
using Xwt.Backends;

namespace Xwt.GtkBackend.Mac
{
	public class GtkMacEngine : Xwt.GtkBackend.GtkEngine
	{
		public static Xwt.Backends.EngineBackend MacEngine {
			get;
			set;
		}

		public static WidgetRegistry MacWidgetRegistry {
			get;
			set;
		}

		public override void InitializeRegistry (WidgetRegistry registry)
		{
			Console.WriteLine ("Using GtkMac backend");
			// We let Gtk engine register its types first
			base.InitializeRegistry (registry);
			// Then we overwrite the custom one we have
			registry.RegisterBackend (typeof(Xwt.Popover), typeof (PopoverMacBackend));
			// Finally we initialize a mac registry to get their widgets
			MacWidgetRegistry = new WidgetRegistry ();
			MacEngine = new Xwt.Mac.MacEngine ();
			MacEngine.InitializeRegistry (MacWidgetRegistry);
		}
	}
}

