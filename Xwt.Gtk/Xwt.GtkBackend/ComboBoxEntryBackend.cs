// 
// ComboBoxEntryBackend.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2011 Xamarin Inc
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
	public class ComboBoxEntryBackend: ComboBoxBackend, IComboBoxEntryBackend
	{
		TextEntryBackend entryBackend;
		int textColumn = 0;
		bool completes;

		public bool Completes
		{
			get
			{
				if (Widget.Model == null)
					return completes;
				return entryBackend.TextEntry?.Completion?.Model == Widget.Model;
			}
			set
			{
				completes = value;

				if (!completes)
				{
					// unset model only if not using custom completions through TextEntryBackend
					if (entryBackend.TextEntry.Completion?.Model == Widget.Model)
						entryBackend.TextEntry.Completion.Model = null;
				} else {
					BindCompletion ();
				}
			}
		}
		
		protected override Gtk.Widget CreateWidget ()
		{
			var c = GtkWorkarounds.CreateComboBoxEntry ();
			entryBackend = new CustomComboEntryBackend ((Gtk.Entry)c.Child);
			return c;
		}

		void BindCompletion()
		{
			var completion = entryBackend.TextEntry.Completion;
			if (completion == null) {
				completion = entryBackend.TextEntry.Completion = GtkBackend.TextEntryBackend.CreateCompletion();
			}
			completion.Model = Widget.Model;
			SyncCompletionColumn (completion, textColumn);
		}

		static void SyncCompletionColumn (Gtk.EntryCompletion completion, int textColumn)
		{
			if (completion.TextColumn != textColumn) {
				// Gtk.EntryCompletion.TextColumn is using gtk_entry_completion_set_text_column
				// which will create a new Cell on each change. Since we don't want this "convenience",
				// we need to use the "text_column" property instead, as suggested in
				// https://developer.gnome.org/gtk2/stable/GtkEntryCompletion.html#gtk-entry-completion-set-text-column
				using (GLib.Value val = new GLib.Value(textColumn)) {
					completion.SetProperty("text_column", val);
				}
			}
		}

		protected override void OnSourceSet()
		{
			base.OnSourceSet();
			if (completes && entryBackend.TextEntry?.Completion?.Model != Widget.Model) {
				BindCompletion ();
			}
		}

		public void SetTextColumn (int column)
		{
			textColumn = column;
			Widget.SetTextColumn (column);
			if (Completes)
				SyncCompletionColumn (entryBackend.TextEntry.Completion, textColumn);
		}
		
		public ITextEntryBackend TextEntryBackend {
			get {
				return entryBackend;
			}
		}

		protected override void OnSetBackgroundColor (Xwt.Drawing.Color color)
		{
			Widget.SetBackgroundColor (color);
			Widget.SetChildBackgroundColor (color);
		}
	}
	
	class CustomComboEntryBackend: TextEntryBackend
	{
		public CustomComboEntryBackend (Gtk.Entry entry)
		{
			if (entry == null)
				throw new ArgumentNullException(nameof(entry));
			Widget = entry;
		}
		
		public override void Initialize ()
		{
			TextEntry.Show ();
		}
	}
}
