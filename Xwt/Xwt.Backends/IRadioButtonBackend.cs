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
		/// Creates a new radiobutton group. This group is not initially assigned to this
		/// radio button instance. This will be done later on by calling SetRadioGroup.
		/// </summary>
		/// <returns>The radio group backend</returns>
		object CreateRadioGroup ();

		/// <summary>
		/// Assigns a radio group to this radio button. This group is created by calling
		/// the CreateRadioGroup method on this or another instance.
		/// </summary>
		/// <param name="groupBackend">Group backend.</param>
		void SetRadioGroup (object groupBackend);
		
		/// <summary>
		/// Gets or sets whether the radiobutton is checked.
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

