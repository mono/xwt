//
// IFolderSelectorBackend.cs
//
// Author:
//       Lluis Sanchez Gual <lluis@xamarin.com>
//
// Copyright (c) 2016 Xamarin, Inc (http://www.xamarin.com)
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
	public interface IFolderSelectorBackend: IWidgetBackend
	{
		/// <summary>
		/// Gets or sets the current folder.
		/// </summary>
		/// <value>The current folder.</value>
		string CurrentFolder { get; set; }

		/// <summary>
		/// Gets or sets the path to the folder
		/// </summary>
		/// <value>The name of the folder.</value>
		string Folder { get; set; }

		/// <summary>
		/// Gets or sets the title of the folder selection dialog
		/// </summary>
		/// <value>The title.</value>
		string Title { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the user can create folders when using the folder selection dialog
		/// </summary>
		bool CanCreateFolders { get; set; }

	}

	public interface IFolderSelectorEventSink : IWidgetEventSink
	{
		void OnFolderChanged ();
	}

	public enum FolderSelectorEvent
	{
		FolderChanged
	}
}

