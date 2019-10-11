//
// SelectFolderDialogBackend.cs
//  
// Author:
//       James Clancey <clancey@xamarin.com>
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

using System.Collections.Generic;
using System.Linq;
using Xwt.Backends;

namespace Xwt.Mac
{
	public class SelectFolderDialogBackend : OpenFileDialogBackend, ISelectFolderDialogBackend
	{
		public void Initialize (bool multiselect)
		{
			Panel.AllowsMultipleSelection = multiselect;
			Panel.CanChooseFiles = false;
			Panel.CanChooseDirectories = true;
			Panel.CanCreateDirectories = false;

			Panel.Prompt = Application.TranslationCatalog.GetPluralString("Select Directory", "Select Directories", multiselect ? 2 : 1);
		}

		protected override void OnInitialize (IEnumerable<FileDialogFilter> filters, bool multiselect, string initialFileName)
		{
			Initialize (multiselect);
		}

		public bool CanCreateFolders {
			get { return Panel.CanCreateDirectories; }
			set { Panel.CanCreateDirectories = value; }
		}

		public string Folder {
			get {
				return FileName;
			}
		}

		public string[] Folders {
			get {
				return FileNames;
			}
		}
	}
}

