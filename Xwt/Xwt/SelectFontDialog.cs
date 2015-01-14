//
// SelectFontDialog.cs
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
using Xwt.Drawing;
using Xwt.Backends;
using System;

namespace Xwt
{
	public sealed class SelectFontDialog
	{
		Font font = Font.SystemFont;
		string title = "Select a font";
		string previewText = "The quick brown fox jumps over the lazy dog.";

		public SelectFontDialog ()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Xwt.SelectFontDialog"/> class.
		/// </summary>
		/// <param name='title'>
		/// Title of the dialog
		/// </param>
		public SelectFontDialog (string title)
		{
			this.title = title;
		}

		/// <summary>
		/// Gets or sets the title of the dialog
		/// </summary>
		public string Title {
			get { return title ?? ""; }
			set { title = value ?? ""; }
		}

		/// <summary>
		/// Gets or sets the selected font
		/// </summary>
		public Font SelectedFont {
			get { return font; }
			set { font = value; }
		}

		/// <summary>
		/// Gets or sets the text used for the font preview.
		/// </summary>
		/// <value>The preview text.</value>
		public string PreviewText {
			get { return previewText; }
			set { previewText = value; }
		}

		/// <summary>
		/// Shows the dialog.
		/// </summary>
		public bool Run ()
		{
			return Run (null);
		}

		/// <summary>
		/// Shows the dialog.
		/// </summary>
		public bool Run (WindowFrame parentWindow)
		{
			var backend = Toolkit.CurrentEngine.Backend.CreateBackend<ISelectFontDialogBackend> ();
			if (backend == null)
				backend = new DefaultSelectFontDialogBackend (parentWindow);
			try {
				backend.SelectedFont = SelectedFont;
				backend.Title = Title;
				backend.PreviewText = PreviewText;
				return backend.Run ((IWindowFrameBackend)Toolkit.CurrentEngine.GetSafeBackend (parentWindow));
			} catch (Exception ex) {
				Console.WriteLine (ex);
				return false;
			} finally {
				font = backend.SelectedFont;
				backend.Dispose ();
			}
		}
	}

	public class DefaultSelectFontDialogBackend: ISelectFontDialogBackend
	{
		readonly Dialog fontDialog;
		readonly WindowFrame parent;
		readonly FontSelector fontSelector;

		public DefaultSelectFontDialogBackend (WindowFrame parentWindow)
		{
			parent = parentWindow;

			fontDialog = new Dialog ();
			fontDialog.Width = 500;
			fontDialog.Height = 300;

			VBox box = new VBox ();
			fontSelector = new FontSelector ();
			fontSelector.FontChanged += (sender, e) => SelectedFont = fontSelector.SelectedFont;
			box.PackStart (fontSelector, true);

			fontDialog.Content = box;

			fontDialog.Buttons.Add (new DialogButton (Command.Cancel));
			fontDialog.Buttons.Add (new DialogButton (Command.Ok));
		}

		public Font SelectedFont {
			get;
			set;
		}

		public string PreviewText {
			get {
				return fontSelector.PreviewText;
			}
			set {
				fontSelector.PreviewText = value;
			}
		}

		public string Title {
			get {
				return fontDialog.Title;
			}
			set {
				fontDialog.Title = value;
			}
		}

		public bool Run (IWindowFrameBackend parent)
		{
			fontSelector.SelectedFont = SelectedFont;
			return fontDialog.Run (this.parent) == Command.Ok;
		}

		public void Dispose ()
		{
			fontDialog.Dispose ();
		}
	}
}

