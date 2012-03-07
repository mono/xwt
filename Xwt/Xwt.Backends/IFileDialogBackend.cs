// 
// IFileDialogBackend.cs
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
using System.Collections.Generic;

namespace Xwt.Backends
{
	/// <summary>
	/// A file dialog backend.
	/// </summary>
	public interface IFileDialogBackend: IBackend
	{
		/// <summary>
		/// Initializes the file dialog. This method can be called multiple times.
		/// </summary>
		/// <param name='filters'>
		/// File filters that allow the user to chose the kinds of files the dialog displays.
		/// The collection will not change after the call to Initialize
		/// </param>
		/// <param name='multiselect'>
		/// Value indicating whether the user can select multiple files
		/// </param>
		/// <param name='initialFileName'>
		/// File name to show by default. It's only a file name, doesn't need to include a path.
		/// </param>
		void Initialize (IEnumerable<FileDialogFilter> filters, bool multiselect, string initialFileName);
		
		/// <summary>
		/// Gets or sets the title of the dialog
		/// </summary>
		string Title { get; set; }
			
		/// <summary>
		/// Gets the name of the file that the user has selected in the dialog
		/// </summary>
		/// <value>
		/// The name of the file, or null if no selection was made
		/// </value>
		/// <remarks>
		/// This property can be invoked at any time after a call to Initialize, and before the call to Close.
		/// </remarks>
		string FileName { get; }

		/// <summary>
		/// Gets the files the the user has selected in the dialog
		/// </summary>
		/// <value>
		/// The names of the files
		/// </value>
		/// <remarks>
		/// This property can be invoked at any time after a call to Initialize, and before the call to Close
		/// </remarks>
		string[] FileNames { get; }

		/// <summary>
		/// Gets or sets the folder whose contents are shown in the dialog
		/// </summary>
		/// <value>
		/// The current folder.
		/// </value>
		/// <remarks>
		/// This property can be invoked at any time after a call to Initialize, and before the call to Close
		/// </remarks>
		string CurrentFolder { get; set; }

		/// <summary>
		/// Gets or sets the default file filter
		/// </summary>
		/// <remarks>
		/// The filter is guaranteed to be in the filters collection provided in the Initialize method.
		/// This property can be invoked at any time after a call to Initialize, and before the call to Close.
		/// </remarks>
		FileDialogFilter ActiveFilter { get; set; }

		/// <summary>
		/// Runs the dialog, allowing the user to select a file
		/// </summary>
		/// <param name='parent'>
		/// Parent window (the dialog will be modal to this window). It can be null.
		/// </param>
		/// <returns>
		/// <c>true</c> if the user clicked OK, <c>false</c> otherwise
		/// </returns>
		/// <remarks>
		/// The Run method will always be called once (and only once) after an Initialize call.
		/// The dialog must be shown in modal mode. The method returns when the user clicks on
		/// OK or Cancel. The dialog must be already closed when this method returns.
		/// </remarks>
		bool Run (IWindowFrameBackend parent);
		
		/// <summary>
		/// Frees native resources
		/// </summary>
		/// <remarks>
		/// This method is called after Run, so that the backend can release
		/// the native resources. The Initialize method can be called after Cleanup.
		/// </remarks>
		void Cleanup ();
	}
}

