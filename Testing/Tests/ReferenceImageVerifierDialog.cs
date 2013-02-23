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

namespace Xwt
{
	public class ReferenceImageVerifierDialog: Dialog
	{
		int currentImage = -1;
		ImageView img1;
		ImageView img2;
		Label nameLabel;

		Button closeButton;
		Button validButton;
		Button failButton;

		public ReferenceImageVerifierDialog ()
		{
			Width = 500;
			Height = 300;

			Table table = new Table ();
			table.DefaultRowSpacing = table.DefaultColumnSpacing = 6;

			table.Attach (nameLabel = new Label (), 0, 0, AttachOptions.Expand | AttachOptions.Fill, AttachOptions.Shrink);
			table.Attach (new Label ("Reference Image"), 0, 1, AttachOptions.Expand | AttachOptions.Fill, AttachOptions.Shrink);
			table.Attach (new Label ("Test Image"), 1, 1, AttachOptions.Expand | AttachOptions.Fill, AttachOptions.Shrink);
			nameLabel.Font = nameLabel.Font.WithWeight (Xwt.Drawing.FontWeight.Bold);

			img1 = new ImageView ();
			table.Attach (img1, 0, 2, AttachOptions.Expand | AttachOptions.Fill, AttachOptions.Expand | AttachOptions.Fill);
			
			img2 = new ImageView ();
			table.Attach (img2, 1, 2, AttachOptions.Expand | AttachOptions.Fill, AttachOptions.Expand | AttachOptions.Fill);

			var buttonBox = new HBox ();
			table.Attach (buttonBox, 0, 3, 2, 4, AttachOptions.Expand | AttachOptions.Fill, AttachOptions.Shrink);

			closeButton = new Button ("Close");
			validButton = new Button ("Success");
			failButton = new Button ("Failure");

			buttonBox.PackEnd (closeButton);
			buttonBox.PackEnd (failButton);
			buttonBox.PackEnd (validButton);

			closeButton.Clicked += delegate {
				Respond (Command.Ok);
			};

			failButton.Clicked += delegate {
				var info = ReferenceImageManager.ImageFailures[currentImage];
				info.Fail ();
				ShowNextImage ();
			};
			
			validButton.Clicked += delegate {
				var info = ReferenceImageManager.ImageFailures[currentImage];
				info.Validate ();
				ShowNextImage ();
			};

			Content = table;
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
			nameLabel.Text = info.Name;
			img1.Image = info.ReferenceImage;
			img2.Image = info.TestImage;
		}
	}
}

