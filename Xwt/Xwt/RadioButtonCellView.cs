//
// RadioButtonCellView.cs
//
// Author:
//       Vsevolod Kukol <sevoku@microsoft.com>
//
// Copyright (c) 2016 Microsoft Corporation
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xwt.Backends;

namespace Xwt
{
	public class RadioButtonCellView: CellView, IRadioButtonCellViewFrontend
	{
		bool active;
		bool editable;

		public IDataField<bool> ActiveField { get; set; }
		public IDataField<bool> EditableField { get; set; }

		public RadioButtonCellView ()
		{
		}

		public RadioButtonCellView (IDataField<bool> field)
		{
			ActiveField = field;
		}

		[DefaultValue (false)]
		public bool Active {
			get { return GetValue (ActiveField, active); }
			set { active = value; }
		}

		[DefaultValue (false)]
		public bool Editable {
			get {
				return GetValue (EditableField, editable);
			}
			set {
				editable = value;
			}
		}

		public event EventHandler<WidgetEventArgs> Toggled;

		/// <summary>
		/// Raises the toggled event
		/// </summary>
		/// <returns><c>true</c>, if the event was handled, <c>false</c> otherwise.</returns>
		public bool RaiseToggled ()
		{
			if (Toggled != null) {
				var args = new WidgetEventArgs ();
				Toggled (this, args);
				return args.Handled;
			}
			return false;
		}
	}
}
