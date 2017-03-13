//
// GtkMacSelectFolderBackend.cs
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
using Xwt.Backends;
using AppKit;
using Foundation;
using System.Linq;

namespace Xwt.Gtk.Mac
{
	public class GtkMacSelectFolderBackend: NSOpenPanel, ISelectFolderDialogBackend
	{
		public GtkMacSelectFolderBackend ()
		{
		}

		#region ISelectFolderDialogBackend implementation
		public void Initialize (bool multiselect)
		{
			this.AllowsMultipleSelection = multiselect;
			this.CanChooseFiles = false;
			this.CanChooseDirectories = true;

			this.Prompt = Application.TranslationCatalog.GetPluralString("Select Folder", "Select Folders", multiselect ? 2 : 1);
		}

		public bool Run (IWindowFrameBackend parent)
		{
			var returnValue = this.RunModal ();
			return returnValue == 1;
		}

		public void Cleanup ()
		{

		}

		public string Folder {
			get {
				return this.Url == null ? string.Empty : Url.Path;
			}
		}

		public string [] Folders {
			get {
				return this.Urls.Length == 0 ? new string [0] : this.Urls.Select (x => x.Path).ToArray ();
			}
		}

		public string CurrentFolder {
			get {
				return DirectoryUrl.AbsoluteString;
			}
			set {
				this.DirectoryUrl = new NSUrl (value, true);
			}
		}

		public bool CanCreateFolders {
			get {
				return false;
			}
			set {

			}
		}

		#endregion

		#region IBackend implementation

		public void InitializeBackend (object frontend, ApplicationContext context)
		{

		}

		public void EnableEvent (object eventId)
		{

		}

		public void DisableEvent (object eventId)
		{

		}

		#endregion		
	}
}

