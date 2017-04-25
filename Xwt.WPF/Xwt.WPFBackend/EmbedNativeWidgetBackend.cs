using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xwt.Backends;
using Xwt.WPFBackend.Utilities;

namespace Xwt.WPFBackend
{
	class EmbedNativeWidgetBackend: WidgetBackend, IEmbeddedWidgetBackend
	{
		public void SetContent(object nativeWidget, bool reparent)
		{
			if (nativeWidget is FrameworkElement)
				Widget = (FrameworkElement)nativeWidget;
		}
	}
}
