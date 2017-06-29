//
// SelectFolderDialogBackend.cs
//
// Author:
//       Eric Maupin <ermau@xamarin.com>
//
// Copyright (c) 2012 Xamarin, Inc.
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
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Windows.Interop;
using Xwt.Backends;
using IWin32Window = System.Windows.Forms.IWin32Window;
using WindowsFolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;

namespace Xwt.WPFBackend
{
	public class SelectFolderDialogBackend
		: Backend, ISelectFolderDialogBackend
	{
		public void Initialize (bool multiselect)
		{
			if (multiselect)
				throw new NotSupportedException();

			this.dialog = new WindowsFolderBrowserDialog();
			
		}

		public string Title
		{
			get { return this.dialog.Description; }
			set { this.dialog.Description = value; }
		}

		public string Folder
		{
			get { return (this.dialog != null) ? this.dialog.SelectedPath : null; }
		}

		public string[] Folders
		{
			get { return new[] { Folder }; }
		}

		public bool CanCreateFolders {
			get { return dialog != null ? dialog.ShowNewFolderButton : false; }
			set { 
 				if (dialog != null)
					dialog.ShowNewFolderButton = value;
			}
		}

		public string CurrentFolder
		{
			get
			{
				string current = Folder;
				if (String.IsNullOrEmpty (current))
					return null;

				string dir = Path.GetDirectoryName (current);
				return (String.IsNullOrEmpty (dir) ? current : dir);
			}
			set { this.dialog.SelectedPath = value; }
		}

		public bool Run (IWindowFrameBackend parent)
		{
			if (parent != null)
				return (this.dialog.ShowDialog (new XwtWin32Window (parent)) == DialogResult.OK);
			else
				return (this.dialog.ShowDialog () == DialogResult.OK);
		}

		public void Cleanup ()
		{
			this.dialog.Dispose();
		}

		private WindowsFolderBrowserDialog dialog;
	}
}
