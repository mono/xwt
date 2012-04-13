// 
// ButtonBackend.cs
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
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;
using SWC = System.Windows.Controls;
using Xwt.Backends;
using Xwt.Engine;

namespace Xwt.WPFBackend
{
	public class ButtonBackend : WidgetBackend, IButtonBackend
	{
		public ButtonBackend ()
			: this (new SWC.Button())
		{
		}

		protected ButtonBackend (ButtonBase impl)
		{
			if (impl == null)
				throw new ArgumentNullException ("impl");

			Widget = impl;
		}

		protected ButtonBase Button {
			get { return (ButtonBase)Widget; }
		}

		protected new IButtonEventSink EventSink {
			get { return (IButtonEventSink)base.EventSink; }
		}

		public void SetButtonStyle (ButtonStyle style) {
			switch (style)
			{
				case ButtonStyle.Normal:
					Button.ClearValue (SWC.Control.BackgroundProperty);
					Button.ClearValue (SWC.Control.BorderThicknessProperty);
					Button.ClearValue (SWC.Control.BorderBrushProperty);
					break;
				case ButtonStyle.Flat:
					Button.Background = Brushes.Transparent;
					Button.BorderBrush = Brushes.Transparent;
					break;
				case ButtonStyle.Borderless:
					Button.ClearValue (SWC.Control.BackgroundProperty);
					Button.BorderThickness = new Thickness (0);
					Button.BorderBrush = Brushes.Transparent;
					break;
			}
			Button.InvalidateMeasure ();
		}

		public virtual void SetButtonType (ButtonType type) {
			switch (type) {
			case ButtonType.Normal:
				Button.Style = null;
				break;

			case ButtonType.DropDown:
				Button.Style = (Style) ButtonResources ["NormalDropDown"];
				break;
			}

			Button.InvalidateMeasure ();
		}

		public void SetContent (string label, object imageBackend, ContentPosition position)
		{
			if (imageBackend == null)
				Button.Content = label;
			else if (String.IsNullOrEmpty (label))
				Button.Content = new SWC.Image { Source = DataConverter.AsImageSource (imageBackend) };
			else
			{
				SWC.DockPanel grid = new SWC.DockPanel ();

				var img = DataConverter.AsImageSource (imageBackend);
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
			Button.InvalidateMeasure ();
			Button.UpdateLayout ();
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
			Toolkit.Invoke (EventSink.OnClicked);
		}

		private static ResourceDictionary buttonsDictionary;
		protected static ResourceDictionary ButtonResources
		{
			get
			{
				if (buttonsDictionary == null) {
					Uri uri = new Uri ("pack://application:,,,/Xwt.WPF;component/XWT.WPFBackend/Buttons.xaml");
					buttonsDictionary = (ResourceDictionary)XamlReader.Load (System.Windows.Application.GetResourceStream (uri).Stream);
				}

				return buttonsDictionary;
			}
		}
	}
}
