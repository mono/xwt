using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Xwt.WPFBackend
{
	class PlaceholderTextAdorner: Adorner
	{
		public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register ("PlaceholderText", typeof (string), typeof (PlaceholderTextAdorner), new PropertyMetadata (OnPlaceHolderTextChanged));
		
		static void OnPlaceHolderTextChanged (DependencyObject obj, DependencyPropertyChangedEventArgs args)
		{
			((PlaceholderTextAdorner)obj).InvalidateVisual ();
		}

		PasswordBox AdornedPasswordBox {
			get { return AdornedElement as PasswordBox; }
		}
		
		System.Windows.Controls.TextBox AdornedTextBox {
			get { return AdornedElement as System.Windows.Controls.TextBox; }
		}

		public string PlaceholderText {
			get { return (string) GetValue (PlaceholderTextProperty); }
			set {
				SetValue (PlaceholderTextProperty, value);
				AdornedWidgetChanged (this, null);
			}
		}

		public PlaceholderTextAdorner (UIElement adornedElement)
			: base (adornedElement)
		{
			IsHitTestVisible = false;
			if (AdornedPasswordBox != null) {
				AdornedPasswordBox.PasswordChanged += AdornedWidgetChanged;
			} else if (AdornedTextBox != null) {
				AdornedTextBox.TextChanged += AdornedWidgetChanged;
			}
		}

		void AdornedWidgetChanged(object sender, RoutedEventArgs e)
		{
			if (AdornedPasswordBox !=null)
				Visibility = string.IsNullOrEmpty (AdornedPasswordBox.Password) ? Visibility.Visible: Visibility.Hidden;
			if (AdornedTextBox != null)
				Visibility = string.IsNullOrEmpty (AdornedTextBox.Text) ? Visibility.Visible : Visibility.Hidden;
		}

		protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
		{
			double fontSize;
			Typeface typeFace;
			TextAlignment alignment;
			FlowDirection flowDirection;
			bool multiline = false;
			double ypos = 3, xpos = 6;

			if (AdornedPasswordBox != null) {
				alignment = ConvertAlignment (AdornedPasswordBox.HorizontalContentAlignment);
				flowDirection = AdornedPasswordBox.FlowDirection;
				fontSize = AdornedPasswordBox.FontSize;
				typeFace = AdornedPasswordBox.FontFamily.GetTypefaces ().FirstOrDefault ();
			}
			else {
				multiline = AdornedTextBox.AcceptsReturn;
				alignment = AdornedTextBox.ReadLocalValue (System.Windows.Controls.TextBox.TextAlignmentProperty) !=DependencyProperty.UnsetValue ? AdornedTextBox.TextAlignment : ConvertAlignment (AdornedTextBox.HorizontalContentAlignment);
				flowDirection = AdornedTextBox.FlowDirection;
				fontSize = AdornedTextBox.FontSize;
				typeFace = AdornedTextBox.FontFamily.GetTypefaces ().FirstOrDefault ();
			}
			var text = new System.Windows.Media.FormattedText (PlaceholderText ?? "", CultureInfo.CurrentCulture, flowDirection, typeFace, fontSize, System.Windows.Media.Brushes.LightGray);


			if (!multiline)
				ypos = (RenderSize.Height - text.Height) / 2;

			switch (alignment) {
			case TextAlignment.Center:
				xpos = (RenderSize.Width - text.Width) * 0.5;
				break;
			case TextAlignment.Right:
				xpos = (RenderSize.Width - text.Width) - 6;
				break;
			}

			drawingContext.DrawText (text, new System.Windows.Point (xpos, ypos));
		}

		private TextAlignment ConvertAlignment(System.Windows.HorizontalAlignment horizontalAlignment)
		{
			switch (horizontalAlignment) {
				case System.Windows.HorizontalAlignment.Center:
					return TextAlignment.Center;

				case System.Windows.HorizontalAlignment.Right:
					return TextAlignment.Right;

				case System.Windows.HorizontalAlignment.Stretch:
					return TextAlignment.Justify;

				case System.Windows.HorizontalAlignment.Left:
				default:
					return TextAlignment.Left;
			}
		}
	}
}
