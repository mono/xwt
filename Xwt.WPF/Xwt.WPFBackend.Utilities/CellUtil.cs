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
using Xwt.Backends;
using SWC = System.Windows.Controls;
using SWM = System.Windows.Media;

namespace Xwt.WPFBackend.Utilities
{
	public static class CellUtil
	{
		internal static FrameworkElementFactory CreateBoundColumnTemplate (Widget parent, CellViewCollection views, string dataPath = ".")
		{
			if (views.Count == 1)
                return CreateBoundCellRenderer(parent, views[0], dataPath);
			
			FrameworkElementFactory container = new FrameworkElementFactory (typeof (StackPanel));
			container.SetValue (StackPanel.OrientationProperty, System.Windows.Controls.Orientation.Horizontal);

			foreach (CellView view in views) {
                container.AppendChild(CreateBoundCellRenderer(parent, view, dataPath));
			}

			return container;
		}

		private static readonly Thickness CellMargins = new Thickness (2);
		internal static FrameworkElementFactory CreateBoundCellRenderer (Widget parent, CellView view, string dataPath = ".")
		{
            ICellViewFrontend fr = view;
			TextCellView textView = view as TextCellView;
			if (textView != null) {
				// if it's an editable textcontrol, use a TextBox, if not use a TextBlock. Reason for this is that 
				// a user usually expects to be able to edit a text if a text cursor is appearing above a field.
				FrameworkElementFactory factory;
				if (textView.EditableBinding == null)
				{
					if (textView.Editable)
					{
						factory = new FrameworkElementFactory(typeof(SWC.TextBox));
						factory.SetValue(FrameworkElement.MarginProperty, CellMargins);
						factory.SetValue(SWC.TextBox.IsReadOnlyProperty, false);
						if (textView.TextBinding != null)
						{
							factory.SetBinding(SWC.TextBox.TextProperty, new Binding(dataPath + "[" + textView.TextBinding.Index + "]"));
						}
					}
					else
					{
						factory = new FrameworkElementFactory(typeof(SWC.TextBlock));
						factory.SetValue(FrameworkElement.MarginProperty, CellMargins);
						if (textView.TextBinding != null)
						{
							factory.SetBinding(SWC.TextBlock.TextProperty, new Binding(dataPath + "[" + textView.TextBinding.Index + "]"));
						}
					}
				}
				else
				{
					factory = new FrameworkElementFactory(typeof(SWC.TextBox));
					factory.SetValue(FrameworkElement.MarginProperty, CellMargins);
					factory.SetBinding(SWC.TextBox.IsEnabledProperty, new Binding(dataPath + "[" + textView.EditableBinding.Index + "]"));
					if (textView.TextBinding != null)
					{
						factory.SetBinding(SWC.TextBox.TextProperty, new Binding(dataPath + "[" + textView.TextBinding.Index + "]"));
					}
				}

                var cb = new CellViewBackend();
                cb.Initialize(view, factory);
                fr.AttachBackend(parent, cb);
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

                var cb = new CellViewBackend();
                cb.Initialize(view, factory);
                fr.AttachBackend(parent, cb);
                return factory;
			}

			CanvasCellView canvasView = view as CanvasCellView;
			if (canvasView != null)
			{
                var cb = new CanvasCellViewBackend();
                FrameworkElementFactory factory = new FrameworkElementFactory(typeof(CanvasCellViewPanel));
				factory.SetValue(FrameworkElement.MarginProperty, CellMargins);
				factory.SetValue(CanvasCellViewPanel.CellViewBackendProperty, cb);

                cb.Initialize(view, factory);
                fr.AttachBackend(parent, cb);
                return factory;
			}
			
			CheckBoxCellView cellView = view as CheckBoxCellView;
					if (cellView != null)
					{
						FrameworkElementFactory factory = new FrameworkElementFactory(typeof(SWC.CheckBox));
						if (cellView.EditableBinding == null)
						{
							factory.SetValue(FrameworkElement.IsEnabledProperty, cellView.Editable);
						}
						else
						{
							factory.SetBinding(SWC.CheckBox.IsEnabledProperty, new Binding(dataPath + "[" + cellView.EditableBinding.Index + "]"));
						}

						factory.SetValue(SWC.CheckBox.IsThreeStateProperty, cellView.AllowMixed);
						factory.SetValue(FrameworkElement.MarginProperty, CellMargins);
						if (cellView.ActiveBinding != null)
						{
								factory.SetBinding(SWC.CheckBox.IsCheckedProperty, new Binding(dataPath + "[" + cellView.ActiveBinding.Index + "]"));
						}

                        var cb = new CellViewBackend();
                        cb.Initialize(view, factory);
                        fr.AttachBackend(parent, cb);
                        return factory;
					}

			throw new NotImplementedException ();
		}
	}
}
