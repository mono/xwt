//
// WpfDesktopBackend.cs
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
using SWF = System.Windows.Forms;
using System.Collections.Generic;

namespace Xwt.WPFBackend
{
	public class WpfDesktopBackend: DesktopBackend
	{
		public WpfDesktopBackend ()
		{
			Microsoft.Win32.SystemEvents.DisplaySettingsChanged += delegate
			{
				System.Windows.Application.Current.Dispatcher.BeginInvoke (new Action (OnScreensChanged));
			};
		}

		#region implemented abstract members of DesktopBackend

		public override IEnumerable<object> GetScreens ()
		{
			return SWF.Screen.AllScreens;
		}

		public override bool IsPrimaryScreen (object backend)
		{
			return ((SWF.Screen)backend) == SWF.Screen.PrimaryScreen;
		}

		public override Rectangle GetScreenBounds (object backend)
		{
			var r = ((SWF.Screen)backend).Bounds;
			return new Rectangle (r.X, r.Y, r.Width, r.Height);
		}

		public override Rectangle GetScreenVisibleBounds (object backend)
		{
			var r = ((SWF.Screen)backend).WorkingArea;
			return new Rectangle (r.X, r.Y, r.Width, r.Height);
		}

		public override string GetScreenDeviceName (object backend)
		{
			return ((SWF.Screen)backend).DeviceName;
		}

		#endregion
	}
}

