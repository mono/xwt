﻿//
// SeparatorMenuItemBackend.cs
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

using Xwt.Backends;
using SWC = System.Windows.Controls;

namespace Xwt.WPFBackend
{
	public class SeparatorMenuItemBackend
		: MenuItemBackend, ISeparatorMenuItemBackend
	{
		public SeparatorMenuItemBackend()
			: base (new SWC.Separator())
		{
            _visible = true;   
		}

        public MenuBackend Parent { get; set; }

        private bool _visible;
        private int _savedIndex;

        public new bool Visible
        {
            get { return _visible; }
            set { SetVisibilit(value); }
        }

        private void SetVisibilit(bool value)
        {
            _visible = value;
            if (!_visible)
            {
                _savedIndex = Parent.Items.IndexOf(this);
                Parent.RemoveItem(this);
            }
            else
            {
                Parent.InsertItem(_savedIndex, this);
            }
        }
    }
}