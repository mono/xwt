//
// SaveFileDialogBackend.cs
//
// Author:
//       Jan Strnadek <jan.strnadek@gmail.com>
//
// Copyright (c) 2014 strnadj
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

using System.Collections.Generic;
using AppKit;
using Foundation;
using Xwt.Backends;

namespace Xwt.Mac
{
	public class SaveFileDialogBackend : FileDialogBackend, ISaveFileDialogBackend
	{
		protected override NSSavePanel GetFilePanel ()
		{
			return NSSavePanel.SavePanel;
		}

		protected override void OnInitialize (IEnumerable<FileDialogFilter> filters, bool multiselect, string initialFileName)
		{
			base.OnInitialize (filters, multiselect, initialFileName);

			Panel.Prompt = Application.TranslationCatalog.GetString ("Select File");
		}
	}
}