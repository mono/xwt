// 
// ButtonEventArgs.cs
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
	/// Button event arguments, containing information about pressed/released mouse buttons.
	/// </summary>
	public class ButtonEventArgs: EventArgs
	{
		public ButtonEventArgs ()
		{
		}

		/// <summary>
		/// Gets or sets the button.
		/// </summary>
		/// <value>The button.</value>
		public PointerButton Button { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this event has been handled
		/// </summary>
		/// <value><c>true</c> if this event has been handled; otherwise, <c>false</c>.</value>
		/// <remarks>
		/// Setting <see cref="Xwt.ButtonEventArgs.Handled"/> to <c>true</c> will mark the event
		/// as handled and prevent the event from bubbling up in the widget hierarchy.
		/// Handled events will be ignored by a backend in most cases.
		/// For example marking a left button press of a <see cref="Xwt.Button"/> as handled will prevent
		/// the backend from activating the button.
		/// </remarks>
		public bool Handled { get; set; }
		
		/// <summary>
		/// Gets or sets the X coordinate of the mouse cursor (relative to the widget receiving the event).
		/// </summary>
		/// <value>The X coordinate of the cursor.</value>
		public double X { get; set; }

		/// <summary>
		/// Gets or sets the Y coordinate of the mouse cursor (relative to the widget receiving the event).
		/// </summary>
		/// <value>The Y coordinate of the cursor.</value>
		public double Y { get; set; }

		/// <summary>
		/// Location of the mouse cursor (in widget coordinates).
		/// </summary>
		public Point Position {
			get { return new Point (X, Y); }
		}

		/// <summary>
		/// Gets the number of clicks (e.g. 2 for double-click)
		/// </summary>
		/// <value>The number of the presses of the same button.</value>
		public int MultiplePress { get; set; }
	}
}

