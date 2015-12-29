// Util.cs
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
using System.Windows.Media;
using System;

namespace Xwt.WPFBackend
{
	public static class Util
	{
		public static Size GetPixelRatios (this Visual self)
		{
			var source = PresentationSource.FromVisual (self);
			if (source == null)
				return new Size (1, 1);

			Matrix m = source.CompositionTarget.TransformToDevice;
			return new Size (m.M11, m.M22);
		}

		public static double GetScaleFactor (this Visual self)
		{
			PresentationSource source = PresentationSource.FromVisual (self);
			if (source == null)
				return 1;

			Matrix m = source.CompositionTarget.TransformToDevice;
			return m.M11;
		}

		public static System.Windows.Point PointToScreenDpiAware(this Visual visual, System.Windows.Point point)
		{
			point = visual.PointToScreen(point);

			PresentationSource source = PresentationSource.FromVisual(visual);

			double scaleFactorX = source.CompositionTarget.TransformToDevice.M11;
			double scaleFactorY = source.CompositionTarget.TransformToDevice.M22;

			return new System.Windows.Point(point.X / scaleFactorX, point.Y / scaleFactorY);
		}

		public static HorizontalAlignment ToWpfHorizontalAlignment(Alignment alignment)
		{
			switch (alignment) {
			case Alignment.Start:
				return HorizontalAlignment.Left;
			case Alignment.Center:
				return HorizontalAlignment.Center;
			case Alignment.End:
				return HorizontalAlignment.Right;
			}

			throw new InvalidOperationException("Invalid alignment value: " + alignment);
        }

		public static System.Windows.Window GetParentWindow (this FrameworkElement element)
		{
			FrameworkElement current = element;
			while (current != null) {
				if (current is System.Windows.Window)
					return (System.Windows.Window)current;

				current = VisualTreeHelper.GetParent (current) as FrameworkElement;
			}

			return null;
		}
    }
}