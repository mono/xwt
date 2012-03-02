// 
// ComboBoxBackend.cs
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xwt.Backends;
using Xwt.WPFBackend.Utilities;
using WindowsSeparator = System.Windows.Controls.Separator;
using WindowsComboBox = System.Windows.Controls.ComboBox;
using WindowsOrientation = System.Windows.Controls.Orientation;
using WindowsComboBoxItem = System.Windows.Controls.ComboBoxItem;

namespace Xwt.WPFBackend
{
	public class ComboBoxBackend
		: WidgetBackend, IComboBoxBackend
	{
		private static readonly Style ContainerStyle;
		private static readonly DataTemplate Defaulttemplate;

		static ComboBoxBackend()
		{
			var factory = new FrameworkElementFactory (typeof (WindowsSeparator));
	        factory.SetValue (UIElement.IsEnabledProperty, false);
	        factory.SetValue (FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
			
			var sepTemplate = new ControlTemplate (typeof (ComboBoxItem));
			sepTemplate.VisualTree = factory;

			DataTrigger trigger = new DataTrigger();
			trigger.Binding = new Binding (".[1]") { Converter = new TypeToStringConverter() };
			trigger.Value = typeof(ItemSeparator).Name;
			trigger.Setters.Add (new Setter (Control.TemplateProperty, sepTemplate));

			ContainerStyle = new Style (typeof (ComboBoxItem));
			ContainerStyle.Triggers.Add (trigger);

			FrameworkElementFactory f = new FrameworkElementFactory (typeof (TextBlock));
			f.SetBinding (TextBlock.TextProperty, new Binding (".[0]"));
			Defaulttemplate = new DataTemplate { VisualTree = f };
		}

		public ComboBoxBackend()
		{
			Widget = new WindowsComboBox();

			ComboBox.ItemTemplate = Defaulttemplate;
			ComboBox.ItemContainerStyle = ContainerStyle;
		}

		public void SetViews (CellViewCollection views)
		{
			ComboBox.ItemTemplate = GetDataTemplate (views);
		}

		public void SetSource (IListDataSource source, IBackend sourceBackend)
		{
			ComboBox.ItemsSource = new DataSourceNotifyWrapper (source);
		}

		public int SelectedRow
		{
			get { return ComboBox.SelectedIndex; }
			set { ComboBox.SelectedIndex = value; }
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			
			if (eventId is ComboBoxEvent) {

				switch ((ComboBoxEvent)eventId) {
					case ComboBoxEvent.SelectionChanged:
						ComboBox.SelectionChanged += OnSelectionChanged;
					break;
				}
			}
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			
			if (eventId is ComboBoxEvent) {

				switch ((ComboBoxEvent)eventId) {
					case ComboBoxEvent.SelectionChanged:
						ComboBox.SelectionChanged -= OnSelectionChanged;
					break;
				}
			}
		}

		protected WindowsComboBox ComboBox
		{
			get { return (WindowsComboBox) Widget; }
		}

		protected IComboBoxEventSink ComboBoxEventSink
		{
			get { return (IComboBoxEventSink) EventSink; }
		}

		private void OnSelectionChanged (object sender, SelectionChangedEventArgs e)
		{
			ComboBoxEventSink.OnSelectionChanged();
		}

		private DataTemplate GetDataTemplate (IList<CellView> views)
		{
			var template = new DataTemplate (typeof (object[]));

			FrameworkElementFactory root;
			if (views.Count > 1) {
				FrameworkElementFactory spFactory = new FrameworkElementFactory (typeof (StackPanel));
				spFactory.SetValue (StackPanel.OrientationProperty, WindowsOrientation.Horizontal);

				foreach (var view in views) {
					spFactory.AppendChild (CellUtil.CreateBoundCellRenderer (view));
				}

				root = spFactory;
			} else {
				root = CellUtil.CreateBoundCellRenderer (views [0]);
			}

			template.VisualTree = root;
			return template;
		}
	}
}
