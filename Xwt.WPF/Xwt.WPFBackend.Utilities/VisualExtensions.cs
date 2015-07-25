
using System.Windows;
using System.Windows.Media;

namespace Xwt.WPF.Xwt.WPFBackend.Utilities
{
	internal static class VisualExtensions
	{
		public static System.Windows.Point PointToScreenDpiAware(this Visual visual, System.Windows.Point point)
		{
			point = visual.PointToScreen(point);

			PresentationSource source = PresentationSource.FromVisual(visual);

			double scaleFactorX = source.CompositionTarget.TransformToDevice.M11;
			double scaleFactorY = source.CompositionTarget.TransformToDevice.M22;

			return new System.Windows.Point(point.X / scaleFactorX, point.Y / scaleFactorY);
		}
	}
}
