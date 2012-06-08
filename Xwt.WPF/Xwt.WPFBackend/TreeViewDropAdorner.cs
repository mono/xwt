//
// TreeViewDropAdorner.cs
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

using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Xwt.WPFBackend
{
	public class TreeViewDropAdorner
		: Adorner
	{
		public TreeViewDropAdorner (ExTreeViewItem item, RowDropPosition position)
			: base (item)
		{
			IsHitTestVisible = false;
			
			double indent = ((item.Level + 1) * LevelToIndentConverter.IndentSize) + 5;

			this.shape = new System.Windows.Shapes.Rectangle();
			this.shape.Stroke = Brushes.DimGray;
			this.shape.Width = item.ActualWidth - indent;

			switch (position) {
				case RowDropPosition.Into:
					this.shape.Height = item.ActualHeight;
					this.shape.Margin = new Thickness (indent, 0, 0, 0);
					this.shape.StrokeDashArray = new DoubleCollection { 1, 0, 1 };
				break;

				case RowDropPosition.Before:
					this.shape.Height = 1;
					this.shape.Margin = new Thickness (indent, 0, 0, 0);
				break;

				case RowDropPosition.After:
					this.shape.Height = 1;
					this.shape.Margin = new Thickness (indent, item.ActualHeight, 0, 0);
				break;
			}
		}

		private readonly Shape shape;

		protected override int VisualChildrenCount
		{
		    get { return 1; }
		}

		protected override Visual GetVisualChild (int index)
		{
		    return this.shape;
		}

		protected override System.Windows.Size MeasureOverride(System.Windows.Size constraint)
		{
		    this.shape.Measure (constraint);
		    return this.shape.DesiredSize;
		}

		protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
		{
		    this.shape.Arrange (new Rect (this.shape.DesiredSize));
		    return finalSize;
		}
	}
}