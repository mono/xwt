using System;
using Xwt;
using Xwt.Drawing;

namespace Samples
{
	public class NotebookSample: VBox
	{
		public NotebookSample ()
		{
			Notebook nb = new Notebook ();
			nb.Add (new Label ("First tab content"), "First Tab");
			nb.Add (new MyWidget (), "Second Tab");
			PackStart (nb, BoxMode.FillAndExpand);
		}
	}
	
	class MyWidget: Canvas
	{
		protected override void OnDraw (Context ctx)
		{
			ctx.SetLineWidth (5);
			ctx.SetColor (new Color (1.0f, 0f, 0.5f));
			ctx.Rectangle (5, 5, 200, 100);
			ctx.FillPreserve ();
			ctx.SetColor (new Color (0f, 0f, 1f));
			ctx.Stroke ();
		}
	}
}

