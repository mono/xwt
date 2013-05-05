﻿//
// CanvasCellViewBackend.cs
//
// Author:
//       David Karlaš <david.karlas@gmail.com>
//
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
using System.Windows;

namespace Xwt.WPFBackend
{
	class CanvasCellViewBackend : ExCanvas, ICellDataSource
	{
		public CanvasCellViewBackend()
		{
			DataContextChanged += OnDataChanged;
		}

		void OnDataChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (e.OldValue != null)
				((ValuesContainer)e.OldValue).PropertyChanged -= CanvasCellRenderer_PropertyChanged;
			((ValuesContainer)DataContext).PropertyChanged += CanvasCellRenderer_PropertyChanged;
		}

		void CanvasCellRenderer_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			InvalidateMeasure();
			InvalidateVisual();
		}

		public event DependencyPropertyChangedEventHandler CellViewChanged;

		public static readonly DependencyProperty CellViewProperty =
		 DependencyProperty.Register("CellView", typeof(CanvasCellView),
		 typeof(CanvasCellViewBackend), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnCellViewChanged)));

		public CanvasCellView CellView
		{
			get { return (CanvasCellView)GetValue(CellViewProperty); }
			set { SetValue(CellViewProperty, value); }
		}

		public static void OnCellViewChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			var sl = sender as CanvasCellViewBackend;
			if (sl != null)
				sl.RaiseCellViewChangedEvent(e);
		}

		private void RaiseCellViewChangedEvent(DependencyPropertyChangedEventArgs e)
		{
			if (this.CellViewChanged != null)
				this.CellViewChanged(this, e);
		}
		
		protected override void OnRender(System.Windows.Media.DrawingContext dc)
		{
			base.OnRender(dc);
			var r = (ICanvasCellRenderer)CellView;
			((CellView)CellView).Initialize(this);
			r.ApplicationContext.InvokeUserCode(delegate
			{
				DrawingContext ctx = new DrawingContext(dc, 1);
				r.Draw(ctx, new Rectangle(this.RenderTransform.Value.OffsetX, this.RenderTransform.Value.OffsetY, this.RenderSize.Width, this.RenderSize.Height));
			});
		}

		protected override System.Windows.Size MeasureOverride(System.Windows.Size constraint)
		{
			var r = (ICanvasCellRenderer)CellView;
			var size = new System.Windows.Size();
			((CellView)CellView).Initialize(this);
			r.ApplicationContext.InvokeUserCode(delegate
			{
				var s = r.GetRequiredSize();
				size = new System.Windows.Size(s.Width, s.Height);
			});
			if (size.Width > constraint.Width)
				size.Width = constraint.Width;
			if (size.Height > constraint.Height)
				size.Height = constraint.Height;
			return size;
		}

		public object GetValue(IDataField field)
		{
			if (DataContext == null)
				return null;
			return ((ValuesContainer)DataContext)[field.Index];
		}
	}
}

