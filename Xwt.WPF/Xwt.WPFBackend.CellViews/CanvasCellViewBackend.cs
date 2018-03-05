using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Xwt.Backends;

namespace Xwt.WPFBackend
{
    class CanvasCellViewBackend: CellViewBackend, ICanvasCellViewBackend
    {
        public void QueueDraw()
        {
            CurrentElement.InvalidateVisual ();
        }

		public void QueueResize ()
		{
			CurrentElement.InvalidateVisual ();
		}

        public bool IsHighlighted {
            get {
                return false;
            }
        }
    }
}
