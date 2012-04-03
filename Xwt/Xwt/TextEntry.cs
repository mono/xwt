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
	public class TextEntry: Widget
	{
		EventHandler changed;
		
		static TextEntry ()
		{
			MapEvent (TextEntryEvent.Changed, typeof(TextEntry), "OnChanged");
		}
		
		protected new class WidgetBackendHost: Widget.WidgetBackendHost, ITextEntryEventSink
		{
			public void OnChanged ()
			{
				((TextEntry)Parent).OnChanged (EventArgs.Empty);
			}
			
			public override Size GetDefaultNaturalSize ()
			{
				return Xwt.Backends.DefaultNaturalSizes.TextEntry;
			}
		}
		
		public TextEntry ()
		{
		}
		
		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}
		
		ITextEntryBackend Backend {
			get { return (ITextEntryBackend) BackendHost.Backend; }
		}
		
		[DefaultValue ("")]
		public string Text {
			get { return Backend.Text; }
			set { Backend.Text = value; }
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
		
		protected virtual void OnChanged (EventArgs e)
		{
			if (changed != null)
				changed (this, e);
		}
		
		public event EventHandler Changed {
			add {
				BackendHost.OnBeforeEventAdd (TextEntryEvent.Changed, changed);
				changed += value;
			}
			remove {
				changed -= value;
				BackendHost.OnAfterEventRemove (TextEntryEvent.Changed, changed);
			}
		}
	}
}

