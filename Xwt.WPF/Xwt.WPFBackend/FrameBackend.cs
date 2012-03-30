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


using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xwt.Backends;
using Xwt.WPFBackend.Utilities;
using Color = Xwt.Drawing.Color;
using WpfLabel = System.Windows.Controls.Label;

namespace Xwt.WPFBackend
{
	public class FrameBackend
		: WidgetBackend, IFrameBackend
	{
		public FrameBackend()
		{
			GroupBox = new WpfGroupBox ();
		}

		public string Label
		{
			get { return this.label; }
			set
			{
				if (this.label == value)
					return;

				// TODO: Handle no label
				this.label = value;
				GroupBox.Header = value;
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
			GroupBox.Content = child.NativeWidget;
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

		protected GroupBox GroupBox
		{
			get { return (GroupBox) Widget; }
			set { Widget = value; }
		}

		private FrameType frameType;
		private string label;
	}

	class WpfGroupBox : GroupBox, IWpfWidget
	{
		public WidgetBackend Backend { get; set; }

		protected override System.Windows.Size MeasureOverride (System.Windows.Size constraint)
		{
			var s = base.MeasureOverride (constraint);
			return Backend.MeasureOverride (constraint, s);
		}

		protected override System.Windows.Size ArrangeOverride (System.Windows.Size arrangeBounds)
		{
			return base.ArrangeOverride (arrangeBounds);
		}
	}
}