//
// FileSelectorBackend.cs
//
// Author:
//       Harry <cra0zy@gmail.com>
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

using System;
using Xwt.Backends;

namespace Xwt.GtkBackend
{
	public class FileSelectorBackend : WidgetBackend, IFileSelectorBackend
	{
		private FileDialogFilter activeFilter;
		private Gtk.FileChooserButton fileChooser;

		public override void Initialize ()
		{
			fileChooser = new Gtk.FileChooserButton ("", Gtk.FileChooserAction.Open);
			fileChooser.ButtonReleaseEvent += Base_Widget_ButtonPressEvent;
			fileChooser.SelectionChanged += (sender, e) => EventSink.OnFileChanged ();

			Widget = fileChooser;
			Widget.ShowAll ();
		}

		protected new Gtk.FileChooserButton Widget {
			get { return (Gtk.FileChooserButton)base.Widget; }
			set { base.Widget = value; }
		}

		protected new IFileSelectorEventSink EventSink {
			get { return (IFileSelectorEventSink)base.EventSink; }
		}

		public string CurrentFolder {
			get {
				return fileChooser.CurrentFolder;
			}
			set {
				fileChooser.SetCurrentFolder (value);
			}
		}

		public FileDialogFilter ActiveFilter {
			get {
				return activeFilter;
			}
			set {
				var filters = ((FileSelector)Frontend).Filters;

				if (!filters.Contains (value))
					throw new ArgumentException ("The active filter must be one of the filters included in the Filters collection");

				activeFilter = value;
			}
		}

		public string FileName {
			get {
				return fileChooser.Filename;
			}
			set {
				fileChooser.SetFilename (value);
			}
		}

		public FileSelectionMode FileSelectionMode {
			get {
				return (fileChooser.Action == Gtk.FileChooserAction.Open) ?
					FileSelectionMode.Open : FileSelectionMode.Save;
			}
			set {
				fileChooser.Action = (value == FileSelectionMode.Open) ?
					Gtk.FileChooserAction.Open : Gtk.FileChooserAction.Save;
			}
		}

		public string Title {
			get {
				return fileChooser.Title;
			}
			set {
				fileChooser.Title = value;
			}
		}

		[GLib.ConnectBefore]
		private void Base_Widget_ButtonPressEvent (object o, EventArgs args)
		{
			var gtkfilters = fileChooser.Filters;
			foreach (var filter in gtkfilters)
				fileChooser.RemoveFilter (filter);

			var filters = ((FileSelector)Frontend).Filters;
			foreach (var filter in filters) {
				var gtkfilter = new Gtk.FileFilter ();

				if (!string.IsNullOrEmpty (filter.Name))
					gtkfilter.Name = filter.Name;

				if (filter.Patterns != null)
					foreach (var pattern in filter.Patterns)
						gtkfilter.AddPattern (pattern);

				fileChooser.AddFilter (gtkfilter);

				if (filter == activeFilter)
					fileChooser.Filter = gtkfilter;
			}
		}
	}
}

