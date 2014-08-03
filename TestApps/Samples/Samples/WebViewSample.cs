//
// WebViewSample.cs
//
// Author:
//       Cody Russell <cody@xamarin.com>
//       Vsevolod Kukol <sevo@sevo.org>
//
// Copyright (c) 2014 Xamarin Inc.
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
using Xwt.Drawing;
using System.Net;
using System.Timers;

namespace Samples
{
	public class WebViewSample : VBox
	{
		readonly WebView wb = new WebView ();
		readonly ProgressBar pg = new ProgressBar ();

		public WebViewSample ()
		{
			var toolbar = new HBox ();
			var back = new Button ("<");
			var forward = new Button (">");
			var lbl = new Label ("Address:");
			var go = new Button ("Go!");
			var stop = new Button ("Stop!");
			stop.Sensitive = false;
			back.Sensitive = false;
			forward.Sensitive = false;
			var uri = new TextEntry ();
			uri.Text = wb.Url;
			toolbar.PackStart (back);
			toolbar.PackStart (forward);
			toolbar.PackStart (lbl);
			toolbar.PackStart (uri, true);
			toolbar.PackEnd (go);
			toolbar.PackEnd (stop);
			PackStart (toolbar);

			var title = new Label ("Title: ");
			PackStart (title);

			var wbscroll = new ScrollView (wb);
			wbscroll.VerticalScrollPolicy = ScrollPolicy.Automatic;
			wbscroll.HorizontalScrollPolicy = ScrollPolicy.Automatic;
			PackStart (wbscroll, true);

			pg.Fraction = 0.0;
			var timer = new Timer (100);
			timer.Elapsed += UpdateProgress;
			PackStart (pg);

			var loadhtml = new Button ("Load sample Html");
			PackStart (loadhtml);

			wb.TitleChanged += (object sender, EventArgs e) => title.Text = "Title: " + wb.Title;

			wb.Loading += delegate(object sender, EventArgs e) {
				uri.Text = wb.Url;
				pg.Fraction = 0.0;
				stop.Sensitive = true;
				timer.Start ();
				back.Sensitive = wb.CanGoBack;
				forward.Sensitive = wb.CanGoForward;
			};
			wb.Loaded += delegate(object sender, EventArgs e) {
				uri.Text = wb.Url;
				stop.Sensitive = false;
				timer.Stop ();
				pg.Fraction = 1.0;
				back.Sensitive = wb.CanGoBack;
				forward.Sensitive = wb.CanGoForward;
			};
			wb.NavigateToUrl += delegate(object sender, NavigateToUrlEventArgs e) {
				if (e.Uri.OriginalString.Contains("facebook.com")) {
					e.SetHandled ();
					MessageDialog.ShowMessage ("Loading *.facebook.com overriden");
				}
				else {
				}
			};

			uri.Activated += (sender, e) => wb.Url = uri.Text;
			go.Clicked += (sender, e) => wb.Url = uri.Text;
			stop.Clicked += (sender, e) => wb.StopLoading ();
			back.Clicked += (sender, e) => wb.GoBack ();
			forward.Clicked += (sender, e) => wb.GoForward ();
			loadhtml.Clicked += LoadHtmlString;

			wb.Url = "http://www.xamarin.com";
		}

		void LoadHtmlString (object sender, EventArgs e)
		{
			string html = 
				"<html><head>" +
				"<title>Xwt.WebView HTML Test</title>" +
				"<style type=\"text/css\">" +
				"<!-- h1 {text-align:center;font-family:Arial, Helvetica, Sans-Serif;}-->" +
				"</style></head><body>" +
				"<h1>Hello, World!</h1>" +
				"<p>Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor" +
				"invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam" +
				"et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est" +
				"Lorem ipsum dolor sit amet.</p>" +
				"<p>Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor" +
				"invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam" +
				"et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est" +
				"Lorem ipsum dolor sit amet.</p>" +
				"</body></html>";
			wb.LoadHtml (html, "sample.html");
		}

		public void UpdateProgress (object sender, ElapsedEventArgs args)
		{
			Application.Invoke ( () => pg.Fraction = wb.LoadProgress );
		}
	}
}

