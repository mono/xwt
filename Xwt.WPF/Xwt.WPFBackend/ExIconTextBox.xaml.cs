//
// ExIconTextBox.xaml.cs
//
// Author:
//       Vsevolod Kukol <sevo@sevo.org>
//
// Copyright (c) 2015 Vsevolod Kukol
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
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Xwt.Backends;
using Xwt.WPFBackend.Utilities;

namespace Xwt.WPFBackend
{
	public class ExIconTextBox : ExTextBox
	{
		static ResourceDictionary resIconTextBox;
		ImageBox rightIcon;
		ImageBox leftIcon;

		public ExIconTextBox ()
		{
			Resources.MergedDictionaries.Add (TextBoxResources);
		}

		protected static ResourceDictionary TextBoxResources
		{
			get {
				if (resIconTextBox == null) {
					var uri = new Uri ("pack://application:,,,/Xwt.WPF;component/XWT.WPFBackend/ExIconTextBox.xaml");
					resIconTextBox = (ResourceDictionary)XamlReader.Load (System.Windows.Application.GetResourceStream (uri).Stream);
				}
				return resIconTextBox;
			}
		}

		public override void OnApplyTemplate ()
		{
			leftIcon = GetTemplateChild("PART_LeftIcon") as ImageBox;
			if (leftIcon != null) {
				leftIcon.MouseUp += (sender, e) => {
					if (LeftIconMouseUp != null)
						LeftIconMouseUp (this, e);
				};
				leftIcon.MouseDown += (sender, e) => {
					if (LeftIconMouseDown != null)
						LeftIconMouseDown (this, e);
				};
			}

			rightIcon = GetTemplateChild("PART_RightIcon") as ImageBox;
			if (rightIcon != null) {
				rightIcon.MouseUp += (sender, e) => {
					if (RightIconMouseUp != null)
						RightIconMouseUp (this, e);
				};
				rightIcon.MouseDown += (sender, e) => {
					if (RightIconMouseDown != null)
						RightIconMouseDown (this, e);
				};
			}

			base.OnApplyTemplate ();
		}

		public event EventHandler<MouseButtonEventArgs> LeftIconMouseUp;
		public event EventHandler<MouseButtonEventArgs> LeftIconMouseDown;
		public event EventHandler<MouseButtonEventArgs> RightIconMouseUp;
		public event EventHandler<MouseButtonEventArgs> RightIconMouseDown;

		protected override void OnPropertyChanged (DependencyPropertyChangedEventArgs e)
		{
			if (e.Property == RightIconProperty)
				HasRightIcon = !RightIcon.IsNull;
			if (e.Property == LeftIconProperty)
				HasLeftIcon = !LeftIcon.IsNull;
			base.OnPropertyChanged (e);
		}

		protected override void OnTextChanged(TextChangedEventArgs e)
		{
			base.OnTextChanged(e);
			HasText = Text.Length != 0;
		}

		public string PlaceholderText {
			get { return (string)GetValue(PlaceholderTextProperty); }
			set { SetValue(PlaceholderTextProperty, value); }
		}

		public Brush PlaceholderTextColor {
			get { return (Brush)GetValue(PlaceholderTextColorProperty); }
			set { SetValue(PlaceholderTextColorProperty, value); }
		}

		public bool HasText {
			get { return (bool)GetValue(HasTextProperty); }
			private set { SetValue(HasTextPropertyKey, value); }
		}

		public ImageDescription LeftIcon {
			get { return (ImageDescription)GetValue(LeftIconProperty); }
			set { SetValue(LeftIconProperty, value); }
		}

		public ImageDescription RightIcon {
			get { return (ImageDescription)GetValue(RightIconProperty); }
			set { SetValue(RightIconProperty, value); }
		}

		public bool HasLeftIcon {
			get { return (bool)GetValue(HasLeftIconProperty); }
			private set { SetValue(HasLeftIconPropertyKey, value); }
		}

		public bool HasRightIcon {
			get { return (bool)GetValue(HasRightIconProperty); }
			private set { SetValue(HasRightIconPropertyKey, value); }
		}

		public static DependencyProperty PlaceholderTextProperty = DependencyProperty.Register (
			"PlaceholderText",
			typeof(string),
			typeof(ExIconTextBox));

		public static DependencyProperty PlaceholderTextColorProperty = DependencyProperty.Register (
			"PlaceholderTextColor",
			typeof(Brush),
			typeof(ExIconTextBox));

		public static DependencyProperty LeftIconProperty = DependencyProperty.Register (
			"LeftIcon",
			typeof(ImageDescription),
			typeof(ExIconTextBox));

		public static DependencyProperty RightIconProperty = DependencyProperty.Register (
			"RightIcon",
			typeof(ImageDescription),
			typeof(ExIconTextBox));

		static DependencyPropertyKey HasLeftIconPropertyKey = DependencyProperty.RegisterReadOnly (
			"HasLeftIcon",
			typeof(bool),
			typeof(ExIconTextBox),
			new PropertyMetadata ());

		static DependencyPropertyKey HasRightIconPropertyKey = DependencyProperty.RegisterReadOnly (
			"HasRightIcon",
			typeof(bool),
			typeof(ExIconTextBox),
			new PropertyMetadata ());

		static DependencyPropertyKey HasTextPropertyKey = DependencyProperty.RegisterReadOnly (
			"HasText",
			typeof(bool),
			typeof(ExIconTextBox),
			new PropertyMetadata ());

		public static DependencyProperty HasLeftIconProperty = HasLeftIconPropertyKey.DependencyProperty;
		public static DependencyProperty HasRightIconProperty = HasRightIconPropertyKey.DependencyProperty;
		public static DependencyProperty HasTextProperty = HasTextPropertyKey.DependencyProperty;
	}
}

