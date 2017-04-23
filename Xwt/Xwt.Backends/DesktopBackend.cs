//
// DesktopBackend.cs
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
using System.Collections.Generic;
using System.Diagnostics;

namespace Xwt.Backends
{
	public abstract class DesktopBackend: BackendHandler
	{
		/// <summary>
		/// Gets the current global mouse location.
		/// </summary>
		/// <returns>The mouse location.</returns>
		public abstract Point GetMouseLocation ();

		/// <summary>
		/// List of screens that compose the desktop
		/// </summary>
		public abstract IEnumerable<object> GetScreens ();

		/// <summary>
		/// Determines whether the provided screen is the primary screen
		/// </summary>
		/// <returns><c>true</c> if the screen is the primary screen; otherwise, <c>false</c>.</returns>
		/// <param name="backend">Screen backend</param>
		/// <remarks>The primary screen is considered the screen where the 'main desktop' lives.</remarks>
		public abstract bool IsPrimaryScreen (object backend);

		/// <summary>
		/// Gets the bounds of a screen, in desktop coordinates
		/// </summary>
		/// <returns>The screen bounds.</returns>
		/// <param name="backend">A screen backend</param>
		public abstract Rectangle GetScreenBounds (object backend);
		
		/// <summary>
		/// Gets the visible bounds of a screen (excluding dock area and other decorations), in desktop coordinates
		/// </summary>
		/// <returns>The screen bounds.</returns>
		/// <param name="backend">A screen backend</param>
		public abstract Rectangle GetScreenVisibleBounds (object backend);

		/// <summary>
		/// Gets the name of the screen device.
		/// </summary>
		/// <returns>The screen device name.</returns>
		/// <param name="backend">A screen backend</param>
		public abstract string GetScreenDeviceName (object backend);

		/// <summary>
		/// Gets the scale factor for the screen.
		/// </summary>
		/// <returns>The scale factor.</returns>
		/// <param name="backend">A screen backend</param>
		/// <remarks>The normal value is 1. In a retina display the value is 2.</remarks>
		public virtual double GetScaleFactor (object backend)
		{
			return 1d;
		}

		/// <summary>
		/// Raises the ScreensChanged event.
		/// </summary>
		/// <remarks>To be called by the subclass when there is a change in the configuration of screens</remarks>
		public void OnScreensChanged ()
		{
			ApplicationContext.InvokeUserCode (Desktop.NotifyScreensChanged);
		}

		public virtual void OpenFile (string filename)
		{
			Process.Start ("file://" + filename);
		}

		public virtual void OpenFolder (string folderPath)
		{
			Process.Start ("file://" + folderPath);
		}

		public virtual void OpenUrl (string url)
		{
			Process.Start (url);
		}

		public virtual object GetFileIcon (string fileName)
		{
			return null;
		}
	}
}

