using System;
using Xwt;

namespace Samples
{
	public class DragDrop: VBox
	{
		public DragDrop ()
		{
			HBox box = new HBox ();
			
			SimpleBox b1 = new SimpleBox (30);
			box.PackStart (b1, BoxMode.None);
			
			Button b2 = new Button ("Drop here");
			box.PackEnd (b2, BoxMode.None);
			
			b1.ButtonPressed += delegate {
				var d = b1.CreateDragOperation ();
				d.Data.AddValue ("Hola");
				d.Start ();
			};
			
			b2.SetDragDropTarget (TransferDataType.Text, TransferDataType.Uri);
			PackStart (box);
			
			b2.DragDrop += HandleB2DragDrop;
		//	b2.DragOver += HandleB2DragOver;
		}

		void HandleB2DragOver (object sender, DragOverEventArgs e)
		{
			Console.WriteLine ("Drag over");
			e.AllowedAction = DragDropAction.Copy | DragDropAction.Move | DragDropAction.Link;
		}

		void HandleB2DragDrop (object sender, DragEventArgs e)
		{
			Console.WriteLine ("Dropped!");
			Console.WriteLine ("Text: " + e.Data.GetValue (TransferDataType.Text));
			Console.WriteLine ("Uris:");
			foreach (var u in e.Data.Uris)
				Console.WriteLine ("u:" + u);
			e.Success = true;
		}
	}
}

