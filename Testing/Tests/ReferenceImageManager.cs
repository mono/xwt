//
// ReferenceImageManager.cs
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
using Xwt.Drawing;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Xwt
{
	public class ReferenceImageManager
	{
		internal static string ProjectReferenceImageDir;
		internal static string ProjectCustomReferenceImageDir;

		internal static string FailedImageCacheDir;
		
		public static List<FailedImageInfo> ImageFailures = new List<FailedImageInfo> ();

		static ReferenceImageManager ()
		{
			var baseDir = Path.GetDirectoryName (System.Reflection.Assembly.GetEntryAssembly ().Location);

			ProjectReferenceImageDir = Path.Combine (baseDir, "..", "..", "..", "Tests", "ReferenceImages");
			ProjectCustomReferenceImageDir = Path.Combine (baseDir, "..", "..", "ReferenceImages");
			FailedImageCacheDir = Path.Combine (baseDir, "FailedImageCache");
		}

		public static void ShowImageVerifier ()
		{
			if (ImageFailures.Count > 0) {
				var dlg = new ReferenceImageVerifierDialog ();
				dlg.Run ();
			}
		}
		
		public static void CheckImage (string refImageName, Image img)
		{
			Image refImage = null;

			try {
				refImage = Image.FromResource (System.Reflection.Assembly.GetEntryAssembly (), refImageName);
			} catch {
			}

			if (refImage == null) {
				try {
					refImage = Image.FromResource (typeof(ReferenceImageManager), refImageName);
				} catch {
				}
			}
			
			if (refImage == null) {
				ImageFailures.Add (new FailedImageInfo () {
					TestImage = img,
					ReferenceImage = img,
					Name = refImageName,
					TargetDir = ProjectReferenceImageDir
				});
				return;
			}
			
			if (!CompareImages (img, refImage)) {
				bool knownFailure = false;
				var failedImageFile = Path.Combine (FailedImageCacheDir, refImageName);
				if (File.Exists (failedImageFile)) {
					var failedImage = Image.FromFile (Path.Combine (FailedImageCacheDir, refImageName));
					if (CompareImages (img, failedImage))
						knownFailure = true;
				}
				
				if (!knownFailure) {
					ImageFailures.Add (new FailedImageInfo () {
						TestImage = img,
						ReferenceImage = refImage,
						Name = refImageName,
						TargetDir = ProjectCustomReferenceImageDir
					});
				}
				Assert.Fail ("Image " + refImageName + " doesn't match");
			}
		}
		
		public static bool CompareImages (Image img1, Image img2)
		{
			var bmp1 = img1.ToBitmap ();
			var bmp2 = img2.ToBitmap ();
			
			if (bmp1.Size != bmp2.Size)
				return false;
			
			for (int y=0; y<bmp1.Size.Height; y++) {
				for (int x=0; x<bmp1.Size.Width; x++)
					if (bmp1.GetPixel (x, y) != bmp2.GetPixel (x, y))
						return false;
			}
			return true;
		}
	}
	
	public class FailedImageInfo
	{
		public Image TestImage { get; set; }
		public Image ReferenceImage { get; set; }
		public string Name { get; set; }
		public string TargetDir { get; set; }

		public void Validate ()
		{
			if (!Directory.Exists (TargetDir))
				Directory.CreateDirectory (TargetDir);
			TestImage.Save (Path.Combine (TargetDir, Name), ImageFileType.Png);
		}

		public void Fail ()
		{
			if (!Directory.Exists (ReferenceImageManager.FailedImageCacheDir))
				Directory.CreateDirectory (ReferenceImageManager.FailedImageCacheDir);
			TestImage.Save (Path.Combine (ReferenceImageManager.FailedImageCacheDir, Name), ImageFileType.Png);
		}
	}
}

