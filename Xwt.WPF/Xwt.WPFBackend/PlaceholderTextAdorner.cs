using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Xwt.WPFBackend
{
	class PlaceholderTextAdorner :Adorner
	{
		public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register ("PlaceholderText", typeof (string), typeof (PlaceholderTextAdorner), new PropertyMetadata (OnPlaceHolderTextChanged));
		
		static void OnPlaceHolderTextChanged (DependencyObject obj, DependencyPropertyChangedEventArgs args)
		{
			((PlaceholderTextAdorner)obj).InvalidateVisual ();
		}

		PasswordBox AdornedPasswordBox {
			get { return AdornedElement as PasswordBox; }
		}
		
		TextBox AdornedTextBox {
			get { return AdornedElement as TextBox; }
		}

		public string PlaceholderText {
			get { return (string) GetValue (PlaceholderTextProperty); }
			set { SetValue (PlaceholderTextProperty, value); }
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
			Typeface typeFace = null;
			double fontSize = -1;
			if (AdornedPasswordBox != null) {
				typeFace = AdornedPasswordBox.FontFamily.GetTypefaces ().FirstOrDefault ();
				fontSize = AdornedPasswordBox.FontSize;
			} else {
				typeFace = AdornedTextBox.FontFamily.GetTypefaces ().FirstOrDefault ();
				fontSize = AdornedTextBox.FontSize;
			}
			var text = new System.Windows.Media.FormattedText (PlaceholderText ?? "", System.Globalization.CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight, typeFace, fontSize, System.Windows.Media.Brushes.LightGray);
			drawingContext.DrawText(text, new System.Windows.Point (4, 0));
		}
	}
}
