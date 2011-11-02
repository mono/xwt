using System;
using Xwt;
using Xwt.Drawing;

namespace Samples
{
	public class Boxes: VBox
	{
		public Boxes ()
		{
			HBox box1 = new HBox ();
			
			VBox box2 = new VBox ();
			box2.PackStart (new SimpleBox (30), BoxMode.None);
			box2.PackStart (new SimpleBox (30), BoxMode.None);
			box2.PackStart (new SimpleBox (30), BoxMode.FillAndExpand);
			
			box1.PackStart (box2,BoxMode.FillAndExpand);
			box1.PackStart (new SimpleBox (30),BoxMode.None);
			box1.PackStart (new SimpleBox (30),BoxMode.Expand);
			PackStart (box1, BoxMode.None);
		}
	}
	
	class SimpleBox: Canvas
	{
		double coreSize;
		double margin = 1;
		
		public SimpleBox (double coreSize)
		{
			this.coreSize = coreSize;
			MinSize = new Size (coreSize + margin*2, coreSize + margin*2);
		}
		
		protected override void OnDraw (Context ctx)
		{
			ctx.SetColor (new Color (0.5, 0.5, 0.5));
			ctx.Rectangle (Bounds);
			ctx.Fill ();
			ctx.SetColor (new Color (0.8, 0.8, 0.8));
			ctx.Rectangle (Rectangle.Inflate (Bounds,-margin, -margin)); 
			ctx.Fill ();
			ctx.SetColor (new Color (0.5, 0.5, 1));
			ctx.Rectangle (Bounds.Width/2 - coreSize/2, Bounds.Height/2 - coreSize/2, coreSize, coreSize);
			ctx.Fill ();
		}
	}
}

