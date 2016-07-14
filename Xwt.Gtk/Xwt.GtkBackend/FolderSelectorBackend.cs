//
// FolderSelectorBackend.cs
//
// Author:
//       harry <>
//
// Copyright (c) 2016 harry
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

using Xwt.Backends;

namespace Xwt.GtkBackend
{
	public class FolderSelectorBackend : WidgetBackend, IFolderSelectorBackend
	{
		private Gtk.FileChooserButton folderChooser;

		public override void Initialize ()
		{
			folderChooser = new Gtk.FileChooserButton ("", Gtk.FileChooserAction.SelectFolder);
			folderChooser.SelectionChanged += (sender, e) => EventSink.OnFolderChanged ();

			Widget = folderChooser;
			Widget.ShowAll ();
		}

		protected new Gtk.FileChooserButton Widget {
			get { return (Gtk.FileChooserButton)base.Widget; }
			set { base.Widget = value; }
		}

		protected new IFolderSelectorEventSink EventSink {
			get { return (IFolderSelectorEventSink)base.EventSink; }
		}

		public string CurrentFolder {
			get {
				return folderChooser.CurrentFolder;
			}
			set {
				folderChooser.SetCurrentFolder (value);
			}
		}

		public string Folder {
			get {
				return folderChooser.Filename;
			}
			set {
				folderChooser.SetFilename (value);
			}
		}

		public string Title {
			get {
				return folderChooser.Title;
			}
			set {
				folderChooser.Title = value;
			}
		}

		public bool CanCreateFolders {
			get {
				return true;
			}
			set {
				// Gtk.FileChooserAction.CreateFolder is not a valid action for Gtk.FileChooserButton
			}
		}
	}
}

