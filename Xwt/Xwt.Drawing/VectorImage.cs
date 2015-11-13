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
			ToolkitEngine.VectorImageRecorderContextHandler.Draw (ctx.Handler, Toolkit.GetBackend (ctx), data);
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
		SetStyles,
		End
	}

	class VectorContextBackend: VectorBackend
	{
		double width;
		double height;
		object imageBuilder;

		public VectorContextBackend (Toolkit toolkit, double width, double height): base (toolkit)
		{
			NativePathHandler = toolkit.ContextBackendHandler;
			NativeContextHandler = toolkit.ContextBackendHandler;
			this.width = width;
			this.height = height;
		}

		public override object CreateNativeBackend ()
		{
			imageBuilder = toolkit.ImageBuilderBackendHandler.CreateImageBuilder ((int)width, (int)height, ImageFormat.ARGB32);
			if (toolkit.ImageBuilderBackendHandler.DisposeHandleOnUiThread)
				ResourceManager.RegisterResource (imageBuilder);
			else
				GC.SuppressFinalize (this);

			return toolkit.ImageBuilderBackendHandler.CreateContext (imageBuilder);
		}

		~VectorContextBackend ()
		{
			if (imageBuilder != null)
				ResourceManager.FreeResource (imageBuilder);
		}

		public override void Dispose ()
		{
			base.Dispose ();
			if (imageBuilder != null) {
				if (toolkit.ImageBuilderBackendHandler.DisposeHandleOnUiThread) {
					GC.SuppressFinalize (this);
					ResourceManager.FreeResource (imageBuilder);
				}
				else
					toolkit.ImageBuilderBackendHandler.Dispose (imageBuilder);
			}
		}
	}

	class VectorPathBackend: VectorBackend
	{
		public VectorPathBackend (Toolkit toolkit): base (toolkit)
		{
			NativePathHandler = toolkit.DrawingPathBackendHandler;
		}

		public override object CreateNativeBackend ()
		{
			return toolkit.DrawingPathBackendHandler.CreatePath ();
		}
	}

	abstract class VectorBackend
	{
		public List<DrawingCommand> Commands = new List<DrawingCommand> ();
		public List<double> Doubles = new List<double> ();
		public List<Color> Colors = new List<Color> ();
		public List<int> Ints = new List<int> ();
		public List<Rectangle> Rectangles = new List<Rectangle> ();
		public List<object> Objects = new List<object> ();
		public List<ImageDescription> Images = new List<ImageDescription> ();
		public List<TextLayoutData> TextLayouts = new List<TextLayoutData> ();

		public object NativeBackend;
		public DrawingPathBackendHandler NativePathHandler;
		public ContextBackendHandler NativeContextHandler;
		protected Toolkit toolkit;

		public VectorBackend (Toolkit toolkit)
		{
			this.toolkit = toolkit;
		}

		public void AddTextLayout (TextLayoutData td)
		{
			var found = TextLayouts.FirstOrDefault (t => t.Equals (td)) ?? td;
			TextLayouts.Add (found);
		}

		public abstract object CreateNativeBackend ();

		public VectorPathBackend CopyPath ()
		{
			var c = new VectorPathBackend (toolkit);
			c.Commands.AddRange (Commands);
			c.Doubles.AddRange (Doubles);
			c.Colors.AddRange (Colors);
			c.Images.AddRange (Images);
			c.Ints.AddRange (Ints);
			c.Rectangles.AddRange (Rectangles);
			c.Objects.AddRange (Objects);
			c.TextLayouts.AddRange (TextLayouts);
			if (NativeBackend != null)
				c.NativeBackend = c.NativePathHandler.CopyPath (NativeBackend);
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

		public virtual void Dispose ()
		{
			if (NativeBackend != null)
				NativePathHandler.Dispose (NativeBackend);
		}
	}

	class VectorImageRecorderContextHandler: ContextBackendHandler
	{
		Toolkit toolkit;

		public VectorImageRecorderContextHandler (Toolkit toolkit)
		{
			this.toolkit = toolkit;
		}

		#region implemented abstract members of DrawingPathBackendHandler

		public override void Arc (object backend, double xc, double yc, double radius, double angle1, double angle2)
		{
			var ctx = (VectorBackend)backend;
			ctx.Commands.Add (DrawingCommand.Arc);
			ctx.Doubles.Add (xc);
			ctx.Doubles.Add (yc);
			ctx.Doubles.Add (radius);
			ctx.Doubles.Add (angle1);
			ctx.Doubles.Add (angle2);

			if (ctx.NativeBackend != null)
				ctx.NativePathHandler.Arc (ctx.NativeBackend, xc, yc, radius, angle1, angle2);
		}

		public override void ArcNegative (object backend, double xc, double yc, double radius, double angle1, double angle2)
		{
			var ctx = (VectorBackend)backend;
			ctx.Commands.Add (DrawingCommand.ArcNegative);
			ctx.Doubles.Add (xc);
			ctx.Doubles.Add (yc);
			ctx.Doubles.Add (radius);
			ctx.Doubles.Add (angle1);
			ctx.Doubles.Add (angle2);
			
			if (ctx.NativeBackend != null)
				ctx.NativePathHandler.ArcNegative (ctx.NativeBackend, xc, yc, radius, angle1, angle2);
		}

		public override void ClosePath (object backend)
		{
			var ctx = (VectorBackend)backend;
			ctx.Commands.Add (DrawingCommand.ClosePath);
			
			if (ctx.NativeBackend != null)
				ctx.NativePathHandler.ClosePath (ctx.NativeBackend);
		}

		public override void CurveTo (object backend, double x1, double y1, double x2, double y2, double x3, double y3)
		{
			var ctx = (VectorBackend)backend;
			ctx.Commands.Add (DrawingCommand.CurveTo);
			ctx.Doubles.Add (x1);
			ctx.Doubles.Add (y1);
			ctx.Doubles.Add (x2);
			ctx.Doubles.Add (y2);
			ctx.Doubles.Add (x3);
			ctx.Doubles.Add (y3);
			
			if (ctx.NativeBackend != null)
				ctx.NativePathHandler.CurveTo (ctx.NativeBackend, x1, y1, x2, y2, x3, y3);
		}

		public override void LineTo (object backend, double x, double y)
		{
			var ctx = (VectorBackend)backend;
			ctx.Commands.Add (DrawingCommand.LineTo);
			ctx.Doubles.Add (x);
			ctx.Doubles.Add (y);
			
			if (ctx.NativeBackend != null)
				ctx.NativePathHandler.LineTo (ctx.NativeBackend, x, y);
		}

		public override void MoveTo (object backend, double x, double y)
		{
			var ctx = (VectorBackend)backend;
			ctx.Commands.Add (DrawingCommand.MoveTo);
			ctx.Doubles.Add (x);
			ctx.Doubles.Add (y);
			
			if (ctx.NativeBackend != null)
				ctx.NativePathHandler.MoveTo (ctx.NativeBackend, x, y);
		}

		public override void Rectangle (object backend, double x, double y, double width, double height)
		{
			var ctx = (VectorBackend)backend;
			ctx.Commands.Add (DrawingCommand.Rectangle);
			ctx.Doubles.Add (x);
			ctx.Doubles.Add (y);
			ctx.Doubles.Add (width);
			ctx.Doubles.Add (height);
			
			if (ctx.NativeBackend != null)
				ctx.NativePathHandler.Rectangle (ctx.NativeBackend, x, y, width, height);
		}

		public override void RelCurveTo (object backend, double dx1, double dy1, double dx2, double dy2, double dx3, double dy3)
		{
			var ctx = (VectorBackend)backend;
			ctx.Commands.Add (DrawingCommand.RelCurveTo);
			ctx.Doubles.Add (dx1);
			ctx.Doubles.Add (dy1);
			ctx.Doubles.Add (dx2);
			ctx.Doubles.Add (dy2);
			ctx.Doubles.Add (dx3);
			ctx.Doubles.Add (dy3);
			
			if (ctx.NativeBackend != null)
				ctx.NativePathHandler.RelCurveTo (ctx.NativeBackend, dx1, dy1, dx2, dy2, dx3, dy3);
		}

		public override void RelLineTo (object backend, double dx, double dy)
		{
			var ctx = (VectorBackend)backend;
			ctx.Commands.Add (DrawingCommand.RelLineTo);
			ctx.Doubles.Add (dx);
			ctx.Doubles.Add (dy);
			
			if (ctx.NativeBackend != null)
				ctx.NativePathHandler.RelLineTo (ctx.NativeBackend, dx, dy);
		}

		public override void RelMoveTo (object backend, double dx, double dy)
		{
			var ctx = (VectorBackend)backend;
			ctx.Commands.Add (DrawingCommand.RelMoveTo);
			ctx.Doubles.Add (dx);
			ctx.Doubles.Add (dy);
			
			if (ctx.NativeBackend != null)
				ctx.NativePathHandler.RelMoveTo (ctx.NativeBackend, dx, dy);
		}

		public override object CreatePath ()
		{
			return new VectorPathBackend (toolkit);
		}

		public override object CopyPath (object backend)
		{
			var ctx = (VectorBackend)backend;
			return ctx.CopyPath ();
		}

		public override void AppendPath (object backend, object otherBackend)
		{
			var ctx = (VectorBackend)backend;
			ctx.Commands.Add (DrawingCommand.AppendPath);

			if (otherBackend is VectorBackend) {
				var data = ((VectorBackend)otherBackend).ToVectorImageData ();
				ctx.Objects.Add (data);
				if (ctx.NativeBackend != null)
					toolkit.VectorImageRecorderContextHandler.Draw (ctx.NativePathHandler, ctx.NativeBackend, data);
			} else {
				otherBackend = ctx.NativePathHandler.CopyPath (otherBackend);
				ctx.Objects.Add (otherBackend);
				if (ctx.NativeBackend != null)
					ctx.NativePathHandler.AppendPath (ctx.NativeBackend, otherBackend);
			}
		}

		#endregion

		#region implemented abstract members of ContextBackendHandler

		public override void Save (object backend)
		{
			var ctx = (VectorBackend)backend;
			ctx.Commands.Add (DrawingCommand.Save);

			if (ctx.NativeBackend != null && ctx.NativeContextHandler != null)
				ctx.NativeContextHandler.Save (ctx.NativeBackend);
		}

		public override void Restore (object backend)
		{
			var ctx = (VectorBackend)backend;
			ctx.Commands.Add (DrawingCommand.Restore);
			
			if (ctx.NativeBackend != null && ctx.NativeContextHandler != null)
				ctx.NativeContextHandler.Restore (ctx.NativeBackend);
		}

		public override void Clip (object backend)
		{
			var ctx = (VectorBackend)backend;
			ctx.Commands.Add (DrawingCommand.Clip);
		}

		public override void ClipPreserve (object backend)
		{
			var ctx = (VectorBackend)backend;
			ctx.Commands.Add (DrawingCommand.ClipPreserve);
		}

		public override void Fill (object backend)
		{
			var ctx = (VectorBackend)backend;
			ctx.Commands.Add (DrawingCommand.Fill);
		}

		public override void FillPreserve (object backend)
		{
			var ctx = (VectorBackend)backend;
			ctx.Commands.Add (DrawingCommand.FillPreserve);
		}

		public override void NewPath (object backend)
		{
			var ctx = (VectorBackend)backend;
			ctx.Commands.Add (DrawingCommand.NewPath);
			
			if (ctx.NativeBackend != null && ctx.NativeContextHandler != null)
				ctx.NativeContextHandler.NewPath (ctx.NativeBackend);
		}

		public override void Stroke (object backend)
		{
			var ctx = (VectorBackend)backend;
			ctx.Commands.Add (DrawingCommand.Stroke);
		}

		public override void StrokePreserve (object backend)
		{
			var ctx = (VectorBackend)backend;
			ctx.Commands.Add (DrawingCommand.StrokePreserve);
		}

		public override void SetColor (object backend, Color color)
		{
			var ctx = (VectorBackend)backend;
			ctx.Commands.Add (DrawingCommand.SetColor);
			ctx.Colors.Add (color);
		}

		public override void SetLineWidth (object backend, double width)
		{
			var ctx = (VectorBackend)backend;
			ctx.Commands.Add (DrawingCommand.SetLineWidth);
			ctx.Doubles.Add (width);
			
			if (ctx.NativeBackend != null && ctx.NativeContextHandler != null)
				ctx.NativeContextHandler.SetLineWidth (ctx.NativeBackend, width);
		}

		public override void SetLineDash (object backend, double offset, params double[] pattern)
		{
			var ctx = (VectorBackend)backend;
			ctx.Commands.Add (DrawingCommand.SetLineDash);
			ctx.Doubles.Add (offset);
			ctx.Ints.Add (pattern.Length);
			for (int n=0; n<pattern.Length; n++)
				ctx.Doubles.Add (pattern [n]);
		}

		public override void SetPattern (object backend, object p)
		{
			var ctx = (VectorBackend)backend;
			ctx.Commands.Add (DrawingCommand.SetPattern);
			ctx.Objects.Add (p);
		}

		public override void DrawTextLayout (object backend, TextLayout layout, double x, double y)
		{
			var ctx = (VectorBackend)backend;
			ctx.Commands.Add (DrawingCommand.DrawTextLayout);
			var la = layout.GetData ();
			ctx.AddTextLayout (la);
			ctx.Doubles.Add (x);
			ctx.Doubles.Add (y);
		}

		public override void DrawImage (object backend, ImageDescription img, double x, double y)
		{
			var ctx = (VectorBackend)backend;
			ctx.Commands.Add (DrawingCommand.DrawImage);
			ctx.Images.Add (img);
			ctx.Doubles.Add (x);
			ctx.Doubles.Add (y);
		}

		public override void DrawImage (object backend, ImageDescription img, Xwt.Rectangle srcRect, Xwt.Rectangle destRect)
		{
			var ctx = (VectorBackend)backend;
			ctx.Commands.Add (DrawingCommand.DrawImage2);
			ctx.Images.Add (img);
			ctx.Rectangles.Add (srcRect);
			ctx.Rectangles.Add (destRect);
		}

		public override void Rotate (object backend, double angle)
		{
			var ctx = (VectorBackend)backend;
			ctx.Commands.Add (DrawingCommand.Rotate);
			ctx.Doubles.Add (angle);
			
			if (ctx.NativeBackend != null && ctx.NativeContextHandler != null)
				ctx.NativeContextHandler.Rotate (ctx.NativeBackend, angle);
		}

		public override void Scale (object backend, double scaleX, double scaleY)
		{
			var ctx = (VectorBackend)backend;
			ctx.Commands.Add (DrawingCommand.Scale);
			ctx.Doubles.Add (scaleX);
			ctx.Doubles.Add (scaleY);
			
			if (ctx.NativeBackend != null && ctx.NativeContextHandler != null)
				ctx.NativeContextHandler.Scale (ctx.NativeBackend, scaleX, scaleY);
		}

		public override void Translate (object backend, double tx, double ty)
		{
			var ctx = (VectorBackend)backend;
			ctx.Commands.Add (DrawingCommand.Translate);
			ctx.Doubles.Add (tx);
			ctx.Doubles.Add (ty);
			
			if (ctx.NativeBackend != null && ctx.NativeContextHandler != null)
				ctx.NativeContextHandler.Translate (ctx.NativeBackend, tx, ty);
		}

		public override void ModifyCTM (object backend, Matrix t)
		{
			var ctx = (VectorBackend)backend;
			CreateNativePathBackend (ctx);
			ctx.NativeContextHandler.ModifyCTM (ctx.NativeBackend, t);
		}

		public override Matrix GetCTM (object backend)
		{
			var ctx = (VectorBackend)backend;
			CreateNativePathBackend (ctx);
			return ctx.NativeContextHandler.GetCTM (ctx.NativeBackend);
		}

		public override bool IsPointInStroke (object backend, double x, double y)
		{
			var ctx = (VectorBackend)backend;
			CreateNativePathBackend (ctx);
			return ctx.NativeContextHandler.IsPointInStroke (ctx.NativeBackend, x, y);
		}

		public override void SetGlobalAlpha (object backend, double globalAlpha)
		{
			var ctx = (VectorBackend)backend;
			ctx.Commands.Add (DrawingCommand.SetGlobalAlpha);
			ctx.Doubles.Add (globalAlpha);
		}

		public override void SetStyles (object backend, StyleSet styles)
		{
			var ctx = (VectorBackend)backend;
			ctx.Commands.Add (DrawingCommand.SetStyles);
			ctx.Objects.Add (styles);
		}

		public override bool IsPointInFill (object backend, double x, double y)
		{
			var ctx = (VectorBackend)backend;
			CreateNativePathBackend (ctx);
			return ctx.NativePathHandler.IsPointInFill (ctx.NativeBackend, x, y);
		}

		public override double GetScaleFactor (object backend)
		{
			var ctx = (VectorBackend)backend;
			CreateNativePathBackend (ctx);
			return ctx.NativeContextHandler.GetScaleFactor (ctx.NativeBackend);
		}

		public override void Dispose (object backend)
		{
			var ctx = (VectorBackend)backend;
			ctx.Dispose ();
		}

		void CreateNativePathBackend (VectorBackend b)
		{
			if (b.NativeBackend == null) {
				b.NativeBackend = b.CreateNativeBackend ();
				Draw (b.NativePathHandler, b.NativeBackend, b.ToVectorImageData ());
			}
		}

		#endregion

		internal void Draw (DrawingPathBackendHandler targetHandler, object ctx, VectorImageData cm)
		{
			int di = 0;
			int ci = 0;
			int ii = 0;
			int ri = 0;
			int oi = 0;
			int imi = 0;
			int ti = 0;

			ContextBackendHandler handler = targetHandler as ContextBackendHandler;
			DrawingPathBackendHandler pathHandler = targetHandler;

			for (int n=0; n<cm.Commands.Length; n++)
			{
				switch (cm.Commands [n]) {
				case DrawingCommand.AppendPath:
					var p = cm.Objects [oi++];
					if (p is VectorImageData)
						Draw (targetHandler, ctx, (VectorImageData)p);
					else
						pathHandler.AppendPath (ctx, p);
					break;
				case DrawingCommand.Arc:
					pathHandler.Arc (ctx, cm.Doubles [di++], cm.Doubles [di++], cm.Doubles [di++], cm.Doubles [di++], cm.Doubles [di++]);
					break;
				case DrawingCommand.ArcNegative:
					pathHandler.ArcNegative (ctx, cm.Doubles [di++], cm.Doubles [di++], cm.Doubles [di++], cm.Doubles [di++], cm.Doubles [di++]);
					break;
				case DrawingCommand.Clip:
					handler.Clip (ctx);
					break;
				case DrawingCommand.ClipPreserve:
					handler.ClipPreserve (ctx);
					break;
				case DrawingCommand.ClosePath:
					pathHandler.ClosePath (ctx);
					break;
				case DrawingCommand.CurveTo:
					pathHandler.CurveTo (ctx, cm.Doubles [di++], cm.Doubles [di++], cm.Doubles [di++], cm.Doubles [di++], cm.Doubles [di++], cm.Doubles [di++]);
					break;
				case DrawingCommand.DrawImage2:
					handler.DrawImage (ctx, cm.Images [imi++], cm.Rectangles [ri++], cm.Rectangles [ri++]);
					break;
				case DrawingCommand.DrawImage:
					handler.DrawImage (ctx, cm.Images [imi++], cm.Doubles [di++], cm.Doubles [di++]);
					break;
				case DrawingCommand.DrawTextLayout:
					var lad = (TextLayoutData)cm.TextLayouts [ti++];
					var la = new TextLayout (toolkit);
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
					pathHandler.LineTo (ctx, cm.Doubles [di++], cm.Doubles [di++]);
					break;
				case DrawingCommand.MoveTo:
					pathHandler.MoveTo (ctx, cm.Doubles [di++], cm.Doubles [di++]);
					break;
				case DrawingCommand.NewPath:
					handler.NewPath (ctx);
					break;
				case DrawingCommand.Rectangle:
					pathHandler.Rectangle (ctx, cm.Doubles [di++], cm.Doubles [di++], cm.Doubles [di++], cm.Doubles [di++]);
					break;
				case DrawingCommand.RelCurveTo:
					pathHandler.RelCurveTo (ctx, cm.Doubles [di++], cm.Doubles [di++], cm.Doubles [di++], cm.Doubles [di++], cm.Doubles [di++], cm.Doubles [di++]);
					break;
				case DrawingCommand.RelLineTo:
					pathHandler.RelLineTo (ctx, cm.Doubles [di++], cm.Doubles [di++]);
					break;
				case DrawingCommand.RelMoveTo:
					pathHandler.RelMoveTo (ctx, cm.Doubles [di++], cm.Doubles [di++]);
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
				case DrawingCommand.SetStyles:
					handler.SetStyles (ctx, (StyleSet)cm.Objects [oi++]);
					break;
				}
			}
		}
	}
}

