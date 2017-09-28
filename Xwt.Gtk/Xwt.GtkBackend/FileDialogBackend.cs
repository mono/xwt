// 
// FileDialogBackend.cs
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
using Xwt.Backends;
using System.Linq;


namespace Xwt.GtkBackend
{
	public class FileDialogBackend: IFileDialogBackend
	{
		Gtk.FileChooserDialog dialog;
		Gtk.FileChooserAction action;
		List<Gtk.FileFilter> gtkFilters;
		List<FileDialogFilter> filters;

		public ApplicationContext ApplicationContext { get; private set; }

		public FileDialogBackend (Gtk.FileChooserAction action)
		{
			this.action = action;
		}

		public void InitializeBackend (object frontend, ApplicationContext context)
		{
			ApplicationContext = context;
		}
		
		public void EnableEvent (object eventId)
		{
		}
		
		public void DisableEvent (object eventId)
		{
		}
		
		#region IFileDialogBackend implementation
		
		public void Initialize (bool multiselect)
		{
			dialog = new Gtk.FileChooserDialog ("", null, action);
			dialog.SelectMultiple = multiselect;

			switch (action) {
				case Gtk.FileChooserAction.Open:
					dialog.AddButton (Gtk.Stock.Cancel, Gtk.ResponseType.Cancel);
					dialog.AddButton (Gtk.Stock.Open, Gtk.ResponseType.Ok);
					break;
				case Gtk.FileChooserAction.SelectFolder:
				case Gtk.FileChooserAction.CreateFolder:
					dialog.AddButton (Gtk.Stock.Cancel, Gtk.ResponseType.Cancel);
					dialog.AddButton (Application.TranslationCatalog.GetString("Select"), Gtk.ResponseType.Ok);
					break;
				case Gtk.FileChooserAction.Save:
					dialog.AddButton (Gtk.Stock.Cancel, Gtk.ResponseType.Cancel);
					dialog.AddButton (Gtk.Stock.Save, Gtk.ResponseType.Ok);
					break;
				default:
					break;
			}
			dialog.DefaultResponse = Gtk.ResponseType.Ok;
		}
		
		public void Initialize (IEnumerable<FileDialogFilter> filters, bool multiselect, string initialFileName)
		{
			Initialize (multiselect);
			
			if (!string.IsNullOrEmpty (initialFileName))
				dialog.CurrentName = initialFileName;
			
			this.filters = filters.ToList ();
			gtkFilters = new List<Gtk.FileFilter> ();
			
			foreach (var filter in filters) {
				var gf = new Gtk.FileFilter ();
				gtkFilters.Add (gf);
				if (!string.IsNullOrEmpty (filter.Name))
					gf.Name = filter.Name;
				if (filter.Patterns != null)
					foreach (var pattern in filter.Patterns)
						gf.AddPattern (pattern);
			}
			
			foreach (var filter in gtkFilters)
				dialog.AddFilter (filter);
		}
		
		public string Title {
			get { return dialog.Title; }
			set { dialog.Title = value; }
		}
		
		public FileDialogFilter ActiveFilter {
			get {
				int i = gtkFilters.IndexOf (dialog.Filter);
				if (i != -1 && i < filters.Count)
					return filters [i];
				else
					return null;
			}
			set {
				int i = filters.IndexOf (value);
				dialog.Filter = gtkFilters [i];
			}
		}
		
		public bool Run (IWindowFrameBackend parent)
		{
			var p = parent != null ? ApplicationContext.Toolkit.GetNativeWindow (parent) as Gtk.Window : null;
			int result = MessageService.RunCustomDialog (dialog, p);
			return result == (int) Gtk.ResponseType.Ok;
		}
		
		public void Cleanup ()
		{
			dialog.Destroy ();
		}

		protected Gtk.FileChooserDialog Dialog {
			get { return dialog; }
		}

		public string FileName {
			get {
				return dialog.Filename;
			}
		}
		
		public string[] FileNames {
			get {
				return dialog.Filenames;
			}
		}

		public string Folder {
			get { return dialog.Filename; }
		}

		public string[] Folders {
			get { return dialog.Filenames; }
		}

		public string CurrentFolder {
			get {
				return dialog.CurrentFolder;
			}
			set {
				dialog.SetCurrentFolder (value);
			}
		}
		
		#endregion
	}
	
	public class OpenFileDialogBackend: FileDialogBackend, IOpenFileDialogBackend
	{
		public OpenFileDialogBackend (): base (Gtk.FileChooserAction.Open)
		{
		}
	}
	
	public class SaveFileDialogBackend: FileDialogBackend, ISaveFileDialogBackend
	{
		public SaveFileDialogBackend (): base (Gtk.FileChooserAction.Save)
		{
		}
	}
	
	public class SelectFolderDialogBackend: FileDialogBackend, ISelectFolderDialogBackend
	{
		public SelectFolderDialogBackend (): base (Gtk.FileChooserAction.SelectFolder)
		{
		}

		#region ISelectFolderDialogBackend implementation

		public bool CanCreateFolders {
			get {
				if (Dialog != null)
					return Dialog.Action == Gtk.FileChooserAction.CreateFolder;
				return false;
			}

			set {
				if (Dialog != null)
					Dialog.Action = Gtk.FileChooserAction.CreateFolder;
			}
		}

		#endregion
	}
}
