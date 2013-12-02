//
// NinePatch.cs
//
// Author:
//       Lluis Sanchez Gual <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin, Inc (http://www.xamarin.com)
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

using NUnit.Framework;
using Xwt.Drawing;

namespace Xwt
{
	public class NinePatchTests: DrawingTestsBase
	{
		[Test]
		public void NinePatchStretchStretchDefaultSize ()
		{
			var np = Image.FromResource ("ninep-ss.9.png");
			InitBlank ((int)np.Width, (int)np.Height);
			context.DrawImage (np, 0, 0);
			CheckImage ("NinePatchStretchStretchDefaultSize.png");
		}

		[Test]
		public void NinePatchStretchStretchWiderHigher ()
		{
			var np = Image.FromResource ("ninep-ss.9.png");
			np = np.WithSize (np.Width * 3, np.Height * 3);
			InitBlank ((int)np.Width, (int)np.Height);
			context.DrawImage (np, 0, 0);
			CheckImage ("NinePatchStretchStretchWiderHigher.png");
		}

		[Test]
		public void NinePatchStretchStretchWider ()
		{
			var np = Image.FromResource ("ninep-ss.9.png");
			np = np.WithSize (np.Width * 3, np.Height);
			InitBlank ((int)np.Width, (int)np.Height);
			context.DrawImage (np, 0, 0);
			CheckImage ("NinePatchStretchStretchWider.png");
		}

		[Test]
		public void NinePatchStretchStretchHigher ()
		{
			var np = Image.FromResource ("ninep-ss.9.png");
			np = np.WithSize (np.Width, np.Height * 3);
			InitBlank ((int)np.Width, (int)np.Height);
			context.DrawImage (np, 0, 0);
			CheckImage ("NinePatchStretchStretchHigher.png");
		}


		[Test]
		public void NinePatchStretchTileDefaultSize ()
		{
			var np = Image.FromResource ("ninep-st.9.png");
			InitBlank ((int)np.Width, (int)np.Height);
			context.DrawImage (np, 0, 0);
			CheckImage ("NinePatchStretchTileDefaultSize.png");
		}

		[Test]
		public void NinePatchStretchTileWiderHigher ()
		{
			var np = Image.FromResource ("ninep-st.9.png");
			np = np.WithSize (np.Width * 3, np.Height * 3);
			InitBlank ((int)np.Width, (int)np.Height);
			context.DrawImage (np, 0, 0);
			CheckImage ("NinePatchStretchTileWiderHigher.png");
		}

		[Test]
		public void NinePatchStretchTileWider ()
		{
			var np = Image.FromResource ("ninep-st.9.png");
			np = np.WithSize (np.Width * 3, np.Height);
			InitBlank ((int)np.Width, (int)np.Height);
			context.DrawImage (np, 0, 0);
			CheckImage ("NinePatchStretchTileWider.png");
		}

		[Test]
		public void NinePatchStretchTileHigher ()
		{
			var np = Image.FromResource ("ninep-st.9.png");
			np = np.WithSize (np.Width, np.Height * 3);
			InitBlank ((int)np.Width, (int)np.Height);
			context.DrawImage (np, 0, 0);
			CheckImage ("NinePatchStretchTileHigher.png");
		}


		[Test]
		public void NinePatchTileStretchDefaultSize ()
		{
			var np = Image.FromResource ("ninep-ts.9.png");
			InitBlank ((int)np.Width, (int)np.Height);
			context.DrawImage (np, 0, 0);
			CheckImage ("NinePatchTileStretchDefaultSize.png");
		}

		[Test]
		public void NinePatchTileStretchWiderHigher ()
		{
			var np = Image.FromResource ("ninep-ts.9.png");
			np = np.WithSize (np.Width * 3, np.Height * 3);
			InitBlank ((int)np.Width, (int)np.Height);
			context.DrawImage (np, 0, 0);
			CheckImage ("NinePatchTileStretchWiderHigher.png");
		}

		[Test]
		public void NinePatchTileStretchWider ()
		{
			var np = Image.FromResource ("ninep-ts.9.png");
			np = np.WithSize (np.Width * 3, np.Height);
			InitBlank ((int)np.Width, (int)np.Height);
			context.DrawImage (np, 0, 0);
			CheckImage ("NinePatchTileStretchWider.png");
		}

		[Test]
		public void NinePatchTileStretchHigher ()
		{
			var np = Image.FromResource ("ninep-ts.9.png");
			np = np.WithSize (np.Width, np.Height * 3);
			InitBlank ((int)np.Width, (int)np.Height);
			context.DrawImage (np, 0, 0);
			CheckImage ("NinePatchTileStretchHigher.png");
		}	


		[Test]
		public void NinePatchTileTileDefaultSize ()
		{
			var np = Image.FromResource ("ninep-tt.9.png");
			InitBlank ((int)np.Width, (int)np.Height);
			context.DrawImage (np, 0, 0);
			CheckImage ("NinePatchTileTileDefaultSize.png");
		}

		[Test]
		public void NinePatchTileTileWiderHigher ()
		{
			var np = Image.FromResource ("ninep-tt.9.png");
			np = np.WithSize (np.Width * 3, np.Height * 3);
			InitBlank ((int)np.Width, (int)np.Height);
			context.DrawImage (np, 0, 0);
			CheckImage ("NinePatchTileTileWiderHigher.png");
		}

		[Test]
		public void NinePatchTileTileWider ()
		{
			var np = Image.FromResource ("ninep-tt.9.png");
			np = np.WithSize (np.Width * 3, np.Height);
			InitBlank ((int)np.Width, (int)np.Height);
			context.DrawImage (np, 0, 0);
			CheckImage ("NinePatchTileTileWider.png");
		}

		[Test]
		public void NinePatchTileTileHigher ()
		{
			var np = Image.FromResource ("ninep-tt.9.png");
			np = np.WithSize (np.Width, np.Height * 3);
			InitBlank ((int)np.Width, (int)np.Height);
			context.DrawImage (np, 0, 0);
			CheckImage ("NinePatchTileTileHigher.png");
		}

		// 2x scale factor

		[Test]
		public void NinePatchStretchStretchDefaultSize2x ()
		{
			var np = Image.FromResource ("ninep-ss.9.png");
			InitBlank ((int)np.Width, (int)np.Height);
			context.DrawImage (np, 0, 0);
			CheckImage ("NinePatchStretchStretchDefaultSize2x.png", 2);
		}

		[Test]
		public void NinePatchStretchStretchWiderHigher2x ()
		{
			var np = Image.FromResource ("ninep-ss.9.png");
			np = np.WithSize (np.Width * 3, np.Height * 3);
			InitBlank ((int)np.Width, (int)np.Height);
			context.DrawImage (np, 0, 0);
			CheckImage ("NinePatchStretchStretchWiderHigher2x.png", 2);
		}

		[Test]
		public void NinePatchStretchStretchWider2x ()
		{
			var np = Image.FromResource ("ninep-ss.9.png");
			np = np.WithSize (np.Width * 3, np.Height);
			InitBlank ((int)np.Width, (int)np.Height);
			context.DrawImage (np, 0, 0);
			CheckImage ("NinePatchStretchStretchWider2x.png", 2);
		}

		[Test]
		public void NinePatchStretchStretchHigher2x ()
		{
			var np = Image.FromResource ("ninep-ss.9.png");
			np = np.WithSize (np.Width, np.Height * 3);
			InitBlank ((int)np.Width, (int)np.Height);
			context.DrawImage (np, 0, 0);
			CheckImage ("NinePatchStretchStretchHigher2x.png", 2);
		}


		[Test]
		public void NinePatchStretchTileDefaultSize2x ()
		{
			var np = Image.FromResource ("ninep-st.9.png");
			InitBlank ((int)np.Width, (int)np.Height);
			context.DrawImage (np, 0, 0);
			CheckImage ("NinePatchStretchTileDefaultSize2x.png", 2);
		}

		[Test]
		public void NinePatchStretchTileWiderHigher2x ()
		{
			var np = Image.FromResource ("ninep-st.9.png");
			np = np.WithSize (np.Width * 3, np.Height * 3);
			InitBlank ((int)np.Width, (int)np.Height);
			context.DrawImage (np, 0, 0);
			CheckImage ("NinePatchStretchTileWiderHigher2x.png", 2);
		}

		[Test]
		public void NinePatchStretchTileWider2x ()
		{
			var np = Image.FromResource ("ninep-st.9.png");
			np = np.WithSize (np.Width * 3, np.Height);
			InitBlank ((int)np.Width, (int)np.Height);
			context.DrawImage (np, 0, 0);
			CheckImage ("NinePatchStretchTileWider2x.png", 2);
		}

		[Test]
		public void NinePatchStretchTileHigher2x ()
		{
			var np = Image.FromResource ("ninep-st.9.png");
			np = np.WithSize (np.Width, np.Height * 3);
			InitBlank ((int)np.Width, (int)np.Height);
			context.DrawImage (np, 0, 0);
			CheckImage ("NinePatchStretchTileHigher2x.png", 2);
		}


		[Test]
		public void NinePatchTileStretchDefaultSize2x ()
		{
			var np = Image.FromResource ("ninep-ts.9.png");
			InitBlank ((int)np.Width, (int)np.Height);
			context.DrawImage (np, 0, 0);
			CheckImage ("NinePatchTileStretchDefaultSize2x.png", 2);
		}

		[Test]
		public void NinePatchTileStretchWiderHigher2x ()
		{
			var np = Image.FromResource ("ninep-ts.9.png");
			np = np.WithSize (np.Width * 3, np.Height * 3);
			InitBlank ((int)np.Width, (int)np.Height);
			context.DrawImage (np, 0, 0);
			CheckImage ("NinePatchTileStretchWiderHigher2x.png", 2);
		}

		[Test]
		public void NinePatchTileStretchWider2x ()
		{
			var np = Image.FromResource ("ninep-ts.9.png");
			np = np.WithSize (np.Width * 3, np.Height);
			InitBlank ((int)np.Width, (int)np.Height);
			context.DrawImage (np, 0, 0);
			CheckImage ("NinePatchTileStretchWider2x.png", 2);
		}

		[Test]
		public void NinePatchTileStretchHigher2x ()
		{
			var np = Image.FromResource ("ninep-ts.9.png");
			np = np.WithSize (np.Width, np.Height * 3);
			InitBlank ((int)np.Width, (int)np.Height);
			context.DrawImage (np, 0, 0);
			CheckImage ("NinePatchTileStretchHigher2x.png", 2);
		}	


		[Test]
		public void NinePatchTileTileDefaultSize2x ()
		{
			var np = Image.FromResource ("ninep-tt.9.png");
			InitBlank ((int)np.Width, (int)np.Height);
			context.DrawImage (np, 0, 0);
			CheckImage ("NinePatchTileTileDefaultSize2x.png", 2);
		}

		[Test]
		public void NinePatchTileTileWiderHigher2x ()
		{
			var np = Image.FromResource ("ninep-tt.9.png");
			np = np.WithSize (np.Width * 3, np.Height * 3);
			InitBlank ((int)np.Width, (int)np.Height);
			context.DrawImage (np, 0, 0);
			CheckImage ("NinePatchTileTileWiderHigher2x.png", 2);
		}

		[Test]
		public void NinePatchTileTileWider2x ()
		{
			var np = Image.FromResource ("ninep-tt.9.png");
			np = np.WithSize (np.Width * 3, np.Height);
			InitBlank ((int)np.Width, (int)np.Height);
			context.DrawImage (np, 0, 0);
			CheckImage ("NinePatchTileTileWider2x.png", 2);
		}

		[Test]
		public void NinePatchTileTileHigher2x ()
		{
			var np = Image.FromResource ("ninep-tt.9.png");
			np = np.WithSize (np.Width, np.Height * 3);
			InitBlank ((int)np.Width, (int)np.Height);
			context.DrawImage (np, 0, 0);
			CheckImage ("NinePatchTileTileHigher2x.png", 2);
		}
	}
}

