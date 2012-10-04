using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xwt;

namespace Samples
{
	class ReliefFrameSample : VBox
	{
		public ReliefFrameSample ()
		{
			var box = new VBox ();
			box.PackStart (new ReliefFrame (new Button ("Hello")));
			box.PackStart (new ReliefFrame (new Button ("World")));
			PackStart (box);
		}
	}
}
