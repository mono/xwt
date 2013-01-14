//
// Desktop.cs
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
using System.Linq;
using System.Collections.ObjectModel;

namespace Xwt
{
	/// <summary>
	/// Provides information about the screens that compose the desktop
	/// </summary>
	public static class Desktop
	{
		static Screen[] screens;
		static Screen primary;

		/// <summary>
		/// Occurs when there is some change in the geometry of the screens
		/// </summary>
		public static event EventHandler ScreensChanged;

		internal static void NotifyScreensChanged ()
		{
			screens = null;
			if (ScreensChanged != null)
				ScreensChanged (null, EventArgs.Empty);
		}

		/// <summary>
		/// List of screens that compose the desktop
		/// </summary>
		/// <value>The screens.</value>
		public static ReadOnlyCollection<Screen> Screens {
			get {
				if (screens == null) {
					screens = Toolkit.CurrentEngine.DesktopBackend.GetScreens ().Select (s => new Screen (s)).ToArray ();
					primary = screens.FirstOrDefault (s => s.IsPrimary);
				}
				return new ReadOnlyCollection<Screen> (screens);
			}
		}

		/// <summary>
		/// Gets the primary screen.
		/// </summary>
		/// <remarks>
		/// The primary screen is considered the screen where the 'main desktop' lives.
		/// </remarks>
		public static Screen PrimaryScreen {
			get {
				return primary;
			}
		}

		/// <summary>
		/// Bounds of the desktop
		/// </summary>
		/// <remarks>
		/// The bounds of the desktop is the union of the areas of all screens
		/// </remarks>
		public static Rectangle Bounds {
			get {
				Rectangle r = new Rectangle (0,0,0,0);
				foreach (var s in Screens)
					r = r.Union (s.Bounds);
				return r;
			}
		}

		/// <summary>
		/// Gets the screen at location.
		/// </summary>
		/// <returns>The screen at location.</returns>
		/// <param name="p">A point</param>
		public static Screen GetScreenAtLocation (Point p)
		{
			return GetScreenAtLocation (p.X, p.Y);
		}

		/// <summary>
		/// Gets the screen at location.
		/// </summary>
		/// <returns>The screen at location.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public static Screen GetScreenAtLocation (double x, double y)
		{
			return screens.FirstOrDefault (s => s.Bounds.Contains (x, y));
		}

		static Screen GetScreen (object sb)
		{
			foreach (var s in Screens) {
				var backend = Toolkit.GetBackend (s);
				if (backend == sb || backend.Equals (sb))
					return s;
			}
			return null;
		}
	}
}

