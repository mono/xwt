//
// NinePatchImage.cs
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
using System.Collections.Generic;
using System.Linq;

namespace Xwt.Drawing
{
	/// <summary>
	/// A NinePatchImage is a stretchable bitmap image, 
	/// which will automatically resize to accommodate the contents 
	/// of the BitmapImage placed within.
	/// For more information see: http://developer.android.com/guide/topics/graphics/2d-graphics.html#nine-patch
	/// </summary>
	public class NinePatchImage: DrawingImage
	{
		List<ImageFrame> frames = new List<ImageFrame> ();

		class ImageFrame {
			public BitmapImage Bitmap;
			public double ScaleFactor;
			public List<ImageSection> HorizontalSections;
			public List<ImageSection> VerticalSections;
			public double StretchableWidth;
			public double StretchableHeight;
			public BitmapImage[] TileCache;
		}

		class ImageSection {
			public int Start;
			public double Size;
			public RenderMode Mode;
		}

		enum RenderMode {
			Fixed,
			Stretch,
			Tile
		}

		public NinePatchImage ()
		{
		}

		public NinePatchImage (BitmapImage bitmap)
		{
			AddFrame (bitmap, 1);
		}

		internal void AddFrame (BitmapImage bitmap, double scaleFactor)
		{
			ImageFrame frame = new ImageFrame {
				Bitmap = bitmap,
				ScaleFactor = scaleFactor
			};
			frames.Add (frame);

			frame.HorizontalSections = CreateSections (frame, Enumerable.Range (1, (int)bitmap.Width - 2).Select (n => bitmap.GetPixel (n, 0)));
			frame.VerticalSections = CreateSections (frame, Enumerable.Range (1, (int)bitmap.Height - 2).Select (n => bitmap.GetPixel (0, n)));

			double padLeft = 0, padTop = 0, padRight = 0, padBottom = 0;
			var hbox = CreateSections (frame, Enumerable.Range (1, (int)bitmap.Width - 1).Select (n => bitmap.GetPixel (n, (int)bitmap.Height - 1)));
			var sec = hbox.FirstOrDefault (s => s.Mode != RenderMode.Fixed);
			if (sec != null) {
				padLeft = sec.Start;
				padRight = bitmap.Width - 2 - padLeft - sec.Size;
			}

			var vbox = CreateSections (frame, Enumerable.Range (1, (int)bitmap.Height - 1).Select (n => bitmap.GetPixel ((int)bitmap.Width - 1, n)));
			sec = vbox.FirstOrDefault (s => s.Mode != RenderMode.Fixed);
			if (sec != null) {
				padTop = sec.Start;
				padBottom = bitmap.Height - 2 - padTop - sec.Size;
			}

			Padding = new WidgetSpacing (padLeft / frame.ScaleFactor, padTop / frame.ScaleFactor, padRight / frame.ScaleFactor, padBottom / frame.ScaleFactor);

			frame.StretchableWidth = frame.HorizontalSections.Where (s => s.Mode != RenderMode.Fixed).Sum (s => s.Size);
			frame.StretchableHeight = frame.VerticalSections.Where (s => s.Mode != RenderMode.Fixed).Sum (s => s.Size);
		}

		List<ImageSection> CreateSections (ImageFrame frame, IEnumerable<Color> pixels)
		{
			List<ImageSection> sections = new List<ImageSection> ();

			ImageSection section = null;
			int n = 0;

			foreach (var p in pixels) {
				RenderMode mode;
				// Don't compare exact colors. This is a workaround to make sure regions are properly detected when
				// there are small variations in colors.
				if (p.Red > 0.9 && p.Blue < 0.2 && p.Green < 0.2 && p.Alpha == 1) // Red-ish
					mode = RenderMode.Tile;
				else if (p.Red < 0.2 && p.Blue < 0.2 && p.Green < 0.2 && p.Alpha == 1) // Black-ish
					mode = RenderMode.Stretch;
				else
					mode = RenderMode.Fixed;

				if (section == null || mode != section.Mode) {
					section = new ImageSection {
						Start = n,
						Size = 1,
						Mode = mode
					};
					sections.Add (section);
				} else
					section.Size++;
				n++;
			}
			return sections;
		}

		ImageFrame GetFrame (double scaleFactor)
		{
			return frames.FirstOrDefault (i => Math.Abs (i.ScaleFactor - scaleFactor) < 0.1) ?? frames[0];
		}

		protected sealed override void OnDraw (Context ctx, Rectangle bounds)
		{
			if (bounds.Width <= 0 || bounds.Height <= 0)
				return;

			var frame = GetFrame (ctx.ScaleFactor);
			var fixedWidth = frame.Bitmap.Width - 2 - frame.StretchableWidth;
			var fixedHeight = frame.Bitmap.Height - 2 - frame.StretchableHeight;
			double totalVariableWidth = bounds.Width - fixedWidth / frame.ScaleFactor;
			double totalVariableHeight = bounds.Height - fixedHeight / frame.ScaleFactor;
			double remainingVariableHeight = totalVariableHeight;

			double y = bounds.Y, yb = 1;
			int tileIndex = 0;

			ctx.Save ();
			if (totalVariableWidth < 0) {
				if (fixedWidth > 0)
					ctx.Scale (bounds.Width / fixedWidth, 1);
				totalVariableWidth = 0;
			}
			if (totalVariableHeight < 0) {
				if (fixedHeight > 0)
					ctx.Scale (1, bounds.Height / fixedHeight);
				totalVariableHeight = 0;
			}

			foreach (var vs in frame.VerticalSections) {

				double sh = CalcSectionSize (frame, vs, totalVariableHeight, frame.StretchableHeight, ref remainingVariableHeight);

				double x = bounds.X, xb = 1;
				double remainingVariableWidth = totalVariableWidth;

				foreach (var hs in frame.HorizontalSections) {
					var sourceRegion = new Rectangle (xb, yb, hs.Size, vs.Size);
					double sw = CalcSectionSize (frame, hs, totalVariableWidth, frame.StretchableWidth, ref remainingVariableWidth);
					var targetRegion = new Rectangle (x, y, sw, sh);

					if (vs.Mode != RenderMode.Tile && hs.Mode != RenderMode.Tile) {
						var t = GetTile (frame, tileIndex, sourceRegion);
						ctx.DrawImage (t, targetRegion);
					} else {
						double pw = hs.Size / frame.ScaleFactor;
						double ph = vs.Size / frame.ScaleFactor;
						if (hs.Mode == RenderMode.Stretch) {
							pw = targetRegion.Width;
						}
						if (vs.Mode == RenderMode.Stretch) {
							ph = targetRegion.Height;
						}

						if (pw <= 0 || ph <= 0)
							continue;

						ctx.Save ();
						ctx.Translate (targetRegion.Location);
						targetRegion.Location = Point.Zero;
						ctx.Pattern = new ImagePattern (GetTile (frame, tileIndex, sourceRegion).WithSize (pw, ph));
						ctx.NewPath ();
						ctx.Rectangle (targetRegion);
						ctx.Fill ();
						ctx.Restore ();
					}
					x += sw;
					xb += hs.Size;
					tileIndex++;
				}
				yb += vs.Size;
				y += sh;
			}
			ctx.Restore ();
		}

		double CalcSectionSize (ImageFrame frame, ImageSection sec, double totalVariable, double stretchableSize, ref double remainingVariable)
		{
			if (sec.Mode != RenderMode.Fixed) {
				double sw = Math.Round (totalVariable * (sec.Size / stretchableSize));
				if (sw > remainingVariable)
					sw = remainingVariable;
				remainingVariable -= sw;
				return sw;
			}
			else {
				return sec.Size / frame.ScaleFactor;
			}
		}

		BitmapImage GetTile (ImageFrame frame, int tileIndex, Rectangle sourceRegion)
		{
			if (frame.TileCache == null)
				frame.TileCache = new BitmapImage [frame.HorizontalSections.Count * frame.VerticalSections.Count];

			var img = frame.TileCache [tileIndex];
			if (img != null)
				return img;

			img = frame.Bitmap.Crop (sourceRegion);
			return frame.TileCache [tileIndex] = img;
		}

		protected sealed override Size GetDefaultSize ()
		{
			var frame = GetFrame (1);
			if (frame == null)
				frame = frames [0];
			return new Size ((frame.Bitmap.Width - 2) / frame.ScaleFactor, (frame.Bitmap.Height - 2) / frame.ScaleFactor);
		}

		public WidgetSpacing Padding { get; private set; }
	}
}

