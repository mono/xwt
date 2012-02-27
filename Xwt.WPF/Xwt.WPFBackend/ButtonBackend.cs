// 
// ButtonBackend.cs
//  
// Author:
//       Luís Reis <luiscubal@gmail.com>
// 
// Copyright (c) 2012 Luís Reis
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
using System.Windows.Media;
using SWC = System.Windows.Controls;
using SWMI = System.Windows.Media.Imaging;

using Xwt.Backends;

namespace Xwt.WPFBackend
{
	public class ButtonBackend : WidgetBackend, IButtonBackend
	{
		public ButtonBackend ()
		{
			Widget = new SWC.Button ();
		}

		protected SWC.Button Button {
			get { return (SWC.Button)Widget; }
		}

		protected new IButtonEventSink EventSink {
			get { return (IButtonEventSink)base.EventSink; }
		}

		public void SetButtonStyle (ButtonStyle style) {
			switch (style)
			{
				case ButtonStyle.Normal:
					Button.ClearValue (SWC.Button.BackgroundProperty);
					Button.ClearValue (SWC.Button.BorderThicknessProperty);
					Button.ClearValue (SWC.Button.BorderBrushProperty);
					break;
				case ButtonStyle.Flat:
					Button.Background = SystemColors.ControlBrush;
					Button.ClearValue (SWC.Button.BorderThicknessProperty);
					Button.ClearValue (SWC.Button.BorderBrushProperty);
					break;
				case ButtonStyle.Borderless:
					Button.ClearValue (SWC.Button.BackgroundProperty);
					Button.BorderThickness = new Thickness (0);
					Button.BorderBrush = new SolidColorBrush (Colors.Transparent);
					break;
			}
		}

		public void SetButtonType(ButtonType type) {
			//TODO
		}

		public void SetContent(string label, object imageBackend, ContentPosition position) {
			if (imageBackend == null)
			{
				Button.Content = label;
			}
			else
			{
				SWC.DockPanel grid = new SWC.DockPanel ();

				var img = (SWMI.BitmapSource)imageBackend;
				SWC.Image imageCtrl = new SWC.Image
				{
					Source = img,
					Width = img.Width,
					Height = img.Height
				};

				SWC.DockPanel.SetDock (imageCtrl, DataConverter.ToWpfDock (position));
				grid.Children.Add (imageCtrl);

				SWC.Label labelCtrl = new SWC.Label ();
				labelCtrl.Content = label;
				grid.Children.Add (labelCtrl);

				Button.Content = grid;
			}
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is ButtonEvent)
			{
				switch ((ButtonEvent)eventId)
				{
					case ButtonEvent.Clicked: Button.Click += HandleWidgetClicked; break;
				}
			}
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is ButtonEvent)
			{
				switch ((ButtonEvent)eventId)
				{
					case ButtonEvent.Clicked: Button.Click -= HandleWidgetClicked; break;
				}
			}
		}

		void HandleWidgetClicked (object sender, EventArgs e)
		{
			EventSink.OnClicked ();
		}
	}
}
