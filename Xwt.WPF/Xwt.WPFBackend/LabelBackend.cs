// 
// LabelBackend.cs
//  
// Author:
//       Carlos Alberto Cortez <calberto.cortez@gmail.com>
// 
// Copyright (c) 2012 Carlos Alberto Cortez
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using SWC = System.Windows.Controls;
using SWM = System.Windows.Media;

using Xwt.Backends;

namespace Xwt.WPFBackend
{
	public class LabelBackend : WidgetBackend, ILabelBackend
	{
		public LabelBackend ()
		{
			Widget = new WpfLabel ();
		}

		WpfLabel Label {
			get { return (WpfLabel)Widget; }
		}

		public string Text {
			get { return Label.TextBlock.Text; }
			set {
				Label.TextBlock.Text = value;
				Widget.InvalidateMeasure();
			}
		}

		public void SetFormattedText (FormattedText text)
		{
		}

		public Xwt.Drawing.Color TextColor {
			get {
				SWM.Color color = SystemColors.ControlColor;

				if (Label.Foreground != null)
					color = ((SWM.SolidColorBrush) Label.Foreground).Color;

				return DataConverter.ToXwtColor (color);
			}
			set {
				Label.Foreground = ResPool.GetSolidBrush (value);
			}
		}

		public Alignment TextAlignment {
			get { return DataConverter.ToXwtAlignment (Label.HorizontalContentAlignment); }
			set { Label.HorizontalContentAlignment = DataConverter.ToWpfAlignment (value); }
		}

		public EllipsizeMode Ellipsize {
			get {
				if (Label.TextBlock.TextTrimming == TextTrimming.None)
					return Xwt.EllipsizeMode.None;
				else
					return Xwt.EllipsizeMode.End;
			}
			set {
				if (value == EllipsizeMode.None)
					Label.TextBlock.TextTrimming = TextTrimming.None;
				else
					Label.TextBlock.TextTrimming = TextTrimming.CharacterEllipsis;
			}
		}

		public WrapMode Wrap {
			get {
				if (Label.TextBlock.TextWrapping == TextWrapping.NoWrap)
					return WrapMode.None;
				else
					return WrapMode.Word;
			} set {
				if (value == WrapMode.None)
					Label.TextBlock.TextWrapping = TextWrapping.NoWrap;
				else
					Label.TextBlock.TextWrapping = TextWrapping.Wrap;
			}
		}

		public override WidgetSize GetPreferredWidth ()
		{
			if (Label.TextBlock.TextWrapping == TextWrapping.Wrap)
				return new WidgetSize (0);
			else
				return base.GetPreferredWidth ();
		}
	}

	class WpfLabel : SWC.Label, IWpfWidget
	{
		public WpfLabel ()
		{
			TextBlock = new SWC.TextBlock ();
			Content = TextBlock;
			Padding = new Thickness (0);
		}

		public WidgetBackend Backend { get; set; }

		protected override System.Windows.Size MeasureOverride (System.Windows.Size constraint)
		{
			var s = base.MeasureOverride (constraint);
			return Backend.MeasureOverride (constraint, s);
		}

		public SWC.TextBlock TextBlock {
			get;
			set;
		}
	}
}
