// 
// KeyEventArgs.cs
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

namespace Xwt
{
	/// <summary>
	/// Key event arguments, containing information about pressed/released keys.
	/// </summary>
	public class KeyEventArgs: EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Xwt.KeyEventArgs"/> class.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="modifiers">The modifier keys.</param>
		/// <param name="isRepeat">the key has been pressed more then once.</param>
		/// <param name="timestamp">The timestamp of the key event.</param>
		public KeyEventArgs (Key key, ModifierKeys modifiers, bool isRepeat, long timestamp)
		{
			this.Key = key;
			this.Modifiers = modifiers;
			this.IsRepeat = isRepeat;
			this.Timestamp = timestamp;
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Xwt.KeyEventArgs"/> class.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="nativeKeyCode">The native key code.</param>
		/// <param name="modifiers">The modifier keys.</param>
		/// <param name="isRepeat">the key has been pressed more then once.</param>
		/// <param name="timestamp">The timestamp of the key event.</param>
		public KeyEventArgs (Key key, int nativeKeyCode, ModifierKeys modifiers, bool isRepeat, long timestamp)
			: this (key, modifiers, isRepeat, timestamp)
		{
			this.NativeKeyCode = nativeKeyCode;
		}
		
		/// <summary>
		/// Gets the key.
		/// </summary>
		/// <value>The key.</value>
		public Key Key { get; private set; }

		/// <summary>
		/// Gets the native key code.
		/// </summary>
		/// <value>The native key code.</value>
		public int NativeKeyCode { get; private set; }
		
		/// <summary>
		/// Gets the modifier keys.
		/// </summary>
		/// <value>The modifier keys.</value>
		public ModifierKeys Modifiers { get; private set; }
		
		/// <summary>
		/// Gets a value indicating whether the key has been pressed more then once.
		/// </summary>
		/// <value><c>true</c> if the key has been pressed more then once; otherwise, <c>false</c>.</value>
		public bool IsRepeat { get; private set; }
		
		/// <summary>
		/// Gets the time when this event occurred.
		/// </summary>
		/// <value>The timestamp of the event.</value>
		public long Timestamp { get; private set; }
		
		/// <summary>
		/// Gets or sets a value indicating whether this event has been handled.
		/// </summary>
		/// <value><c>true</c> this event has been handled; otherwise, <c>false</c>.</value>
		/// <remarks>
		/// Setting <see cref="Xwt.KeyEventArgs.Handled"/> to <c>true</c> will mark the event
		/// as handled and prevent the event from bubbling up in the widget hierarchy.
		/// Handled events will be ignored by a backend in most cases.
		/// For example marking a character key press of a <see cref="Xwt.TextEntry"/> as handled will prevent
		/// the backend from processing the key and adding the appropriate character to <see cref="Xwt.TextEntry.Text"/>.
		/// </remarks>
		public bool Handled { get; set; }
	}
}

