//
// SelectFontDialogBackend.cs
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
using Xwt.Backends;
using Xwt.Drawing;

#if MONOMAC
using MonoMac.AppKit;
using MonoMac.Foundation;
#else
using AppKit;
using Foundation;
#endif

namespace Xwt.Mac
{
	public class SelectFontDialogBackend : ISelectFontDialogBackend
	{
		readonly NSFontPanel fontPanel;

		public SelectFontDialogBackend ()
		{
			fontPanel = NSFontPanel.SharedFontPanel;
		}

		public bool Run (IWindowFrameBackend parent)
		{
			fontPanel.Delegate = new FontPanelDelegate ();

			if (SelectedFont != null) {
				NSFontManager.SharedFontManager.SetSelectedFont (((FontData)Toolkit.GetBackend (SelectedFont)).Font, false);
			}

			NSApplication.SharedApplication.RunModalForWindow (fontPanel);

			var font = NSFontPanel.SharedFontPanel.PanelConvertFont (NSFont.SystemFontOfSize (0));
			SelectedFont = Font.FromName (FontData.FromFont (font).ToString ());

			return true;
		}

		public string Title {
			get {
				return fontPanel.Title;
			}
			set {
				fontPanel.Title = value;
			}
		}

		public Font SelectedFont { get; set; }

		public string PreviewText { get; set; }

		public void Dispose ()
		{
		}
	}

	class FontPanelDelegate : NSWindowDelegate
	{
		[Export ("validModesForFontPanel:")]
		NSFontPanelMode ValidModesForFontPanel (NSFontPanel panel)
		{
			return NSFontPanelMode.CollectionMask | NSFontPanelMode.FaceMask | NSFontPanelMode.SizeMask;
		}

		public override void WillClose (NSNotification notification)
		{
			NSApplication.SharedApplication.StopModal ();
		}
	}
}

