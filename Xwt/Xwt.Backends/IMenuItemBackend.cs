// 
// IMenuItemBackend.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//       Eric Maupin <ermau@xamarin.com>
// 
// Copyright (c) 2011-2012 Xamarin Inc
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
	public interface IMenuItemBackend: IBackend
	{
		void Initialize (IMenuItemEventSink eventSink);

		/// <summary>
		/// Sets a submenu for this menu item.
		/// </summary>
		/// <param name="menu">The menu to use as a submenu of this item.</param>
		void SetSubmenu (IMenuBackend menu);

		/// <summary>
		/// Sets the image to display for the menu item.
		/// </summary>
		/// <param name="image">The image backend to set as the image for this menu item.</param>
		void SetImage (ImageDescription image);

		/// <summary>
		/// Gets or sets the label for the menu item.
		/// </summary>
		string Label { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Xwt.Backends.IMenuItemBackend"/> use a mnemonic.
		/// </summary>
		/// <value><c>true</c> if it use a mnemonic; otherwise, <c>false</c>.</value>
		bool UseMnemonic { get; set; }

		/// <summary>
		/// Gets or sets whether the menu item is enabled.
		/// </summary>
		bool Sensitive { get; set; }

		/// <summary>
		/// Gets or sets whether the menu item is visible.
		/// </summary>
		bool Visible { get; set; }
	}
	
	public interface IMenuItemEventSink
	{
		void OnClicked ();
	}
	
	public enum MenuItemEvent
	{
		Clicked = 1
	}
}

