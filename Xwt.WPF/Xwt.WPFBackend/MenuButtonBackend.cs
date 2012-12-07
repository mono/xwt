//
// MenuButtonBackend.cs
//
// Author:
//       Eric Maupin <ermau@xamarin.com>
//
// Copyright (c) 2012 Xamarin, Inc.
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
using System.Windows;
using Xwt.Backends;


namespace Xwt.WPFBackend
{
	public class MenuButtonBackend
		: ButtonBackend, IMenuButtonBackend
	{
		public MenuButtonBackend()
			: base (new DropDownButton())
		{
			DropDownButton.MenuOpening += OnMenuOpening;
		}

		protected DropDownButton DropDownButton {
			get { return (DropDownButton) Button; }
		}

		protected IMenuButtonEventSink MenuButtonEventSink {
			get { return (IMenuButtonEventSink) EventSink; }
		}

		public override void SetButtonType (ButtonType type)
		{
			switch (type) {
			case ButtonType.Normal:
				DropDownButton.Style = null;
				break;

			case ButtonType.DropDown:
				DropDownButton.Style = (Style) ButtonResources["MenuDropDown"];
				break;
			}

			DropDownButton.InvalidateMeasure();
		}

		private void OnMenuOpening (object sender, DropDownButton.MenuOpeningEventArgs e)
		{
			Context.InvokeUserCode (() =>
				e.ContextMenu = ((MenuBackend) MenuButtonEventSink.OnCreateMenu ()).CreateContextMenu ());
		}
	}
}