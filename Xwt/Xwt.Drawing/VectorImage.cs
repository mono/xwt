//
// VectorImage.cs
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
using Xwt.Backends;
using System.Collections.Generic;
using System.Linq;

namespace Xwt.Drawing
{
	class VectorImage: DrawingImage
	{
		VectorImageData data;

		public VectorImage (Size s, VectorImageData data)
		{
			this.data = data;
			Size = s;
		}

		protected override void OnDraw (Context ctx, Rectangle bounds)
		{
			ctx.Save ();
			ctx.Translate (bounds.Location);
			ctx.Scale (bounds.Width / Size.Width, bounds.Height / Size.Height);
			VectorImageRecorderContextHandler.Draw (ToolkitEngine, ToolkitEngine.ContextBackendHandler, Toolkit.GetBackend (ctx), data);
			ctx.Restore ();
		}
	}

	class VectorImageData
	{
		public DrawingCommand[] Commands;
		public double[] Doubles;
		public Color[] Colors;
		public int[] Ints;
		public Rectangle[] Rectangles;
		public object[] Objects;
		public ImageDescription[] Images;
		public TextLayoutData[] TextLayouts;
	}

	enum DrawingCommand
	{
		Save,
		Restore,
		SetGlobalAlpha,
		SetColor,
		Clip,
		ClipPreserve,
		Arc,
		ArcNegative,
		ClosePath,
		CurveTo,
		LineTo,
		Fill,
		FillPreserve,
		NewPath,
		Stroke,
		StrokePreserve,
		SetLineWidth,
		SetLineDash,
		SetPattern,
		SetFont,
		DrawTextLayout,
		CanDrawImage,
		DrawImage,
		DrawImage2,
		Rotate,
		Scale,
		Translate,
		MoveTo,
		Rectangle,
		RelCurveTo,
		RelLineTo,
		RelMoveTo,
		AppendPath,
		End
	}

	class VectorContextBackend
	{
		public List<DrawingCommand> Commands = new List<DrawingCommand> ();
		public List<double> Doubles = new List<double> ();
		public List<Color> Colors = new List<Color> ();
		public List<int> Ints = new List<int> ();
		public List<Rectangle> Rectangles = new List<Rectangle> ();
		public List<object> Objects = new List<object> ();
		public List<ImageDescription> Images = new List<ImageDescription> ();
		public List<TextLayoutData> TextLayouts = new List<TextLayoutData> ();

		public void AddTextLayout (TextLayoutData td)
		{
			var found = TextLayouts.FirstOrDefault (t => t.Equals (td)) ?? td;
			TextLayouts.Add (found);
		}

		public VectorContextBackend Clone ()
		{
			var c = new VectorContextBackend ();
			c.Commands.AddRange (Commands);
			c.Doubles.AddRange (Doubles);
			c.Colors.AddRange (Colors);
			c.Images.AddRange (Images);
			c.Ints.AddRange (Ints);
			c.Rectangles.AddRange (Rectangles);
			c.Objects.AddRange (Objects);
			c.TextLayouts.AddRange (TextLayouts);
			return c;
		}

		public VectorImageData ToVectorImageData ()
		{
			return new VectorImageData () {
				Commands = Commands.ToArray (),
				Doubles = Doubles.ToArray (),
				Colors = Colors.ToArray (),
				Images = Images.ToArray (),
				Ints = Ints.ToArray (),
				Rectangles = Rectangles.ToArray (),
				Objects = Objects.ToArray (),
				TextLayouts = TextLayouts.ToArray (),
			};
		}
	}

	class VectorImageRecorderContextHandler: ContextBackendHandler
	{
		#region implemented abstract members of DrawingPathBackendHandler

		public override void Arc (object backend, double xc, double yc, double radius, double angle1, double angle2)
		{
			var ctx = (VectorContextBackend)backend;
			ctx.Commands.Add (DrawingCommand.Arc);
			ctx.Doubles.Add (xc);
			ctx.Doubles.Add (yc);
			ctx.Doubles.Add (radius);
			ctx.Doubles.Add (angle1);
			ctx.Doubles.Add (angle2);
		}

		public override void ArcNegative (object backend, double xc, double yc, double radius, double angle1, double angle2)
		{
			var ctx = (VectorContextBackend)backend;
			ctx.Commands.Add (DrawingCommand.ArcNegative);
			ctx.Doubles.Add (xc);
			ctx.Doubles.Add (yc);
			ctx.Doubles.Add (radius);
			ctx.Doubles.Add (angle1);
			ctx.Doubles.Add (angle2);
		}

		public override void ClosePath (object backend)
		{
			var ctx = (VectorContextBackend)backend;
			ctx.Commands.Add (DrawingCommand.ClosePath);
		}

		public override void CurveTo (object backend, double x1, double y1, double x2, double y2, double x3, double y3)
		{
			var ctx = (VectorContextBackend)backend;
			ctx.Commands.Add (DrawingCommand.CurveTo);
			ctx.Doubles.Add (x1);
			ctx.Doubles.Add (y1);
			ctx.Doubles.Add (x2);
			ctx.Doubles.Add (y2);
			ctx.Doubles.Add (x3);
			ctx.Doubles.Add (y3);
		}

		public override void LineTo (object backend, double x, double y)
		{
			var ctx = (VectorContextBackend)backend;
			ctx.Commands.Add (DrawingCommand.LineTo);
			ctx.Doubles.Add (x);
			ctx.Doubles.Add (y);
		}

		public override void MoveTo (object backend, double x, double y)
		{
			var ctx = (VectorContextBackend)backend;
			ctx.Commands.Add (DrawingCommand.MoveTo);
			ctx.Doubles.Add (x);
			ctx.Doubles.Add (y);
		}

		public override void Rectangle (object backend, double x, double y, double width, double height)
		{
			var ctx = (VectorContextBackend)backend;
			ctx.Commands.Add (DrawingCommand.Rectangle);
			ctx.Doubles.Add (x);
			ctx.Doubles.Add (y);
			ctx.Doubles.Add (width);
			ctx.Doubles.Add (height);
		}

		public override void RelCurveTo (object backend, double dx1, double dy1, double dx2, double dy2, double dx3, double dy3)
		{
			var ctx = (VectorContextBackend)backend;
			ctx.Commands.Add (DrawingCommand.RelCurveTo);
			ctx.Doubles.Add (dx1);
			ctx.Doubles.Add (dy1);
			ctx.Doubles.Add (dx2);
			ctx.Doubles.Add (dy2);
			ctx.Doubles.Add (dx3);
			ctx.Doubles.Add (dy3);
		}

		public override void RelLineTo (object backend, double dx, double dy)
		{
			var ctx = (VectorContextBackend)backend;
			ctx.Commands.Add (DrawingCommand.RelLineTo);
			ctx.Doubles.Add (dx);
			ctx.Doubles.Add (dy);
		}

		public override void RelMoveTo (object backend, double dx, double dy)
		{
			var ctx = (VectorContextBackend)backend;
			ctx.Commands.Add (DrawingCommand.RelMoveTo);
			ctx.Doubles.Add (dx);
			ctx.Doubles.Add (dy);
		}

		public override object CreatePath ()
		{
			return new VectorContextBackend ();
		}

		public override object CopyPath (object backend)
		{
			var ctx = (VectorContextBackend)backend;
			return ctx.Clone ();
		}

		public override void AppendPath (object backend, object otherBackend)
		{
			var ctx = (VectorContextBackend)backend;
			ctx.Commands.Add (DrawingCommand.AppendPath);
			ctx.Objects.Add (((VectorContextBackend)otherBackend).ToVectorImageData ());
		}

		#endregion

		#region implemented abstract members of ContextBackendHandler

		public override void Save (object backend)
		{
			var ctx = (VectorContextBackend)backend;
			ctx.Commands.Add (DrawingCommand.Save);
		}

		public override void Restore (object backend)
		{
			var ctx = (VectorContextBackend)backend;
			ctx.Commands.Add (DrawingCommand.Restore);
		}

		public override void Clip (object backend)
		{
			var ctx = (VectorContextBackend)backend;
			ctx.Commands.Add (DrawingCommand.Clip);
		}

		public override void ClipPreserve (object backend)
		{
			var ctx = (VectorContextBackend)backend;
			ctx.Commands.Add (DrawingCommand.ClipPreserve);
		}

		public override void Fill (object backend)
		{
			var ctx = (VectorContextBackend)backend;
			ctx.Commands.Add (DrawingCommand.Fill);
		}

		public override void FillPreserve (object backend)
		{
			var ctx = (VectorContextBackend)backend;
			ctx.Commands.Add (DrawingCommand.FillPreserve);
		}

		public override void NewPath (object backend)
		{
			var ctx = (VectorContextBackend)backend;
			ctx.Commands.Add (DrawingCommand.NewPath);
		}

		public override void Stroke (object backend)
		{
			var ctx = (VectorContextBackend)backend;
			ctx.Commands.Add (DrawingCommand.Stroke);
		}

		public override void StrokePreserve (object backend)
		{
			var ctx = (VectorContextBackend)backend;
			ctx.Commands.Add (DrawingCommand.StrokePreserve);
		}

		public override void SetColor (object backend, Color color)
		{
			var ctx = (VectorContextBackend)backend;
			ctx.Commands.Add (DrawingCommand.SetColor);
			ctx.Colors.Add (color);
		}

		public override void SetLineWidth (object backend, double width)
		{
			var ctx = (VectorContextBackend)backend;
			ctx.Commands.Add (DrawingCommand.SetLineWidth);
			ctx.Doubles.Add (width);
		}

		public override void SetLineDash (object backend, double offset, params double[] pattern)
		{
			var ctx = (VectorContextBackend)backend;
			ctx.Commands.Add (DrawingCommand.SetLineDash);
			ctx.Doubles.Add (offset);
			ctx.Ints.Add (pattern.Length);
			for (int n=0; n<pattern.Length; n++)
				ctx.Doubles.Add (pattern [n]);
		}

		public override void SetPattern (object backend, object p)
		{
			var ctx = (VectorContextBackend)backend;
			ctx.Commands.Add (DrawingCommand.SetPattern);
			ctx.Objects.Add (p);
		}

		public override void SetFont (object backend, Font font)
		{
			var ctx = (VectorContextBackend)backend;
			ctx.Commands.Add (DrawingCommand.SetFont);
			ctx.Objects.Add (font);
		}

		public override void DrawTextLayout (object backend, TextLayout layout, double x, double y)
		{
			var ctx = (VectorContextBackend)backend;
			ctx.Commands.Add (DrawingCommand.DrawTextLayout);
			var la = layout.GetData ();
			ctx.AddTextLayout (la);
			ctx.Doubles.Add (x);
			ctx.Doubles.Add (y);
		}

		public override void DrawImage (object backend, ImageDescription img, double x, double y)
		{
			var ctx = (VectorContextBackend)backend;
			ctx.Commands.Add (DrawingCommand.DrawImage);
			ctx.Images.Add (img);
			ctx.Doubles.Add (x);
			ctx.Doubles.Add (y);
		}

		public override void DrawImage (object backend, ImageDescription img, Xwt.Rectangle srcRect, Xwt.Rectangle destRect)
		{
			var ctx = (VectorContextBackend)backend;
			ctx.Commands.Add (DrawingCommand.DrawImage2);
			ctx.Images.Add (img);
			ctx.Rectangles.Add (srcRect);
			ctx.Rectangles.Add (destRect);
		}

		public override void Rotate (object backend, double angle)
		{
			var ctx = (VectorContextBackend)backend;
			ctx.Commands.Add (DrawingCommand.Rotate);
			ctx.Doubles.Add (angle);
		}

		public override void Scale (object backend, double scaleX, double scaleY)
		{
			var ctx = (VectorContextBackend)backend;
			ctx.Commands.Add (DrawingCommand.Scale);
			ctx.Doubles.Add (scaleX);
			ctx.Doubles.Add (scaleY);
		}

		public override void Translate (object backend, double tx, double ty)
		{
			var ctx = (VectorContextBackend)backend;
			ctx.Commands.Add (DrawingCommand.Translate);
			ctx.Doubles.Add (tx);
			ctx.Doubles.Add (ty);
		}

		public override Matrix GetCTM (object backend)
		{
			throw new NotSupportedException ();
		}

		public override bool IsPointInStroke (object backend, double x, double y)
		{
			return false;
		}

		public override void SetGlobalAlpha (object backend, double globalAlpha)
		{
			var ctx = (VectorContextBackend)backend;
			ctx.Commands.Add (DrawingCommand.SetGlobalAlpha);
			ctx.Doubles.Add (globalAlpha);
		}

		public override bool IsPointInFill (object backend, double x, double y)
		{
			throw new NotSupportedException ();
		}

		public override void Dispose (object backend)
		{
		}

		#endregion

		internal static void Draw (Toolkit tk, ContextBackendHandler handler, object ctx, VectorImageData cm)
		{
			int di = 0;
			int ci = 0;
			int ii = 0;
			int ri = 0;
			int oi = 0;
			int imi = 0;
			int ti = 0;

			for (int n=0; n<cm.Commands.Length; n++)
			{
				switch (cm.Commands [n]) {
				case DrawingCommand.AppendPath:
					var p = (VectorImageData)cm.Objects [oi++];
					Draw (tk, handler, ctx, p);
					break;
				case DrawingCommand.Arc:
					handler.Arc (ctx, cm.Doubles [di++], cm.Doubles [di++], cm.Doubles [di++], cm.Doubles [di++], cm.Doubles [di++]);
					break;
				case DrawingCommand.ArcNegative:
					handler.ArcNegative (ctx, cm.Doubles [di++], cm.Doubles [di++], cm.Doubles [di++], cm.Doubles [di++], cm.Doubles [di++]);
					break;
				case DrawingCommand.Clip:
					handler.Clip (ctx);
					break;
				case DrawingCommand.ClipPreserve:
					handler.ClipPreserve (ctx);
					break;
				case DrawingCommand.ClosePath:
					handler.ClosePath (ctx);
					break;
				case DrawingCommand.CurveTo:
					handler.CurveTo (ctx, cm.Doubles [di++], cm.Doubles [di++], cm.Doubles [di++], cm.Doubles [di++], cm.Doubles [di++], cm.Doubles [di++]);
					break;
				case DrawingCommand.DrawImage2:
					handler.DrawImage (ctx, cm.Images [imi++], cm.Rectangles [ri++], cm.Rectangles [ri++]);
					break;
				case DrawingCommand.DrawImage:
					handler.DrawImage (ctx, cm.Images [imi++], cm.Doubles [di++], cm.Doubles [di++]);
					break;
				case DrawingCommand.DrawTextLayout:
					var lad = (TextLayoutData)cm.TextLayouts [ti++];
					var la = new TextLayout (tk);
					lad.InitLayout (la);
					handler.DrawTextLayout (ctx, la, cm.Doubles [di++], cm.Doubles [di++]);
					break;
				case DrawingCommand.Fill:
					handler.Fill (ctx);
					break;
				case DrawingCommand.FillPreserve:
					handler.FillPreserve (ctx);
					break;
				case DrawingCommand.LineTo:
					handler.LineTo (ctx, cm.Doubles [di++], cm.Doubles [di++]);
					break;
				case DrawingCommand.MoveTo:
					handler.MoveTo (ctx, cm.Doubles [di++], cm.Doubles [di++]);
					break;
				case DrawingCommand.NewPath:
					handler.NewPath (ctx);
					break;
				case DrawingCommand.Rectangle:
					handler.Rectangle (ctx, cm.Doubles [di++], cm.Doubles [di++], cm.Doubles [di++], cm.Doubles [di++]);
					break;
				case DrawingCommand.RelCurveTo:
					handler.RelCurveTo (ctx, cm.Doubles [di++], cm.Doubles [di++], cm.Doubles [di++], cm.Doubles [di++], cm.Doubles [di++], cm.Doubles [di++]);
					break;
				case DrawingCommand.RelLineTo:
					handler.RelLineTo (ctx, cm.Doubles [di++], cm.Doubles [di++]);
					break;
				case DrawingCommand.RelMoveTo:
					handler.RelMoveTo (ctx, cm.Doubles [di++], cm.Doubles [di++]);
					break;
				case DrawingCommand.Restore:
					handler.Restore (ctx);
					break;
				case DrawingCommand.Rotate:
					handler.Rotate (ctx, cm.Doubles [di++]);
					break;
				case DrawingCommand.Save:
					handler.Save (ctx);
					break;
				case DrawingCommand.Scale:
					handler.Scale (ctx, cm.Doubles [di++], cm.Doubles [di++]);
					break;
				case DrawingCommand.SetColor:
					handler.SetColor (ctx, cm.Colors [ci++]);
					break;
				case DrawingCommand.SetFont:
					handler.SetFont (ctx, (Font)cm.Objects [oi++]);
					break;
				case DrawingCommand.SetGlobalAlpha:
					handler.SetGlobalAlpha (ctx, cm.Doubles [di++]);
					break;
				case DrawingCommand.SetLineDash:
					var off = cm.Doubles [di++];
					var ds = new double [cm.Ints [ii++]];
					for (int i = 0; i < ds.Length; i++)
						ds [i] = cm.Doubles [di++];
					handler.SetLineDash (ctx, off, ds);
					break;
				case DrawingCommand.SetLineWidth:
					handler.SetLineWidth (ctx, cm.Doubles [di++]);
					break;
				case DrawingCommand.SetPattern:
					handler.SetPattern (ctx, cm.Objects [oi++]);
					break;
				case DrawingCommand.Stroke:
					handler.Stroke (ctx);
					break;
				case DrawingCommand.StrokePreserve:
					handler.StrokePreserve (ctx);
					break;
				case DrawingCommand.Translate:
					handler.Translate (ctx, cm.Doubles [di++], cm.Doubles [di++]);
					break;
				}
			}
		}
	}
}

