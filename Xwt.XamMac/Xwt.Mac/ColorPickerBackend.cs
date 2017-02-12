//
// ColorPickerBackend.cs
//
// Author:
//       Lluis Sanchez Gual <lluis@xamarin.com>
//
// Copyright (c) 2014 Xamarin, Inc (http://www.xamarin.com)
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
using AppKit;
using Xwt.Backends;

namespace Xwt.Mac
{
	public class ColorPickerBackend: ViewBackend<NSColorWell,IColorPickerEventSink>, IColorPickerBackend
	{
		public override void Initialize ()
		{
			ViewObject = new MacColorWell (EventSink, ApplicationContext);
		}

		public Xwt.Drawing.Color Color {
			get { return Widget.Color.ToXwtColor (); }
			set { Widget.Color = value.ToNSColor (); }
		}

		public bool SupportsAlpha { get; set; }

		public string Title { get; set; }

		public void SetButtonStyle (ButtonStyle style)
		{
			// The Borderless property can't be used because when set the NSColorPanel popup
			// is not shown when the picker is clicked.
		}

		public override Size GetPreferredSize (SizeConstraint widthConstraint, SizeConstraint heightConstraint)
		{
			return new Size (35, 20);
		}
	}

	class MacColorWell: NSColorWell, IViewObject
	{
		IColorPickerEventSink eventSink;
		ApplicationContext context;

		public MacColorWell (IColorPickerEventSink eventSink, ApplicationContext context)
		{
			this.eventSink = eventSink;
			this.context = context;
		}

		static string defaultTitle;

		public override void Activate (bool exclusive)
		{
			NSColorPanel.SharedColorPanel.ShowsAlpha = ((ColorPickerBackend)Backend).SupportsAlpha;
			if (defaultTitle == null)
				defaultTitle = NSColorPanel.SharedColorPanel.Title;
			NSColorPanel.SharedColorPanel.Title = ((ColorPickerBackend)Backend).Title ?? defaultTitle;
			base.Activate (exclusive);
		}

		public bool SupportsAlpha { get; set; }

		public ViewBackend Backend { get; set; }

		public NSView View {
			get { return this; }
		}

		public override NSColor Color {
			get {
				return base.Color;
			}
			set {
				base.Color = value;
				if (context != null)
					context.InvokeUserCode (eventSink.OnColorChanged);
			}
		}
	}
}

