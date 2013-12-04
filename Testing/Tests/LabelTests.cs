//
// LabelTests.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2012 Xamarin Inc
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
using NUnit.Framework;

namespace Xwt
{
	public class LabelTests: WidgetTests
	{
		public override Widget CreateWidget ()
		{
			return new Label ("Hello World");
		}

		[Test]
		public void DefaultValues ()
		{
			var la = new Label ();
			Assert.AreEqual ("", la.Text);
			Assert.AreEqual (Alignment.Start, la.TextAlignment);
			Assert.AreEqual (EllipsizeMode.None, la.Ellipsize);
			Assert.AreEqual (WrapMode.None, la.Wrap);
		}

		[Test]
		public void AlignCenter ()
		{
			var la = new Label ("Some text here");
			la.TextAlignment = Alignment.Center;
			la.BackgroundColor = Xwt.Drawing.Colors.LightGray;
			CheckWidgetRender ("Label.AlignCenter.png", la);
		}

		[Test]
		public void AlignCenterWrapped ()
		{
			var la = new Label ("Some text here Some text here Some text here Some text here");
			la.TextAlignment = Alignment.Center;
			la.Wrap = WrapMode.Word;
			la.WidthRequest = 200;
			la.BackgroundColor = Xwt.Drawing.Colors.LightGray;
			CheckWidgetRender ("Label.AlignCenterWrapped.png", la);
		}

		[Test]
		public void AlignCenterWrappedChangeText ()
		{
			var la = new Label ("Some text here");
			la.TextAlignment = Alignment.Center;
			la.Wrap = WrapMode.Word;
			la.WidthRequest = 200;
			la.BackgroundColor = Xwt.Drawing.Colors.LightGray;

			using (var win = new Window { Width = 200, Height = 100 }) {
				win.Content = la;
				ShowWindow (win);
				la.Text = "Some text here Some text here";
				WaitForEvents ();
				var img = Toolkit.CurrentEngine.RenderWidget (la);
				ReferenceImageManager.CheckImage ("Label.AlignCenterWrappedChangeText.png", img);
			}
		}

		[Test]
		public void AlignLeft ()
		{
			var la = new Label ("Some text here");
			la.TextAlignment = Alignment.Start;
			la.BackgroundColor = Xwt.Drawing.Colors.LightGray;
			CheckWidgetRender ("Label.AlignLeft.png", la);
		}

		[Test]
		public void AlignLeftWrapped ()
		{
			var la = new Label ("Some text here Some text here Some text here Some text here");
			la.TextAlignment = Alignment.Start;
			la.Wrap = WrapMode.Word;
			la.WidthRequest = 200;
			la.BackgroundColor = Xwt.Drawing.Colors.LightGray;
			CheckWidgetRender ("Label.AlignLeftWrapped.png", la);
		}

		[Test]
		public void AlignRight ()
		{
			var la = new Label ("Some text here");
			la.TextAlignment = Alignment.End;
			la.BackgroundColor = Xwt.Drawing.Colors.LightGray;
			CheckWidgetRender ("Label.AlignRight.png", la);
		}

		[Test]
		public void AlignRightWrapped ()
		{
			var la = new Label ("Some text here Some text here Some text here Some text here");
			la.TextAlignment = Alignment.End;
			la.Wrap = WrapMode.Word;
			la.WidthRequest = 200;
			la.BackgroundColor = Xwt.Drawing.Colors.LightGray;
			CheckWidgetRender ("Label.AlignRightWrapped.png", la);
		}
	}
}

