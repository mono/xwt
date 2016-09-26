// 
// IWindowFrameBackend.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2011 Xamarin Inc
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
using Xwt.Drawing;

namespace Xwt.Backends
{
	public interface IWindowFrameBackend: IBackend
	{
		void Initialize (IWindowFrameEventSink eventSink);
		void Dispose ();

		/// <summary>
		/// Gets or sets the name of the window.
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// Size and position of the window content in screen coordinates
		/// </summary>
		Rectangle Bounds { get; set; }
		void Move (double x, double y);

		/// <summary>
		/// Sets the size of the window
		/// </summary>
		/// <param name='width'>
		/// New width, or -1 if the width doesn't have to be changed
		/// </param>
		/// <param name='height'>
		/// New height, or -1 if the height doesn't have to be changed
		/// </param>
		/// <remarks>
		/// </remarks>
		void SetSize (double width, double height);

		bool Visible { get; set; }
		bool Sensitive { get; set; }
		string Title { get; set; }		
		bool Decorated { get; set; }
		bool ShowInTaskbar { get; set; }
		void SetTransientFor (IWindowFrameBackend window);
		bool Resizable { get; set; }
		double Opacity { get; set; }

		void SetIcon (ImageDescription image);
		
		/// <summary>
		/// Presents a window to the user. This may mean raising the window in the stacking order,
		/// deiconifying it, moving it to the current desktop, and/or giving it the keyboard focus
		/// </summary>
		void Present ();

		/// <summary>
		/// Closes the window
		/// </summary>
		/// <returns><c>true</c> if the window could be closed</returns>
		/// <remarks>
		/// Closes the window like if the user clicked on the close window button.
		/// The CloseRequested event is fired and subscribers can cancel the closing,
		/// so there is no guarantee that the window will actually close.
		/// This method doesn't dispose the window. The Dispose method has to be called.
		/// </remarks>
		bool Close ();

		/// <summary>
		/// Gets or sets a value indicating whether this window is in full screen mode
		/// </summary>
		/// <value><c>true</c> if the window is in full screen mode; otherwise, <c>false</c>.</value>
		bool FullScreen { get; set; }

		/// <summary>
		/// Gets the screen on which most of the area of this window is placed
		/// </summary>
		/// <value>The screen.</value>
		object Screen { get; }

		/// <summary>
		/// Gets the reference to the native window.
		/// </summary>
		/// <value>The native window.</value>
		object Window { get; }

		/// <summary>
		/// Gets the system handle of the native Window.
		/// </summary>
		/// <value>The native handle.</value>
		/// <remarks>
		/// The native handle is the platform specific (Cocoa, X, Win32, etc.) window handle,
		/// which is not necessarily the handle of the toolkit window.
		/// </remarks>
		IntPtr NativeHandle { get; }
	}
	
	public interface IWindowFrameEventSink
	{
		void OnBoundsChanged (Rectangle bounds);
		void OnShown ();
		void OnHidden ();
		bool OnCloseRequested ();
		void OnClosed ();
	}

	[Flags]
	public enum WindowFrameEvent
	{
		BoundsChanged = 1,
		Shown = 2,
		Hidden = 4,
		CloseRequested = 8,
		Closed = 16
	}
}

