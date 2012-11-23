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
	public interface IWidgetSurface
	{
		void Reallocate ();
		
		SizeRequestMode SizeRequestMode { get; }
		
		/// <summary>
		/// Gets the preferred width of the widget (includes the margin)
		/// </summary>
		/// <returns>
		/// The preferred width.
		/// </returns>
		WidgetSize GetPreferredWidth ();
		
		/// <summary>
		/// Gets the preferred height of the widget (includes the margin)
		/// </summary>
		/// <returns>
		/// The preferred height.
		/// </returns>
		WidgetSize GetPreferredHeight ();
		
		/// <summary>
		/// Gets the preferred height for a given width (includes the margin). Called only when using HeightForWidth size request mode.
		/// </summary>
		/// <returns>
		/// The preferred height
		/// </returns>
		/// <param name='width'>
		/// Width.
		/// </param>
		WidgetSize GetPreferredHeightForWidth (double width);
		
		/// <summary>
		/// Gets the preferred width for a given height (includes the margin).  Called only when using WidthForHeight size request mode.
		/// </summary>
		/// <returns>
		/// The preferred width
		/// </returns>
		/// <param name='height'>
		/// Height.
		/// </param>
		/// 
		WidgetSize GetPreferredWidthForHeight (double height);
		
		object NativeWidget { get; }

		IEnumerable<Widget> Children { get; }

		Toolkit ToolkitEngine { get; }
	}
}

