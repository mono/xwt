//
// SearchTextEntryBackendGtk3.cs
//
// Author:
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
using Xwt.CairoBackend;

namespace Xwt.GtkBackend
{
	public class SearchTextEntryBackend: TextEntryBackend, ISearchTextEntryBackend
	{
		public override void Initialize ()
		{
			base.Initialize ();
			TextEntry.PrimaryIconName = "edit-find-symbolic";
			TextEntry.PrimaryIconActivatable = false;
			TextEntry.PrimaryIconSensitive = false;
			TextEntry.Changed += ShowHideClearButton;
			TextEntry.IconRelease += ResetSearch;
		}

		void ResetSearch (object o, Gtk.IconReleaseArgs args)
		{
			if (args.P0 == Gtk.EntryIconPosition.Secondary)
				Text = String.Empty;
		}

		void ShowHideClearButton (object sender, EventArgs e)
		{
			if (String.IsNullOrEmpty (this.Text)) {
				TextEntry.SecondaryIconName = null;
				TextEntry.SecondaryIconActivatable = false;
				TextEntry.SecondaryIconSensitive = false;
			} else {
				TextEntry.SecondaryIconName = "edit-clear-symbolic";
				TextEntry.SecondaryIconActivatable = true;
				TextEntry.SecondaryIconSensitive = true;
			}
		}
	}
}



