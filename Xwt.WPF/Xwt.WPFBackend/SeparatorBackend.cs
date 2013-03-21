// 
// SeparatorBackend.cs
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
using Xwt.Backends;
using WindowsRectangle = System.Windows.Shapes.Rectangle;

namespace Xwt.WPFBackend
{
	public class SeparatorBackend
		: WidgetBackend, ISeparatorBackend
	{
		public SeparatorBackend()
		{
			Separator = new WindowsRectangle ();
			Separator.Stroke = Brushes.DarkGray;
		}

		public void Initialize (Orientation dir)
		{
			this.direction = dir;
		}

		private Orientation direction;

		protected WindowsRectangle Separator
		{
			get { return (WindowsRectangle) Widget; }
			set { Widget = value; }
		}

		public override WidgetSize GetPreferredHeight()
		{
			if (this.direction == Orientation.Vertical)
				return new WidgetSize (((FrameworkElement) Widget.Parent).ActualHeight);

			return new WidgetSize (1);
		}

		public override WidgetSize GetPreferredWidth()
		{
			if (this.direction == Orientation.Horizontal)
				return new WidgetSize (((FrameworkElement) Widget.Parent).ActualWidth);

			return new WidgetSize (1);
		}
	}
}