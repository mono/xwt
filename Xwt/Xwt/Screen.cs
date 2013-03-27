//
// Screen.cs
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

namespace Xwt
{
	/// <summary>
	/// Object representing a physical screen
	/// </summary>
	public class Screen: XwtObject
	{
		internal Screen (object backend): this (backend, null)
		{
		}
		
		internal Screen (object backend, Toolkit toolkit): base (backend, toolkit)
		{
		}

		/// <summary>
		/// Bounds of the screen, including the dock area (and menu bar on Mac) 
		/// </summary>
		/// <value>The bounds.</value>
		public Rectangle Bounds {
			get {
				return ToolkitEngine.DesktopBackend.GetScreenBounds (Backend);
			}
		}

		/// <summary>
		/// Bounds of the screen, not including the dock area (or menu bar on Mac) 
		/// </summary>
		public Rectangle VisibleBounds {
			get {
				return ToolkitEngine.DesktopBackend.GetScreenVisibleBounds (Backend);
			}
		}

		/// <summary>
		/// Gets the name of the device.
		/// </summary>
		/// <value>The name of the device.</value>
		public string DeviceName {
			get {
				return ToolkitEngine.DesktopBackend.GetScreenDeviceName (Backend);
			}
		}

		/// <summary>
		/// Gets a value indicating whether this screen is the primary screen
		/// </summary>
		/// <value><c>true</c> if this instance is the primary screen; otherwise, <c>false</c>.</value>
		/// <remarks>The primary screen is considered the screen where the 'main desktop' lives.</remarks>
		public bool IsPrimary {
			get {
				return ToolkitEngine.DesktopBackend.IsPrimaryScreen (Backend);
			}
		}

		/// <summary>
		/// Gets the screen scale factor.
		/// </summary>
		/// <value>This is the scale of user space pixels in relation to phisical pixels. The normal value is 1. In a retina display the value is 2.</value>
		public double ScaleFactor {
			get {
				return ToolkitEngine.DesktopBackend.GetScaleFactor (Backend);
			}
		}
	}
}

