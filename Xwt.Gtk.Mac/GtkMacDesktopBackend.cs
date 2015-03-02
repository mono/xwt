//
// GtkMacDesktopBackend.cs
//
// Author:
//       Lluis Sanchez Gual <lluis@xamarin.com>
//
// Copyright (c) 2014 Xamarin, Inc (http://www.xamarin.com)
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
using Xwt.GtkBackend;
using AppKit;
using System.Drawing;
using System.IO;
using GTK = global::Gtk;
using CoreGraphics;

namespace Xwt.Gtk.Mac
{
	public class GtkMacDesktopBackend: GtkDesktopBackend
	{
		//this is a BCD value of the form "xxyz", where x = major, y = minor, z = bugfix
		//eg. 0x1071 = 10.7.1
		int systemVersion;

		public GtkMacDesktopBackend ()
		{
			systemVersion = Carbon.Gestalt ("sysv");
		}

		public static Gdk.Pixbuf GetPixbufFromNSImage (NSImage icon, int width, int height)
		{
			var rect = new CGRect (0, 0, width, height);

			var rep = icon.BestRepresentation (rect, null, null);
			var bitmap = rep as NSBitmapImageRep;
			try {
				if (bitmap == null) {
					if (rep != null)
						rep.Dispose ();
					using (var cgi = icon.AsCGImage (ref rect, null, null)) {
						if (cgi == null)
							return null;
						bitmap = new NSBitmapImageRep (cgi);
					}
				}
				return GetPixbufFromNSBitmapImageRep (bitmap, width, height);
			} finally {
				if (bitmap != null)
					bitmap.Dispose ();
			}
		}

		static Gdk.Pixbuf GetPixbufFromNSBitmapImageRep (NSBitmapImageRep bitmap, int width, int height)
		{
			byte[] data;
			using (var tiff = bitmap.TiffRepresentation) {
				data = new byte[tiff.Length];
				System.Runtime.InteropServices.Marshal.Copy (tiff.Bytes, data, 0, data.Length);
			}

			int pw = (int)bitmap.PixelsWide, ph = (int)bitmap.PixelsHigh;
			var pixbuf = new Gdk.Pixbuf (data, pw, ph);

			// if one dimension matches, and the other is same or smaller, use as-is
			if ((pw == width && ph <= height) || (ph == height && pw <= width))
				return pixbuf;

			// otherwise scale proportionally such that the largest dimension matches the desired size
			if (pw == ph) {
				pw = width;
				ph = height;
			} else if (pw > ph) {
				ph = (int) (width * ((float) ph / pw));
				pw = width;
			} else {
				pw = (int) (height * ((float) pw / ph));
				ph = height;
			}

			var scaled = pixbuf.ScaleSimple (pw, ph, Gdk.InterpType.Bilinear);
			pixbuf.Dispose ();

			return scaled;
		}

		public override object GetFileIcon (string filename)
		{
			//this only works on MacOS 10.6.0 and greater
			if (systemVersion < 0x1060)
				return base.GetFileIcon (filename);

			NSImage icon = null;

			if (Path.IsPathRooted (filename) && File.Exists (filename)) {
				icon = NSWorkspace.SharedWorkspace.IconForFile (filename);
			} else {
				string extension = Path.GetExtension (filename);
				if (!string.IsNullOrEmpty (extension))
					icon = NSWorkspace.SharedWorkspace.IconForFileType (extension);
			}

			if (icon == null) {
				return base.GetFileIcon (filename);
			}

			int w, h;
			if (!GTK.Icon.SizeLookup (GTK.IconSize.Menu, out w, out h)) {
				w = h = 22;
			}

			var res = GetPixbufFromNSImage (icon, w, h);
			return res != null ? new GtkImage (res) : base.GetFileIcon (filename);
		}
	}
}

