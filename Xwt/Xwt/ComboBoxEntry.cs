// 
// ComboBoxEntry.cs
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

namespace Xwt
{
	[BackendType (typeof(IComboBoxEntryBackend))]
	public class ComboBoxEntry: ComboBox
	{
		TextEntry entry;
		IDataField<object> textField;
		
		protected new class WidgetBackendHost: ComboBox.WidgetBackendHost
		{
			protected override void OnBackendCreated ()
			{
				base.OnBackendCreated ();
				((IComboBoxEntryBackend)Backend).SetTextColumn (0);
			}
		}
		
		public ComboBoxEntry ()
		{
			entry = new CustomComboTextEntry (Backend.TextEntryBackend);
		}
		
		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}
		
		IComboBoxEntryBackend Backend {
			get { return (IComboBoxEntryBackend) BackendHost.Backend; }
		}
		
		public TextEntry TextEntry {
			get {
				return entry;
			}
		}
		
		/// <summary>
		/// Gets or sets the column that contains the text to be shown in the text entry when an item is selected
		/// </summary>
		/// <value>
		/// The text field.
		/// </value>
		public IDataField<object> TextField {
			get { return textField; }
			set {
				textField = value;
				if (value != null)
					Backend.SetTextColumn (value.Index);
				else
					Backend.SetTextColumn (0);
			}
		}
	}
	
	class CustomComboTextEntry: TextEntry
	{
		ITextEntryBackend backend;
		
		protected new class WidgetBackendHost: TextEntry.WidgetBackendHost
		{
			protected override IBackend OnCreateBackend ()
			{
				return ((CustomComboTextEntry)Parent).backend;
			}
		}
		
		protected override Xwt.Backends.BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}
		
		public CustomComboTextEntry (ITextEntryBackend backend)
		{
			this.backend = backend;
		}
	}
}

