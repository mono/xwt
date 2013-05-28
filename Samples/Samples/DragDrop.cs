using System;
using Xwt;
using Xwt.Drawing;

namespace Samples
{
	public class DragDrop: VBox
	{
		Button b2;
		public DragDrop ()
		{
			HBox box = new HBox ();
			
			SimpleBox b1 = new SimpleBox (30);
			box.PackStart (b1);
			
			b2 = new Button ("Drop here");
			box.PackEnd (b2);
			
			b1.ButtonPressed += delegate {
				var d = b1.CreateDragOperation ();
				d.Data.AddValue ("Hola");
				var img = Image.FromResource (GetType(), "class.png");
				d.SetDragImage (img, (int)img.Size.Width, (int)img.Size.Height);
				d.AllowedActions = DragDropAction.All;
				d.Start ();
			};
			
			b2.SetDragDropTarget (TransferDataType.Text, TransferDataType.Uri);
			PackStart (box);
			
			b2.DragDrop += HandleB2DragDrop;
			b2.DragOver += HandleB2DragOver;
		}

		void HandleB2DragOver (object sender, DragOverEventArgs e)
		{
			if (e.Action == DragDropAction.All)
				e.AllowedAction = DragDropAction.Move;
			else
				e.AllowedAction = e.Action;
		}

		void HandleB2DragDrop (object sender, DragEventArgs e)
		{
			Console.WriteLine ("Dropped! " + e.Action);
			Console.WriteLine ("Text: " + e.Data.GetValue (TransferDataType.Text));
			Console.WriteLine ("Uris:");
			foreach (var u in e.Data.Uris)
				Console.WriteLine ("u:" + u);
			e.Success = true;
			b2.Label = "Dropped!";
		}
	}
}

