//
// GtkMacEngine.cs
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
using AppKit;
using Xwt.Mac;
using System.Collections.Generic;
using CoreGraphics;

namespace Xwt.Gtk.Mac
{
	public class MacPlatformBackend: GtkPlatformBackend
	{

		public override void Initialize (ToolkitEngineBackend toolit)
		{
			toolit.RegisterBackend <DesktopBackend,GtkMacDesktopBackend> ();
			toolit.RegisterBackend <FontBackendHandler,GtkMacFontBackendHandler> ();
			toolit.RegisterBackend <KeyboardHandler, GtkMacKeyboardHandler>();
			toolit.RegisterBackend <IAccessibleBackend, GtkMacAccessibleBackend> ();
			toolit.RegisterBackend <IOpenFileDialogBackend, GtkMacOpenFileDialogBackend> ();
			toolit.RegisterBackend <IPopoverBackend,GtkMacPopoverBackend> ();
			toolit.RegisterBackend <ISaveFileDialogBackend, GtkMacSaveFileDialogBackend> ();
			toolit.RegisterBackend <ISelectFolderDialogBackend, GtkMacSelectFolderBackend> ();
			toolit.RegisterBackend <IWebViewBackend,WebViewBackend> ();
			toolit.RegisterBackend <IWindowFrameBackend, GtkMacWindowFrameBackend> ();
		}

		public override Type GetBackendImplementationType (Type backendType)
		{
			if (backendType == typeof (IAccessibleBackend) ||
			    backendType == typeof (IOpenFileDialogBackend) ||
			    backendType == typeof (ISaveFileDialogBackend) ||
			    backendType == typeof (ISelectFolderDialogBackend) ||
			    backendType == typeof (IWebViewBackend) ||
			    backendType == typeof (IWindowFrameBackend))
			{
				Xwt.Mac.NSApplicationInitializer.Initialize ();
			}
			return base.GetBackendImplementationType (backendType);
		}

		protected override Gdk.Rectangle OnGetScreenVisibleBounds(Gdk.Screen screen, int monitor)
		{
			var screens = NSScreen.Screens;
			if ((uint)monitor >= screens.Length)
				return base.GetScreenVisibleBounds(screen, monitor);

			var macScreen = screens[monitor];
			CGRect visible = macScreen.VisibleFrame;
			CGRect frame = macScreen.Frame;

			Gdk.Rectangle ygeometry = base.GetScreenVisibleBounds(screen, monitor);
			Gdk.Rectangle xgeometry = base.GetScreenVisibleBounds(screen, 0);

			// Note: Frame and VisibleFrame rectangles are relative to monitor 0, but we need absolute
			// coordinates.
			visible.X += xgeometry.X;
			frame.X += xgeometry.X;

			// VisibleFrame.Y is the height of the Dock if it is at the bottom of the screen, so in order
			// to get the menu height, we just figure out the difference between the visibleFrame height
			// and the actual frame height, then subtract the Dock height.
			//
			// We need to swap the Y offset with the menu height because our callers expect the Y offset
			// to be from the top of the screen, not from the bottom of the screen.
			double x, y, width, height;

			if (visible.Height < frame.Height)
			{
				double dockHeight = visible.Y - frame.Y;
				double menubarHeight = (frame.Height - visible.Height) - dockHeight;

				height = frame.Height - menubarHeight - dockHeight;
				y = ygeometry.Y + menubarHeight;
			}
			else
			{
				height = frame.Height;
				y = ygeometry.Y;
			}

			// Takes care of the possibility of the Dock being positioned on the left or right edge of the screen.
			width = Math.Min(visible.Width, frame.Width);
			x = Math.Max(visible.X, frame.X);

			return new Gdk.Rectangle((int)x, (int)y, (int)width, (int)height);
		}

		public override void RequestUserAttention(bool critical)
		{
			NSRequestUserAttentionType kind = critical ? NSRequestUserAttentionType.CriticalRequest : NSRequestUserAttentionType.InformationalRequest;
			NSApplication.SharedApplication.RequestUserAttention(kind);
		}

		public override void GrabDesktopFocus ()
		{
			NSApplication.SharedApplication.ActivateIgnoringOtherApps (flag: true);
		}
	}
}

