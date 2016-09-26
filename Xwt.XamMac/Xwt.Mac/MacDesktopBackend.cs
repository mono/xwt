//
// MacDesktopBackend.cs
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

#if MONOMAC
using nint = System.Int32;
using nfloat = System.Single;
using CGPoint = System.Drawing.PointF;
using CGRect = System.Drawing.RectangleF;
using MonoMac.AppKit;
#else
using Foundation;
using AppKit;
using CoreGraphics;
#endif

namespace Xwt.Mac
{
	public class MacDesktopBackend: DesktopBackend
	{
		#region implemented abstract members of DesktopBackend

		internal static MacDesktopBackend Instance;
		internal static Rectangle desktopBounds;

		public MacDesktopBackend ()
		{
			Instance = this;
			CalcDesktopBounds ();
		}

		internal void NotifyScreensChanged ()
		{
			CalcDesktopBounds ();
			OnScreensChanged ();
		}

		static void CalcDesktopBounds ()
		{
			desktopBounds = new Rectangle ();
			foreach (var s in NSScreen.Screens) {
				var r = s.Frame;
				desktopBounds = desktopBounds.Union (new Rectangle (r.X, r.Y, r.Width, r.Height));
			}
		}

		public override Point GetMouseLocation ()
		{
			return ToDesktopPoint (NSEvent.CurrentMouseLocation);
		}

		public override IEnumerable<object> GetScreens ()
		{
			return NSScreen.Screens;
		}

		public override bool IsPrimaryScreen (object backend)
		{
			return NSScreen.Screens[0] == (NSScreen) backend;
		}

		public static Point ToDesktopPoint (CGPoint loc)
		{
			var result = new Point (loc.X, desktopBounds.Height - loc.Y);
			if (desktopBounds.Y < 0)
				result.Y += desktopBounds.Y;
			return result;
		}

		public static Rectangle ToDesktopRect (CGRect r)
		{
			r.Y = (nfloat)desktopBounds.Height - r.Y - r.Height;
			if (desktopBounds.Y < 0)
				r.Y += (nfloat)desktopBounds.Y;
			return new Rectangle (r.X, r.Y, r.Width, r.Height);
		}

		public static CGRect FromDesktopRect (Rectangle r)
		{
			r.Y = (float)desktopBounds.Height - r.Y - r.Height;
			if (desktopBounds.Y < 0)
				r.Y += (float)desktopBounds.Y;
			return new CGRect ((nfloat)r.X, (nfloat)r.Y, (nfloat)r.Width, (nfloat)r.Height);
		}
		
		public override Rectangle GetScreenBounds (object backend)
		{
			var r = ((NSScreen)backend).Frame;
			return ToDesktopRect (r);
		}

		public override Rectangle GetScreenVisibleBounds (object backend)
		{
			var r = ((NSScreen)backend).VisibleFrame;
			return ToDesktopRect (r);
		}

		public override string GetScreenDeviceName (object backend)
		{
			return ((NSScreen)backend).DeviceDescription ["NSScreenNumber"].ToString ();
		}

		public override double GetScaleFactor (object backend)
		{
			return ((NSScreen)backend).BackingScaleFactor;
		}
		
		#endregion
	}
}

