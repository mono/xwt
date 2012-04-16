// 
// ICanvasBackend.cs
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

namespace Xwt.Backends
{
	public interface ICanvasBackend: IWidgetBackend
	{
		/// <summary>
		/// Invalidates and forces the redraw of the whole area of the widget
		/// </summary>
		void QueueDraw ();
		
		/// <summary>
		/// Invalidates and forces the redraw of a rectangular area of the widget
		/// </summary>
		void QueueDraw (Rectangle rect);
		
		/// <summary>
		/// Adds a child widget.
		/// </summary>
		/// <param name='widget'>
		/// The widget
		/// </param>
		/// <param name='bounds'>
		/// Position and size of the child widget, in container coordinates
		/// </param>
		/// <remarks>
		/// The widget is placed in the canvas area, in the specified coordinates and
		/// using the specified size
		/// </remarks>
		void AddChild (IWidgetBackend widget, Rectangle bounds);
		
		/// <summary>
		/// Sets the position and size of a child widget
		/// </summary>
		/// <param name='widget'>
		/// The child widget. It must have been added using the AddChild method.
		/// </param>
		/// <param name='bounds'>
		/// New bounds, in container coordinates
		/// </param>
		/// <exception cref="System.InvalidOperationException">If the widget is not a child of this canvas</exception>
		void SetChildBounds (IWidgetBackend widget, Rectangle bounds);
		
		/// <summary>
		/// Removes a child widget
		/// </summary>
		/// <param name='widget'>
		/// The widget to remove. The widget must have been previously added using AddChild.
		/// </param>
		/// <exception cref="System.InvalidOperationException">If the widget is not a child of this canvas</exception>
		void RemoveChild (IWidgetBackend widget);
	}
	
	public interface ICanvasEventSink: IWidgetEventSink
	{
		/// <summary>
		/// Raises the draw event.
		/// </summary>
		/// <param name='context'>
		/// Drawing context
		/// </param>
		void OnDraw (object context, Rectangle dirtyRect);
	}
}

