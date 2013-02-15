//
// ReferenceImageVerifierDialog.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc.
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

namespace Tests
{
	public class ReferenceImageVerifierDialog: Dialog
	{
		int currentImage = -1;
		ImageView img1;
		ImageView img2;

		Button closeButton;
		Button validButton;
		Button failButton;

		public ReferenceImageVerifierDialog ()
		{
			Width = 500;
			Height = 300;

			Table table = new Table ();

			table.Attach (new Label ("Reference Image"), 0, 0, AttachOptions.Expand | AttachOptions.Fill, AttachOptions.Shrink);
			table.Attach (new Label ("Test Image"), 1, 0, AttachOptions.Expand | AttachOptions.Fill, AttachOptions.Shrink);

			img1 = new ImageView ();
			table.Attach (img1, 0, 1, AttachOptions.Expand | AttachOptions.Fill, AttachOptions.Expand | AttachOptions.Fill);
			
			img2 = new ImageView ();
			table.Attach (img2, 1, 1, AttachOptions.Expand | AttachOptions.Fill, AttachOptions.Expand | AttachOptions.Fill);

			var buttonBox = new HBox ();
			table.Attach (buttonBox, 0, 2, 2, 3, AttachOptions.Expand | AttachOptions.Fill, AttachOptions.Shrink);

			var closeButton = new Button ("Close");
			var validButton = new Button ("Success");
			var failButton = new Button ("Failure");

			buttonBox.PackEnd (closeButton);
			buttonBox.PackEnd (failButton);
			buttonBox.PackEnd (validButton);

			closeButton.Clicked += delegate {
				Respond (Command.Ok);
			};

			failButton.Clicked += delegate {
				var info = ReferenceImageManager.ImageFailures[currentImage];
				info.Fail ();
			};
			
			validButton.Clicked += delegate {
				var info = ReferenceImageManager.ImageFailures[currentImage];
				info.Validate ();
			};

			ShowNextImage ();
		}

		void ShowNextImage ()
		{
			currentImage++;
			if (currentImage >= ReferenceImageManager.ImageFailures.Count) {
				validButton.Sensitive = false;
				failButton.Sensitive = false;
				return;
			}
			var info = ReferenceImageManager.ImageFailures [currentImage];
			img1.Image = info.ReferenceImage;
			img2.Image = info.TestImage;
		}
	}
}

