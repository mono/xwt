// 
// FileDialog.cs
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
using System.ComponentModel;


namespace Xwt
{
	public abstract class FileDialog: XwtComponent
	{
		FileDialogFilterCollection filters;
		bool running;
		bool multiselect;
		string initialFileName;
		FileDialogFilter activeFilter;
		string currentFolder;
		string title = "";
		string fileName;
		string[] fileNames = new string[0];
		
		internal FileDialog ()
		{
			filters = new FileDialogFilterCollection (AddRemoveItem);
		}

		internal FileDialog (string title): this ()
		{
			this.title = title;
		}

		IFileDialogBackend Backend {
			get { return (IFileDialogBackend) base.BackendHost.Backend; }
		}
		
		void AddRemoveItem (FileDialogFilter filter, bool added)
		{
			CheckNotRunning ();
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
		/// Gets the name of the file that the user has selected in the dialog
		/// </summary>
		/// <value>
		/// The name of the file, or null if no selection was made
		/// </value>
		public string FileName {
			get { return running ? Backend.FileName : fileName; }
		}
		
		/// <summary>
		/// Gets the files the the user has selected in the dialog
		/// </summary>
		/// <value>
		/// The names of the files
		/// </value>
		public string[] FileNames {
			get {
				return running ? Backend.FileNames : fileNames;
			}
		}
		
		/// <summary>
		/// Gets or sets the current folder.
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
		/// Gets or sets a value indicating whether the user can select multiple files
		/// </summary>
		/// <value>
		/// <c>true</c> if multiselection is allowed; otherwise, <c>false</c>.
		/// </value>
		public bool Multiselect {
			get { return multiselect; }
			set { CheckNotRunning (); multiselect = value; }
		}

		/// <summary>
		/// File name to show by default.
		/// </summary>
		public string InitialFileName {
			get { return initialFileName; }
			set { CheckNotRunning (); initialFileName = value; }
		}

		/// <summary>
		/// Filters that allow the user to chose the kinds of files the dialog displays.
		/// </summary>
		public FileDialogFilterCollection Filters {
			get { return filters; }
		}

		/// <summary>
		/// The filter currently selected in the file dialog
		/// </summary>
		public FileDialogFilter ActiveFilter {
			get { return running ? Backend.ActiveFilter : activeFilter; }
			set {
				if (!filters.Contains (value))
					throw new ArgumentException ("The active filter must be one of the filters included in the Filters collection");
				if (running)
					Backend.ActiveFilter = value;
				else
					activeFilter = value;
			}
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
				Backend.Initialize (filters, multiselect, initialFileName);
				if (!string.IsNullOrEmpty (currentFolder))
					Backend.CurrentFolder = currentFolder;
				if (activeFilter != null)
					Backend.ActiveFilter = activeFilter;
				if (!string.IsNullOrEmpty (title))
					Backend.Title = title;
				bool result = false;
				BackendHost.ToolkitEngine.InvokePlatformCode (delegate {
					result = Backend.Run ((IWindowFrameBackend)Toolkit.GetBackend (parentWindow));
				});
				return result;
			} finally {
				currentFolder = Backend.CurrentFolder;
				activeFilter = Backend.ActiveFilter;
				fileName = Backend.FileName;
				fileNames = Backend.FileNames; 
				currentFolder = Backend.CurrentFolder;
				running = false;
				Backend.Cleanup ();
			}
		}
	}
}

