// 
// ISelectColorDialog.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2012 Xamarin Inc
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
	public interface ISelectColorDialogBackend
	{
		/// <summary>
		/// Gets or sets the selected color
		/// </summary>
		Color Color { get; set; }

		/// <summary>
		/// Runs the dialog, allowing the user to select a color
		/// </summary>
		/// <param name='parent'>
		/// Parent window (the dialog will be modal to this window). It can be null.
		/// </param>
		/// <param name='title'>
		/// Title of the dialog
		/// </param>
		/// <param name='supportsAlpha'>
		/// <c>true</c> if the dialog has to allow selecting an alpha value for the color
		/// </param>
		/// <returns>
		/// <c>true</c> if the user clicked OK, <c>false</c> otherwise
		/// </returns>
		/// <remarks>
		/// The Run method will always be called once (and only once).
		/// The dialog must be shown in modal mode. The method returns when the user clicks on
		/// OK or Cancel. The dialog must be already closed when this method returns.
		/// </remarks>
		bool Run (IWindowFrameBackend parent, string title, bool supportsAlpha);
		
		/// <summary>
		/// Frees native resources
		/// </summary>
		void Dispose ();	
	}
}

