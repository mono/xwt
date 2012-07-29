using System;

namespace Xwt.Backends
{
	public interface IForeignPlugBackend : IWidgetBackend
	{
		Xwt.Widget Embedded { get; set; }
	}
}

