// 
// WindowFrameBackend.cs
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
using System.Windows.Data;
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
			this.clip = Rectangle.Zero;
			OnRender (this, EventArgs.Empty);
		}

		public void QueueDraw (Rectangle rect)
		{
			this.clip = rect;
			OnRender (this, EventArgs.Empty);
			this.clip = Rectangle.Zero;
		}

		public void AddChild (IWidgetBackend widget, Rectangle bounds)
		{
			UIElement element = widget.NativeWidget as UIElement;
			if (element == null)
				throw new ArgumentException();

			Canvas.Children.Add (element);
			SetChildBounds (widget, bounds);
		}

		public void SetChildBounds (IWidgetBackend widget, Rectangle bounds)
		{
			UIElement element = widget.NativeWidget as UIElement;
			if (element == null)
				throw new ArgumentException();

			SWC.Canvas.SetTop (element, bounds.Top);
			SWC.Canvas.SetLeft (element, bounds.Left);
			SWC.Canvas.SetRight (element, bounds.Right);
			SWC.Canvas.SetBottom (element, bounds.Bottom);
		}

		public void RemoveChild (IWidgetBackend widget)
		{
			UIElement element = widget.NativeWidget as UIElement;
			if (element == null)
				throw new ArgumentException();

			Canvas.Children.Remove (element);
		}

		private readonly SWC.Image image;
		private WriteableBitmap wbitmap;
		private Bitmap bbitmap;
		private Rectangle clip;

		private double pwidth;
		private double pheight;

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
				CanvasEventSink.OnDraw (new DrawingContext (g));

			int x = 0, y = 0, width = this.wbitmap.PixelWidth, height = this.wbitmap.PixelHeight;
			if (this.clip != Rectangle.Zero) {
				x = (int)this.clip.X;
				y = (int)this.clip.Y;
				width = (int)this.clip.Width;
				height = (int)this.clip.Height;
			}

			this.wbitmap.AddDirtyRect (new Int32Rect (x, y, width, height));
			this.wbitmap.Unlock();
		}
	}
}