//
// ThemedImage.cs
//
// Author:
//       Lluis Sanchez Gual <lluis@xamarin.com>
//
// Copyright (c) 2015 Xamarin, Inc (http://www.xamarin.com)
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
using System.Linq;
using System.Collections.Generic;

namespace Xwt.Drawing
{
	public class ThemedImage: DrawingImage
	{
		readonly List<Tuple<Image, string []>> images;

		int globalStylesVersion = -1;

		public ThemedImage (List<Tuple<Image, string []>> images, Size size = default (Size))
		{
			this.images = images;
			Size = size;
		}

		public IEnumerable<Tuple<Image, string []>> Images {
			get {
				return images;
			}
		}

		protected override void OnDraw (Context ctx, Rectangle bounds)
		{
			var best = GetImage (ctx.Styles);
			if (best != null)
				ctx.DrawImage (best, bounds);
		}

		public Image GetImage (IEnumerable<string> tags)
		{
			// sort the images only if global styles changed
			if (globalStylesVersion != Context.GlobalStylesVersion) {
				images.Sort (StylePriorityComparison);
				globalStylesVersion = Context.GlobalStylesVersion;
			}
			Image best = null;
			int bestMatches = -1;
			int bestNoMatches = int.MaxValue;

			foreach (var img in images) {
				int matches = 0;
				foreach (var t in tags) {
					if (img.Item2.Contains (t))
						matches++;
				}
				var noMatches = img.Item2.Length - matches;
				if (matches > bestMatches || (matches == bestMatches && noMatches < bestNoMatches)) {
					best = img.Item1;
					bestMatches = matches;
					bestNoMatches = noMatches;
				}
			}
			return best;
		}

		/// <summary>
		/// Sort images by global style
		/// </summary>
		/// <remarks>
		/// <see cref="GetImage(IEnumerable{string})"/> is optimized for performace and therefore
		/// just loops over the images and selects the first best matching one based on given styles.
		/// The result is not predictable if the image list is not sorted and several alternatives
		/// match different global and explicit styles.
		/// Since we want to prioritize explicit styles first (<see cref="Image.WithStyles(string[])"/>),
		/// we need to make sure that we don't select an image with a given global style
		/// (<see cref="Context.SetGlobalStyle(string)"/>) but without an explicit match.
		/// The simplest approach without slowing down rendering by a sophisticated lookup is to
		/// sort images matching global styles last, so that <see cref="GetImage(IEnumerable{string})"/>
		/// would always select an alternative with an explicit rather than a global style.
		/// </remarks>
		static int StylePriorityComparison (Tuple<Image, string []> img1, Tuple<Image, string []> img2)
		{
			var hasGlobal1 = img1.Item2.Any (style => Context.HasGlobalStyle (style));
			var hasGlobal2 = img2.Item2.Any (style => Context.HasGlobalStyle (style));
			if (hasGlobal1 && hasGlobal2)
				return 0;
			if (hasGlobal1)
				return 1; // sort global styles last
			return -1; // explicit styles go first
		}
	}
}

