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
			
			box1.PackStart (box2, BoxMode.FillAndExpand);
			box1.PackStart (new SimpleBox (30), BoxMode.None);
			box1.PackStart (new SimpleBox (30), BoxMode.Expand);
			PackStart (box1, BoxMode.None);
			
			HBox box3 = new HBox ();
			box3.PackEnd (new SimpleBox (30));
			box3.PackStart (new SimpleBox (20) {Color = new Color (1, 0.5, 0.5)});
			box3.PackEnd (new SimpleBox (40));
			box3.PackStart (new SimpleBox (10) {Color = new Color (1, 0.5, 0.5)});
			box3.PackEnd (new SimpleBox (30));
			box3.PackStart (new SimpleBox (10) {Color = new Color (1, 0.5, 0.5)}, BoxMode.FillAndExpand);
			PackStart (box3);
			
			HBox box4 = new HBox ();
			Button b = new Button ("Click me");
			b.Clicked += delegate {
				b.Label = "Button has grown";
			};
			box4.PackStart (new SimpleBox (30), BoxMode.FillAndExpand);
			box4.PackStart (b);
			box4.PackStart (new SimpleBox (30), BoxMode.FillAndExpand);
			PackStart (box4);
			
			HBox box5 = new HBox ();
			Button b2 = new Button ("Hide / Show");
			box5.PackStart (new SimpleBox (30), BoxMode.FillAndExpand);
			var hsb = new SimpleBox (20);
			box5.PackStart (hsb, BoxMode.None);
			box5.PackStart (b2);
			box5.PackStart (new SimpleBox (30), BoxMode.FillAndExpand);
			b2.Clicked += delegate {
				hsb.Visible = !hsb.Visible;
			};
			PackStart (box5);
			
			HBox box6 = new HBox ();
			for (int n=0; n<15; n++) {
				var w = new Label ("TestLabel" + n);
				w.MinWidth = 10;
				box6.PackStart (w);
			}
			PackStart (box6);
		}
	}
	
	class SimpleBox: Canvas
	{
		Size coreSize;
		double margin = 1;
		bool highlight;
		
		public Color Color { get; set; }
		
		public SimpleBox (double coreSize)
		{
			Color = new Color (0.5, 0.5, 1);
			this.coreSize = new Size (coreSize, coreSize);
			MinWidth = MinHeight = coreSize + margin * 2;
		}
		
		public SimpleBox (double coreWidth, double coreHeight)
		{
			Color = new Color (0.5, 0.5, 1);
			this.coreSize = new Size (coreWidth, coreHeight);
			MinWidth = coreSize.Width + margin * 2;
			MinHeight = coreSize.Height + margin * 2;
		}
		
		protected override void OnMouseEntered (EventArgs args)
		{
			base.OnMouseEntered (args);
			highlight = true;
			QueueDraw ();
		}
		
		protected override void OnMouseExited (EventArgs args)
		{
			base.OnMouseExited (args);
			QueueDraw ();
			highlight = false;
		}
		
		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			ctx.SetColor (new Color (0.5, 0.5, 0.5));
			ctx.Rectangle (Bounds);
			ctx.Fill ();
			ctx.SetColor (new Color (0.8, 0.8, 0.8));
			ctx.Rectangle (Bounds.Inflate (-margin, -margin)); 
			ctx.Fill ();
			ctx.SetColor (highlight ? Color.BlendWith (Xwt.Drawing.Colors.White, 0.5) : Color);
			ctx.Rectangle (Bounds.Width / 2 - coreSize.Width / 2, Bounds.Height / 2 - coreSize.Height / 2, coreSize.Width, coreSize.Height);
			ctx.Fill ();
		}
	}
}

