// 
// ScrollWindowSample.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2011 Xamarin Inc
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
using Xwt;
using Xwt.Drawing;

namespace Samples
{
	public class ScrollWindowSample: VBox
	{
		public ScrollWindowSample ()
		{
			ScrollView v1 = new ScrollView ();
			VBox b1 = new VBox ();
			for (int n=0; n<30; n++)
				b1.PackStart (new Label ("Line " + n));
			Button u = new Button ("Click to remove");
			u.Clicked += delegate {
				b1.Remove (u);
			};
			b1.PackStart (u);
			
			v1.Content = b1;
			v1.VerticalScrollPolicy = ScrollPolicy.Always;
			v1.BorderVisible = false;
			PackStart (v1, fill:true, expand:true);
			
			ScrollView v2 = new ScrollView ();
			VBox b2 = new VBox ();
			for (int n=0; n<10; n++)
				b2.PackStart (new Label ("Line " + n));
			v2.Content = b2;
			v2.VerticalScrollPolicy = ScrollPolicy.Never;
			PackStart (v2, fill:true, expand:true);
			
			ScrollView v3 = new ScrollView ();
			VBox b3 = new VBox ();
			Button b = new Button ("Click to add items");
			b.Clicked += delegate {
				for (int n=0; n<10; n++)
					b3.PackStart (new Label ("Line " + n));
			};
			b3.PackStart (b);
			v3.Content = b3;
			v3.VerticalScrollPolicy = ScrollPolicy.Automatic;
			PackStart (v3, fill:true, expand:true);
			
			ScrollView v4 = new ScrollView ();
			PackStart (v4, fill:true, expand:true);
			var sb = new ScrollableCanvas ();
			v4.Content = sb;
			v4.VerticalScrollPolicy = ScrollPolicy.Always;
		}
	}
	
	class ScrollableCanvas: Canvas
	{
		ScrollAdjustment hscroll;
		ScrollAdjustment vscroll;
		int imageSize = 500;
		
		public ScrollableCanvas ()
		{
			MinWidth = 100;
			MinHeight = 100;
		}
		
		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			ctx.Save ();
			ctx.Translate (-hscroll.Value, -vscroll.Value);
			ctx.Rectangle (new Rectangle (0, 0, imageSize, imageSize));
			ctx.SetColor (Xwt.Drawing.Colors.White);
			ctx.Fill ();
			ctx.Arc (imageSize / 2, imageSize / 2, imageSize / 2 - 20, 0, 360);
			ctx.SetColor (new Color (0,0,1));
			ctx.Fill ();
			ctx.Restore ();

			ctx.Rectangle (0, 0, Bounds.Width, 30);
			ctx.SetColor (new Color (1, 0, 0, 0.5));
			ctx.Fill ();
		}
		
		protected override bool SupportsCustomScrolling {
			get {
				return true;
			}
		}
		
		protected override void SetScrollAdjustments (ScrollAdjustment horizontal, ScrollAdjustment vertical)
		{
			hscroll = horizontal;
			vscroll = vertical;
			
			hscroll.UpperValue = imageSize;
			hscroll.PageIncrement = Bounds.Width;
			hscroll.PageSize = Bounds.Width;
			hscroll.ValueChanged += delegate {
				QueueDraw ();
			};
			
			vscroll.UpperValue = imageSize;
			vscroll.PageIncrement = Bounds.Height;
			vscroll.PageSize = Bounds.Height;
			vscroll.ValueChanged += delegate {
				QueueDraw ();
			};
		}
		
		protected override void OnBoundsChanged ()
		{
			if (vscroll == null)
				return;
			vscroll.PageSize = vscroll.PageIncrement = Bounds.Height;
			hscroll.PageSize = hscroll.PageIncrement = Bounds.Width;
		}
	}
}

