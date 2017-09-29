// 
// SelectFolderDialog.cs
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
using Xwt.Backends;


namespace Xwt
{
	[BackendType (typeof(ISelectFolderDialogBackend))]
	public sealed class SelectFolderDialog: XwtComponent
	{
		bool running;
		bool multiselect;
		string currentFolder;
		bool canCreateFolders;
		string title = "";
		string folder;
		string[] folders = new string[0];
		
		public SelectFolderDialog ()
		{
		}

		public SelectFolderDialog (string title)
		{
			this.title = title;
		}

		ISelectFolderDialogBackend Backend {
			get { return (ISelectFolderDialogBackend) BackendHost.Backend; }
		}
		
		public string Title {
			get {
				return title ?? "";
			}
			set {
				title = value ?? "";
				if (running)
					Backend.Title = title;
			}
		}
		
		/// <summary>
		/// Gets the path of the folder that the user has selected in the dialog
		/// </summary>
		/// <value>
		/// The path of the folder, or null if no selection was made
		/// </value>
		public string Folder {
			get { return running ? Backend.Folder : folder; }
		}
		
		/// <summary>
		/// Gets the paths of the folders that the user has selected in the dialog
		/// </summary>
		/// <value>
		/// The names of the files
		/// </value>
		public string[] Folders {
			get {
				return running ? Backend.Folders : folders;
			}
		}
		
		/// <summary>
		/// Gets or sets the folder whose contents are shown in the dialog
		/// </summary>
		/// <value>
		/// The current folder.
		/// </value>
		public string CurrentFolder {
			get {
				return running ? Backend.CurrentFolder : currentFolder;
			}
			set {
				if (running)
					Backend.CurrentFolder = value;
				else
					currentFolder = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance can create folders.
		/// </summary>
		/// <value><c>true</c> if this instance can create folders; otherwise, <c>false</c>.</value>
		public bool CanCreateFolders {
			get { return running ? Backend.CanCreateFolders : canCreateFolders; }
			set {
				if (running)
					Backend.CanCreateFolders = value;
				else
					canCreateFolders = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the user can select multiple files
		/// </summary>
		/// <value>
		/// <c>true</c> if multiselection is allowed; otherwise, <c>false</c>.
		/// </value>
		public bool Multiselect {
			get { return multiselect; }
			set { CheckNotRunning (); multiselect = value; }
		}

		void CheckNotRunning ()
		{
			if (running)
				throw new InvalidOperationException ("Options can't be modified when the dialog is running");
		}
 
		/// <summary>
		/// Shows the dialog.
		/// </summary>
		public bool Run ()
		{
			return Run (null);
		}

		/// <summary>
		/// Shows the dialog.
		/// </summary>
		public bool Run (WindowFrame parentWindow)
		{
			try {
				running = true;
				Backend.Initialize (multiselect);
				if (!string.IsNullOrEmpty (currentFolder))
					Backend.CurrentFolder = currentFolder;
				if (!string.IsNullOrEmpty (title))
					Backend.Title = title;
				Backend.CanCreateFolders = canCreateFolders;

				bool result = false;
				BackendHost.ToolkitEngine.InvokePlatformCode (delegate {
					result = Backend.Run ((IWindowFrameBackend)Toolkit.GetBackend (parentWindow));
				});
				return result;
			} finally {
				currentFolder = Backend.CurrentFolder;
				folder = Backend.Folder;
				folders = Backend.Folders; 
				currentFolder = Backend.CurrentFolder;
				running = false;
				Backend.Cleanup ();
			}
		}	
	}
}

