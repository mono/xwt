﻿//
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
		static readonly Thickness CellMargins = new Thickness (2);

		internal static FrameworkElementFactory CreateBoundColumnTemplate (ApplicationContext ctx, Widget parent, CellViewCollection views, string dataPath = ".")
		{
			if (views.Count == 1)
                return CreateBoundCellRenderer(ctx, parent, views[0], dataPath);
			
			FrameworkElementFactory container = new FrameworkElementFactory (typeof (StackPanel));
			container.SetValue (StackPanel.OrientationProperty, System.Windows.Controls.Orientation.Horizontal);

			foreach (CellView view in views) {
				var factory = CreateBoundCellRenderer(ctx, parent, view, dataPath);

				factory.SetValue(FrameworkElement.MarginProperty, CellMargins);

				if (view.VisibleField != null)
				{
					var binding = new Binding(dataPath + "[" + view.VisibleField.Index + "]");
					binding.Converter = new BooleanToVisibilityConverter();
					factory.SetBinding(UIElement.VisibilityProperty, binding);
				}
				else if (!view.Visible)
					factory.SetValue(UIElement.VisibilityProperty, Visibility.Collapsed);

				container.AppendChild(factory);
			}

			return container;
		}
		internal static FrameworkElementFactory CreateBoundCellRenderer (ApplicationContext ctx, Widget parent, CellView view, string dataPath = ".")
		{
            ICellViewFrontend fr = view;
			TextCellView textView = view as TextCellView;
			if (textView != null) {
				// if it's an editable textcontrol, use a TextBox, if not use a TextBlock. Reason for this is that 
				// a user usually expects to be able to edit a text if a text cursor is appearing above a field.
				FrameworkElementFactory factory;
				if (textView.Editable || textView.EditableField != null)
				{
					factory = new FrameworkElementFactory(typeof(SWC.TextBox));
					if (textView.Editable)
						factory.SetValue(SWC.TextBox.IsReadOnlyProperty, false);
					else
						factory.SetBinding(SWC.TextBox.IsEnabledProperty, new Binding(dataPath + "[" + textView.EditableField.Index + "]"));

					if (textView.TextField != null)
						factory.SetBinding(SWC.TextBox.TextProperty, new Binding(dataPath + "[" + textView.TextField.Index + "]"));
					else
						factory.SetValue(SWC.TextBox.TextProperty, textView.Text);
				}
				else
				{
					factory = new FrameworkElementFactory(typeof(SWC.TextBlock));

					if (textView.MarkupField != null)
						factory.SetBinding(SWC.TextBlock.TextProperty, new Binding(dataPath + "[" + textView.MarkupField.Index + "]") { Converter = new MarkupToPlainTextConverter () });
					else if (textView.TextField != null)
						factory.SetBinding(SWC.TextBlock.TextProperty, new Binding(dataPath + "[" + textView.TextField.Index + "]"));
					else
						factory.SetValue(SWC.TextBlock.TextProperty, textView.Text);
				}

                var cb = new CellViewBackend();
                cb.Initialize(view, factory);
                fr.AttachBackend(parent, cb);
				return factory;
			}

			ImageCellView imageView = view as ImageCellView;
			if (imageView != null) {
				FrameworkElementFactory factory = new FrameworkElementFactory (typeof (ImageBox));

				if (imageView.ImageField != null) {
					var binding = new Binding (dataPath + "[" + imageView.ImageField.Index + "]");
					binding.Converter = new ImageToImageSourceConverter (ctx);

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
				factory.SetValue(CanvasCellViewPanel.CellViewBackendProperty, cb);

                cb.Initialize(view, factory);
                fr.AttachBackend(parent, cb);
                return factory;
			}
			
			CheckBoxCellView cellView = view as CheckBoxCellView;
			if (cellView != null) {
				FrameworkElementFactory factory = new FrameworkElementFactory (typeof(SWC.CheckBox));
				if (cellView.EditableField == null)
					factory.SetValue (FrameworkElement.IsEnabledProperty, cellView.Editable);
				else
					factory.SetBinding (SWC.CheckBox.IsEnabledProperty, new Binding (dataPath + "[" + cellView.EditableField.Index + "]"));

				factory.SetValue (SWC.CheckBox.IsThreeStateProperty, cellView.AllowMixed);
				if (cellView.ActiveField != null)
					factory.SetBinding (SWC.CheckBox.IsCheckedProperty, new Binding (dataPath + "[" + cellView.ActiveField.Index + "]"));

				var cb = new CellViewBackend ();
				cb.Initialize (view, factory);
				fr.AttachBackend (parent, cb);
				return factory;
			}

			throw new NotImplementedException ();
		}
	}
}
