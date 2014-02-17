// 
// CheckBoxCellView.cs
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
using Xwt.Drawing;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using Xwt.Backends;

namespace Xwt
{
	public sealed class CheckBoxCellView: CellView, ICheckBoxCellViewFrontend
	{
		CheckBoxState state;
		bool editable;
		bool allowMixed;

		public Binding ActiveBinding { get; set; }
		public Binding StateBinding { get; set; }
		public Binding EditableBinding { get; set; }
		public Binding AllowMixedBinding { get; set; }

		public CheckBoxCellView ()
		{
		}
		
		[DefaultValue (false)]
		public bool Active {
			get { return State == CheckBoxState.On; }
			set { State = value.ToCheckBoxState (); }
		}

		[DefaultValue (CheckBoxState.Off)]
		public CheckBoxState State {
			get {
				if (StateBinding != null)
					return GetValue (StateBinding, state);
				return GetValue (ActiveBinding, false).ToCheckBoxState ();
			}
			set {
				if (!value.IsValid ()) {
					throw new ArgumentOutOfRangeException ("Invalid checkbox state");
				}
				state = value;
			}
		}
		
		[DefaultValue (false)]
		public bool Editable {
			get {
				return GetValue (EditableBinding, editable);
			}
			set {
				editable = value;
			}
		}

		[DefaultValue (false)]
		public bool AllowMixed {
			get {
				return GetValue (AllowMixedBinding, allowMixed);
			}
			set {
				allowMixed = value;
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
