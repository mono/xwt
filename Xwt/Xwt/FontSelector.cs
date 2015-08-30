//
// FontSelector.cs
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
using System.Collections.Generic;
using System.Linq;
using Xwt.Backends;
using Xwt.Drawing;

namespace Xwt
{
	/// <summary>
	/// The <see cref="Xwt.FontSelector"/> widget allows the user to select a font.
	/// The user can choose from available/installed fonts, styles and sizes.
	/// </summary>
	/// <remarks>
	/// The widget contains a preview text box with the selected font. The preview
	/// text can be modified.
	/// The widget raises the <see cref="Xwt.FontSelector.FontChanged"/> event, when
	/// the selected font changes.
	/// </remarks>
	[BackendType (typeof(IFontSelectorBackend))]
	public class FontSelector: Widget
	{
		protected new class WidgetBackendHost: Widget.WidgetBackendHost, IFontSelectorEventSink
		{
			protected override IBackend OnCreateBackend ()
			{
				var b = base.OnCreateBackend ();
				if (b == null)
					b = new DefaultFontSelectorBackend ();
				return b;
			}

			public void OnFontChanged ()
			{
				((FontSelector)Parent).OnFontChanged (EventArgs.Empty);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Xwt.FontSelector"/> widget.
		/// </summary>
		public FontSelector ()
		{
		}

		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}

		IFontSelectorBackend Backend {
			get { return (IFontSelectorBackend) BackendHost.Backend; }
		}

		/// <summary>
		/// Gets or sets the selected font.
		/// </summary>
		/// <value>The selected font.</value>
		public Font SelectedFont {
			get { return Backend.SelectedFont; }
			set { Backend.SelectedFont = value; }
		}

		/// <summary>
		/// Gets or sets the text used for the font preview.
		/// </summary>
		/// <value>The preview text.</value>
		public string PreviewText {
			get { return Backend.PreviewText; }
			set { Backend.PreviewText = value; }
		}

		protected virtual void OnFontChanged (EventArgs args)
		{
			if (fontChanged != null)
				fontChanged (this, args);
		}

		EventHandler fontChanged;

		/// <summary>
		/// Occurs when the font changed.
		/// </summary>
		public event EventHandler FontChanged {
			add {
				BackendHost.OnBeforeEventAdd (FontSelectorEvent.FontChanged, fontChanged);
				fontChanged += value;
			}
			remove {
				fontChanged -= value;
				BackendHost.OnAfterEventRemove (FontSelectorEvent.FontChanged, fontChanged);
			}
		}
	}

	class DefaultFontSelectorBackend: XwtWidgetBackend, IFontSelectorBackend
	{
		Font selectedFont = Font.SystemFont;
		TextEntry previewText = new TextEntry ();

		ListView listFonts = new ListView ();
		ListStore storeFonts;
		ListView listFace = new ListView ();
		ListStore storeFace;
		ListBox listSize = new ListBox ();
		SpinButton spnSize = new SpinButton();

		static readonly double[] DefaultFontSizes = {
			6.0,     7.0,   8.0,   9.0,   10.0,   11.0,  12.0,  13.0,  14.0,  15.0,  16.0,  17.0,  18.0,
			20.0,   22.0,  24.0,  26.0,   28.0,   32.0,  36.0,  40.0,  44.0,  48.0,  54.0,  60.0,  66.0,
			72.0,   80.0,  88.0,  96.0,  104.0,  112.0, 120.0, 128.0, 136.0, 144.0
		};

		DataField<Font> dfaceFont = new DataField<Font> ();
		DataField<string> dfaceName = new DataField<string> ();
		DataField<string> dfaceMarkup = new DataField<string> ();
		DataField<string> dfamily = new DataField<string> ();
		DataField<string> dfamilymarkup = new DataField<string> ();

		List<string> families = new List<string>();

		bool enableFontChangedEvent;

		public DefaultFontSelectorBackend ()
		{
			families = Font.AvailableFontFamilies.ToList ();
			families.Sort ();

			storeFonts = new ListStore (dfamily, dfamilymarkup);
			listFonts.DataSource = storeFonts;
			listFonts.HeadersVisible = false;
			listFonts.Columns.Add ("Font", new TextCellView () { TextField = dfamily, MarkupField = dfamilymarkup });
			listFonts.MinWidth = 150;

			foreach (var family in families) {
				var row = storeFonts.AddRow ();
				storeFonts.SetValues (row, dfamily, family, dfamilymarkup, "<span font=\"" + family + " " + (listFonts.Font.Size) + "\">" + family + "</span>");
			}

			storeFace = new ListStore (dfaceName, dfaceMarkup, dfaceFont);
			listFace.DataSource = storeFace;
			listFace.HeadersVisible = false;
			listFace.Columns.Add ("Style", new TextCellView () { TextField = dfaceName, MarkupField = dfaceMarkup });
			listFace.MinWidth = 60;
			//listFace.HorizontalScrollPolicy = ScrollPolicy.Never;

			foreach (var size in DefaultFontSizes)
				listSize.Items.Add (size);

			spnSize.Digits = 1;
			spnSize.MinimumValue = 1;
			spnSize.MaximumValue = 800;
			spnSize.IncrementValue = 1;
			PreviewText = "The quick brown fox jumps over the lazy dog.";

			spnSize.ValueChanged += (sender, e) => {
				if (DefaultFontSizes.Contains (spnSize.Value)) {
					var row = Array.IndexOf(DefaultFontSizes, spnSize.Value);
					listSize.ScrollToRow(row);
					listSize.SelectRow(row);
				}
				else
					listSize.UnselectAll ();
				SetFont(selectedFont.WithSize (spnSize.Value));
			};

			SelectedFont = Font.SystemFont;
			UpdateFaceList (selectedFont); // family change not connected at this point, update manually


			listFonts.SelectionChanged += (sender, e) => {
				if (listFonts.SelectedRow >= 0) {
					var newFont = selectedFont.WithFamily (storeFonts.GetValue (listFonts.SelectedRow, dfamily));
					UpdateFaceList (newFont);
					SetFont(newFont);
				}
			};

			listFace.SelectionChanged += (sender, e) => {
				if (listFace.SelectedRow >= 0)
					SetFont (storeFace.GetValue (listFace.SelectedRow, dfaceFont).WithSize (selectedFont.Size));
			};

			listSize.SelectionChanged += (sender, e) => {
				if (listSize.SelectedRow >= 0 && Math.Abs (DefaultFontSizes [listSize.SelectedRow] - spnSize.Value) > double.Epsilon)
					spnSize.Value = DefaultFontSizes[listSize.SelectedRow];
			};

			VBox familyBox = new VBox ();
			familyBox.PackStart (new Label ("Font:"));
			familyBox.PackStart (listFonts, true);

			VBox styleBox = new VBox ();
			styleBox.PackStart (new Label ("Style:"));
			styleBox.PackStart (listFace, true);

			VBox sizeBox = new VBox ();
			sizeBox.PackStart (new Label ("Size:"));
			sizeBox.PackStart (spnSize);
			sizeBox.PackStart (listSize, true);

			HBox fontBox = new HBox ();
			fontBox.PackStart (familyBox, true);
			fontBox.PackStart (styleBox, true);
			fontBox.PackStart (sizeBox);

			VBox mainBox = new VBox ();
			mainBox.MinWidth = 350;
			mainBox.MinHeight = 300;
			mainBox.PackStart (fontBox, true);
			mainBox.PackStart (new Label ("Preview:"));
			mainBox.PackStart (previewText);

			Content = mainBox;
		}

		void SetFont(Font newFont)
		{
			selectedFont = newFont;
			previewText.Font = selectedFont;

			if (Math.Abs (spnSize.Value - selectedFont.Size) > double.Epsilon)
				spnSize.Value = selectedFont.Size;

			if (enableFontChangedEvent)
				Application.Invoke (delegate {
				EventSink.OnFontChanged ();
			});
		}

		void UpdateFaceList (Font font)
		{
			storeFace.Clear ();
			int row = -1;
			foreach (var face in font.GetAvailableFontFaces ()) {
				row = storeFace.AddRow ();
				storeFace.SetValues (row, dfaceName, face.Name, dfaceFont, face.Font, dfaceMarkup, "<span font=\"" + face.Font.WithSize (listFace.Font.Size) + "\">" + face.Name + "</span>");
			}
			if (row >= 0) {
				listFace.SelectRow (0);
			}
			listFace.QueueForReallocate ();
		}

		public Font SelectedFont {
			get {
				return selectedFont;
			}
			set {
				selectedFont = value;
				previewText.Font = selectedFont;

				if (Math.Abs (spnSize.Value - selectedFont.Size) > double.Epsilon)
					spnSize.Value = selectedFont.Size;

				int rowFamily = families.IndexOf (selectedFont.Family);
				if (listFonts.SelectedRow != rowFamily) {
					listFonts.ScrollToRow (rowFamily);
					listFonts.SelectRow (rowFamily);
				}
			}
		}

		public string PreviewText {
			get { return previewText.Text; }
			set { previewText.Text = value; }
		}

		protected new IFontSelectorEventSink EventSink {
			get { return (IFontSelectorEventSink)base.EventSink; }
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is FontSelectorEvent) {
				switch ((FontSelectorEvent)eventId) {
					case FontSelectorEvent.FontChanged: enableFontChangedEvent = true; break;
				}
			}
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is FontSelectorEvent) {
				switch ((FontSelectorEvent)eventId) {
					case FontSelectorEvent.FontChanged: enableFontChangedEvent = false; break;
				}
			}
		}
	}
}

