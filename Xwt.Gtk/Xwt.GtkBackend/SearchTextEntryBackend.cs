//
// SearchTextEntryBackend.cs
//
// Author:
//       Lluis Sanchez Gual <lluis@xamarin.com>
//       Aaron Bockover <abockover@novell.com>
//       Gabriel Burt <gburt@novell.com>
//       Vsevolod Kukol <sevo@sevo.org>
//
// Copyright (c) 2014 Xamarin, Inc (http://www.xamarin.com)
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
#if XWT_GTK3
using GtkEntry = Gtk.Entry;
#endif

namespace Xwt.GtkBackend
{
	public class SearchTextEntryBackend: TextEntryBackend, ISearchTextEntryBackend
	{
		GtkEntry searchEntry;

		protected override Gtk.Entry TextEntry {
			get {
				return searchEntry;
			}
		}

		public override void Initialize ()
		{
			searchEntry = new GtkEntry ();
			searchEntry.PrimaryIconName = "edit-find-symbolic";
			searchEntry.PrimaryIconActivatable = false;
			searchEntry.PrimaryIconSensitive = false;
			searchEntry.Changed += ShowHideClearButton;
			searchEntry.IconRelease += ResetSearch;
			((WidgetBackend)this).Widget = searchEntry;
			searchEntry.Show ();
		}

		void ResetSearch (object o, Gtk.IconReleaseArgs args)
		{
			if (args.P0 == Gtk.EntryIconPosition.Secondary)
				Text = String.Empty;
		}

		void ShowHideClearButton (object sender, EventArgs e)
		{
			if (String.IsNullOrEmpty (this.Text)) {
				searchEntry.SecondaryIconName = null;
				searchEntry.SecondaryIconActivatable = false;
				searchEntry.SecondaryIconSensitive = false;
			} else {
				searchEntry.SecondaryIconName = "edit-clear-symbolic";
				searchEntry.SecondaryIconActivatable = true;
				searchEntry.SecondaryIconSensitive = true;
			}
		}

		public override void SetFocus ()
		{
			base.SetFocus ();
			TextEntry.GrabFocus ();
		}
	}
}

