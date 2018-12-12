//
// FileSelector.cs
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

namespace Xwt
{
	/// <summary>
	/// The <see cref="Xwt.FileSelector"/> widget allows the user to select a file.
	/// </summary>
	/// <remarks>
	/// The widget contains a preview text box with the selected font.
	/// The widget raises the <see cref="Xwt.FileSelector.FileChanged"/> event, when
	/// the selected file changes.
	/// </summary>
	[BackendType (typeof (IFileSelectorBackend))]
	public class FileSelector: Widget
	{
		protected new class WidgetBackendHost : Widget.WidgetBackendHost, IFileSelectorEventSink
		{
			protected override IBackend OnCreateBackend ()
			{
				var b = base.OnCreateBackend ();
				if (b == null)
					b = new DefaultFileSelectorBackend ();
				return b;
			}

			public void OnFileChanged ()
			{
				((FileSelector)Parent).OnFileChanged (EventArgs.Empty);
			}
		}

		static FileSelector ()
		{
			MapEvent (FileSelectorEvent.FileChanged, typeof (FileSelector), "OnFileChanged");
		}

		public FileSelector ()
		{
		}

		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}

		IFileSelectorBackend Backend {
			get { return (IFileSelectorBackend)BackendHost.Backend; }
		}

		public string Title {
			get { return Backend.Title; }
			set { Backend.Title = value; }
		}

		/// <summary>
		/// Gets the name of the file that the user has selected
		/// </summary>
		/// <value>
		/// The name of the file, or null if no selection was made
		/// </value>
		public string FileName {
			get { return Backend.FileName; }
			set { Backend.FileName = value; }
		}

		/// <summary>
		/// Gets or sets the folder to show when opening the file selector dialog
		/// </summary>
		/// <value>
		/// The current folder.
		/// </value>
		public string CurrentFolder {
			get { return Backend.CurrentFolder; }
			set { Backend.CurrentFolder = value; }
		}

		/// <summary>
		/// Filters that allow the user to chose the kinds of files the dialog displays.
		/// </summary>
		public FileDialogFilterCollection Filters {
			get { return Backend.Filters; }
		}

		/// <summary>
		/// The filter currently selected in the file dialog
		/// </summary>
		public FileDialogFilter ActiveFilter {
			get { return Backend.ActiveFilter; }
			set { Backend.ActiveFilter = value; }
		}

		/// <summary>
		/// The filter currently selected in the file dialog
		/// </summary>
		public FileSelectionMode FileSelectionMode {
			get { return Backend.FileSelectionMode; }
			set { Backend.FileSelectionMode = value; }
		}

		protected virtual void OnFileChanged (EventArgs args)
		{
			if (fileChanged != null)
				fileChanged (this, args);
		}

		EventHandler fileChanged;

		/// <summary>
		/// Occurs when the font changed.
		/// </summary>
		public event EventHandler FileChanged {
			add {
				BackendHost.OnBeforeEventAdd (FileSelectorEvent.FileChanged, fileChanged);
				fileChanged += value;
			}
			remove {
				fileChanged -= value;
				BackendHost.OnAfterEventRemove (FileSelectorEvent.FileChanged, fileChanged);
			}
		}
	}

	public enum FileSelectionMode
	{
		Open,
		Save
	}

	class DefaultFileSelectorBackend : XwtWidgetBackend, IFileSelectorBackend
	{
		TextEntry entry;
		FileDialog dialog;
		FileDialogFilterCollection filters;
		FileDialogFilter activeFilter;
		string currentFolder;
		bool enableFileChangedEvent;
		string title;

		public DefaultFileSelectorBackend ()
		{
			filters = new FileDialogFilterCollection (null);
		
			var box = new HBox ();
			entry = new TextEntry ();
			entry.Changed += (sender, e) => NotifyFileChange ();
			box.PackStart (entry, true);

			var btn = new Button ("...");
			box.PackStart (btn);
			btn.Clicked += BtnClicked;
			Content = box;
		}

		public FileDialogFilter ActiveFilter {
			get {
				if (dialog != null)
					return dialog.ActiveFilter;
				else
					return activeFilter;
			}
			set {
				if (!filters.Contains (value))
					throw new ArgumentException ("The active filter must be one of the filters included in the Filters collection");
				if (dialog != null)
					dialog.ActiveFilter = value;
				else
					activeFilter = value;
			}
		}

		public string CurrentFolder {
			get {
				return dialog != null ? dialog.CurrentFolder : currentFolder;
			}
			set {
				if (dialog != null)
					dialog.CurrentFolder = value;
				else
					currentFolder = value;
			}
		}

		public FileDialogFilterCollection Filters {
			get { return filters; }
		}

		public string FileName {
			get { return dialog != null ? dialog.FileName : entry.Text; }
			set { entry.Text = value; }
		}

		void NotifyFileChange ()
		{
			if (enableFileChangedEvent)
				EventSink.OnFileChanged ();
		}

		public FileSelectionMode FileSelectionMode { get; set; }

		protected new IFileSelectorEventSink EventSink {
			get { return (IFileSelectorEventSink)base.EventSink; }
		}

		public string Title {
			get {
				return title;
			}

			set {
				title = value;
				if (dialog != null)
					dialog.Title = value;
			}
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is FileSelectorEvent) {
				switch ((FileSelectorEvent)eventId) {
				case FileSelectorEvent.FileChanged: enableFileChangedEvent = true; break;
				}
			}
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is FileSelectorEvent) {
				switch ((FileSelectorEvent)eventId) {
				case FileSelectorEvent.FileChanged: enableFileChangedEvent = false; break;
				}
			}
		}

		void BtnClicked (object sender, EventArgs e)
		{
			if (FileSelectionMode == FileSelectionMode.Open)
				dialog = new OpenFileDialog ();
			else
				dialog = new SaveFileDialog ();

			try {
				foreach (var f in filters)
					dialog.Filters.Add (f);
				if (!string.IsNullOrEmpty (currentFolder))
					dialog.CurrentFolder = currentFolder;
				if (activeFilter != null)
					dialog.ActiveFilter = activeFilter;
				if (!string.IsNullOrEmpty (title))
					dialog.Title = title;
				if (dialog.Run (ParentWindow))
					FileName = dialog.FileName;
			} finally {
				currentFolder = dialog.CurrentFolder;
				activeFilter = dialog.ActiveFilter;
				dialog.Dispose ();
				dialog = null;
			}
		}
	}
}

