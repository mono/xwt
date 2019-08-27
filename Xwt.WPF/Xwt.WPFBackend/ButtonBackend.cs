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
using System.Text.RegularExpressions;
using SWC = System.Windows.Controls;
using Xwt.Backends;
using System.Windows.Data;


namespace Xwt.WPFBackend
{
	public class ButtonBackend : WidgetBackend, IButtonBackend
	{
		public ButtonBackend ()
			: this (new WpfButton ())
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

		public void SetContent (string label, bool useMnemonic, ImageDescription image, ContentPosition position)
		{
			var accessText = new SWC.AccessText ();
			accessText.Text = label;
			if (image.IsNull)
				if (useMnemonic)
					Button.Content = accessText;
				else
					Button.Content = accessText.Text.Replace ("_", "__");
			else {
				SWC.DockPanel grid = new SWC.DockPanel ();

				var imageCtrl = new ImageBox (Context);
				imageCtrl.ImageSource = image;

				SWC.DockPanel.SetDock (imageCtrl, DataConverter.ToWpfDock (position));
				grid.Children.Add (imageCtrl);

				if (!string.IsNullOrEmpty (label)) {
					SWC.Label labelCtrl = new SWC.Label ();
					if (useMnemonic)
						labelCtrl.Content = accessText;
					else
						labelCtrl.Content = label;
					labelCtrl.SetBinding (SWC.Label.ForegroundProperty, new Binding ("Foreground") { Source = Button });
					grid.Children.Add (labelCtrl);
				}
				Button.Content = grid;
			}
			Button.InvalidateMeasure ();
		}

		public void SetFormattedText (FormattedText text)
		{
			SWC.Label labelCtrl = null;
			if (Button.Content is SWC.DockPanel) {
				var grid = Button.Content as SWC.DockPanel;
				labelCtrl = grid.Children[1] as SWC.Label;
			} else {
				labelCtrl = new SWC.Label ();
				Button.Content = labelCtrl;
			}
			var textCtrl = new SWC.TextBlock();
			textCtrl.ApplyFormattedText(text, null);
			labelCtrl.Content = textCtrl;
		}

		public Xwt.Drawing.Color LabelColor
		{
			get { return Button.Foreground.ToXwtColor(); }
			set { Button.Foreground = ResPool.GetSolidBrush (value.ToWpfColor()); }
		}

		bool isDefault;
		public virtual bool IsDefault {
			get { return (Button as SWC.Button)?.IsDefault ?? isDefault; }
			set {
				var button = Button as SWC.Button;
				if (button != null)
					button.IsDefault = value;
				else
					isDefault = value; // just cache the value
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
			Context.InvokeUserCode (EventSink.OnClicked);
		}

		private static ResourceDictionary buttonsDictionary;
		protected static ResourceDictionary ButtonResources
		{
			get
			{
				if (buttonsDictionary == null)
					buttonsDictionary = CreateButtonResources ();

				return buttonsDictionary;
			}
		}

		static ResourceDictionary CreateButtonResources ()
		{
			// Toggle button template/style
			var menuDropDownTemplate = new SWC.ControlTemplate (typeof (SWC.Primitives.ToggleButton));
			menuDropDownTemplate.VisualTree = CreateButtonChromeFactory ();
			var trigger = new Trigger { Property = SWC.Primitives.ToggleButton.IsKeyboardFocusedProperty, Value = true };
			trigger.Setters.Add (new Setter (Microsoft.Windows.Themes.ButtonChrome.RenderDefaultedProperty, true, "Chrome"));
			menuDropDownTemplate.Triggers.Add (trigger);
			trigger = new Trigger { Property = SWC.Primitives.ToggleButton.IsCheckedProperty, Value = true };
			trigger.Setters.Add (new Setter (Microsoft.Windows.Themes.ButtonChrome.RenderDefaultedProperty, true, "Chrome"));
			menuDropDownTemplate.Triggers.Add (trigger);
			trigger = new Trigger { Property = SWC.Primitives.ToggleButton.IsEnabledProperty, Value = false };
			trigger.Setters.Add (new Setter (SWC.Primitives.ToggleButton.ForegroundProperty, new System.Windows.Media.SolidColorBrush (System.Windows.Media.Color.FromArgb (0xFF, 0xAD, 0xAD, 0xAD))));
			menuDropDownTemplate.Triggers.Add (trigger);
			var menuDropDownStyle = new Style (typeof (SWC.Primitives.ToggleButton));
			menuDropDownStyle.Setters.Add (new Setter () {
				Property = SWC.Control.TemplateProperty,
				Value = menuDropDownTemplate
			});

			// Button template/style
			var normalDropDownTemplate = new SWC.ControlTemplate (typeof (SWC.Button));
			normalDropDownTemplate.VisualTree = CreateButtonChromeFactory ();
			trigger = new Trigger { Property = SWC.Button.IsKeyboardFocusedProperty, Value = true };
			trigger.Setters.Add (new Setter (Microsoft.Windows.Themes.ButtonChrome.RenderDefaultedProperty, true, "Chrome"));
			normalDropDownTemplate.Triggers.Add (trigger);
			trigger = new Trigger { Property = SWC.Button.IsEnabledProperty, Value = false };
			trigger.Setters.Add (new Setter (SWC.Button.ForegroundProperty, new System.Windows.Media.SolidColorBrush (System.Windows.Media.Color.FromArgb (0xFF, 0xAD, 0xAD, 0xAD))));
			normalDropDownTemplate.Triggers.Add (trigger);
			var normalDropDownStyle = new Style (typeof (SWC.Button));
			normalDropDownStyle.Setters.Add (new Setter () {
				Property = SWC.Control.TemplateProperty,
				Value = normalDropDownTemplate
			});

			menuDropDownStyle.Seal ();
			normalDropDownStyle.Seal ();

			var resourceDic = new ResourceDictionary ();
			resourceDic.Add ("MenuDropDown", menuDropDownStyle);
			resourceDic.Add ("NormalDropDown", normalDropDownStyle);

			return resourceDic;
		}

		static FrameworkElementFactory CreateButtonChromeFactory ()
		{
			var panel = new FrameworkElementFactory (typeof (SWC.DockPanel));
			var contentPresenter = new FrameworkElementFactory (typeof (SWC.ContentPresenter));
			contentPresenter.SetValue (SWC.ContentPresenter.MarginProperty, new Thickness (2, 1, 0, 0));
			panel.AppendChild (contentPresenter);
			var path = new FrameworkElementFactory (typeof (System.Windows.Shapes.Path));
			path.SetValue (System.Windows.Shapes.Path.DataProperty, Geometry.Parse ("M 0 0 L 3.5 4 L 7 0 Z"));
			path.SetValue (System.Windows.Shapes.Path.FillProperty, Brushes.Black);
			path.SetValue (System.Windows.Shapes.Path.HorizontalAlignmentProperty, HorizontalAlignment.Right);
			path.SetValue (System.Windows.Shapes.Path.VerticalAlignmentProperty, VerticalAlignment.Center);
			path.SetValue (System.Windows.Shapes.Path.MarginProperty, new Thickness (3, 1, 3, 0));
			path.SetValue (SWC.DockPanel.DockProperty, SWC.Dock.Right);
			panel.AppendChild (path);
			var buttonChromeFactory = new FrameworkElementFactory (typeof (Microsoft.Windows.Themes.ButtonChrome));
			buttonChromeFactory.SetBinding (Microsoft.Windows.Themes.ButtonChrome.BorderBrushProperty, new Binding ("BorderBrush") { RelativeSource = RelativeSource.TemplatedParent });
			buttonChromeFactory.SetBinding (Microsoft.Windows.Themes.ButtonChrome.BackgroundProperty, new Binding ("Background") { RelativeSource = RelativeSource.TemplatedParent });
			buttonChromeFactory.SetBinding (Microsoft.Windows.Themes.ButtonChrome.RenderMouseOverProperty, new Binding ("IsMouseOver") { RelativeSource = RelativeSource.TemplatedParent });
			buttonChromeFactory.SetBinding (Microsoft.Windows.Themes.ButtonChrome.RenderPressedProperty, new Binding ("IsPressed") { RelativeSource = RelativeSource.TemplatedParent });
			buttonChromeFactory.SetBinding (Microsoft.Windows.Themes.ButtonChrome.RenderDefaultedProperty, new Binding ("Button.IsDefaulted") { RelativeSource = RelativeSource.TemplatedParent });
			buttonChromeFactory.SetValue (Microsoft.Windows.Themes.ButtonChrome.SnapsToDevicePixelsProperty, true);
			buttonChromeFactory.Name = "Chrome";
			buttonChromeFactory.AppendChild (panel);

			return buttonChromeFactory;
		}
	}

	class WpfButton : SWC.Button, IWpfWidget
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
