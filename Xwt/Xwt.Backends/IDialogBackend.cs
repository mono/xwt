// 
// IDialogBackend.cs
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

namespace Xwt.Backends
{
	public interface IDialogBackend: IWindowBackend
	{
		/// <summary>
		/// Sets the buttons to be shown in the dialog. It can be called multiple times.
		/// </summary>
		void SetButtons (IEnumerable<DialogButton> buttons);
		
		/// <summary>
		/// Called when the properties of a button have changed
		/// </summary>
		void UpdateButton (DialogButton btn);
		
		/// <summary>
		/// Shows the dialog and starts running the gui loop. The method has to return when EndLoop is called.
		/// </summary>
		/// <param name='parent'>
		/// Parent window
		/// </param>
		void RunLoop (IWindowFrameBackend parent);
		
		/// <summary>
		/// Ends the gui loop, causing the RunLoop method to return
		/// </summary>
		void EndLoop ();
	}
	
	public interface IDialogEventSink: IWindowEventSink
	{
		/// <summary>
		/// Notifies that a dialog button has been clicked. It can only be called while RunLoop is being executed.
		/// </summary>
		/// <param name='btn'>
		/// The clicked button
		/// </param>
		void OnDialogButtonClicked (DialogButton btn);
	}
}

