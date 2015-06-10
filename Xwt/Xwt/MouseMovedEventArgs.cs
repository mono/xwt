// 
// MouseMovedEventArgs.cs
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

namespace Xwt
{
	/// <summary>
	/// Mouse moved event arguments, containing information about mouse movements.
	/// </summary>
	public class MouseMovedEventArgs: EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Xwt.MouseMovedEventArgs"/> class.
		/// </summary>
		/// <param name="timestamp">The timestamp of the event.</param>
		/// <param name="x">The X coordinate of the cursor.</param>
		/// <param name="y">The Y coordinate of the cursor.</param>
		public MouseMovedEventArgs (long timestamp, double x, double y)
		{
			X = x;
			Y = y;
			Timestamp  = timestamp;
		}
		
		/// <summary>
		/// Gets or sets a value indicating whether this event has been handled
		/// </summary>
		/// <value><c>true</c> if handled; otherwise, <c>false</c>.</value>
		/// <remarks>
		/// Setting <see cref="Xwt.MouseMovedEventArgs.Handled"/> to <c>true</c> will mark the event
		/// as handled and prevent the event from bubbling up in the widget hierarchy.
		/// Handled events will be ignored by a backend in most cases.
		/// </remarks>
		public bool Handled { get; set; }

		/// <summary>
		/// Gets or sets the X coordinate of the mouse cursor (relative to the widget receiving the event).
		/// </summary>
		/// <value>The X coordinate of the cursor.</value>
		public double X { get; private set; }

		/// <summary>
		/// Gets or sets the Y coordinate of the mouse cursor (relative to the widget receiving the event).
		/// </summary>
		/// <value>The Y coordinate of the cursor.</value>
		public double Y { get; private set; }

		/// <summary>
		/// Location of the mouse cursor (in widget coordinates).
		/// </summary>
		public Point Position {
			get { return new Point (X, Y); }
		}

		/// <summary>
		/// Gets the time when this event occurred.
		/// </summary>
		/// <value>The timestamp of the event.</value>
		public long Timestamp { get; private set; }
	}
}

