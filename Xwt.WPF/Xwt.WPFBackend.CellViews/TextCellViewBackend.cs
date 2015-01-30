//
// TextCellViewBackend.cs
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
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Xwt.Backends;
using Xwt.WPFBackend;

namespace Xwt.WPFBackend
{
	class TextCellViewBackend: CellViewBackend
	{
		public TextCellViewBackend ()
		{
		}

		public TextBlock TextBlock {
			get {
				return base.CurrentElement as TextBlock;
			}
		}

		protected override void OnLoadData ()
		{
			var view = (ITextCellViewFrontend) CellFrontend;

			if (view.Markup != null && !view.Editable && TextBlock != null) {
				FormattedText tx = FormattedText.FromMarkup (view.Markup);
				SetFormattedText (tx);
			}
		}

		public void SetFormattedText (FormattedText text)
		{
            TextBlock.SetFormattedText(text, this);
		}
    }

    class TextCellViewBlock: TextBlock
    {
        public TextCellViewBlock ()
        {
            DataContextChanged += OnDataChanged;
        }

        void OnDataChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ValuesContainer)
                ((ValuesContainer)e.OldValue).PropertyChanged -= TextCellRenderer_PropertyChanged;

            if (e.NewValue is ValuesContainer)
            {
                ((ValuesContainer)DataContext).PropertyChanged += TextCellRenderer_PropertyChanged;
            }
        }

        void TextCellRenderer_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            CellViewBackend.Load (this);
        }

        public event DependencyPropertyChangedEventHandler CellViewChanged;

        public static readonly DependencyProperty CellViewBackendProperty =
            DependencyProperty.Register("CellViewBackend", typeof(CellViewBackend),
                typeof(TextCellViewBlock), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnCellViewChanged)));

        public CellViewBackend CellViewBackend
        {
            get { return (CellViewBackend)GetValue(CellViewBackendProperty); }
            set { SetValue(CellViewBackendProperty, value); }
        }

        public static void OnCellViewChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var sl = sender as TextCellViewBlock;
            if (sl != null)
                sl.RaiseCellViewChangedEvent(e);
        }

        private void RaiseCellViewChangedEvent(DependencyPropertyChangedEventArgs e)
        {
            CellViewBackend.Load (this);
            if (this.CellViewChanged != null)
                this.CellViewChanged(this, e);
        }
    }
}

