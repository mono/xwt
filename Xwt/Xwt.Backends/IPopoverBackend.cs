using System;
using Xwt;
using Xwt.Drawing;

namespace Xwt.Backends
{
	public interface IPopoverBackend : IDisposable
	{
		event EventHandler Closed;
		void Init (IWindowFrameBackend parent, IWidgetBackend child, Popover.Position arrowPosition);
		void Run (Point reference);
	}
}

