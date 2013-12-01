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
	}
}

