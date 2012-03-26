// 
// CellUtil.cs
//  
// Author:
//       Luís Reis <luiscubal@gmail.com>
//       Eric Maupin <ermau@xamarin.com>
// 
// Copyright (c) 2012 Luís Reis
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
using System.Windows.Data;
using SWC = System.Windows.Controls;
using SWM = System.Windows.Media;
using Xwt.Engine;

namespace Xwt.WPFBackend.Utilities
{
	public static class CellUtil
	{
		internal static FrameworkElement CreateCellRenderer (TreeNode node, CellView view)
		{
			if (view is TextCellView)
			{
				DataField field = ((TextCellView) view).TextField;
				int index = field.Index;
				SWC.TextBlock label = new SWC.TextBlock ();
				label.Text = (node.Values[index] ?? "null").ToString();
				label.Padding = new Thickness(2);
				return label;
			}
			else if (view is ImageCellView)
			{
				DataField field = ((ImageCellView)view).ImageField;
				int index = field.Index;
				SWM.ImageSource image = (SWM.ImageSource) WidgetRegistry.GetBackend(node.Values[index]);
				if (image == null)
					return null;

				SWC.Image imageCtrl = new SWC.Image
				{
					Source = image,
					Width = image.Width,
					Height = image.Height
				};
				return imageCtrl;
			}
			throw new NotImplementedException ();
		}

		internal static FrameworkElementFactory CreateBoundCellRenderer (CellView view)
		{
			TextCellView textView = view as TextCellView;
			if (textView != null) {
				FrameworkElementFactory factory = new FrameworkElementFactory (typeof (SWC.TextBlock));
				factory.SetValue (FrameworkElement.MarginProperty, new Thickness (2));

				if (textView.TextField != null)
					factory.SetBinding (SWC.TextBlock.TextProperty, new Binding (".[" + textView.TextField.Index + "]"));

				return factory;
			}

			ImageCellView imageView = view as ImageCellView;
			if (imageView != null) {
				FrameworkElementFactory factory = new FrameworkElementFactory (typeof (SWC.Image));
				factory.SetValue (FrameworkElement.MarginProperty, new Thickness (2));

				if (imageView.ImageField != null) {
					var binding = new Binding (".[" + imageView.ImageField.Index + "]")
					{ Converter = new ImageToImageSourceConverter () };

					factory.SetBinding (SWC.Image.SourceProperty, binding);
				}

				return factory;
			}

			throw new NotImplementedException ();
		}
	}
}
