//
// MouseCursors.cs
//
// Author:
//       Vsevolod Kukol <sevo@sevo.org>
//
// Copyright (c) 2014 Vsevolod Kukol
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
using System.Reflection;
using Xwt;
using Xwt.Drawing;

namespace Samples
{
	public class MouseCursors: VBox
	{
		public MouseCursors ()
		{
			PackStart (new Label ("Move the mouse over the labels \nto see the cursors:"));
			var cursorTypes = typeof (CursorType).GetFields (BindingFlags.Public | BindingFlags.Static);
			var perRow = 6;

			HBox row = null;
			for (var i = 0; i < cursorTypes.Length; i++) {
				if (cursorTypes [i].FieldType != typeof (CursorType))
					continue;

				if ((i % perRow) == 0) {
					if (row != null)
						PackStart (row);
					row = new HBox ();
				}

				var cursor = (CursorType)cursorTypes [i].GetValue (typeof(CursorType));
				var label = new Label (cursorTypes [i].Name);
				label.BackgroundColor = Colors.White;
				label.Cursor = cursor;

				row.PackStart (label);
			}
			if (row != null)
				PackStart (row);
		}
	}
}

