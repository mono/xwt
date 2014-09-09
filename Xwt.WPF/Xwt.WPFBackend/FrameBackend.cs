// 
// FrameBackend.cs
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
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using SWC = System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Xwt.Backends;
using Color = Xwt.Drawing.Color;

namespace Xwt.WPFBackend
{
	public class FrameBackend
		: WidgetBackend, IFrameBackend
	{
		public FrameBackend()
		{
			ExGrid grid = new ExGrid();
			grid.Children.Add (this.groupBox = new SWC.GroupBox ());
			grid.Children.Add (this.flippedGroupBox = new SWC.GroupBox ());
			groupBox.SizeChanged += delegate (object sender, SizeChangedEventArgs e)
			{
				flippedGroupBox.RenderSize = groupBox.RenderSize;
			};

			this.flippedGroupBox.SetBinding (UIElement.IsEnabledProperty, new Binding ("IsEnabled") { Source = this.groupBox });
			this.flippedGroupBox.SetBinding (Control.BorderBrushProperty, new Binding ("BorderBrush") { Source = this.groupBox });
			this.flippedGroupBox.SetBinding (Control.BorderThicknessProperty,
				new Binding ("BorderThickness") {
					Source = this.groupBox,
					Converter =  new HFlippedBorderThicknessConverter ()
				});

			this.flippedGroupBox.RenderTransformOrigin = new System.Windows.Point (0.5, 0.5);
			this.flippedGroupBox.RenderTransform = new ScaleTransform (-1, 1);
			this.flippedGroupBox.Focusable = false;
			SWC.Panel.SetZIndex (this.flippedGroupBox, -1);

			Widget = grid;
		}

		public string Label
		{
			get { return this.label; }
			set
			{
				if (this.label == value)
					return;

				this.label = value;
				GroupBox.Header = value;

				this.flippedGroupBox.Visibility = String.IsNullOrEmpty (value) ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		public Color BorderColor
		{
			get { return GroupBox.BorderBrush.ToXwtColor(); }
			set
			{
				if (this.frameType == FrameType.WidgetBox)
					return;

				GroupBox.BorderBrush = new SolidColorBrush (value.ToWpfColor ());
			}
		}

		public void SetFrameType (FrameType type)
		{
			this.frameType = type;
			
			if (type == FrameType.WidgetBox) {
				GroupBox.ClearValue (Control.BorderThicknessProperty);
				GroupBox.ClearValue (Control.BorderBrushProperty);
				GroupBox.ClearValue (Control.PaddingProperty);
			}
		}

		public void SetContent (IWidgetBackend child)
		{
			if (child == null)
			{
				GroupBox.Content = null;
			}
			else
			{
				GroupBox.Content = child.NativeWidget;
				SetChildPlacement(child);
			}
		}

		public void SetBorderSize (double left, double right, double top, double bottom)
		{
			if (this.frameType == FrameType.WidgetBox)
				return;

			GroupBox.BorderThickness = new Thickness (left, top, right, bottom);
		}

		public void SetPadding (double left, double right, double top, double bottom)
		{
			if (this.frameType == FrameType.WidgetBox)
				return;

			GroupBox.Padding = new Thickness (left, top, right, bottom);
		}

		private SWC.GroupBox GroupBox {
			get { return this.groupBox; }
		}

		private readonly SWC.GroupBox groupBox;
		private readonly SWC.GroupBox flippedGroupBox;

		private FrameType frameType;
		private string label;

		private class HFlippedBorderThicknessConverter
			: IValueConverter
		{
			public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
			{
				Thickness t = (Thickness) value;
				double right = t.Right;
				t.Right = t.Left;
				t.Left = right;

				return t;
			}

			public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
			{
				throw new NotImplementedException ();
			}
		}
	}
}