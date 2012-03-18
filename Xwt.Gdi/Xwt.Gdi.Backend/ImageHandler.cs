using System.Drawing;
using Xwt.Backends;

namespace Xwt.Gdi.Backend {
    public class ImageHandler : ImageBackendHandler {

        public override object LoadFromStream (System.IO.Stream stream) {
            return Image.FromStream(stream);
        }

        public override object LoadFromIcon (string id, IconSize size) {
            throw new System.NotImplementedException ();
        }

        public override Size GetSize (object handle) {
            throw new System.NotImplementedException ();
        }

        public override object Resize (object handle, double width, double height) {
            throw new System.NotImplementedException ();
        }

        public override object Copy (object handle) {
            throw new System.NotImplementedException ();
        }

        public override void CopyArea (object srcHandle, int srcX, int srcY, int width, int height, object destHandle, int destX, int destY) {
            throw new System.NotImplementedException ();
        }

        public override object Crop (object handle, int srcX, int srcY, int width, int height) {
            throw new System.NotImplementedException ();
        }

        public override object ChangeOpacity (object backend, double opacity) {
            throw new System.NotImplementedException ();
        }
    }
}