// 
// LabelBackend.cs
//  
// Author:
//       Carlos Alberto Cortez <calberto.cortez@gmail.com>
// 
// Copyright (c) 2012 Carlos Alberto Cortez
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
using System.Linq;
using System.Text;
using System.Windows;
using SWC = System.Windows.Controls;

using Xwt.Backends;

namespace Xwt.WPFBackend
{
	public class LabelBackend : WidgetBackend, ILabelBackend
	{
		public LabelBackend ()
		{
			Widget = new SWC.Label ();
		}

		SWC.Label Label {
			get { return (SWC.Label)Widget; }
		}

		public string Text {
			get { return (string)Label.Content; }
			set { Label.Content = value; }
		}

		public Alignment TextAlignment {
			get { return DataConverter.ToXwtAlignment (Label.HorizontalContentAlignment); }
			set { Label.HorizontalContentAlignment = DataConverter.ToWpfAlignment (value); }
		}
	}
}
