//
// TextCellViewBackend.cs
//
// Author:
//       Vsevolod Kukol <sevoku@microsoft.com>
//
// Copyright (c) 2017 (c) Vsevolod Kukol
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
using System.Windows;
using Xwt.Backends;
using SWC = System.Windows.Controls;

namespace Xwt.WPFBackend
{
	class TextCellViewBackend : CellViewBackend
	{
		public override void OnInitialize (CellView cellView, FrameworkElementFactory factory)
		{
			factory.AddHandler (SWC.Primitives.TextBoxBase.TextChangedEvent, new SWC.TextChangedEventHandler (OnTextChanged));
			base.OnInitialize (cellView, factory);
		}

		void OnTextChanged (object sender, SWC.TextChangedEventArgs routedEventArgs)
		{
			var view = (ITextCellViewFrontend)CellView;
			var cell = sender as SWC.TextBox;
			Load (cell);
			SetCurrentEventRow ();
			routedEventArgs.Handled = view.RaiseTextChanged (cell.Text);
			if (routedEventArgs.Handled) {
				var cursorPos = cell.SelectionStart;
				cell.Text = view.Text;
				cell.SelectionStart = cursorPos;
			} else {
				var e = cell.GetBindingExpression (SWC.TextBox.TextProperty);
				e?.UpdateSource();
			}
		}
	}
}
