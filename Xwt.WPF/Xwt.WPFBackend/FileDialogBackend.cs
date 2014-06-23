//
// FileDialogBackend.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xwt.Backends;
using WindowsFileDialog = Microsoft.Win32.FileDialog;

namespace Xwt.WPFBackend
{
	public abstract class FileDialogBackend<T>
		: Backend, IFileDialogBackend
		where T : WindowsFileDialog
	{
		public string Title
		{
			get { return this.dialog.Title; }
			set { this.dialog.Title = value; }
		}

		public string FileName
		{
			get { return this.dialog.FileName; }
		}

		public string[] FileNames
		{
			get { return this.dialog.FileNames; }
		}

		public string CurrentFolder
		{
			get { return this.dialog.InitialDirectory; }
			set { this.dialog.InitialDirectory = value; }
		}

		public FileDialogFilter ActiveFilter
		{
			get
			{
				if (this.filters.Count == 0 || this.dialog.FilterIndex < 1)
					return null;

				return this.filters [this.dialog.FilterIndex - 1];
			}

			set { this.dialog.FilterIndex = this.filters.IndexOf (value); }
		}

		public bool Run (IWindowFrameBackend parent)
		{
			bool? ok;

			WindowBackend windowBackend = parent as WindowBackend;
			if (windowBackend != null){
				ok = this.dialog.ShowDialog (windowBackend.Window);				
			} else {
				ok = this.dialog.ShowDialog ();
			}

			return ok.HasValue && ok.Value;
		}

		public void Cleanup ()
		{
			this.dialog = null;
		}

		public virtual void Initialize (IEnumerable<FileDialogFilter> newFilters, bool multiselect, string initialFileName)
		{
			Initialize (newFilters, initialFileName);
		}

		private List<FileDialogFilter> filters;
		protected T dialog;

		protected void Initialize (IEnumerable<FileDialogFilter> newFilters, string initialFileName)
		{
			this.filters = newFilters.ToList();
			this.dialog.Filter = GetFilters (this.filters);

			this.dialog.FileName = initialFileName;
		}

		private string GetFilters (IEnumerable<FileDialogFilter> filters)
		{
			StringBuilder builder = new StringBuilder();
			foreach (FileDialogFilter filter in filters) {
				if (builder.Length > 0)
					builder.Append ("|");

				builder.Append (filter.Name);
				builder.Append ("|");

				int i = 0;
				foreach (var pattern in filter.Patterns) {
					if (i++ > 0)
						builder.Append (";");

					builder.Append (pattern);
				}
			}

			return builder.ToString ();
		}
	}
}
