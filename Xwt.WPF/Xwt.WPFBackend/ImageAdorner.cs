//
// ImageAdorner.cs
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
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Xwt.WPFBackend
{
	public class ImageAdorner
		: Adorner
	{
		public ImageAdorner (UIElement owner, object image)
			: base (owner)
		{
			var source = DataConverter.AsImageSource (image);
			this.child = new Image {
			    Source = source,
				Width = source.Width,
				Height = source.Height
			};
		}

		public System.Windows.Point Offset
		{
			set
			{
				this.offset = value;
				UpdatePosition ();
			}
		}

		public override GeneralTransform GetDesiredTransform (GeneralTransform transform)
		{
			GeneralTransformGroup result = new GeneralTransformGroup();
			result.Children.Add (transform);
			result.Children.Add (new TranslateTransform (this.offset.X, this.offset.Y));
			return result;
		}

		private System.Windows.Point offset;
		private readonly UIElement child;

		protected override System.Windows.Size MeasureOverride (System.Windows.Size constraint)
		{
		    this.child.Measure (constraint);
		    return this.child.DesiredSize;
		}

		protected override System.Windows.Size ArrangeOverride (System.Windows.Size finalSize)
		{
		    this.child.Arrange (new Rect (this.child.DesiredSize));
		    return finalSize;
		}

		protected override Visual GetVisualChild (int index)
		{
			return this.child;
		}

		protected override int VisualChildrenCount
		{
			get { return 1; }
		}

		protected void UpdatePosition()
		{
			AdornerLayer layer = (AdornerLayer) Parent;
			if (layer != null)
				layer.Update (AdornedElement);
		}
	}
}