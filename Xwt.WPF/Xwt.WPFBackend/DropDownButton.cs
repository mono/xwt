using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using SWC = System.Windows.Controls;

namespace Xwt.WPFBackend
{
	public class DropDownButton
		: SWC.Primitives.ToggleButton
	{
		public event EventHandler<MenuOpeningEventArgs> MenuOpening;

		protected override void OnToggle()
		{
			base.OnToggle();

			if (IsChecked.HasValue && IsChecked.Value) {
				var args = new MenuOpeningEventArgs ();

				var opening = MenuOpening;
				if (opening != null)
					opening (this, args);

				var menu = args.ContextMenu;
				if (menu == null) {
					IsChecked = false;
					return;
				}

				menu.Closed += OnMenuClosed;

				menu.PlacementTarget = this;
				menu.Placement = PlacementMode.Bottom;
				menu.IsOpen = true;
			}
		}

		private void OnMenuClosed (object sender, RoutedEventArgs e)
		{
			var menu = sender as SWC.ContextMenu;
			if (menu != null)
				menu.Closed -= OnMenuClosed;

			IsChecked = false;
		}

		public class MenuOpeningEventArgs
			: EventArgs
		{
			public SWC.ContextMenu ContextMenu
			{
				get;
				set;
			}
		}
	}
}