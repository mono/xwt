// 
// DrawingPerformance.cs
//  
// Author:
//       Lytico (http://limada.sourceforge.net)
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2012 Xamarin Inc
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
using System.Diagnostics;

namespace Samples
{
	public class DrawingPerformance: VBox
	{
		public class Painter:Drawings
		{
			public Painter ()
			{
				Stopwatch = new Stopwatch ();
				Iterations = 200;
			}
			
			protected override void OnDraw (Context ctx)
			{
				base.OnDraw (ctx);
				
				Stopwatch.Start ();
				
				SpeedTest (ctx, 5, 5);
				
				Stopwatch.Stop ();
				Frames++;
			}
		
			public string Run ()
			{
				int i = 0;
				while (i++<Iterations) {
					Application.Invoke (() => this.QueueDraw ());
					
				}
				var ms = Stopwatch.ElapsedMilliseconds;
				if (ms == 0)
					ms = 1;
				return string.Format ("Time in sec {0:#0.00}\tFrames per sec {1:#0.00}\tFrames {2}",
				                     ms / 1000d,
				                     Frames / (ms / 1000d),
				                     Frames);
				
				Stopwatch.Reset ();
			}
			
			public int Frames { get; set; }

			public int Iterations { get; set; }

			public Stopwatch Stopwatch { get; set; }

			public virtual void SpeedTest (Xwt.Drawing.Context ctx, double sx, double sy)
			{
				ctx.Save ();
            
				ctx.Translate (sx, sy);
		
				var n = 1000;
				var ll = 80;
				var p = new Point (0, 0);
				for (double i = 1; i<n; i++) {
					
					ctx.MoveTo (p.X, p.Y);
					
					ctx.SetColor (new Color (i / n, i / n, i / n));
					
					ctx.LineTo (p.X + ll, p.Y + ll);
					ctx.Stroke ();
					
					if (p.Y + ll > this.Bounds.Bottom) {
						p.Y = 0;
						p.X += ll + 5;
						
					} else {
						p.Y++;
					}
				
				}
			
				ctx.Restore ();
			}		
		}
		
		public DrawingPerformance ()
		{
			var drawing = new Painter ();
			var b1 = new Button ("start");
			b1.Clicked += delegate {
				var m = drawing.Run ();
				b1.Label = m;
				System.Console.WriteLine (m);
			};
			
			PackEnd (b1);
			PackStart (drawing, BoxMode.FillAndExpand);
			
		}
		
		

	}
}

