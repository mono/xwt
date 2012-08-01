using System;
using Xwt.Backends;

namespace Xwt
{
	/* This widget can be used to embed Xwt widgets using a different backend than the main application
	 */
	public class ForeignPlug : Widget
	{
		public ForeignPlug ()
		{
		}

		IForeignPlugBackend Backend {
			get {
				return (IForeignPlugBackend)BackendHost.Backend;
			}
		}

		public Xwt.Widget Embedded {
			get {
				return Backend.Embedded;
			}
			set {
				Backend.Embedded = value;
			}
		}
	}
}

