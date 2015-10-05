//
// SelectFontDialogBackend.cs
//
// Author:
//       Vsevolod Kukol <sevo@sevo.org>
//
// Copyright (c) 2015 Vsevolod Kukol
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
using Xwt.Drawing;

namespace Xwt.GtkBackend
{
	#if XWT_GTK3
	using FontSelectionDialog = Gtk3FontChooserDialog;
	#else
	using FontSelectionDialog = Gtk.FontSelectionDialog;
	#endif

	public class SelectFontDialogBackend: ISelectFontDialogBackend
	{
		readonly FontSelectionDialog dlg;
		string defaultTitle;
		string defaultPreviewText;

		public Font SelectedFont {
			get;
			set;
		}

		public string Title {
			get;
			set;
		}

		public string PreviewText {
			get;
			set;
		}

		public SelectFontDialogBackend ()
		{
			dlg = new FontSelectionDialog (null);
			defaultTitle = dlg.Title;
			defaultPreviewText = dlg.PreviewText;
		}

		public bool Run (IWindowFrameBackend parent)
		{
			if (!String.IsNullOrEmpty (Title))
				dlg.Title = Title;
			else
				dlg.Title = defaultTitle;

			if (!String.IsNullOrEmpty (PreviewText))
				dlg.PreviewText = PreviewText;
			else
				dlg.PreviewText = defaultPreviewText;

			if (SelectedFont != null)
				dlg.SetFontName (SelectedFont.ToString ());

			var p = (WindowFrameBackend)parent;
			int result = MessageService.RunCustomDialog (dlg, p != null ? p.Window : null);

			if (result == (int)Gtk.ResponseType.Ok) {
				SelectedFont = Font.FromName (dlg.FontName);
				return true;
			}
			return false;
		}

		public void Dispose ()
		{
			dlg.Destroy ();
		}
	}
}

