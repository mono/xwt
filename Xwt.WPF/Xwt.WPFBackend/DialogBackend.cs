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
			panelFactory.SetValue (FrameworkElement.MarginProperty, new Thickness (0, 7, 7, 7));

			rightPanelTemplate = new ItemsPanelTemplate (panelFactory);

			panelFactory = new FrameworkElementFactory(typeof(StackPanel));
			panelFactory.SetValue(StackPanel.OrientationProperty, SWC.Orientation.Horizontal);
			panelFactory.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 7, 0, 7));

			leftPanelTemplate = new ItemsPanelTemplate(panelFactory);

			ButtonStyle.Setters.Add (new Setter (FrameworkElement.MarginProperty, new Thickness (7, 0, 0, 0)));
			ButtonStyle.Setters.Add (new Setter (FrameworkElement.MinWidthProperty, 80d));
		}

		private DelegatedCommand cmd;

		public DialogBackend()
		{
			cmd = new DelegatedCommand<WpfDialogButton> (OnButtonClicked);

			// Surprisingly, the ItemsControls are focusable by default; disable that to fix tab navigation
			this.leftButtonContainer.Focusable = false;
			this.rightButtonContainer.Focusable = false;

			this.leftButtonContainer.ItemsPanel = leftPanelTemplate;
			this.leftButtonContainer.ItemTemplateSelector = new DialogButtonTemplateSelector(ButtonStyle, cmd);
			this.leftButtonContainer.ItemsSource = this.leftButtons;
			this.leftButtonContainer.HorizontalAlignment = HorizontalAlignment.Left;

			this.rightButtonContainer.ItemsPanel = rightPanelTemplate;
			this.rightButtonContainer.ItemTemplateSelector = new DialogButtonTemplateSelector(ButtonStyle, cmd);
			this.rightButtonContainer.ItemsSource = this.rightButtons;
			this.rightButtonContainer.HorizontalAlignment = HorizontalAlignment.Right;

			this.rootPanel.RowDefinitions.Add (new RowDefinition { Height = new GridLength (0, GridUnitType.Auto) });
			separator = new SWC.Separator ();
			separator.Visibility = Visibility.Collapsed;
			Grid.SetRow (separator, 2);
			this.rootPanel.Children.Add (separator);

			this.rootPanel.RowDefinitions.Add (new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });

			this.buttonContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto  });
			this.buttonContainer.ColumnDefinitions.Add(new ColumnDefinition ());
			this.buttonContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto  });
			Grid.SetColumn(this.leftButtonContainer, 0);
			Grid.SetColumn(this.rightButtonContainer, 2);
			this.buttonContainer.Children.Add(this.leftButtonContainer);
			this.buttonContainer.Children.Add(this.rightButtonContainer);

			Grid.SetRow (buttonContainer, 3);
			this.rootPanel.Children.Add (buttonContainer);
			buttonContainer.Visibility = Visibility.Collapsed;
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
			this.leftButtons.Clear();
			foreach (var button in newButtons.Where(b => b.PackOrigin == PackOrigin.Start && b != defaultButton)) {
				this.leftButtons.Add(new WpfDialogButton(button));
			}
			this.rightButtons.Clear();
			foreach (var button in newButtons.Where(b => b.PackOrigin == PackOrigin.End).OrderBy(b => b == DefaultButton)) {
				this.rightButtons.Add(new WpfDialogButton(button, button == DefaultButton));
			}
			UpdateSeparatorVisibility ();
		}

		public void UpdateButton (DialogButton updatedButton)
		{
			for (int i = 0; i < this.leftButtons.Count; ++i) {
				var button = this.leftButtons [i];
				if (button.Button == updatedButton) {
					this.leftButtons.RemoveAt (i);
					this.leftButtons.Insert (i, new WpfDialogButton(updatedButton, updatedButton == DefaultButton));
					break;
				}
			}
			for (int i = 0; i < this.rightButtons.Count; ++i) {
				var button = this.rightButtons[i];
				if (button.Button == updatedButton) {
					this.rightButtons.RemoveAt(i);
					this.rightButtons.Insert(i, new WpfDialogButton(updatedButton, updatedButton == DefaultButton));
					break;
				}
			}
			UpdateSeparatorVisibility ();
		}

		void UpdateSeparatorVisibility ()
		{
			buttonContainer.Visibility = separator.Visibility = leftButtons.Concat(rightButtons).Any (b => b.Button.Visible) ? Visibility.Visible : Visibility.Collapsed;
		}

		public void RunLoop (IWindowFrameBackend parent)
		{
			if (parent != null)
				SetTransientFor(parent);
			Window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			Window.ShowDialog ();
		}

		public void EndLoop ()
		{
			InhibitCloseRequested = true;
			Window.Close();
			InhibitCloseRequested = false;
		}

		private readonly Grid buttonContainer = new Grid();
		private readonly ItemsControl rightButtonContainer = new ItemsControl();
		private readonly ItemsControl leftButtonContainer = new ItemsControl();
		private readonly ObservableCollection<WpfDialogButton> rightButtons = new ObservableCollection<WpfDialogButton>();
		private readonly ObservableCollection<WpfDialogButton> leftButtons = new ObservableCollection<WpfDialogButton>();
		readonly SWC.Separator separator;
		DialogButton defaultButton;

		protected IDialogEventSink DialogEventSink {
			get { return (IDialogEventSink) EventSink; }
		}

		public DialogButton DefaultButton
		{
			get {
				return defaultButton;
			}
			set {
				defaultButton = value;
				SetButtons(leftButtons.Concat(rightButtons).Select(b => b.Button).ToArray());
			}
		}

		private void OnButtonClicked (WpfDialogButton button)
		{
			Context.InvokeUserCode (() => DialogEventSink.OnDialogButtonClicked (button.Button));
		}

		private static readonly ItemsPanelTemplate leftPanelTemplate;
		private static readonly ItemsPanelTemplate rightPanelTemplate;
		private static readonly Style ButtonStyle = new Style (typeof (SWC.Button));

		private class DialogButtonTemplateSelector
			: DataTemplateSelector
		{
			static void SetupButtonFactory (FrameworkElementFactory factory, Style style, ICommand command)
			{
				factory.SetBinding (UIElement.IsEnabledProperty, new Binding ("Button.Sensitive"));
				factory.SetBinding (UIElement.VisibilityProperty, new Binding ("Button.Visible") { Converter = VisibilityConverter });
				factory.SetBinding (SWC.Button.IsDefaultProperty, new Binding ("IsDefault"));
				factory.SetBinding (SWC.Button.IsCancelProperty, new Binding("IsCancel"));
				factory.SetValue (FrameworkElement.StyleProperty, style);
				factory.SetValue (ButtonBase.CommandProperty, command);
				factory.SetBinding (ButtonBase.CommandParameterProperty, new Binding ());
			}

			public DialogButtonTemplateSelector (Style style, ICommand command)
			{
				var buttonFactory = new FrameworkElementFactory (typeof (SWC.Button));
				SetupButtonFactory (buttonFactory, style, command);
				buttonFactory.SetBinding (ContentControl.ContentProperty, new Binding ("Button.Label"));

				this.normalTemplate = new DataTemplate { VisualTree = buttonFactory };

				buttonFactory = new FrameworkElementFactory (typeof (SWC.Button));
				SetupButtonFactory (buttonFactory, style, command);

				var contentFactory = new FrameworkElementFactory (typeof (DockPanel));

				var imageFactory = new FrameworkElementFactory (typeof (Image));
				imageFactory.SetBinding (Image.SourceProperty, new Binding ("Button.Image.NativeWidget"));
				imageFactory.SetValue (DockPanel.DockProperty, Dock.Left);

				contentFactory.AppendChild (imageFactory);

				var textFactory = new FrameworkElementFactory (typeof (TextBlock));
				textFactory.SetBinding (TextBlock.TextProperty, new Binding ("Button.Label"));
				textFactory.SetValue (DockPanel.DockProperty, Dock.Right);

				contentFactory.AppendChild (textFactory);

				buttonFactory.AppendChild (contentFactory);

				this.imageTemplate = new DataTemplate { VisualTree = buttonFactory };
			}

			public override DataTemplate SelectTemplate (object item, DependencyObject container)
			{
				var button = item as WpfDialogButton;
				if (button == null)
					return base.SelectTemplate (item, container);

				return (button.Button.Image == null) ? this.normalTemplate : this.imageTemplate;
			}

			private static readonly BooleanToVisibilityConverter VisibilityConverter = new BooleanToVisibilityConverter ();
			private readonly DataTemplate imageTemplate;
			private readonly DataTemplate normalTemplate;
		}

		class WpfDialogButton
		{
			public DialogButton Button { get; private set; }

			public bool IsDefault { get; private set; }

			public bool IsCancel { get { return Button.Command == Command.Cancel; } }

			public WpfDialogButton(DialogButton button, bool isDefault = false)
			{
				Button = button;
				IsDefault = isDefault;
			}
		}
	}
}