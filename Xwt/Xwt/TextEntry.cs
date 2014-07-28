// 
// TextEntry.cs
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
using System.ComponentModel;

namespace Xwt
{
	[BackendType (typeof(ITextEntryBackend))]
	public class TextEntry: TextBox
	{
		public TextEntry ()
		{
			// TODO: DEPRECATED! Safe to remove?
			MapEvent (TextEntryEvent.Changed, typeof(TextBox), "OnChanged");
			MapEvent (TextEntryEvent.Activated, typeof(TextBox), "OnActivated");
			MapEvent (TextEntryEvent.SelectionChanged, typeof(TextBox), "OnSelectionChanged");
		}

		protected new class WidgetBackendHost: TextBox.WidgetBackendHost, ITextEntryEventSink
		{
		}

		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}

		ITextEntryBackend Backend {
			get { return (ITextEntryBackend) BackendHost.Backend; }
		}

		[Obsolete("Use TextArea widget instead")]
		[DefaultValue (false)]
		public bool MultiLine {
			get { return Backend.MultiLine; }
			set { Backend.MultiLine = value; }
		}
	}

	public abstract class TextBox: Widget
	{
		EventHandler changed, activated, selectionChanged;
		
		static TextBox ()
		{
			MapEvent (TextBoxEvent.Changed, typeof(TextBox), "OnChanged");
			MapEvent (TextBoxEvent.Activated, typeof(TextBox), "OnActivated");
			MapEvent (TextBoxEvent.SelectionChanged, typeof(TextBox), "OnSelectionChanged");
		}
		
		protected new class WidgetBackendHost: Widget.WidgetBackendHost, ITextBoxEventSink
		{
			public void OnChanged ()
			{
				((TextBox)Parent).OnChanged (EventArgs.Empty);
			}

			public void OnActivated ()
			{
				((TextBox)Parent).OnActivated (EventArgs.Empty);
			}

			public void OnSelectionChanged ()
			{
				((TextBox)Parent).OnSelectionChanged (EventArgs.Empty);
			}
			
			public override Size GetDefaultNaturalSize ()
			{
				return DefaultNaturalSizes.TextEntry;
			}
		}
		
		public TextBox ()
		{
		}
		
		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}
		
		ITextBoxBackend Backend {
			get { return (ITextBoxBackend) BackendHost.Backend; }
		}
		
		[DefaultValue ("")]
		public string Text {
			get { return Backend.Text; }
			set { Backend.Text = value; }
		}

		public Alignment TextAlignment {
			get { return Backend.TextAlignment; }
			set { Backend.TextAlignment = value; }
		}

		[DefaultValue ("")]
		public string PlaceholderText {
			get { return Backend.PlaceholderText; }
			set { Backend.PlaceholderText = value; }
		}
		
		[DefaultValue (false)]
		public bool ReadOnly {
			get { return Backend.ReadOnly; }
			set { Backend.ReadOnly = value; }
		}
		
		[DefaultValue (true)]
		public bool ShowFrame {
			get { return Backend.ShowFrame; }
			set { Backend.ShowFrame = value; }
		}

		[DefaultValue (0)]
		public int CursorPosition {
			get { return Backend.CursorPosition; }
			set { Backend.CursorPosition = value; }
		}

		[DefaultValue (0)]
		public int SelectionStart {
			get { return Backend.SelectionStart; }
			set { Backend.SelectionStart = value; }
		}

		[DefaultValue (0)]
		public int SelectionLength {
			get { return Backend.SelectionLength; }
			set { Backend.SelectionLength = value; }
		}

		[DefaultValue ("")]
		public string SelectedText {
			get { return Backend.SelectedText; }
			set { Backend.SelectedText = value; }
		}

		[DefaultValue (false)]
		public bool HasCompletions {
			get { return Backend.HasCompletions; }
		}

		public void SetCompletions (string[] completions)
		{
			Backend.SetCompletions (completions);
		}

		public void SetCompletionMatchFunction (Func<string, string, bool> matchFunc)
		{
			Backend.SetCompletionMatchFunc (matchFunc);
		}

		protected virtual void OnChanged (EventArgs e)
		{
			if (changed != null)
				changed (this, e);
		}
		
		public event EventHandler Changed {
			add {
				BackendHost.OnBeforeEventAdd (TextBoxEvent.Changed, changed);
				changed += value;
			}
			remove {
				changed -= value;
				BackendHost.OnAfterEventRemove (TextBoxEvent.Changed, changed);
			}
		}

		protected virtual void OnSelectionChanged (EventArgs e)
		{
			if (selectionChanged != null)
				selectionChanged (this, e);
		}

		public event EventHandler SelectionChanged {
			add {
				BackendHost.OnBeforeEventAdd (TextBoxEvent.SelectionChanged, selectionChanged);
				selectionChanged += value;
			}
			remove {
				selectionChanged -= value;
				BackendHost.OnAfterEventRemove (TextBoxEvent.SelectionChanged, selectionChanged);
			}
		}

		protected virtual void OnActivated (EventArgs e)
		{
			if (activated != null)
				activated (this, e);
		}

		public event EventHandler Activated {
			add {
				BackendHost.OnBeforeEventAdd (TextBoxEvent.Activated, activated);
				activated += value;
			}
			remove {
				activated -= value;
				BackendHost.OnAfterEventRemove (TextBoxEvent.Activated, activated);
			}
		}
	}
}

