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
		/// Size and position of the window content in screen coordinates
		/// </summary>
		Rectangle Bounds { get; set; }
		void Move (double x, double y);

		/// <summary>
		/// Sets the size of the window
		/// </summary>
		/// <param name='width'>
		/// New width.
		/// </param>
		/// <param name='height'>
		/// New height.
		/// </param>
		/// <remarks>
		/// </remarks>
		void Resize (double width, double height);

		bool Visible { get; set; }
		string Title { get; set; }
		
		bool Decorated { get; set; }
		bool ShowInTaskbar { get; set; }
		void SetTransientFor (IWindowFrameBackend window);
		bool Resizable { get; set; }

		void SetIcon (ImageDescription image);
		
		/// <summary>
		/// Presents a window to the user. This may mean raising the window in the stacking order,
		/// deiconifying it, moving it to the current desktop, and/or giving it the keyboard focus
		/// </summary>
		void Present ();

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
	}
	
	public interface IWindowFrameEventSink
	{
		void OnBoundsChanged (Rectangle bounds);
		void OnShown ();
		void OnHidden ();
		bool OnCloseRequested ();
	}

	[Flags]
	public enum WindowFrameEvent
	{
		BoundsChanged = 1,
		Shown = 2,
		Hidden = 4,
		CloseRequested = 8
	}
}

