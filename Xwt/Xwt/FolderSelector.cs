//
// FolderSelector.cs
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
	[BackendType (typeof (IFolderSelectorBackend))]
	public class FolderSelector: Widget
	{
		protected new class WidgetBackendHost : Widget.WidgetBackendHost, IFolderSelectorEventSink
		{
			protected override IBackend OnCreateBackend ()
			{
				var b = base.OnCreateBackend ();
				if (b == null)
					b = new DefaultFolderSelectorBackend ();
				return b;
			}

			public void OnFolderChanged ()
			{
				((FolderSelector)Parent).OnFolderChanged (EventArgs.Empty);
			}
		}

		static FolderSelector ()
		{
			MapEvent (FolderSelectorEvent.FolderChanged, typeof (FolderSelector), "OnFolderChanged");
		}

		public FolderSelector ()
		{
		}


		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}

		IFolderSelectorBackend Backend {
			get { return (IFolderSelectorBackend)BackendHost.Backend; }
		}

		public string Title {
			get { return Backend.Title; }
			set { Backend.Title = value; }
		}

		/// <summary>
		/// Gets the name of the folder that the user has selected
		/// </summary>
		/// <value>
		/// The name of the folder, or null if no selection was made
		/// </value>
		public string Folder {
			get { return Backend.Folder; }
			set { Backend.Folder = value; }
		}

		/// <summary>
		/// Gets or sets the folder to show when opening the folder selector dialog
		/// </summary>
		/// <value>
		/// The current folder.
		/// </value>
		public string CurrentFolder {
			get { return Backend.CurrentFolder; }
			set { Backend.CurrentFolder = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether the user can create folders when using the folder selection dialog
		/// </summary>
		/// <value><c>true</c> if this instance can create folders; otherwise, <c>false</c>.</value>
		public bool CanCreateFolders {
			get { return Backend.CanCreateFolders; }
			set { Backend.CanCreateFolders = value; }
		}

		protected virtual void OnFolderChanged (EventArgs args)
		{
			if (folderChanged != null)
				folderChanged (this, args);
		}

		EventHandler folderChanged;

		/// <summary>
		/// Occurs when the font changed.
		/// </summary>
		public event EventHandler FolderChanged {
			add {
				BackendHost.OnBeforeEventAdd (FolderSelectorEvent.FolderChanged, folderChanged);
				folderChanged += value;
			}
			remove {
				folderChanged -= value;
				BackendHost.OnAfterEventRemove (FolderSelectorEvent.FolderChanged, folderChanged);
			}
		}
	}


	class DefaultFolderSelectorBackend : XwtWidgetBackend, IFolderSelectorBackend
	{
		TextEntry entry;
		SelectFolderDialog dialog;
		string currentFolder;
		bool enableFolderChangedEvent;
		string title;
		bool canCreateFolders;

		public DefaultFolderSelectorBackend ()
		{
			var box = new HBox ();
			entry = new TextEntry ();
			entry.Changed += (sender, e) => NotifyFolderChange ();
			box.PackStart (entry, true);

			var btn = new Button ("...");
			box.PackStart (btn);
			btn.Clicked += BtnClicked;
			Content = box;
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

		public string Folder {
			get { return dialog != null ? dialog.Folder : entry.Text; }
			set { entry.Text = value; }
		}

		void NotifyFolderChange ()
		{
			if (enableFolderChangedEvent)
				EventSink.OnFolderChanged ();
		}

		protected new IFolderSelectorEventSink EventSink {
			get { return (IFolderSelectorEventSink)base.EventSink; }
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

		public bool CanCreateFolders {
			get {
				return canCreateFolders;
			}

			set {
				canCreateFolders = value;
				if (dialog != null)
					dialog.CanCreateFolders = value;
			}
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is FolderSelectorEvent) {
				switch ((FolderSelectorEvent)eventId) {
				case FolderSelectorEvent.FolderChanged: enableFolderChangedEvent = true; break;
				}
			}
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is FolderSelectorEvent) {
				switch ((FolderSelectorEvent)eventId) {
				case FolderSelectorEvent.FolderChanged: enableFolderChangedEvent = false; break;
				}
			}
		}

		void BtnClicked (object sender, EventArgs e)
		{
			dialog = new SelectFolderDialog ();

			try {
				if (!string.IsNullOrEmpty (currentFolder))
					dialog.CurrentFolder = currentFolder;
				if (!string.IsNullOrEmpty (title))
					dialog.Title = title;
				if (dialog.Run (ParentWindow))
					Folder = dialog.Folder;
			} finally {
				currentFolder = dialog.CurrentFolder;
				dialog.Dispose ();
				dialog = null;
			}
		}
	}
}

