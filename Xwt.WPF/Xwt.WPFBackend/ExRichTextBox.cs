// 
// ExRichTextBox.cs
//  
// Author:
//       Alan McGovern <alan@xamarin.com>
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

namespace Xwt.WPFBackend.Utilities
{
	public class ExRichTextBox : RichTextBox, IWpfWidget
	{
		Style paragraphStyle;
		int lineSpacing;

		public WidgetBackend Backend {
			get; set;
		}

		protected override System.Windows.Size MeasureOverride (System.Windows.Size constraint)
		{
			var s = base.MeasureOverride (constraint);
			return Backend.MeasureOverride (constraint, s);
		}

		public int LineSpacing {
			get {
				return lineSpacing;
			}

			set {
				lineSpacing = value;
				UpdateParagraphStyle ();
			}
		}

		void UpdateParagraphStyle ()
		{
			if (paragraphStyle != null) {
				Resources.Remove (typeof (Paragraph));
				paragraphStyle = null;
			}

			paragraphStyle = new Style (typeof (Paragraph));
			var lineHeightSetter = new Setter (Block.LineHeightProperty, FontSize + lineSpacing);
			paragraphStyle.Setters.Add (lineHeightSetter);

			Resources.Add (typeof (Paragraph), paragraphStyle);
		}
	}
}