// 
// CanvasWithWidget.cs
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
	public class CanvasWithWidget: VBox
	{
		public CanvasWithWidget ()
		{
			MyCanvas c = new MyCanvas ();
			PackStart (c, BoxMode.FillAndExpand);
		}
	}
	
	class MyCanvas: Canvas
	{
		Rectangle rect = new Rectangle (30, 30, 100, 30);
		
		public MyCanvas ()
		{
			var entry = new TextEntry () { ShowFrame = false };
			AddChild (entry, rect);
			
			var box = new HBox ();
			box.PackStart (new Button ("..."));
			box.PackStart (new TextEntry (), BoxMode.FillAndExpand);
			AddChild (box, new Rectangle (30, 70, 100, 30));
		}
		
		protected override void OnDraw (Xwt.Drawing.Context ctx, Rectangle dirtyRect)
		{
			ctx.Rectangle (0, 0, Bounds.Width, Bounds.Height);
			var g = new LinearGradient (0, 0, Bounds.Width, Bounds.Height);
			g.AddColorStop (0, new Color (1, 0, 0));
			g.AddColorStop (1, new Color (0, 1, 0));
			ctx.Pattern = g;
			ctx.Fill ();
			
			Rectangle r = rect.Inflate (5, 5);
			ctx.Rectangle (r);
			ctx.SetColor (new Color (0,0,1));
			ctx.SetLineWidth (1);
			ctx.Stroke ();
		}
	}
}

