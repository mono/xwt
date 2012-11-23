//
// CanvasBackend.cs
//
// Author:
//       Eric Maupin <ermau@xamarin.com>
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
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using SWC = System.Windows.Controls;
using Xwt.Backends;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Xwt.WPFBackend
{
	public class CanvasBackend
		: WidgetBackend, ICanvasBackend
	{
		public CanvasBackend ()
		{
			Canvas = new ExCanvas();
			Canvas.Render += OnRender;

			this.image = new SWC.Image();
			SWC.Panel.SetZIndex (this.image, -1);

			Canvas.Children.Add (this.image);
		}

		public void QueueDraw()
		{
			this.fullRedraw = true;

			if (!this.queued) {
			    Toolkit.QueueExitAction (Render);
			    this.queued = true;
			}
		}

		public void QueueDraw (Rectangle rect)
		{
			if (this.fullRedraw)
				return;

			this.dirtyRects.Add (rect.ToInt32Rect());

			if (!this.queued) {
			    Toolkit.QueueExitAction (Render);
			    this.queued = true;
			}
		}

		public void AddChild (IWidgetBackend widget, Rectangle bounds)
		{
			UIElement element = widget.NativeWidget as UIElement;
			if (element == null)
				throw new ArgumentException();

			if (!Canvas.Children.Contains (element))
				Canvas.Children.Add (element);

			SetChildBounds (widget, bounds);
		}

		public void SetChildBounds (IWidgetBackend widget, Rectangle bounds)
		{
			FrameworkElement element = widget.NativeWidget as FrameworkElement;
			if (element == null)
				throw new ArgumentException();

			double hratio = HeightPixelRatio;
			double wratio = WidthPixelRatio;

			SWC.Canvas.SetTop (element, bounds.Top * hratio);
			SWC.Canvas.SetLeft (element, bounds.Left * wratio);
			element.Height = (bounds.Height > 0) ? bounds.Height * hratio : 0;
			element.Width = (bounds.Width > 0) ? bounds.Width * wratio : 0;

			((FrameworkElement) widget.NativeWidget).UpdateLayout();
		}

		public void RemoveChild (IWidgetBackend widget)
		{
			UIElement element = widget.NativeWidget as UIElement;
			if (element == null)
				throw new ArgumentException();

			Canvas.Children.Remove (element);
		}

		private bool queued;
		private bool fullRedraw;

		private readonly SWC.Image image;
		private WriteableBitmap wbitmap;
		private Bitmap bbitmap;
		private readonly List<Int32Rect> dirtyRects = new List<Int32Rect> ();

		private double pwidth = -1;
		private double pheight = -1;

		private ExCanvas Canvas
		{
			get { return (ExCanvas) Widget; }
			set { Widget = value; }
		}

		private ICanvasEventSink CanvasEventSink
		{
			get { return (ICanvasEventSink) EventSink; }
		}

		private void OnRender (object sender, EventArgs e)
		{
			Render();
		}

		private void Render()
		{
			this.queued = false;

			if (!Widget.IsVisible)
				return;

			if (Canvas.ActualHeight != this.pheight || Canvas.ActualWidth != this.pwidth)
			{
				double heightRatio = HeightPixelRatio;
				double widthRatio = WidthPixelRatio;

				int nheight = (int) Math.Round (Canvas.ActualHeight * heightRatio) + 1;
				int nwidth = (int) Math.Round (Canvas.ActualWidth * widthRatio) + 1;

				if (nheight == 0 || nwidth == 0)
					return;

				this.wbitmap = new WriteableBitmap (nwidth, nheight, widthRatio * 96, heightRatio * 96, PixelFormats.Pbgra32, null);
				this.bbitmap = new Bitmap (nwidth, nheight, this.wbitmap.BackBufferStride, PixelFormat.Format32bppPArgb, this.wbitmap.BackBuffer);
				this.image.Source = this.wbitmap;

				this.pheight = Canvas.ActualHeight;
				this.pwidth = Canvas.ActualWidth;
			}

			this.wbitmap.Lock();

			using (Graphics g = Graphics.FromImage (this.bbitmap))
				CanvasEventSink.OnDraw (new DrawingContext (g), new Rectangle (0, 0, this.wbitmap.PixelWidth, this.wbitmap.PixelHeight));

			if (this.fullRedraw || this.dirtyRects.Count == 0) {
				this.wbitmap.AddDirtyRect (new Int32Rect (0, 0, this.wbitmap.PixelWidth, this.wbitmap.PixelHeight));
				this.fullRedraw = false;
			} else {
				for (int i = 0; i < this.dirtyRects.Count; ++i) {
					Int32Rect r = this.dirtyRects [i];
					if (r.X >= this.wbitmap.PixelWidth || r.Y >= this.wbitmap.PixelHeight)
						continue;

					if (r.X < 0)
						r.X = 0;
					if (r.Y < 0)
						r.Y = 0;
					if (r.X + r.Width > this.wbitmap.PixelWidth)
						r.Width = this.wbitmap.PixelWidth - r.X;
					if (r.Y + r.Height > this.wbitmap.PixelHeight)
						r.Height = this.wbitmap.PixelHeight - r.Y;

					this.wbitmap.AddDirtyRect (r);
				}
			}

			this.dirtyRects.Clear();
			this.wbitmap.Unlock();
		}
	}
}