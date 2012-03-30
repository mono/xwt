using System.Windows.Controls;

namespace Xwt.WPFBackend
{
	public class ExScrollViewer
		: ScrollViewer, IWpfWidget
	{
		public WidgetBackend Backend
		{
			get;
			set;
		}

		protected override System.Windows.Size MeasureOverride (System.Windows.Size constraint)
		{
			var s = base.MeasureOverride (constraint);
			return Backend.MeasureOverride (constraint, s);
		}
	}
}
