// 
// DrawingContext.cs
//  
// Author:
//       Eric Maupin <ermau@xamarin.com>
//       Lytico (http://limada.sourceforge.net)
// 
// Copyright (c) 2012 Xamarin, Inc.
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
using System.Windows.Media;
using SWM = System.Windows.Media;
using SW = System.Windows;

namespace Xwt.WPFBackend
{
	internal class DrawingContext:IDisposable
	{
		Stack<ContextData> pushes = new Stack<ContextData> ();
		TransformGroup transforms = new TransformGroup ();
		int pushCount;
		bool disposed;
		bool positionSet;

		PathGeometry geometry;
		SWM.Brush patternBrush;
		SWM.SolidColorBrush colorBrush;

		public double ScaleFactor { get; set; }

		public SW.Point LastFigureStart { get; set; }
		public SW.Point EndPoint { get; set; }

		public PathFigure Path { get; private set; }
		public SWM.Pen Pen { get; private set; }

		public SWM.Brush Brush {
			get {
				return patternBrush ?? colorBrush;
			}
		}

		public SWM.SolidColorBrush ColorBrush
		{
			get
			{
				return colorBrush;
			}
		}

		public SWM.Matrix CurrentTransform
		{
			get {
				TransformCollection children = transforms.Children;
				Matrix ctm = Matrix.Identity;
				foreach (Transform t in children) {
					ctm.Prepend (t.Value);
				};
				return ctm;
			}
		}

		class ContextData
		{
			public int PushCount;
			public Color CurrentColor;
			public double Thickness;
			public DashStyle DashStyle;
			public Brush Pattern;
			public int TransformCount;
		}

		public System.Windows.Media.DrawingContext Context { get; private set; }

		internal DrawingContext (System.Windows.Media.DrawingContext dc, double scaleFactor)
		{
			Context = dc;
			ScaleFactor = scaleFactor;
			AllocatePen (Color.FromRgb (0, 0, 0), 1, DashStyles.Solid);
			ResetPath ();
		}

		internal DrawingContext (DrawingContext context)
		{
			Context = context.Context;

			if (context.Pen != null)
				AllocatePen (context.colorBrush.Color, context.Pen.Thickness, context.Pen.DashStyle);

			patternBrush = context.patternBrush;

			geometry = (PathGeometry) context.Geometry.Clone ();
			Path = geometry.Figures[geometry.Figures.Count - 1];
			positionSet = context.positionSet;
		}

		internal DrawingContext()
		{
			ResetPath ();
		}

		public void AppendPath (DrawingContext context)
		{
			foreach (var f in context.Geometry.Figures)
				geometry.Figures.Add (f.Clone ());
			Path = context.geometry.Figures[context.geometry.Figures.Count - 1];
		}

		public void Save ()
		{
			var cd = new ContextData () {
				Thickness = Pen.Thickness,
				CurrentColor = colorBrush.Color,
				PushCount = pushCount,
				Pattern = patternBrush,
				DashStyle = Pen.DashStyle,
				TransformCount = transforms.Children.Count,
			};
			pushes.Push (cd);
			pushCount = 0;
		}

		public void Restore ()
		{
			if (pushes.Count == 0)
				return;

			for (int n = 0; n < pushCount; n++)
				Context.Pop ();

			var cd = pushes.Pop ();
			pushCount = cd.PushCount;

			while (transforms.Children.Count > cd.TransformCount)
				transforms.Children.RemoveAt (transforms.Children.Count - 1);

			AllocatePen (cd.CurrentColor, cd.Thickness, cd.DashStyle);
			patternBrush = cd.Pattern;
		}

		public void NotifyPush ()
		{
			pushCount++;
		}

		public void PushTransform (Transform t)
		{
			Context.PushTransform (t);
			transforms.Children.Add (t);
			pushCount++;
		}

		public void SetColor (Color color)
		{
			patternBrush = null;
			AllocatePen (color, Pen != null ? Pen.Thickness : 1, Pen != null ? Pen.DashStyle : DashStyles.Solid);
		}

		public void SetThickness (double t)
		{
			var ds = Pen.DashStyle;
			if (ds != DashStyles.Solid) {
				var dashes = Pen.DashStyle.Dashes.Clone ();
				for (int n = 0; n < dashes.Count; n++)
					dashes[n] = (dashes[n] * Pen.Thickness) / t;
				var offset = (Pen.DashStyle.Offset * Pen.Thickness) / t;
				ds = new DashStyle (dashes, offset);
			}
			AllocatePen (colorBrush.Color, t, ds);
		}

		internal void SetDash (double offset, double[] pattern)
		{
			DashStyle ds = new DashStyle (pattern.Select (d => d / Pen.Thickness), offset);
			AllocatePen (colorBrush.Color, Pen.Thickness, ds);
		}

		void AllocatePen (Color color, double thickness, DashStyle dashStyle)
		{
			if (colorBrush != null && color == colorBrush.Color) {
				if (Pen.Thickness != thickness || !dashStyle.Equals (Pen.DashStyle))
					Pen = new SWM.Pen (colorBrush, thickness) {
						DashStyle = dashStyle
					};
			}
			else
			{
				colorBrush = new SolidColorBrush (color);
				Pen = new SWM.Pen (colorBrush, thickness) {
					DashStyle = dashStyle
				};
			}
			Pen.DashCap = PenLineCap.Flat;
		}

		public void SetPattern (Brush brush)
		{
			patternBrush = brush;
		}

		public void NewFigure (SW.Point p)
		{
			if (Path.Segments.Count > 0) {
				Path = new PathFigure ();
				geometry.Figures.Add (Path);
			}
			LastFigureStart = p;
			EndPoint = p;
			Path.StartPoint = p;
			positionSet = true;
		}

		public void ConnectToLastFigure (SW.Point p, bool stroke)
		{
			if (EndPoint != p) {
				var pathIsOpen = Path.Segments.Count != 0 || geometry.Figures.Count > 1 || positionSet;
				if (pathIsOpen) {
				//	LastFigureStart = p;
					Path.Segments.Add (new LineSegment (p, stroke));
				}
				else
					NewFigure (p);
			}
			if (!stroke)
				LastFigureStart = p;
		}

		public void ResetPath ()
		{
			Path = new PathFigure ();
			geometry = new PathGeometry ();
			geometry.Figures.Add (Path);
			Path.StartPoint = EndPoint = new SW.Point (0, 0);
			positionSet = false;
		}

		public PathGeometry Geometry {
			get { return geometry; }
		}

		public SW.Point GetStartPoint ()
		{
			if (Path.Segments.Count == 0)
				return Path.StartPoint;
			var last = Path.Segments[0];
			if (last is LineSegment)
				return ((LineSegment)last).Point;
			if (last is PolyLineSegment) {
				var p = ((PolyLineSegment)last).Points;
				return p[0];
			}
			return Path.StartPoint;
		}

		public void Dispose ()
		{
			if (!disposed) {
				Context.Close ();
				disposed = true;
			}
		}
	}
}
