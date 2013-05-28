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
			box2.PackStart (new SimpleBox (30));
			box2.PackStart (new SimpleBox (30));
			box2.PackStart (new SimpleBox (30), true);
			
			box1.PackStart (box2, true);
			box1.PackStart (new SimpleBox (30));
			box1.PackStart (new SimpleBox (30), expand:true, fill:false);
			PackStart (box1);
			
			HBox box3 = new HBox ();
			box3.PackEnd (new SimpleBox (30));
			box3.PackStart (new SimpleBox (20) {Color = new Color (1, 0.5, 0.5)});
			box3.PackEnd (new SimpleBox (40));
			box3.PackStart (new SimpleBox (10) {Color = new Color (1, 0.5, 0.5)});
			box3.PackEnd (new SimpleBox (30));
			box3.PackStart (new SimpleBox (10) {Color = new Color (1, 0.5, 0.5)}, true);
			PackStart (box3);
			
			HBox box4 = new HBox ();
			Button b = new Button ("Click me");
			b.Clicked += delegate {
				b.Label = "Button has grown";
			};
			box4.PackStart (new SimpleBox (30), true);
			box4.PackStart (b);
			box4.PackStart (new SimpleBox (30), true);
			PackStart (box4);
			
			HBox box5 = new HBox ();
			Button b2 = new Button ("Hide / Show");
			box5.PackStart (new SimpleBox (30), true);
			var hsb = new SimpleBox (20);
			box5.PackStart (hsb);
			box5.PackStart (b2);
			box5.PackStart (new SimpleBox (30), true);
			b2.Clicked += delegate {
				hsb.Visible = !hsb.Visible;
			};
			PackStart (box5);
			
			HBox box6 = new HBox ();
			for (int n=0; n<15; n++) {
				var w = new Label ("TestLabel" + n);
				w.WidthRequest = 10;
				box6.PackStart (w);
			}
			PackStart (box6);

			VBox box7 = new VBox () { Spacing = 0 };
			box7.BackgroundColor = Colors.White;

			box7.PackStart (new Label("Hi there") { Margin = new WidgetSpacing (10, 10, 0, 0) });
			box7.PackStart (new SpecialWidget() { MarginTop = 15 });
			box7.PackStart (new SpecialWidget() { Margin = 5 });
			PackStart (box7);
		}
	}

	class SpecialWidget: Widget
	{
		public SpecialWidget ()
		{
			VBox bl = new VBox () { Spacing = 0 };
			bl.BackgroundColor = Colors.Gray;
			bl.PackStart (new Label ("Hi there"));
			Content = bl;
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

