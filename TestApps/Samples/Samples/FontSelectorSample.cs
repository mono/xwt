//
// FontSelectorSample.cs
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
using Xwt;

namespace Samples
{
	public class FontSelectorSample: VBox
	{
		public FontSelectorSample ()
		{
			FontSelector sel = new FontSelector ();
			sel.PreviewText = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy";
			Label preview = new Label ("Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy");
			preview.Ellipsize = EllipsizeMode.End;

			sel.FontChanged += (sender, e) => preview.Font = sel.SelectedFont;

			PackStart (sel);
			PackStart (new HSeparator());
			PackStart (preview);

			var lblSystemFont = new Label ();
			lblSystemFont.Font = Xwt.Drawing.Font.SystemFont;
			lblSystemFont.Text = lblSystemFont.Font.ToString ();
			var selectSystemFont = new Button ("Select");
			selectSystemFont.Clicked += (sender, e) => sel.SelectedFont = Xwt.Drawing.Font.SystemFont;

			var lblSystemMonospaceFont = new Label ();
			lblSystemMonospaceFont.Font = Xwt.Drawing.Font.SystemMonospaceFont;
			lblSystemMonospaceFont.Text = lblSystemMonospaceFont.Font.ToString ();
			var selectSystemMonospaceFont = new Button ("Select");
			selectSystemMonospaceFont.Clicked += (sender, e) => sel.SelectedFont = Xwt.Drawing.Font.SystemMonospaceFont;

			var lblSystemSerifFont = new Label ();
			lblSystemSerifFont.Font = Xwt.Drawing.Font.SystemSerifFont;
			lblSystemSerifFont.Text = lblSystemSerifFont.Font.ToString ();
			var selectSystemSerifFont = new Button ("Select");
			selectSystemSerifFont.Clicked += (sender, e) => sel.SelectedFont = Xwt.Drawing.Font.SystemSerifFont;

			var lblSystemSansSerifFont = new Label ();
			lblSystemSansSerifFont.Font = Xwt.Drawing.Font.SystemSansSerifFont;
			lblSystemSansSerifFont.Text = lblSystemSansSerifFont.Font.ToString ();
			var selectSystemSansSerifFont = new Button ("Select");
			selectSystemSansSerifFont.Clicked += (sender, e) => sel.SelectedFont = Xwt.Drawing.Font.SystemSansSerifFont;

			var tblSystemFonts = new Table ();
			tblSystemFonts.Add (new Label ("System Font:"), 0, 0);
			tblSystemFonts.Add (lblSystemFont, 1, 0);
			tblSystemFonts.Add (selectSystemFont, 2, 0);
			tblSystemFonts.Add (new Label ("System Monospace Font:"), 0, 1);
			tblSystemFonts.Add (lblSystemMonospaceFont, 1, 1);
			tblSystemFonts.Add (selectSystemMonospaceFont, 2, 1);
			tblSystemFonts.Add (new Label ("System Serif Font:"), 0, 2);
			tblSystemFonts.Add (lblSystemSerifFont, 1, 2);
			tblSystemFonts.Add (selectSystemSerifFont, 2, 2);
			tblSystemFonts.Add (new Label ("System SansSerif Font:"), 0, 3);
			tblSystemFonts.Add (lblSystemSansSerifFont, 1, 3);
			tblSystemFonts.Add (selectSystemSansSerifFont, 2, 3);

			PackStart (new HSeparator());
			PackStart (tblSystemFonts);
		}
	}
}

