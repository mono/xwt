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
	public class DrawingTests: DrawingTestsBase
	{
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
		public void LineClosePath ()
		{
			InitBlank ();
			context.MoveTo (1.5, 1.5);
			context.LineTo (20.5, 1.5);
			context.LineTo (20.5, 20.5);
			context.ClosePath ();
			context.Stroke ();
			CheckImage ("LineClosePath.png");
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
		public void RectanglePathConnection ()
		{
			InitBlank ();
			context.Rectangle (5.5, 5.5, 20, 20);
			DrawCurrentPosition ();
			context.Stroke ();
			CheckImage ("RectanglePathConnection.png");
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

		void DrawCurrentPosition ()
		{
			context.RelMoveTo (-2.5, -3);
			context.RelLineTo (6, 0);
			context.RelLineTo (0, 6);
			context.RelLineTo (-6, 0);
			context.RelLineTo (0, -6);
			context.RelLineTo (6, 0);
			context.RelLineTo (-6, 0);
			context.RelMoveTo (2.5, 3);
		}

		#region Arc

		[Test]
		public void Arc ()
		{
			InitBlank ();
			context.Arc (25, 25, 20.5, 0, 90);
			DrawCurrentPosition ();
			context.SetColor (Colors.Black);
			context.Stroke ();

			CheckImage ("Arc.png");
		}

		[Test]
		public void ArcStartingNegative ()
		{
			InitBlank ();
			context.Arc (25, 25, 20.5, -45, 45);
			DrawCurrentPosition ();
			context.SetColor (Colors.Black);
			context.Stroke ();
			CheckImage ("ArcStartingNegative.png");
		}
		
		[Test]
		public void ArcSwappedAngles ()
		{
			InitBlank ();
			context.Arc (25, 25, 20.5, 300, 0);
			DrawCurrentPosition ();
			context.SetColor (Colors.Black);
			context.Stroke ();
			CheckImage ("ArcSwappedAngles.png");
		}
		
		[Test]
		public void ArcMultipleLoops ()
		{
			InitBlank ();
			context.Arc (25, 25, 20.5, 0, 360 + 180);
			context.SetColor (Colors.Black.WithAlpha (0.5));
			context.SetLineWidth (5);
			context.RelLineTo (10, 0);
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
			DrawCurrentPosition ();
			context.LineTo (25.5, 25.5);
			context.SetColor (Colors.Black);
			context.Stroke ();
			CheckImage ("ArcPathConnection.png");
		}
		
		[Test]
		public void ArcClosePath ()
		{
			InitBlank ();
			context.Arc (25, 25, 20.5, 0, 90);
			context.Arc (25, 35, 10.5, 90, 180);
			context.ClosePath ();
			context.SetColor (Colors.Black);
			context.Stroke ();
			
			CheckImage ("ArcClosePath.png");
		}

		#endregion

		#region ArcNegative

		[Test]
		public void ArcNegative ()
		{
			InitBlank ();
			context.ArcNegative (25, 25, 20.5, 0, 90);
			DrawCurrentPosition ();
			context.SetColor (Colors.Black);
			context.Stroke ();
			CheckImage ("ArcNegative.png");
		}
		
		[Test]
		public void ArcNegativeStartingNegative ()
		{
			InitBlank ();
			context.ArcNegative (25, 25, 20.5, -45, 45);
			DrawCurrentPosition ();
			context.SetColor (Colors.Black);
			context.Stroke ();
			CheckImage ("ArcNegativeStartingNegative.png");
		}
		
		[Test]
		public void ArcNegativeSwappedAngles ()
		{
			InitBlank ();
			context.ArcNegative (25, 25, 20.5, 300, 0);
			DrawCurrentPosition ();
			context.SetColor (Colors.Black);
			context.Stroke ();
			CheckImage ("ArcNegativeSwappedAngles.png");
		}
		
		[Test]
		public void ArcNegativeMultipleLoops ()
		{
			InitBlank ();
			context.ArcNegative (25, 25, 20.5, 0, 360 + 180);
			context.SetColor (Colors.Black.WithAlpha (0.5));
			context.SetLineWidth (5);
			context.RelLineTo (10, 0);
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
			DrawCurrentPosition ();
			context.LineTo (25.5, 25.5);
			context.SetColor (Colors.Black);
			context.Stroke ();
			CheckImage ("ArcNegativePathConnection.png");
		}
		
		[Test]
		public void ArcNegativeClosePath ()
		{
			InitBlank ();
			context.ArcNegative (25, 25, 20.5, 0, 180);
			context.ArcNegative (15, 25, 10.5, 180, 90);
			context.ClosePath ();
			context.SetColor (Colors.Black);
			context.Stroke ();
			
			CheckImage ("ArcNegativeClosePath.png");
		}

		#endregion

		#region ImagePattern

		[Test]
		public void ImagePattern ()
		{
			InitBlank (70, 70);
			context.Rectangle (5, 5, 40, 60);
			var img = ReferenceImageManager.LoadReferenceImage ("pattern-sample.png");
			context.Pattern = new Xwt.Drawing.ImagePattern (img);
			context.Fill ();
			CheckImage ("ImagePattern.png");
		}
		
		[Test]
		public void ImagePatternInCircle ()
		{
			InitBlank (50, 50);
			context.Arc (25, 25, 20, 0, 360);
			var img = ReferenceImageManager.LoadReferenceImage ("pattern-sample.png");
			context.Pattern = new Xwt.Drawing.ImagePattern (img);
			context.Fill ();
			CheckImage ("ImagePatternInCircle.png");
		}
		
		[Test]
		public void ImagePatternInTriangle ()
		{
			InitBlank (50, 50);
			context.MoveTo (25, 5);
			context.LineTo (45, 20);
			context.LineTo (5, 45);
			context.ClosePath ();
			var img = ReferenceImageManager.LoadReferenceImage ("pattern-sample.png");
			context.Pattern = new Xwt.Drawing.ImagePattern (img);
			context.Fill ();
			CheckImage ("ImagePatternInTriangle.png");
		}

		[Test]
		public void ImagePatternWithTranslateTransform ()
		{
			InitBlank (70, 70);
			context.Translate (5, 5);
			context.Rectangle (0, 0, 40, 60);
			var img = ReferenceImageManager.LoadReferenceImage ("pattern-sample.png");
			context.Pattern = new Xwt.Drawing.ImagePattern (img);
			context.Fill ();
			CheckImage ("ImagePatternWithTranslateTransform.png");
		}
		
		[Test]
		public void ImagePatternWithRotateTransform ()
		{
			InitBlank (70, 70);
			context.Rotate (4);
			context.Rectangle (5, 5, 40, 60);
			var img = ReferenceImageManager.LoadReferenceImage ("pattern-sample.png");
			context.Pattern = new Xwt.Drawing.ImagePattern (img);
			context.Fill ();
			CheckImage ("ImagePatternWithRotateTransform.png");
		}
		
		[Test]
		public void ImagePatternWithScaleTransform ()
		{
			InitBlank (70, 70);
			context.Scale (2, 0.5);
			context.Rectangle (5, 5, 20, 120);
			var img = ReferenceImageManager.LoadReferenceImage ("pattern-sample.png");
			context.Pattern = new Xwt.Drawing.ImagePattern (img);
			context.Fill ();
			CheckImage ("ImagePatternWithScaleTransform.png");
		}
		
		[Test]
		public void ImagePatternWithAlpha ()
		{
			InitBlank (70, 70);
			context.Rectangle (5, 5, 40, 60);
			var img = ReferenceImageManager.LoadReferenceImage ("pattern-sample.png");
			img = img.WithAlpha (0.5);
			context.Pattern = new Xwt.Drawing.ImagePattern (img);
			context.Fill ();
			CheckImage ("ImagePatternWithAlpha.png");
		}
		
		[Test]
		public void ImagePatternWithTranslateTransformWithAlpha ()
		{
			InitBlank (70, 70);
			context.Translate (5, 5);
			context.Rectangle (0, 0, 40, 60);
			var img = ReferenceImageManager.LoadReferenceImage ("pattern-sample.png");
			context.Pattern = new Xwt.Drawing.ImagePattern (img.WithAlpha (0.5));
			context.Fill ();
			CheckImage ("ImagePatternWithTranslateTransformWithAlpha.png");
		}
		
		[Test]
		public void ImagePatternWithRotateTransformWithAlpha ()
		{
			InitBlank (70, 70);
			context.Rotate (4);
			context.Rectangle (5, 5, 40, 60);
			var img = ReferenceImageManager.LoadReferenceImage ("pattern-sample.png");
			context.Pattern = new Xwt.Drawing.ImagePattern (img.WithAlpha (0.5));
			context.Fill ();
			CheckImage ("ImagePatternWithRotateTransformWithAlpha.png");
		}

		[Test]
		public void ImagePatternWithScaleTransformWithAlpha ()
		{
			InitBlank (70, 70);
			context.Scale (2, 0.5);
			context.Rectangle (5, 5, 20, 120);
			var img = ReferenceImageManager.LoadReferenceImage ("pattern-sample.png");
			context.Pattern = new Xwt.Drawing.ImagePattern (img.WithAlpha (0.5));
			context.Fill ();
			CheckImage ("ImagePatternWithScaleTransformWithAlpha.png");
		}

		#endregion

		#region Clipping

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

		#endregion
		
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

		#region LinearGradient
		
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

		#endregion

		#region RadialGradient
		
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

		#endregion

		#region Curves

		[Test]
		public void Curve ()
		{
			InitBlank (70,70);
			context.MoveTo (5, 35);
			context.CurveTo (35, 5, 35, 65, 65, 35);
			context.Stroke ();
			CheckImage ("Curve.png");
		}
		
		[Test]
		public void CurvePathConnection ()
		{
			InitBlank (70,70);
			context.MoveTo (0, 0);
			context.LineTo (5, 35);
			context.CurveTo (35, 5, 35, 65, 65, 35);
			context.LineTo (70, 70);
			context.Stroke ();
			CheckImage ("CurvePathConnection.png");
		}

		[Test]
		public void CurveFillWithHoles ()
		{
			InitBlank (70, 70);
			// Curve 1
			context.MoveTo (5, 35);
			context.CurveTo (20, 0, 50, 0, 60, 25);

			// curve2 with lineTo; curve1 is closed
			context.LineTo (5, 5);
			context.CurveTo (20, 30, 50, 30, 60, 5);

			context.ClosePath ();
			context.SetColor (Colors.Black);
			context.StrokePreserve ();
			context.SetColor (Colors.LightGray);
			context.Fill ();
			CheckImage ("CurveFillWithHoles.png");
		}
		
		[Test]
		public void CurveClosePath ()
		{
			InitBlank (100,100);
			context.MoveTo (5, 20);
			context.CurveTo (35, 5, 35, 65, 65, 20);
			context.CurveTo (70, 25, 60, 40, 45, 65);
			context.ClosePath ();
			context.Stroke ();
			CheckImage ("CurveClosePath.png");
		}


		#endregion

		#region Save/Restore

		[Test]
		public void SaveRestorePath ()
		{
			// Path is not saved
			InitBlank ();
			context.SetLineWidth (2);
			context.MoveTo (10, 10);
			context.LineTo (40, 10);
			context.Save ();
			context.LineTo (40, 40);
			context.Restore ();
			context.LineTo (10, 40);
			context.SetColor (Colors.Black);
			context.Stroke ();
			CheckImage ("SaveRestorePath.png");
		}
		
		[Test]
		public void SaveRestoreColor ()
		{
			// Color is saved
			InitBlank ();
			context.Rectangle (10, 10, 20, 20);
			context.SetColor (Colors.Black);
			context.Save ();
			context.SetColor (Colors.Red);
			context.Restore ();
			context.Fill ();
			CheckImage ("SaveRestoreColor.png");
		}
		
		[Test]
		public void SaveRestoreImagePattern ()
		{
			// Pattern is saved
			InitBlank (70, 70);
			context.Save ();
			var img = ReferenceImageManager.LoadReferenceImage ("pattern-sample.png");
			context.Pattern = new Xwt.Drawing.ImagePattern (img);
			context.Restore ();
			context.Rectangle (5, 5, 40, 60);
			context.Fill ();
			CheckImage ("SaveRestoreImagePattern.png");
		}

		[Test]
		public void SaveRestoreLinearGradient ()
		{
			// Pattern is saved
			InitBlank ();
			var g = new LinearGradient (5, 5, 5, 45);
			g.AddColorStop (0, Colors.Red);
			g.AddColorStop (0.5, Colors.Green);
			g.AddColorStop (1, Colors.Blue);
			context.Save ();
			context.Pattern = g;
			context.Restore ();
			context.Rectangle (5, 5, 40, 40);
			context.Fill ();
			CheckImage ("SaveRestoreLinearGradient.png");
		}

		[Test]
		public void SaveRestoreRadialGradient ()
		{
			// Pattern is saved
			InitBlank ();
			var g = new RadialGradient (20, 20, 5, 30, 30, 30);
			g.AddColorStop (0, Colors.Red);
			g.AddColorStop (0.5, Colors.Green);
			g.AddColorStop (1, Colors.Blue);
			context.Save ();
			context.Pattern = g;
			context.Restore ();
			context.Rectangle (5, 5, 40, 40);
			context.Fill ();
			CheckImage ("SaveRestoreRadialGradient.png");
		}

		[Test]
		public void SaveRestoreLineWidth ()
		{
			// Line width is saved
			InitBlank ();
			context.SetLineWidth (2);
			context.Save ();
			context.SetLineWidth (8);
			context.Restore ();
			context.Rectangle (10, 10, 30, 30);
			context.Stroke ();
			CheckImage ("SaveRestoreLineWidth.png");
		}
		
		[Test]
		public void SaveRestoreTransform ()
		{
			// Transform is saved
			InitBlank ();
			context.Save ();
			context.Translate (10, 10);
			context.Rotate (10);
			context.Scale (0.5, 0.5);
			context.Restore ();
			context.Rectangle (10, 10, 30, 30);
			context.Fill ();
			CheckImage ("SaveRestoreTransform.png");
		}

		#endregion

		#region Text
		
		[Test]
		public void Text ()
		{
			// Transform is saved
			InitBlank (100, 50);
			var la = new TextLayout ();
			la.Font = Font.FromName ("Arial 12");
			la.Text = "Hello World";
			context.DrawTextLayout (la, 5, 5);
			CheckImage ("Text.png");
		}
		
		[Test]
		public void TextSize ()
		{
			// Transform is saved
			InitBlank (100, 50);
			var la = new TextLayout ();
			la.Font = Font.FromName ("Arial 12");
			la.Text = "Hello World";

			var s = la.GetSize ();
			context.Rectangle (10.5, 10.5, s.Width, s.Height);
			context.SetColor (Colors.Blue);
			context.Stroke ();

			context.SetColor (Colors.Black);
			context.DrawTextLayout (la, 10, 10);
			CheckImage ("TextSize.png");
		}
		
		[Test]
		public void TextLineBreak ()
		{
			// Transform is saved
			InitBlank (100, 50);
			var la = new TextLayout ();
			la.Font = Font.FromName ("Arial 12");
			la.Text = "Hello\nWorld";

			var s = la.GetSize ();
			context.Rectangle (10.5, 10.5, s.Width, s.Height);
			context.SetColor (Colors.Blue);
			context.Stroke ();

			context.SetColor (Colors.Black);
			context.DrawTextLayout (la, 10, 10);
			CheckImage ("TextLineBreak.png");
		}
		
		[Test]
		public void TextWithBlankLines ()
		{
			// Transform is saved
			InitBlank (50, 150);
			var la = new TextLayout ();
			la.Font = Font.FromName ("Arial 12");
			la.Text = "\n\nHello\n\n\nWorld\n\n";

			var s = la.GetSize ();
			context.Rectangle (10.5, 10.5, s.Width, s.Height);
			context.SetColor (Colors.Blue);
			context.Stroke ();

			context.SetColor (Colors.Black);
			context.DrawTextLayout (la, 10, 10);
			CheckImage ("TextWithBlankLines.png");
		}

		[Test]
		public void TextWordWrap ()
		{
			// Transform is saved
			InitBlank (100, 100);
			var la = new TextLayout ();
			la.Font = Font.FromName ("Arial 12");
			la.Text = "One Two Three Four Five Six Seven Eight Nine";
			la.Width = 90;
			la.Trimming = TextTrimming.Word;
			var s = la.GetSize ();
			context.Rectangle (5.5, 5.5, s.Width, s.Height);
			context.SetColor (Colors.Blue);
			context.Stroke ();

			context.SetColor (Colors.Black);
			context.DrawTextLayout (la, 5, 5);
			CheckImage ("TextWordWrap.png");
		}
		
		[Test]
		public void TextTrimmingEllipsis ()
		{
			// Transform is saved
			InitBlank (50, 100);
			var la = new TextLayout ();
			la.Font = Font.FromName ("Arial 12");
			la.Text = "One Two Three Four Five Six Seven Eight Nine";
			la.Width = 45;
			la.Trimming = TextTrimming.WordElipsis;
			var s = la.GetSize ();
			context.Rectangle (5.5, 5.5, s.Width, s.Height);
			context.SetColor (Colors.Blue);
			context.Stroke ();

			context.SetColor (Colors.Black);
			context.DrawTextLayout (la, 5, 5);
			CheckImage ("TextTrimmingEllipsis.png");
		}

		[Test]
		public void TextAlignmentEnd ()
		{
			// Transform is saved
			InitBlank (100, 100);
			var la = new TextLayout ();
			la.Font = Font.FromName ("Arial 12");
			la.Text = "One Two Three Four Five Six Seven Eight Nine";
			la.Width = 90;
			la.TextAlignment = Alignment.End;
			var s = la.GetSize ();
			context.Rectangle (95.5 - s.Width, 5.5, s.Width, s.Height);
			context.SetColor (Colors.Blue);
			context.Stroke ();

			context.SetColor (Colors.Black);
			context.DrawTextLayout (la, 5, 5);
			CheckImage ("TextAlignmentEnd.png");
		}

		[Test]
		public void TextAlignmentCenter ()
		{
			// Transform is saved
			InitBlank (100, 100);
			var la = new TextLayout ();
			la.Font = Font.FromName ("Arial 12");
			la.Text = "One Two Three Four Five Six Seven Eight Nine";
			la.Width = 90;
			la.TextAlignment = Alignment.Center;
			var s = la.GetSize ();
			context.Rectangle (Math.Round ((90 - s.Width) / 2) + 5.5, 5.5, s.Width, s.Height);
			context.SetColor (Colors.Blue);
			context.Stroke ();

			context.SetColor (Colors.Black);
			context.DrawTextLayout (la, 5, 5);
			CheckImage ("TextAlignmentCenter.png");
		}

		#endregion

		#region Paths

		[Test]
		public void DrawPathTwoTimes ()
		{
			InitBlank (50, 50);
			var p = new DrawingPath ();
			p.Rectangle (15, 15, 20, 20);
			p.Rectangle (20, 20, 10, 10);
			context.AppendPath (p);
			context.Stroke ();
			context.Rotate (15);
			context.AppendPath (p);
			context.Stroke ();
			CheckImage ("DrawPathTwoTimes.png");
		}

		#endregion

		#region Hit test checking
		
		[Test]
		public void DrawingPathPointInFill ()
		{
			DrawingPath p = new DrawingPath ();
			p.Rectangle (10, 10, 20, 20);
			Assert.IsTrue (p.IsPointInFill (15, 15));
			Assert.IsFalse (p.IsPointInFill (9, 9));
		}

		[Test]
		public void ContextPointInStroke ()
		{
			InitBlank (50, 50);
			context.Rectangle (10, 10, 20, 20);
			context.SetLineWidth (3);
			Assert.IsTrue (context.IsPointInStroke (10, 10));
			Assert.IsTrue (context.IsPointInStroke (11, 11));
			Assert.IsFalse (context.IsPointInStroke (15, 15));
			context.MoveTo (15, 15);
			context.RelLineTo (5, 0);
			Assert.IsTrue (context.IsPointInStroke (15, 15));
		}
		
		[Test]
		public void ContextPointInFillWithTransform ()
		{
			InitBlank (50, 50);
			context.Translate (50, 50);
			context.Rectangle (10, 10, 20, 20);
			context.SetLineWidth (3);
			Assert.IsTrue (context.IsPointInStroke (10, 10));
			Assert.IsTrue (context.IsPointInStroke (11, 11));
			Assert.IsFalse (context.IsPointInStroke (15, 15));
			Assert.IsTrue (context.IsPointInFill (15, 15));
			Assert.IsFalse (context.IsPointInFill (9, 9));

			context.Translate (50, 50);
			Assert.IsTrue (context.IsPointInStroke (-40, -40));
			Assert.IsTrue (context.IsPointInStroke (-41, -41));
			Assert.IsFalse (context.IsPointInStroke (-45, -45));
			Assert.IsTrue (context.IsPointInFill (-35, -35));
			Assert.IsFalse (context.IsPointInFill (15, 15));
		}

		#endregion
	}
}

