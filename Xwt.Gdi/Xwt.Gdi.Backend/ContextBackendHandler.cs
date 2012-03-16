// 
// ContextBackendHandler.cs
//  
// Author:
//       Lytico 
// 
// Copyright (c) 2012 Lytico (http://limada.sourceforge.net)
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
using System.Drawing;
using System.Drawing.Drawing2D;
using Xwt.Backends;
using Xwt.Engine;

namespace Xwt.Gdi.Backend {

    public class ContextBackendHandler : IContextBackendHandler {

        public virtual object CreateContext (Widget w) {
            var b = (IGdiGraphicsBackend) WidgetRegistry.GetBackend (w);

            var ctx = new GdiContext ();
            if (b.Graphics != null) {
                ctx.Graphics = b.Graphics;
            } else {
                //ctx.Graphics = System.Drawing.BufferedGraphicsManager.Current.Allocate();
            }
            return ctx;
        }

        public virtual void Save (object backend) {
            var gc = (GdiContext) backend;
            gc.Save ();
        }

        public virtual void Restore (object backend) {
            var gc = (GdiContext) backend;
           gc.Restore ();
        }

        // http://cairographics.org/documentation/cairomm/reference/classCairo_1_1Context.html

        // mono-libgdiplus\src\graphics-cairo.c

        /// <summary>
        /// Adds a circular arc of the given radius to the current path.
        /// The arc is centered at (xc, yc), 
        /// begins at angle1 and proceeds in the direction of increasing angles to end at angle2. 
        /// If angle2 is less than angle1 
        /// it will be progressively increased by 2*M_PI until it is greater than angle1.
        /// If there is a current point, an initial line segment will be added to the path 
        /// to connect the current point to the beginning of the arc. 
        /// If this initial line is undesired, 
        /// it can be avoided by calling begin_new_sub_path() before calling arc().
        /// </summary>
        /// <param name="backend"></param>
        /// <param name="xc"></param>
        /// <param name="yc"></param>
        /// <param name="radius"></param>
        /// <param name="angle1"></param>
        /// <param name="angle2"></param>
        public virtual void Arc (object backend, double xc, double yc, double radius, double angle1, double angle2) {
            var gc = (GdiContext) backend;
            //?? look in mono-libgdiplus 
            gc.Path.AddArc (
                (float) (xc - radius), (float) (yc - radius),
                (float) radius * 2, (float) radius * 2,
                (float) angle1, (float) angle2);
        }

        /// <summary>
        /// Establishes a new clip region by intersecting the current clip region with the current Path 
        /// as it would be filled by fill() and according to the current fill rule.
        /// After clip(), the current path will be cleared from the Context.
        /// The current clip region affects all drawing operations by effectively masking out any changes to the surface 
        /// that are outside the current clip region.
        /// Calling clip() can only make the clip region smaller, never larger. 
        /// But the current clip is part of the graphics state, 
        /// so a temporary restriction of the clip region can be achieved by calling clip() within a save()/restore() pair. 
        /// The only other means of increasing the size of the clip region is reset_clip().
        /// </summary>
        public virtual void Clip (object backend) {
            ClipPreserve (backend);
            var gc = (GdiContext) backend;
            gc.Path.Dispose ();
            gc.Path = new GraphicsPath ();
        }

        public virtual void ClipPreserve (object backend) {
            var gc = (GdiContext) backend;
            gc.Graphics.Clip = new Region (gc.Path);
        }

        public virtual void ResetClip (object backend) {
            var gc = (GdiContext) backend;
            gc.Graphics.ResetClip ();
        }

        public virtual void ClosePath (object backend) {
            var gc = (GdiContext) backend;
            gc.Path.CloseFigure ();
        }

        /// <summary>
        /// Adds a cubic Bezier spline to the path from the current point to position (x3, y3) in user-space coordinates, 
        /// using (x1, y1) and (x2, y2) as the control points. 
        /// </summary>
        public virtual void CurveTo (object backend, double x1, double y1, double x2, double y2, double x3, double y3) {
            var gc = (GdiContext) backend;

            gc.Path.AddBezier (
                (float) gc.Current.X, (float) gc.Current.Y,
                (float) x1, (float) y1,
                (float) x2, (float) y2,
                (float) x3, (float) y3);

        }

        public virtual void Fill (object backend) {
            FillPreserve (backend);
            var gc = (GdiContext) backend;
            gc.Path.Dispose ();
            gc.Path = null;
        }

        public virtual void FillPreserve (object backend) {
            var gc = (GdiContext) backend;
            gc.Transform ();
            gc.Graphics.FillPath (gc.Brush, gc.Path);
        }

        public virtual void LineTo (object backend, double x, double y) {
            var gc = (GdiContext) backend;
            gc.Path.AddLine (gc.Current, new PointF ((float) x, (float) y));
        }

        /// <summary>
        /// If the current subpath is not empty, begin a new subpath.
        /// After this call the current point will be (x, y).
        /// </summary>
        /// <param name="backend"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public virtual void MoveTo (object backend, double x, double y) {
            var gc = (GdiContext) backend;
            gc.Current = new PointF ((float) x, (float) y);
        }

        public virtual void NewPath (object backend) {
            var gc = (GdiContext) backend;
            gc.Path = new GraphicsPath ();
        }

        public virtual void Rectangle (object backend, double x, double y, double width, double height) {
            var gc = (GdiContext) backend;
            gc.Path.AddRectangle (new RectangleF ((float) x, (float) y, (float) width, (float) height));
        }

        public virtual void RelCurveTo (object backend, double dx1, double dy1, double dx2, double dy2, double dx3, double dy3) {
            var gc = (GdiContext) backend;

        }

        /// <summary>
        /// Adds a line to the path from the current point to a point that 
        /// is offset from the current point by (dx, dy) in user space. 
        /// After this call the current point will be offset by (dx, dy).
        /// Given a current point of (x, y), 
        /// rel_line_to(dx, dy) is logically equivalent to line_to(x + dx, y + dy).
        public virtual void RelLineTo (object backend, double dx, double dy) {
            var gc = (GdiContext) backend;
            gc.Path.AddLine (gc.Current,
                new PointF ((float) (gc.Current.X + dx), (float) (gc.Current.Y + dy)));
        }

        /// <summary>
        /// If the current subpath is not empty, begin a new subpath.
        /// After this call the current point will offset by (x, y).
        /// Given a current point of (x, y), 
        /// RelMoveTo(dx, dy) is logically equivalent to MoveTo(x + dx, y + dy).
        /// </summary>
        public virtual void RelMoveTo (object backend, double dx, double dy) {
            var gc = (GdiContext) backend;
            gc.Current = new PointF ((float) (gc.Current.X + dx), (float) (gc.Current.Y + dy));
        }

        public virtual void Stroke (object backend) {
            StrokePreserve (backend);
            var gc = (GdiContext) backend;
            gc.Path.Dispose ();
            gc.Path = null;

        }

        public virtual void StrokePreserve (object backend) {
            var gc = (GdiContext) backend;
            gc.Transform ();
            gc.Graphics.DrawPath (gc.Pen, gc.Path);
        }

        public virtual void SetColor (object backend, Xwt.Drawing.Color color) {
            var gc = (GdiContext) backend;
            gc.Color = GdiConverter.ToGdi (color);
        }

        public virtual void SetLineWidth (object backend, double width) {
            var gc = (GdiContext) backend;
            gc.LineWidth = width;
        }

        public virtual void SetLineDash (object backend, double offset, params double[] pattern) {
            var gc = (GdiContext) backend;
            gc.LineDash = pattern;
        }

        public virtual void SetPattern (object backend, object p) {
            var gc = (GdiContext) backend;
            gc.Pattern = p;
        }

        public virtual void SetFont (object backend, Xwt.Drawing.Font font) {
            var gc = (GdiContext) backend;
            gc.Font = font;
        }

        public virtual void DrawTextLayout (object backend, Xwt.Drawing.TextLayout layout, double x, double y) {
            var context = (GdiContext) backend;
            var tl = (TextLayoutBackend) WidgetRegistry.GetBackend (layout);
            var font = tl.Font.ToGdi ();
            var rect = new System.Drawing.RectangleF ((float) x, (float) y, (float) layout.Width, context.Graphics.ClipBounds.Height);

            context.Graphics.DrawString (tl.Text, font, context.Brush, rect, tl.Format);
        }

        public virtual void DrawImage (object backend, object img, double x, double y, double alpha) {

        }

        public virtual void DrawImage (object backend, object img, double x, double y, double width, double height, double alpha) {

        }

        public virtual void Rotate (object backend, double angle) {
            var gc = (GdiContext) backend;
            gc.Rotate ((float) angle);
        }

        public virtual void Translate (object backend, double tx, double ty) {
            var gc = (GdiContext) backend;
            gc.Translate ((float) tx, (float) ty);
        }

        public virtual void ResetTransform (object backend) {
            var gc = (GdiContext) backend;
            gc.ResetTransform ();
        }

        public virtual void Dispose (object backend) {
            var gc = (GdiContext) backend;
            gc.Dispose ();
        }

    }
}