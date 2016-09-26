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
		List<Tuple<Image, string []>> images;

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
	}
}

