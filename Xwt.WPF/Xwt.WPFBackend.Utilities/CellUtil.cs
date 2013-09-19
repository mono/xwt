//
// CellUtil.cs
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

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using SWC = System.Windows.Controls;
using SWM = System.Windows.Media;

namespace Xwt.WPFBackend.Utilities
{
	public static class CellUtil
	{
		internal static FrameworkElementFactory CreateBoundColumnTemplate (CellViewCollection views, string dataPath = ".")
		{
			if (views.Count == 1)
				return CreateBoundCellRenderer (views [0], dataPath);
			
			FrameworkElementFactory container = new FrameworkElementFactory (typeof (StackPanel));
			container.SetValue (StackPanel.OrientationProperty, Orientation.Horizontal);

			foreach (CellView view in views) {
				container.AppendChild (CreateBoundCellRenderer (view, dataPath));
			}

			return container;
		}

		private static readonly Thickness CellMargins = new Thickness (2);
		internal static FrameworkElementFactory CreateBoundCellRenderer (CellView view, string dataPath = ".")
		{
			TextCellView textView = view as TextCellView;
			if (textView != null) {
				FrameworkElementFactory factory = new FrameworkElementFactory (typeof (SWC.TextBlock));
				factory.SetValue (FrameworkElement.MarginProperty, CellMargins);

				if (textView.TextField != null)
					factory.SetBinding (SWC.TextBlock.TextProperty, new Binding (dataPath + "[" + textView.TextField.Index + "]"));

				return factory;
			}

			ImageCellView imageView = view as ImageCellView;
			if (imageView != null) {
				FrameworkElementFactory factory = new FrameworkElementFactory (typeof (ImageBox));
				factory.SetValue (FrameworkElement.MarginProperty, CellMargins);

				if (imageView.ImageField != null) {
					var binding = new Binding (dataPath + "[" + imageView.ImageField.Index + "]")
					{ Converter = new ImageToImageSourceConverter () };

					factory.SetBinding (ImageBox.ImageSourceProperty, binding);
				}

				return factory;
			}

			CanvasCellView canvasView = view as CanvasCellView;
			if (canvasView != null)
			{
				FrameworkElementFactory factory = new FrameworkElementFactory(typeof(CanvasCellViewBackend));
				factory.SetValue(FrameworkElement.MarginProperty, CellMargins);

				factory.SetValue(CanvasCellViewBackend.CellViewProperty, view);

				return factory;
			}
			
			CheckBoxCellView cellView = view as CheckBoxCellView;
			if (cellView != null) {
				FrameworkElementFactory factory = new FrameworkElementFactory (typeof (SWC.CheckBox));
				factory.SetValue (FrameworkElement.MarginProperty, CellMargins);
				if (cellView.ActiveField != null) {
					factory.SetBinding (SWC.CheckBox.IsCheckedProperty, new Binding (dataPath + "[" + cellView.ActiveField.Index + "]"));
				}

				return factory;
			}

			throw new NotImplementedException ();
		}
	}
}
