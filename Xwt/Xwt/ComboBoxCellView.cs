// 
// ComboBoxCellView.cs
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
using System.Collections;

namespace Xwt {
	public sealed class ComboBoxCellView: CellView
	{
		bool editable;
		int selectedIndex;
        IEnumerable source;
        string displayMemberPath;
        string valueMemberPath;

        // Datentyp prüfen, muss IENumerable sein, außerdem wird noch eine Property benötigt
		public IDataField<IEnumerable> SourceField{ get; set; }

		public IDataField<bool> EditableField { get; set; }
        public IDataField<string> DisplayMemberPathField {get; set;}
        public IDataField<string> ValueMemberPathField { get; set; }
        public IDataField<int> SelectedIndexField { get; set; }

		public ComboBoxCellView ()
		{
		}

		public ComboBoxCellView (IDataField<IEnumerable> field)
		{
			SourceField = field;
		}

        public int SelectedIndex
        {
            get { return GetValue(SelectedIndexField, SelectedIndex); }
            set { SelectedIndex = value; }
        }

        public IEnumerable Source
        {
            get { return GetValue(SourceField, source); }
            set { source = value; }
        }

        public string DisplayMemberPath
        {
            get { return GetValue(DisplayMemberPathField, displayMemberPath); }
            set { displayMemberPath = value; }
        }

        public string ValueMemberPath
        {
            get { return GetValue(ValueMemberPathField, valueMemberPath); }
            set { valueMemberPath = value; }
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

        public event EventHandler<WidgetEventArgs> SelectedIndexChanged;

        /// <summary>
        /// Raises the toggled event
        /// </summary>
        /// <returns><c>true</c>, if the event was handled, <c>false</c> otherwise.</returns>
        public bool RaiseToggled()
        {
            if (SelectedIndexChanged != null)
            {
                var args = new WidgetEventArgs();
                SelectedIndexChanged(this, args);
                return args.Handled;
            }
            return false;
        }
	}
}
