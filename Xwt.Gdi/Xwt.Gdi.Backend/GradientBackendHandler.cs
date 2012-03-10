using System.Drawing;
using System.Drawing.Drawing2D;
using Xwt.Backends;

namespace Xwt.Gdi.Backend {
    /// <summary>
    /// todo
    /// </summary>
    public class GradientBackendHandler : IGradientBackendHandler {
        /// <summary>
        /// todo: make a GradientData-class
        /// </summary>
        public object CreateLinear(double x0, double y0, double x1, double y1) {
            var color1 = Color.Black;
            var color2 = Color.Black;
            return new LinearGradientBrush(new PointF((float)x0, (float)y0), new PointF((float)x1, (float)y1), color1,color2);
        }

        /// <summary>
        /// todo: make a GradientData-class and set a color
        /// </summary>
        public void AddColorStop(object backend, double position, Xwt.Drawing.Color color) {
            var g = (LinearGradientBrush)backend;
           
        }
    }
}