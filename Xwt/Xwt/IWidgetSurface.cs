// 
// IWidgetSurface.cs
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
using System.Collections.Generic;


namespace Xwt
{
	/// <summary>
	/// A widget surface handles the sizing and allocation of a widget.
	/// </summary>
	public interface IWidgetSurface
	{
		/// <summary>
		/// Relocates the children of the widget, to fit a new size.
		/// </summary>
		/// <remarks>Must be called after changing the size of the widget, to relocate its children.</remarks>
		void Reallocate ();
		
		/// <summary>
		/// Gets the preferred size of the widget
		/// </summary>
		/// <returns>
		/// The preferred size.
		/// </returns>
		/// <remarks>
		/// The returned size is >= 0. If a constraint is specified, the returned size will not
		/// be bigger than the constraint.
		/// </remarks>
		Size GetPreferredSize (SizeConstraint widthConstraint, SizeConstraint heightConstraint, bool includeMargin = false);
		
		/// <summary>
		/// Gets the preferred size of the widget
		/// </summary>
		/// <returns>
		/// The preferred size.
		/// </returns>
		/// <remarks>
		/// The returned size is >= 0
		/// </remarks>
		Size GetPreferredSize (bool includeMargin = false);

		/// <summary>
		/// Gets the native toolkit widget.
		/// </summary>
		/// <value>The native widget.</value>
		object NativeWidget { get; }

		/// <summary>
		/// Gets the children of the widget.
		/// </summary>
		/// <value>The children widgets.</value>
		IEnumerable<Widget> Children { get; }

		/// <summary>
		/// Gets the current toolkit engine.
		/// </summary>
		/// <value>The toolkit engine.</value>
		Toolkit ToolkitEngine { get; }

		/// <summary>
		/// Given a rectangle that defines the available area for a widget, returns the area that the widget will fill,
		/// taking into account the margin and alignment settings.
		/// </summary>
		/// <returns>The placement in rect.</returns>
		/// <param name="rect">Rect.</param>
		Rectangle GetPlacementInRect (Rectangle rect);
	}
}

