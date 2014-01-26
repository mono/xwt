using System;
using Xwt;
using MonoDevelop.Components.Chart;
using Xwt.Drawing;

namespace Samples
{
	public class ChartSample: Xwt.VBox
	{
		public ChartSample ()
		{
			BasicChart chart = new BasicChart ();
			chart.AllowSelection = true;
			chart.AddAxis (new IntegerAxis (true), AxisPosition.Left);
			chart.AddAxis (new IntegerAxis (true), AxisPosition.Bottom);
			
			Serie s = new Serie ("Some Data");
			s.Color = new Color (0,1,0);
			s.AddData (10, 10);
			s.AddData (20, 11);
			s.AddData (30, 15);
			s.AddData (40, 9);
			chart.AddSerie (s);
			s = new Serie ("Other Data");
			s.Color = new Color (0,0,1);
			s.AddData (10, 20);
			s.AddData (20, 19);
			s.AddData (30, 25);
			s.AddData (40, 26);
			chart.AddSerie (s);
			chart.SetAutoScale (AxisDimension.X, true, true);
			chart.SetAutoScale (AxisDimension.Y, true, true);
			
			PackStart (chart, true);
		}
	}
}

