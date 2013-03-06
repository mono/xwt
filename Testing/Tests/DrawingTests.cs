//
// DrawingTests.cs
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
using NUnit.Framework;
using System.Threading;
using Xwt.Drawing;
using System.IO;
using System.Collections.Generic;

namespace Xwt
{
	[TestFixture]
	public class DrawingTests
	{
		ImageBuilder builder;
		Context context;

		void InitBlank (int width = 50, int height = 50)
		{
			if (builder != null)
				builder.Dispose ();

			builder = new ImageBuilder (width, height);
			context = builder.Context;
			context.Rectangle (0, 0, width, height);
			context.SetColor (Colors.White);
			context.Fill ();
			context.SetColor (Colors.Black);
			context.SetLineWidth (1);
		}

		void CheckImage (string refImageName)
		{
			if (builder == null)
				return;
			var img = builder.ToImage ();
			builder.Dispose ();
			builder = null;

			ReferenceImageManager.CheckImage (refImageName, img);
		}
		
		[Test]
		public void Line ()
		{
			InitBlank ();
			context.MoveTo (1, 1.5);
			context.LineTo (20, 1.5);
			context.Stroke ();
			CheckImage ("Line.png");
		}

		[Test]
		public void LineWidth ()
		{
			InitBlank ();
			context.MoveTo (10, 20.5);
			context.LineTo (40, 20.5);
			context.SetLineWidth (1);
			context.Stroke ();
			context.MoveTo (10, 25);
			context.LineTo (40, 25);
			context.SetLineWidth (2);
			context.Stroke ();
			context.MoveTo (10, 30);
			context.LineTo (40, 30);
			context.SetLineWidth (4);
			context.Stroke ();
			CheckImage ("LineWidth.png");
		}
		
		[Test]
		public void Rectangle ()
		{
			InitBlank ();
			context.Rectangle (1.5, 1.5, 20, 20);
			context.Stroke ();
			CheckImage ("Rectangle.png");
		}
		
		[Test]
		public void RectangleFill ()
		{
			InitBlank ();
			context.Rectangle (1, 1, 20, 20);
			context.SetColor (Colors.Blue);
			context.Fill ();
			CheckImage ("RectangleFill.png");
		}
		
		[Test]
		public void FillPreserve ()
		{
			InitBlank ();
			context.Rectangle (1, 1, 20, 20);
			context.SetColor (Colors.Yellow);
			context.FillPreserve ();
			context.SetColor (Colors.Blue);
			context.SetLineWidth (2);
			context.Stroke ();
			CheckImage ("FillPreserve.png");
		}
		
		[Test]
		public void StrokePreserve ()
		{
			InitBlank ();
			context.MoveTo (10, 25);
			context.LineTo (40, 25);
			context.SetColor (Colors.Blue);
			context.SetLineWidth (10);
			context.StrokePreserve ();
			context.SetLineWidth (5);
			context.SetColor (Colors.Yellow);
			context.Stroke ();
			CheckImage ("StrokePreserve.png");
		}

		[Test]
		public void Arc ()
		{
			InitBlank ();
			context.Arc (25, 25, 20, 0, 90);
			context.SetColor (Colors.Black);
			context.Stroke ();
			CheckImage ("Arc.png");
		}
		
		[Test]
		public void ArcStartingNegative ()
		{
			InitBlank ();
			context.Arc (25, 25, 20, -45, 45);
			context.SetColor (Colors.Black);
			context.Stroke ();
			CheckImage ("ArcStartingNegative.png");
		}
		
		[Test]
		public void ArcSwappedAngles ()
		{
			InitBlank ();
			context.Arc (25, 25, 20, 300, 0);
			context.SetColor (Colors.Black);
			context.Stroke ();
			CheckImage ("ArcSwappedAngles.png");
		}
		
		[Test]
		public void ArcMultipleLoops ()
		{
			InitBlank ();
			context.Arc (25, 25, 20, 0, 360 + 180);
			context.SetColor (Colors.Black.WithAlpha (0.5));
			context.SetLineWidth (5);
			context.Stroke ();
			CheckImage ("ArcMultipleLoops.png");
		}

		[Test]
		public void ArcFill ()
		{
			InitBlank ();
			context.Arc (25, 25, 20, 0, 135);
			context.SetColor (Colors.Black);
			context.Fill ();
			CheckImage ("ArcFill.png");
		}
		
		[Test]
		public void ArcPathConnection ()
		{
			InitBlank ();
			context.MoveTo (25.5, 25.5);
			context.Arc (25.5, 25.5, 20, 0, 135);
			context.LineTo (25.5, 25.5);
			context.SetColor (Colors.Black);
			context.Stroke ();
			CheckImage ("ArcPathConnection.png");
		}

		[Test]
		public void ArcNegative ()
		{
			InitBlank ();
			context.ArcNegative (25, 25, 20, 0, 90);
			context.SetColor (Colors.Black);
			context.Stroke ();
			CheckImage ("ArcNegative.png");
		}
		
		[Test]
		public void ArcNegativeStartingNegative ()
		{
			InitBlank ();
			context.ArcNegative (25, 25, 20, -45, 45);
			context.SetColor (Colors.Black);
			context.Stroke ();
			CheckImage ("ArcNegativeStartingNegative.png");
		}
		
		[Test]
		public void ArcNegativeSwappedAngles ()
		{
			InitBlank ();
			context.ArcNegative (25, 25, 20, 300, 0);
			context.SetColor (Colors.Black);
			context.Stroke ();
			CheckImage ("ArcNegativeSwappedAngles.png");
		}
		
		[Test]
		public void ArcNegativeMultipleLoops ()
		{
			InitBlank ();
			context.ArcNegative (25, 25, 20, 0, 360 + 180);
			context.SetColor (Colors.Black.WithAlpha (0.5));
			context.SetLineWidth (5);
			context.Stroke ();
			CheckImage ("ArcNegativeMultipleLoops.png");
		}
		
		[Test]
		public void ArcNegativeFill ()
		{
			InitBlank ();
			context.ArcNegative (25, 25, 20, 0, 135);
			context.SetColor (Colors.Black);
			context.Fill ();
			CheckImage ("ArcNegativeFill.png");
		}
		
		[Test]
		public void ArcNegativePathConnection ()
		{
			InitBlank ();
			context.MoveTo (25.5, 25.5);
			context.ArcNegative (25.5, 25.5, 20, 0, 135);
			context.LineTo (25.5, 25.5);
			context.SetColor (Colors.Black);
			context.Stroke ();
			CheckImage ("ArcNegativePathConnection.png");
		}


		[Test]
		public void ImagePattern ()
		{
			InitBlank (70, 70);
			context.Rectangle (5, 5, 40, 60);
			var img = Image.FromResource (GetType(), "pattern-sample.png");
			context.Pattern = new Xwt.Drawing.ImagePattern (img);
			context.Fill ();
			CheckImage ("ImagePattern.png");
		}
		
		[Test]
		public void ImagePatternWithTranslateTransform ()
		{
			InitBlank (70, 70);
			context.Translate (5, 5);
			context.Rectangle (0, 0, 40, 60);
			var img = Image.FromResource (GetType(), "pattern-sample.png");
			context.Pattern = new Xwt.Drawing.ImagePattern (img);
			context.Fill ();
			CheckImage ("ImagePatternWithTranslateTransform.png");
		}
		
		void DrawSimplePattern ()
		{
			context.Rectangle (0, 0, 20, 20);
			context.SetColor (Colors.Red);
			context.Fill ();
			context.Rectangle (20, 0, 20, 20);
			context.SetColor (Colors.Blue);
			context.Fill ();
			context.Rectangle (0, 20, 20, 20);
			context.SetColor (Colors.Green);
			context.Fill ();
			context.Rectangle (20, 20, 20, 20);
			context.SetColor (Colors.Yellow);
			context.Fill ();
		}

		[Test]
		public void Clip ()
		{
			InitBlank ();

			context.Rectangle (15, 15, 20, 20);
			context.Clip ();
			DrawSimplePattern ();

			CheckImage ("Clip.png");
		}
		
		[Test]
		public void ClipAccumulated ()
		{
			InitBlank ();
			
			context.Rectangle (15, 15, 20, 20);
			context.Clip ();
			context.Rectangle (0, 0, 20, 20);
			context.Clip ();
			DrawSimplePattern ();
			
			CheckImage ("ClipAccumulated.png");
		}
		
		[Test]
		public void ClipPreserve ()
		{
			InitBlank ();
			
			DrawSimplePattern ();
			context.Rectangle (15, 15, 20, 20);
			context.ClipPreserve ();
			context.SetColor (Colors.Violet);
			context.Fill ();

			CheckImage ("ClipPreserve.png");
		}
		
		[Test]
		public void ClipSaveRestore ()
		{
			InitBlank ();
			context.Rectangle (15, 15, 20, 20);
			context.Clip ();
			context.Save ();
			context.Rectangle (0, 0, 20, 20);
			context.Clip ();
			context.Restore ();
			DrawSimplePattern ();
			CheckImage ("ClipSaveRestore.png");
		}
		
		[Test]
		public void NewPath ()
		{
			InitBlank ();
			context.MoveTo (1, 1.5);
			context.LineTo (20, 1.5);
			context.NewPath ();
			context.MoveTo (0, 0);
			context.LineTo (20, 20);
			context.Stroke ();
			CheckImage ("NewPath.png");
		}
		
		[Test]
		public void LinearGradient ()
		{
			InitBlank ();
			var g = new LinearGradient (5, 5, 5, 45);
			g.AddColorStop (0, Colors.Red);
			g.AddColorStop (0.5, Colors.Green);
			g.AddColorStop (1, Colors.Blue);
			context.Rectangle (5, 5, 40, 40);
			context.Pattern = g;
			context.Fill ();
			CheckImage ("LinearGradient.png");
		}
		
		[Test]
		public void LinearGradientDiagonal ()
		{
			InitBlank ();
			var g = new LinearGradient (5, 5, 45, 45);
			g.AddColorStop (0, Colors.Red);
			g.AddColorStop (0.5, Colors.Green);
			g.AddColorStop (1, Colors.Blue);
			context.Rectangle (5, 5, 40, 40);
			context.Pattern = g;
			context.Fill ();
			CheckImage ("LinearGradientDiagonal.png");
		}
		
		[Test]
		public void LinearGradientReverse ()
		{
			InitBlank ();
			var g = new LinearGradient (5, 45, 5, 5);
			g.AddColorStop (0, Colors.Red);
			g.AddColorStop (0.5, Colors.Green);
			g.AddColorStop (1, Colors.Blue);
			context.Rectangle (5, 5, 40, 40);
			context.Pattern = g;
			context.Fill ();
			CheckImage ("LinearGradientReverse.png");
		}
		
		[Test]
		public void LinearGradientInternalBox ()
		{
			InitBlank ();
			var g = new LinearGradient (25, 15, 35, 35);
			g.AddColorStop (0, Colors.Red);
			g.AddColorStop (0.5, Colors.Green);
			g.AddColorStop (1, Colors.Blue);
			context.Rectangle (5, 5, 40, 40);
			context.Pattern = g;
			context.Fill ();
			CheckImage ("LinearGradientInternalBox.png");
		}
		
		[Test]
		public void RadialGradient ()
		{
			InitBlank ();
			var g = new RadialGradient (20, 20, 5, 30, 30, 30);
			g.AddColorStop (0, Colors.Red);
			g.AddColorStop (0.5, Colors.Green);
			g.AddColorStop (1, Colors.Blue);
			context.Rectangle (5, 5, 40, 40);
			context.Pattern = g;
			context.Fill ();
			CheckImage ("RadialGradient.png");
		}
		
		[Test]
		public void RadialGradientReverse ()
		{
			InitBlank ();
			var g = new RadialGradient (20, 20, 30, 30, 25, 5);
			g.AddColorStop (0, Colors.Red);
			g.AddColorStop (0.5, Colors.Green);
			g.AddColorStop (1, Colors.Blue);
			context.Rectangle (5, 5, 40, 40);
			context.Pattern = g;
			context.Fill ();
			CheckImage ("RadialGradientReverse.png");
		}
		
		[Test]
		public void RadialGradientSmall ()
		{
			InitBlank ();
			var g = new RadialGradient (5, 5, 5, 30, 30, 15);
			g.AddColorStop (0, Colors.Red);
			g.AddColorStop (0.5, Colors.Green);
			g.AddColorStop (1, Colors.Blue);
			context.Rectangle (5, 5, 40, 40);
			context.Pattern = g;
			context.Fill ();
			CheckImage ("RadialGradientSmall.png");
		}
	}
}

