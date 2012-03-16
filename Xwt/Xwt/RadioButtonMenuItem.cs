// 
// RadioButtonMenuItem.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2012 Xamarin Inc
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
using Xwt.Backends;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;

namespace Xwt
{
	public class RadioButtonMenuItem: MenuItem
	{
		RadioButtonGroup radioGroup;
		
		public RadioButtonMenuItem ()
		{
		}
		
		public RadioButtonMenuItem (Command command): base (command)
		{
		}
		
		public RadioButtonMenuItem (string label): base (label)
		{
		}
		
		new IRadioButtonMenuItemBackend Backend {
			get { return (IRadioButtonMenuItemBackend) base.Backend; }
		}
		
		[DefaultValue (true)]
		public bool Checked {
			get { return Backend.Checked; }
			set {
				Backend.Checked = value;
				if (value)
					ValidateCheckedValue ();
			}
		}
		
		public RadioButtonGroup RadioButtonGroup {
			get { return radioGroup; }
			set {
				if (radioGroup != null)
					radioGroup.Items.Remove (this);
				radioGroup = value;
				if (radioGroup != null) {
					radioGroup.Items.Add (this);
					if (Checked)
						ValidateCheckedValue ();
				}
			}
		}
		
		internal override void DoClick ()
		{
			ValidateCheckedValue ();
			base.DoClick ();
		}
		
		void ValidateCheckedValue ()
		{
			if (radioGroup != null) {
				foreach (var rb in radioGroup.Items.OfType<RadioButtonMenuItem> ()) {
					if (rb != this && rb.Checked)
						rb.Checked = false;
				}
			}
		}
	}
	
	public class RadioButtonGroup
	{
		internal List<object> Items = new List<object> ();
	}
}

