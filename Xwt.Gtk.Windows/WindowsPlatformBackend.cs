//
// GtkWin32Engine.cs
//
// Author:
//       Lluis Sanchez Gual <lluis@xamarin.com>
//
// Copyright (c) 2014 Xamarin, Inc (http://www.xamarin.com)
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
using Xwt.GtkBackend;
using Xwt.Backends;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Xwt.Gtk.Windows
{
	public class WindowsPlatformBackend: GtkPlatformBackend
	{
		public override void Initialize (Backends.ToolkitEngineBackend toolit)
		{
			base.Initialize (toolit);
			toolit.RegisterBackend<DesktopBackend, GtkWindowsDesktopBackend> ();
			toolit.RegisterBackend<IWebViewBackend, WebViewBackend> ();
		}

		// Note: we can't reuse RectangleF because the layout is different...
		[StructLayout(LayoutKind.Sequential)]
		struct Rect
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;

			public int X { get { return Left; } }
			public int Y { get { return Top; } }
			public int Width { get { return Right - Left; } }
			public int Height { get { return Bottom - Top; } }
		}

		const int MonitorInfoFlagsPrimary = 0x01;

		[StructLayout(LayoutKind.Sequential)]
		unsafe struct MonitorInfo
		{
			public int Size;
			public Rect Frame;         // Monitor
			public Rect VisibleFrame;  // Work
			public int Flags;
			public fixed byte Device[32];
		}

		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		delegate int EnumMonitorsCallback(IntPtr hmonitor, IntPtr hdc, IntPtr prect, IntPtr user_data);

		[DllImport("User32.dll")]
		extern static int EnumDisplayMonitors(IntPtr hdc, IntPtr clip, EnumMonitorsCallback callback, IntPtr user_data);

		[DllImport("User32.dll")]
		extern static int GetMonitorInfoA(IntPtr hmonitor, ref MonitorInfo info);

		protected override Gdk.Rectangle OnGetScreenVisibleBounds(Gdk.Screen screen, int monitor_id)
		{
			Gdk.Rectangle geometry = screen.GetMonitorGeometry(monitor_id);
			List<MonitorInfo> screens = new List<MonitorInfo>();

			EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, delegate (IntPtr hmonitor, IntPtr hdc, IntPtr prect, IntPtr user_data) {
				var info = new MonitorInfo();

				unsafe
				{
					info.Size = sizeof(MonitorInfo);
				}

				GetMonitorInfoA(hmonitor, ref info);

				// In order to keep the order the same as Gtk, we need to put the primary monitor at the beginning.
				if ((info.Flags & MonitorInfoFlagsPrimary) != 0)
					screens.Insert(0, info);
				else
					screens.Add(info);

				return 1;
			}, IntPtr.Zero);

			MonitorInfo monitor = screens[monitor_id];
			Rect visible = monitor.VisibleFrame;
			Rect frame = monitor.Frame;

			// Rebase the VisibleFrame off of Gtk's idea of this monitor's geometry (since they use different coordinate systems)
			int x = geometry.X + (visible.Left - frame.Left);
			int width = visible.Width;

			int y = geometry.Y + (visible.Top - frame.Top);
			int height = visible.Height;

			return new Gdk.Rectangle(x, y, width, height);
		}
	}
}

