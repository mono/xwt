//
// IRadioButtonBackend.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc
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
	public interface IRadioButtonBackend: IWidgetBackend
	{
		void SetContent (IWidgetBackend widget);
		void SetContent (string label);

		/// <summary>
		/// Gets or sets the group of the radio button
		/// </summary>
		/// <value>The group.</value>
		/// <remarks>
		/// If the radio button has not been explicitly assigned to a group the getter
		/// should return a group which contains this radio as only member.
		/// If a group has previously been set using the setter, the getter must return that group.
		/// The setter will be invoked using a value obtained from the getter of this or any other
		/// radio button. It will never be null.
		/// </remarks>
		object Group { get; set; }

		/// <summary>
		/// Gets or sets whether the radiobutton is checked. If set to 'true', this radio button is checked,
		/// and any other button in the same group has to be unchecked. If set to 'false', this radio buttton
		/// is unchecked, and no other button is checked.
		/// </summary>
		bool Active { get; set; }
	}
	
	public interface IRadioButtonEventSink: IWidgetEventSink
	{
		void OnClicked ();
		void OnToggled ();
	}
	
	public enum RadioButtonEvent
	{
		Clicked = 1,
		ActiveChanged = 2
	}
}

