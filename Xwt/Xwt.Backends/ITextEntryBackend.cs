// 
// ITextEntryBackend.cs
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

namespace Xwt.Backends
{
	public interface ITextEntryBackend: ITextBoxBackend
	{
		[Obsolete("Use ITextAreaBackend instead")]
		bool MultiLine { get; set; }
	}

	public interface ITextEntryEventSink: ITextBoxEventSink
	{
	}

	public interface ITextBoxBackend: IWidgetBackend
	{
		string Text { get; set; }
		Alignment TextAlignment { get; set; }
		string PlaceholderText { get; set; }
		bool ReadOnly { get; set; }
		bool ShowFrame { get; set; }
		int CursorPosition { get; set; }
		int SelectionStart { get; set; }
		int SelectionLength { get; set; }
		string SelectedText { get; set; }
		bool HasCompletions { get; }
		void SetCompletions (string[] completions);
		void SetCompletionMatchFunc (Func<string, string, bool> matchFunc);
	}
	
	public interface ITextBoxEventSink: IWidgetEventSink
	{
		void OnChanged ();
		void OnActivated ();
		void OnSelectionChanged ();
	}
	
	public enum TextBoxEvent
	{
		Changed,
		Activated,
		SelectionChanged
	}

	public enum TextEntryEvent
	{
		[Obsolete("Use Xwt.Backends.TextBoxEvent.Changed instead")]
		Changed,
		[Obsolete("Use Xwt.Backends.TextBoxEvent.Activated instead")]
		Activated,
		[Obsolete("Use Xwt.Backends.TextBoxEvent.SelectionChanged instead")]
		SelectionChanged
	}
}

