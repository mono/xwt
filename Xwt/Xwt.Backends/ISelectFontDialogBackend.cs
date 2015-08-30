//
// ISelectFontDialogBackend.cs
//
// Author:
//       Vsevolod Kukol <sevo@sevo.org>
//
// Copyright (c) 2015 Vsevolod Kukol
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
using Xwt.Drawing;

namespace Xwt.Backends
{
	public interface ISelectFontDialogBackend
	{
		/// <summary>
		/// Gets or sets the title of the dialog
		/// </summary>
		string Title { get; set; }

		/// <summary>
		/// Gets or sets the selected font
		/// </summary>
		Font SelectedFont { get; set; }

		/// <summary>
		/// Gets or sets the text used for the font preview.
		/// </summary>
		/// <value>The preview text.</value>
		string PreviewText { get; set; }

		/// <summary>
		/// Runs the dialog, allowing the user to select a font
		/// </summary>
		/// <param name='parent'>
		/// Parent window (the dialog will be modal to this window). It can be null.
		/// </param>
		/// <param name='title'>
		/// Title of the dialog
		/// </param>
		/// <returns>
		/// <c>true</c> if the user clicked OK, <c>false</c> otherwise
		/// </returns>
		/// <remarks>
		/// The Run method will always be called once (and only once).
		/// The dialog must be shown in modal mode. The method returns when the user clicks on
		/// OK or Cancel. The dialog must be already closed when this method returns.
		/// </remarks>
		bool Run (IWindowFrameBackend parent);

		/// <summary>
		/// Frees native resources
		/// </summary>
		void Dispose ();	
	}
}

