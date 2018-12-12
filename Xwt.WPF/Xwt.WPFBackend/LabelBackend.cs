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
			var atts = new List<Drawing.TextAttribute> (text.Attributes);
			atts.Sort ((a, b) => {
				var c = a.StartIndex.CompareTo (b.StartIndex);
				if (c == 0)
					c = -(a.Count.CompareTo (b.Count));
				return c;
			});

			int i = 0, attrIndex = 0;
			Label.TextBlock.Inlines.Clear ();
			GenerateBlocks (Label.TextBlock.Inlines, text.Text, ref i, text.Text.Length, atts, ref attrIndex);
		}

		void GenerateBlocks (SWD.InlineCollection col, string text, ref int i, int spanEnd, List<Drawing.TextAttribute> attributes, ref int attrIndex)
		{
			while (attrIndex < attributes.Count) {
				var at = attributes[attrIndex];
				if (at.StartIndex > spanEnd) {
					FlushText (col, text, ref i, spanEnd);
					return;
				}

				FlushText (col, text, ref i, at.StartIndex);

				var s = new SWD.Span ();

				if (at is Drawing.BackgroundTextAttribute) {
					s.Background = new SWM.SolidColorBrush (((Drawing.BackgroundTextAttribute)at).Color.ToWpfColor ());
				}
				else if (at is Drawing.FontWeightTextAttribute) {
					s.FontWeight = ((Drawing.FontWeightTextAttribute)at).Weight.ToWpfFontWeight ();
				}
				else if (at is Drawing.FontStyleTextAttribute) {
					s.FontStyle = ((Drawing.FontStyleTextAttribute)at).Style.ToWpfFontStyle ();
				}
				else if (at is Drawing.UnderlineTextAttribute) {
					var xa = (Drawing.UnderlineTextAttribute)at;
					var dec = new TextDecoration (TextDecorationLocation.Underline, null, 0, TextDecorationUnit.FontRecommended, TextDecorationUnit.FontRecommended);
					s.TextDecorations.Add (dec);
				}
				else if (at is Drawing.StrikethroughTextAttribute) {
					var xa = (Drawing.StrikethroughTextAttribute)at;
					var dec = new TextDecoration (TextDecorationLocation.Strikethrough, null, 0, TextDecorationUnit.FontRecommended, TextDecorationUnit.FontRecommended);
					s.TextDecorations.Add (dec);
				}
				else if (at is Drawing.FontTextAttribute) {
					var xa = (Drawing.FontTextAttribute)at;
					s.FontFamily = new SWM.FontFamily (xa.Font.Family);
					s.FontSize = WpfFontBackendHandler.GetPointsFromDeviceUnits (xa.Font.Size);
					s.FontStretch = xa.Font.Stretch.ToWpfFontStretch ();
					s.FontStyle = xa.Font.Style.ToWpfFontStyle ();
					s.FontWeight = xa.Font.Weight.ToWpfFontWeight ();
				}
				else if (at is Drawing.ColorTextAttribute) {
					s.Foreground = new SWM.SolidColorBrush (((Drawing.ColorTextAttribute)at).Color.ToWpfColor ());
				}
				else if (at is Drawing.LinkTextAttribute) {
					var link = new SWD.Hyperlink () {
						NavigateUri = ((Drawing.LinkTextAttribute)at).Target
					};
					link.RequestNavigate += new System.Windows.Navigation.RequestNavigateEventHandler (link_RequestNavigate);
					s = link;
				}

				col.Add (s);

				var max = i + at.Count;
				if (max > spanEnd)
					max = spanEnd;

				attrIndex++;
				GenerateBlocks (s.Inlines, text, ref i, i + at.Count, attributes, ref attrIndex);
			}
			FlushText (col, text, ref i, spanEnd);
		}

		void link_RequestNavigate (object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
		{
			Context.InvokeUserCode (delegate {
				EventSink.OnLinkClicked (e.Uri);
			});
		}

		void FlushText (SWD.InlineCollection col, string text, ref int i, int pos)
		{
			if (pos > i) {
				col.Add (text.Substring (i, pos - i));
				i = pos;
			}
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
