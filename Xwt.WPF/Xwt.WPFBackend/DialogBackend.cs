//
// DialogBackend.cs
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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Xwt.Backends;
using System.Linq;

using SWC = System.Windows.Controls;

namespace Xwt.WPFBackend
{
	public class DialogBackend
		: WindowBackend, IDialogBackend
	{
		static DialogBackend()
		{
			var panelFactory = new FrameworkElementFactory (typeof (StackPanel));
			panelFactory.SetValue (StackPanel.OrientationProperty, SWC.Orientation.Horizontal);
			panelFactory.SetValue (StackPanel.MarginProperty, new Thickness (0, 7, 7, 7));

			PanelTemplate = new ItemsPanelTemplate (panelFactory);

			ButtonStyle.Setters.Add (new Setter (FrameworkElement.MarginProperty, new Thickness (7, 0, 0, 0)));
			ButtonStyle.Setters.Add (new Setter (FrameworkElement.MinWidthProperty, 80d));
		}

		private DelegatedCommand cmd;

		public DialogBackend()
		{
			cmd = new DelegatedCommand<DialogButton> (OnButtonClicked);

			this.buttonContainer.ItemsPanel = PanelTemplate;
			this.buttonContainer.ItemTemplateSelector =  new DialogButtonTemplateSelector (ButtonStyle, cmd);
			this.buttonContainer.ItemsSource = this.buttons;
			this.buttonContainer.HorizontalAlignment = HorizontalAlignment.Right;

			this.rootPanel.RowDefinitions.Add (new RowDefinition { Height = new GridLength (0, GridUnitType.Auto) });
			separator = new SWC.Separator ();
			separator.Visibility = Visibility.Collapsed;
			Grid.SetRow (separator, 2);
			this.rootPanel.Children.Add (separator);

			this.rootPanel.RowDefinitions.Add (new RowDefinition { Height = new GridLength (0, GridUnitType.Auto) });
			Grid.SetRow (this.buttonContainer, 3);
			this.rootPanel.Children.Add (this.buttonContainer);
			this.buttonContainer.Visibility = Visibility.Collapsed;
		}

		public override void SetMinSize (Size s)
		{
			// Take into account the size of the button bar and the separator

			buttonContainer.InvalidateMeasure ();
			buttonContainer.Measure (new System.Windows.Size (double.PositiveInfinity, double.PositiveInfinity));
			separator.InvalidateMeasure ();
			separator.Measure (new System.Windows.Size (double.PositiveInfinity, double.PositiveInfinity));
			s.Height += buttonContainer.DesiredSize.Height + separator.DesiredSize.Height;
			s.Width = System.Math.Max(buttonContainer.DesiredSize.Width, separator.DesiredSize.Width);
			base.SetMinSize (s);
		}

		public void SetButtons (IEnumerable<DialogButton> newButtons)
		{
			this.buttons.Clear();
			foreach (var button in newButtons) {
				this.buttons.Add (button);
			}
			UpdateSeparatorVisibility ();
		}

		public void UpdateButton (DialogButton updatedButton)
		{
			for (int i = 0; i < this.buttons.Count; ++i) {
				var button = this.buttons [i];
				if (button == updatedButton) {
					this.buttons.RemoveAt (i);
					this.buttons.Insert (i, updatedButton);
					break;
				}
			}
			UpdateSeparatorVisibility ();
		}

		void UpdateSeparatorVisibility ()
		{
			buttonContainer.Visibility = separator.Visibility = buttons.Any (b => b.Visible) ? Visibility.Visible : Visibility.Collapsed;
		}

		public void RunLoop (IWindowFrameBackend parent)
		{
			if (parent != null)
				Window.Owner = ((WindowFrameBackend) parent).Window;
			Window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			Window.ShowDialog ();
		}

		public void EndLoop ()
		{
			InhibitCloseRequested = true;
			Window.Close();
			InhibitCloseRequested = false;
		}

		private readonly ItemsControl buttonContainer = new ItemsControl();
		private readonly ObservableCollection<DialogButton> buttons = new ObservableCollection<DialogButton> ();
		readonly SWC.Separator separator;

		protected IDialogEventSink DialogEventSink {
			get { return (IDialogEventSink) EventSink; }
		}

		private void OnButtonClicked (DialogButton button)
		{
			Context.InvokeUserCode (() => DialogEventSink.OnDialogButtonClicked (button));
		}

		private static readonly ItemsPanelTemplate PanelTemplate;
		private static readonly Style ButtonStyle = new Style (typeof (SWC.Button));

		private class DialogButtonTemplateSelector
			: DataTemplateSelector
		{
			static void SetupButtonFactory (FrameworkElementFactory factory, Style style, ICommand command)
			{
				factory.SetBinding (UIElement.IsEnabledProperty, new Binding ("Sensitive"));
				factory.SetBinding (UIElement.VisibilityProperty, new Binding ("Visible") { Converter = VisibilityConverter });
				factory.SetValue (FrameworkElement.StyleProperty, style);
				factory.SetValue (ButtonBase.CommandProperty, command);
				factory.SetBinding (ButtonBase.CommandParameterProperty, new Binding ());
			}

			public DialogButtonTemplateSelector (Style style, ICommand command)
			{
				var buttonFactory = new FrameworkElementFactory (typeof (SWC.Button));
				SetupButtonFactory (buttonFactory, style, command);
				buttonFactory.SetBinding (ContentControl.ContentProperty, new Binding ("Label"));

				this.normalTemplate = new DataTemplate { VisualTree = buttonFactory };

				buttonFactory = new FrameworkElementFactory (typeof (SWC.Button));
				SetupButtonFactory (buttonFactory, style, command);

				var contentFactory = new FrameworkElementFactory (typeof (DockPanel));

				var imageFactory = new FrameworkElementFactory (typeof (Image));
				imageFactory.SetBinding (Image.SourceProperty, new Binding ("Image.NativeWidget"));
				imageFactory.SetValue (DockPanel.DockProperty, Dock.Left);

				contentFactory.AppendChild (imageFactory);

				var textFactory = new FrameworkElementFactory (typeof (TextBlock));
				textFactory.SetBinding (TextBlock.TextProperty, new Binding ("Label"));
				textFactory.SetValue (DockPanel.DockProperty, Dock.Right);

				contentFactory.AppendChild (textFactory);

				buttonFactory.AppendChild (contentFactory);

				this.imageTemplate = new DataTemplate { VisualTree = buttonFactory };
			}

			public override DataTemplate SelectTemplate (object item, DependencyObject container)
			{
				var button = item as DialogButton;
				if (button == null)
					return base.SelectTemplate (item, container);

				return (button.Image == null) ? this.normalTemplate : this.imageTemplate;
			}

			private static readonly BooleanToVisibilityConverter VisibilityConverter = new BooleanToVisibilityConverter ();
			private readonly DataTemplate imageTemplate;
			private readonly DataTemplate normalTemplate;
		}
	}
}