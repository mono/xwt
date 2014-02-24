//
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
using Xwt.Backends;

namespace Xwt.WPFBackend
{
	class CanvasCellViewPanel : CustomPanel
	{
		public CanvasCellViewPanel()
		{
			DataContextChanged += OnDataChanged;
		}

		void OnDataChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (e.OldValue != null && e.OldValue is ValuesContainer)
				((ValuesContainer)e.OldValue).PropertyChanged -= CanvasCellRenderer_PropertyChanged;

			if (e.NewValue is ValuesContainer)
			{
				((ValuesContainer)DataContext).PropertyChanged += CanvasCellRenderer_PropertyChanged;
			}
		}

		void CanvasCellRenderer_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			InvalidateMeasure();
			InvalidateVisual();
		}

		public event DependencyPropertyChangedEventHandler CellViewChanged;

		public static readonly DependencyProperty CellViewBackendProperty =
            DependencyProperty.Register("CellViewBackend", typeof(CellViewBackend),
            typeof(CanvasCellViewPanel), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnCellViewChanged)));

        public CellViewBackend CellViewBackend
		{
            get { return (CellViewBackend)GetValue(CellViewBackendProperty); }
            set { SetValue(CellViewBackendProperty, value); }
		}

		public static void OnCellViewChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			var sl = sender as CanvasCellViewPanel;
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
            CellViewBackend.Load (this);
            CellViewBackend.ApplicationContext.InvokeUserCode(delegate
			{
				DrawingContext ctx = new DrawingContext(dc, 1);
                ((ICanvasCellViewFrontend)CellViewBackend.CellView).Draw(ctx, new Rectangle(this.RenderTransform.Value.OffsetX, this.RenderTransform.Value.OffsetY, this.RenderSize.Width, this.RenderSize.Height));
			});
		}

		protected override System.Windows.Size MeasureOverride(System.Windows.Size constraint)
		{
			var size = new System.Windows.Size();
            CellViewBackend.Load(this);
            CellViewBackend.ApplicationContext.InvokeUserCode(delegate
			{
                var s = ((ICanvasCellViewFrontend)CellViewBackend.CellView).GetRequiredSize();
				size = new System.Windows.Size(s.Width, s.Height);
			});
			if (size.Width > constraint.Width)
				size.Width = constraint.Width;
			if (size.Height > constraint.Height)
				size.Height = constraint.Height;
			return size;
		}
	}
}

