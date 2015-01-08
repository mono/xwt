//
// ColorPicker.cs
//
// Author:
//       Vsevolod Kukol <sevo@sevo.org>
//
// Copyright (c) 2014 Vsevolod Kukol
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
using Xwt.Backends;
using Xwt.Drawing;
using System.ComponentModel;

namespace Xwt
{
	[BackendType (typeof(IColorPickerBackend))]
	public class ColorPicker: Widget
	{
		protected new class WidgetBackendHost: Widget.WidgetBackendHost, IColorPickerEventSink
		{
			protected override IBackend OnCreateBackend ()
			{
				var b = base.OnCreateBackend ();
				if (b == null)
					b = new DefaultColorPickerBackend ();
				return b;
			}

			public void OnColorChanged ()
			{
				((ColorPicker)Parent).OnColorChanged (EventArgs.Empty);
			}
		}

		ButtonStyle style = ButtonStyle.Normal;

		static ColorPicker ()
		{
			MapEvent (ColorPickerEvent.ColorChanged, typeof(ColorPicker), "OnColorChanged");
		}

		public ColorPicker ()
		{
		}

		protected override Xwt.Backends.BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}

		IColorPickerBackend Backend {
			get { return (IColorPickerBackend) BackendHost.Backend; }
		}

		public Color Color {
			get { return Backend.Color; }
			set { Backend.Color = value; }
		}

		[DefaultValue (true)]
		public bool SupportsAlpha {
			get { return Backend.SupportsAlpha; }
			set { Backend.SupportsAlpha = value; }
		}

		/// <summary>
		/// Gets or sets the title of the color picker popup.
		/// </summary>
		/// <value>The popup title.</value>
		[DefaultValue ("")]
		public string Title {
			get { return Backend.Title; }
			set { Backend.Title = value; }
		}

		[DefaultValue (ButtonStyle.Normal)]
		public ButtonStyle Style {
			get { return style; }
			set {
				style = value;
				Backend.SetButtonStyle (style);
				OnPreferredSizeChanged ();
			}
		}

		protected virtual void OnColorChanged (EventArgs args)
		{
			if (colorChanged != null)
				colorChanged (this, args);
		}

		EventHandler colorChanged;

		public event EventHandler ColorChanged {
			add {
				BackendHost.OnBeforeEventAdd (ColorPickerEvent.ColorChanged, colorChanged);
				colorChanged += value;
			}
			remove {
				colorChanged -= value;
				BackendHost.OnAfterEventRemove (ColorPickerEvent.ColorChanged, colorChanged);
			}
		}
	}

	class DefaultColorPickerBackend: XwtWidgetBackend, IColorPickerBackend
	{
		readonly Button colorButton;
		readonly ColorImage colorImage;

		public DefaultColorPickerBackend ()
		{
			colorImage = new ColorImage (Colors.Black);
			colorButton = new Button (colorImage.WithSize (38, 24));
			colorButton.WidthRequest = 48;
			colorButton.HeightRequest = 32;
			colorButton.MinWidth = 48;
			colorButton.MinHeight = 32;
			colorButton.ExpandHorizontal = false;
			colorButton.ExpandVertical = false;
			colorButton.HorizontalPlacement = WidgetPlacement.Start;
			colorButton.VerticalPlacement = WidgetPlacement.Start;
			colorButton.ImagePosition = ContentPosition.Center;

			colorButton.Clicked += HandleClicked;

			Content = colorButton;
		}

		void HandleClicked (object sender, EventArgs e)
		{
			SelectColorDialog dlg = new SelectColorDialog (Title);
			dlg.SupportsAlpha = SupportsAlpha;
			dlg.Color = Color;
			if (dlg.Run (ParentWindow)) {
				Color = dlg.Color;
				if (enabledOnColorChanged)
					EventSink.OnColorChanged ();
			}
		}

		bool enabledOnColorChanged;

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is ColorPickerEvent) {
				switch ((ColorPickerEvent)eventId) {
					case ColorPickerEvent.ColorChanged: enabledOnColorChanged = true; break;
				}
			}
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is ColorPickerEvent) {
				switch ((ColorPickerEvent)eventId) {
					case ColorPickerEvent.ColorChanged: enabledOnColorChanged = false; break;
				}
			}
		}

		protected new IColorPickerEventSink EventSink {
			get { return (IColorPickerEventSink)base.EventSink; }
		}

		public Color Color {
			get {
				return colorImage.Color;
			}
			set {
				colorImage.Color = value;
			}
		}

		public bool SupportsAlpha {
			get;
			set;
		}

		public string Title {
			get;
			set;
		}

		public void SetButtonStyle (ButtonStyle style)
		{
			colorButton.Style = style;
			if (style == ButtonStyle.Borderless) {
				colorButton.Image = colorImage.WithSize (48, 32);
			} else {
				colorButton.Image = colorImage.WithSize (38, 24);
			}
		}
	}

	class ColorImage: DrawingImage
	{
		public ColorImage (Color color)
		{
			Color = color;
		}

		public Color Color {
			get;
			set;
		}

		protected override void OnDraw (Context ctx, Rectangle bounds)
		{
			ctx.Rectangle (bounds);
			ctx.SetColor (Colors.White);
			ctx.Fill ();

			ctx.MoveTo (0, 0);
			ctx.LineTo (Size.Width, 0);
			ctx.LineTo (0, Size.Height);
			ctx.LineTo (0, 0);
			ctx.SetColor (Colors.Black);
			ctx.Fill ();

			ctx.Rectangle (bounds);
			ctx.SetColor (Color);
			ctx.Fill ();
		}
	}
}

