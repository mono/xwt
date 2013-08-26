//
// GtkDesktopBackend.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc.
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
using Xwt.Backends;
using System.Collections.Generic;

namespace Xwt.GtkBackend
{
	public class GtkDesktopBackend: DesktopBackend
	{
		#region implemented abstract members of DesktopBackend

		public GtkDesktopBackend ()
		{
			Gdk.Screen.Default.SizeChanged += delegate {
				OnScreensChanged ();
			};
			Gdk.Screen.Default.CompositedChanged += delegate {
				OnScreensChanged ();
			};
		}

		public override Point GetMouseLocation ()
		{
			int x, y;
			Gdk.Display.Default.GetPointer (out x, out y);
			return new Point (x, y);
		}

		public override IEnumerable<object> GetScreens ()
		{
	         for (int n=0; n<Gdk.Screen.Default.NMonitors; n++)
				yield return n;
		}

		public override bool IsPrimaryScreen (object backend)
		{
			if (Platform.IsMac)
				return (int)backend == 0;
			else
				return (int)backend == Gdk.Screen.Default.GetMonitorAtPoint (0, 0);
		}

		public override Rectangle GetScreenBounds (object backend)
		{
			var r = Gdk.Screen.Default.GetMonitorGeometry ((int)backend);
			return new Rectangle (r.X, r.Y, r.Width, r.Height);
		}

		public override Rectangle GetScreenVisibleBounds (object backend)
		{
			var r = Gdk.Screen.Default.GetUsableMonitorGeometry ((int)backend);
			return new Rectangle (r.X, r.Y, r.Width, r.Height);
		}

		public override string GetScreenDeviceName (object backend)
		{
			return backend.ToString ();
		}

		public override double GetScaleFactor (object backend)
		{
			return Gdk.Screen.Default.GetScaleFactor ((int)backend);
		}

		#endregion
	}
}

