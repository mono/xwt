// 
// LabelBackend.cs
//  
// Author:
//       Carlos Alberto Cortez <calberto.cortez@gmail.com>
// 
// Copyright (c) 2012 Carlos Alberto Cortez
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
using SWC = System.Windows.Controls;
using SWM = System.Windows.Media;
using SWD = System.Windows.Documents;

using Xwt.Backends;
using System.Windows.Automation.Peers;
using System.Windows.Threading;

namespace Xwt.WPFBackend
{
	public class LabelBackend : WidgetBackend, ILabelBackend
	{
		public LabelBackend ()
		{
			Widget = new WpfLabel ();
		}

		WpfLabel Label {
			get { return (WpfLabel)Widget; }
		}

		new ILabelEventSink EventSink
		{
			get { return (ILabelEventSink)base.EventSink; }
		}

		public override Size GetPreferredSize (SizeConstraint widthConstraint, SizeConstraint heightConstraint)
		{
			var s = base.GetPreferredSize (widthConstraint, heightConstraint);

			// If the label is ellipsized or can wrap then the width can't be dermined unless we have a constraint.
			// If there is no constraint, just return the smallest size
			if (!widthConstraint.IsConstrained && (Wrap != WrapMode.None || Ellipsize != EllipsizeMode.None))
				s.Width = 1;
			return s;
		}

		public string Text {
			get { return Label.TextBlock.Text; }
			set {
				Label.TextBlock.Text = value;
				Widget.InvalidateMeasure();
			}
		}

		public new bool CanGetFocus {
			get {
				return ((WpfLabel) Widget).TextBlock.Focusable;
			}
			set {
				((WpfLabel) Widget).TextBlock.Focusable = value;
			}
		}

		void FocusOnUIThread ()
		{
			// Using Render (7) priority here instead of default Normal (9) so that
			// the component has some time to initialize and get ready to receive the focus
			Widget.Dispatcher.BeginInvoke ((Action) (() => {
				((WpfLabel) Widget).TextBlock.Focus ();
			}), DispatcherPriority.Render);
		}

		public new void SetFocus ()
		{
			if (Widget.IsLoaded)
				FocusOnUIThread ();
			else
				Widget.Loaded += DeferredFocus;
		}

		void DeferredFocus (object sender, RoutedEventArgs e)
		{
			Widget.Loaded -= DeferredFocus;
			FocusOnUIThread ();
		}

		public bool Selectable { get; set; } // TODO: this is only supported on Win10 with UWP?

		public void SetFormattedText (FormattedText text)
		{
			Label.TextBlock.ApplyFormattedText (text, link_RequestNavigate);
		}

		void link_RequestNavigate (object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
		{
			Context.InvokeUserCode (delegate {
				EventSink.OnLinkClicked (e.Uri);
			});
		}

		public Xwt.Drawing.Color TextColor {
			get {
				SWM.Color color = SystemColors.ControlColor;

				if (Label.Foreground != null)
					color = ((SWM.SolidColorBrush) Label.Foreground).Color;

				return DataConverter.ToXwtColor (color);
			}
			set {
				Label.Foreground = ResPool.GetSolidBrush (value);
			}
		}

		public Alignment TextAlignment {
			get { return DataConverter.ToXwtAlignment (Label.HorizontalContentAlignment); }
			set { 
				Label.HorizontalContentAlignment = DataConverter.ToWpfAlignment (value); 
				Label.TextBlock.TextAlignment = DataConverter.ToTextAlignment (value);
			}
		}

		public EllipsizeMode Ellipsize {
			get {
				if (Label.TextBlock.TextTrimming == TextTrimming.None)
					return Xwt.EllipsizeMode.None;
				else
					return Xwt.EllipsizeMode.End;
			}
			set {
				if (value == EllipsizeMode.None)
					Label.TextBlock.TextTrimming = TextTrimming.None;
				else
					Label.TextBlock.TextTrimming = TextTrimming.CharacterEllipsis;
				Widget.InvalidateMeasure ();
			}
		}

		public WrapMode Wrap {
			get {
				if (Label.TextBlock.TextWrapping == TextWrapping.NoWrap)
					return WrapMode.None;
				else
					return WrapMode.Word;
			} set {
				if (value == WrapMode.None)
					Label.TextBlock.TextWrapping = TextWrapping.NoWrap;
				else
					Label.TextBlock.TextWrapping = TextWrapping.Wrap;
			}
		}
	}

	class WpfLabel : SWC.Label, IWpfWidget
	{
		public WpfLabel ()
		{
			TextBlock = new SWC.TextBlock ();
			Content = TextBlock;
			Padding = new Thickness (0);
			VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
		}

		public WidgetBackend Backend { get; set; }

		protected override System.Windows.Size MeasureOverride (System.Windows.Size constraint)
		{
			var s = base.MeasureOverride (constraint);
			return Backend.MeasureOverride (constraint, s);
		}

		public SWC.TextBlock TextBlock {
			get;
			set;
		}

		protected override AutomationPeer OnCreateAutomationPeer ()
		{
			return new WpfLabelAutomationPeer (this);
		}

		class WpfLabelAutomationPeer : LabelAutomationPeer
		{
			public WpfLabelAutomationPeer (WpfLabel owner) : base (owner)
			{
			}

			protected override string GetNameCore ()
			{
				return ((WpfLabel)Owner).TextBlock.Text;
			}
		}
	}
}
