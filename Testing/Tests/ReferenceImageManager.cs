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
		public static string ProjectCustomReferenceImageDir;
		public static bool RecheckAll;

		internal static string FailedImageCacheDir;
		
		public static List<FailedImageInfo> ImageFailures = new List<FailedImageInfo> ();

		static ReferenceImageManager ()
		{
		}

		public static void Init (string projectName)
		{
			var baseDir = Path.GetDirectoryName (typeof(ReferenceImageManager).Assembly.Location);
			while (Path.GetFileName (baseDir) != "Testing")
				baseDir = Path.GetDirectoryName (baseDir);
			ProjectReferenceImageDir = Path.Combine (baseDir, "Tests", "ReferenceImages");
			ProjectCustomReferenceImageDir = Path.Combine (baseDir, projectName, "ReferenceImages");
			FailedImageCacheDir = Path.Combine (baseDir, "bin", projectName, "FailedImageCache");
		}

		public static Image LoadReferenceImage (string name)
		{
			var f = Path.Combine (ProjectReferenceImageDir, name);
			if (File.Exists (f))
				return Image.FromFile (f);
			else
				return null;
		}

		public static Image LoadCustomReferenceImage (string name)
		{
			var file = Path.Combine (ProjectCustomReferenceImageDir, name);
			if (File.Exists (file))
				return Image.FromFile (file);
			else
				return null;
		}

		public static void ShowImageVerifier ()
		{
			if (ImageFailures.Count > 0) {
				var dlg = new ReferenceImageVerifierDialog ();
				dlg.Run ();
			}
		}

		static Image TryLoadImage (System.Reflection.Assembly asm, string name)
		{
			try {
				if (asm.GetManifestResourceInfo (name) != null)
					return Image.FromResource (asm, name);
			}
			catch {
			}

			try {
				if (asm.GetManifestResourceInfo ("WpfTestRunner.ReferenceImages." + name) != null)
					return Image.FromResource (asm, "WpfTestRunner.ReferenceImages." + name);
			}
			catch {
			}
			return null;
		}
		
		public static void CheckImage (string refImageName, Image im)
		{
			BitmapImage img = im as BitmapImage ?? im.ToBitmap ();
			Image coreRefImage = LoadReferenceImage (refImageName);

			Image refImage = !RecheckAll ? LoadCustomReferenceImage (refImageName) : null;
			if (refImage == null)
				refImage = coreRefImage;
			
			if (refImage == null) {
				ImageFailures.Add (new FailedImageInfo () {
					TestImage = img.WithSize (img.PixelWidth, img.PixelHeight),
					ReferenceImage = img.WithSize (img.PixelWidth, img.PixelHeight),
					Name = refImageName,
					TargetDir = ProjectReferenceImageDir
				});
				return;
			}

			var diff = DiffImages (img, refImage);
			if (diff != null && refImage != coreRefImage) {
				// Maybe the original image has changed
				refImage = coreRefImage;
				diff = DiffImages (img, refImage);
			}

			if (diff != null) {
				bool knownFailure = false;
				var failedImageFile = Path.Combine (FailedImageCacheDir, refImageName);
				if (File.Exists (failedImageFile)) {
					var failedImage = Image.FromFile (Path.Combine (FailedImageCacheDir, refImageName));
					if (DiffImages (img, failedImage) == null)
						knownFailure = true;
				}
				
				if (!knownFailure) {
					ImageFailures.Add (new FailedImageInfo () {
						TestImage = img.WithSize (img.PixelWidth, img.PixelHeight),
						ReferenceImage = refImage.WithSize (img.PixelWidth, img.PixelHeight),
						DiffImage = diff,
						Name = refImageName,
						TargetDir = ProjectCustomReferenceImageDir
					});
				}
				Assert.Fail ("Image " + refImageName + " doesn't match");
			}
		}
		
		public static Image DiffImages (Image img1, Image img2)
		{
			bool foundDifference = false;
			var bmp1 = (img1 as BitmapImage) ?? img1.ToBitmap ();
			var bmp2 = (img2 as BitmapImage) ?? img2.ToBitmap ();
			var res = new ImageBuilder ((int)Math.Min (bmp1.PixelWidth, bmp2.PixelWidth), (int) Math.Min (bmp1.PixelHeight, bmp2.PixelHeight));
			var bmpr = res.ToBitmap ();
			res.Dispose ();
			for (int y=0; y<bmp1.PixelHeight && y < bmp2.PixelHeight; y++) {
				for (int x=0; x<bmp1.PixelWidth && x<bmp2.PixelWidth; x++) {
					var p1 = bmp1.GetPixel (x, y);
					var p2 = bmp2.GetPixel (x, y);
					var col = Colors.White;
					if (p1 != p2) {
						foundDifference = true;
						var r = Math.Pow (p1.Red - p2.Red, 2) + Math.Pow (p1.Green - p2.Green, 2) + Math.Pow (p1.Blue - p2.Blue, 2) + Math.Pow (p1.Alpha - p2.Alpha, 2);
						if (r < 0.01)
							col = new Color (0.9, 0.9, 0.9);
						else if (r < 0.1)
							col = new Color (0.7, 0.7, 0.7);
						else
							col = Colors.Red;
					}
					bmpr.SetPixel (x, y, col);
				}
			}
			if (foundDifference)
				return bmpr;
			else
				return null;
		}
	}
	
	public class FailedImageInfo
	{
		public Image TestImage { get; set; }
		public Image ReferenceImage { get; set; }
		public Image DiffImage { get; set; } 
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

