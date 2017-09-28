using System;
using Xwt.Drawing;

namespace Xwt.GtkBackend
{
	public static class Conversion
	{
		public static Gtk.IconSize ToGtkValue (Xwt.IconSize size)
		{
			switch (size) {
			case IconSize.Small:
				return Gtk.IconSize.Menu;
			case IconSize.Medium:
				return Gtk.IconSize.Button;
			case IconSize.Large:
				return Gtk.IconSize.Dialog;
			}
			return Gtk.IconSize.Dialog;
		}

		public static Gdk.Color ToGtkValue (this Xwt.Drawing.Color color)
		{
			return new Gdk.Color ((byte)(color.Red * 255), (byte)(color.Green * 255), (byte)(color.Blue * 255));
		}

		public static Color ToXwtValue (this Gdk.Color color)
		{
			return new Color ((double)color.Red / (double)ushort.MaxValue, (double)color.Green / (double)ushort.MaxValue, (double)color.Blue / (double)ushort.MaxValue);
		}

		#if XWT_GTK3
		public static Gdk.RGBA ToGtkRgbaValue (this Xwt.Drawing.Color color)
		{
			var rgba = new Gdk.RGBA ();
			rgba.Red = color.Red;
			rgba.Green = color.Green;
			rgba.Blue = color.Blue;
			rgba.Alpha = color.Alpha;
			return rgba;
		}

		public static Color ToXwtValue (this Gdk.RGBA color)
		{
			return new Color (color.Red, color.Green, color.Blue, color.Alpha);
		}
		#endif

		public static Pango.EllipsizeMode ToGtkValue (this EllipsizeMode value)
		{
			switch (value) {
			case Xwt.EllipsizeMode.None: return Pango.EllipsizeMode.None;
			case Xwt.EllipsizeMode.Start: return Pango.EllipsizeMode.Start;
			case Xwt.EllipsizeMode.Middle: return Pango.EllipsizeMode.Middle;
			case Xwt.EllipsizeMode.End: return Pango.EllipsizeMode.End;
			}
			throw new NotSupportedException ();
		}

		public static EllipsizeMode ToXwtValue (this Pango.EllipsizeMode value)
		{
			switch (value) {
			case Pango.EllipsizeMode.None: return Xwt.EllipsizeMode.None;
			case Pango.EllipsizeMode.Start: return Xwt.EllipsizeMode.Start;
			case Pango.EllipsizeMode.Middle: return Xwt.EllipsizeMode.Middle;
			case Pango.EllipsizeMode.End: return Xwt.EllipsizeMode.End;
			}
			throw new NotSupportedException ();
		}

		public static ScrollPolicy ToXwtValue (this Gtk.PolicyType p)
		{
			switch (p) {
			case Gtk.PolicyType.Always:
				return ScrollPolicy.Always;
			case Gtk.PolicyType.Automatic:
				return ScrollPolicy.Automatic;
			case Gtk.PolicyType.Never:
				return ScrollPolicy.Never;
			}
			throw new InvalidOperationException ("Invalid policy value:" + p);
		}

		public static Gtk.PolicyType ToGtkValue (this ScrollPolicy p)
		{
			switch (p) {
			case ScrollPolicy.Always:
				return Gtk.PolicyType.Always;
			case ScrollPolicy.Automatic:
				return Gtk.PolicyType.Automatic;
			case ScrollPolicy.Never:
				return Gtk.PolicyType.Never;
			}
			throw new InvalidOperationException ("Invalid policy value:" + p);
		}

		public static ScrollDirection ToXwtValue(this Gdk.ScrollDirection d)
		{
			switch(d) {
			case Gdk.ScrollDirection.Up:
				return Xwt.ScrollDirection.Up;
			case Gdk.ScrollDirection.Down:
				return Xwt.ScrollDirection.Down;
			case Gdk.ScrollDirection.Left:
				return Xwt.ScrollDirection.Left;
			case Gdk.ScrollDirection.Right:
				return Xwt.ScrollDirection.Right;
			}
			throw new InvalidOperationException("Invalid mouse scroll direction value: " + d);
		}

		public static Gdk.ScrollDirection ToGtkValue(this ScrollDirection d)
		{
			switch (d) {
			case ScrollDirection.Up:
				return Gdk.ScrollDirection.Up;
			case ScrollDirection.Down:
				return Gdk.ScrollDirection.Down;
			case ScrollDirection.Left:
				return Gdk.ScrollDirection.Left;
			case ScrollDirection.Right:
				return Gdk.ScrollDirection.Right;
			}
			throw new InvalidOperationException("Invalid mouse scroll direction value: " + d);
		}

		public static ModifierKeys ToXwtValue (this Gdk.ModifierType s)
		{
			ModifierKeys m = ModifierKeys.None;
			if ((s & Gdk.ModifierType.ShiftMask) != 0)
				m |= ModifierKeys.Shift;
			if ((s & Gdk.ModifierType.ControlMask) != 0)
				m |= ModifierKeys.Control;
			if ((s & Gdk.ModifierType.Mod1Mask) != 0)
				m |= ModifierKeys.Alt;
			if ((s & Gdk.ModifierType.Mod2Mask) != 0)
				m |= ModifierKeys.Command;
			return m;
		}

		public static Gtk.Requisition ToGtkRequisition (this Size size)
		{
			var req = new Gtk.Requisition ();
			req.Height = (int)size.Height;
			req.Width = (int)size.Width;
			return req;
		}

		public static Gtk.TreeViewGridLines ToGtkValue (this GridLines value)
		{
			switch (value)
			{
				case GridLines.Both:
					return Gtk.TreeViewGridLines.Both;
				case GridLines.Horizontal:
					return Gtk.TreeViewGridLines.Horizontal;
				case GridLines.Vertical:
					return Gtk.TreeViewGridLines.Vertical;
				case GridLines.None:
					return Gtk.TreeViewGridLines.None;
			}
			throw new InvalidOperationException("Invalid GridLines value: " + value);
		}

		public static GridLines ToXwtValue (this Gtk.TreeViewGridLines value)
		{
			switch (value)
			{
				case Gtk.TreeViewGridLines.Both:
					return GridLines.Both;
				case Gtk.TreeViewGridLines.Horizontal:
					return GridLines.Horizontal;
				case Gtk.TreeViewGridLines.Vertical:
					return GridLines.Vertical;
				case Gtk.TreeViewGridLines.None:
					return GridLines.None;
			}
			throw new InvalidOperationException("Invalid TreeViewGridLines value: " + value);
		}

		public static float ToGtkAlignment(this Alignment alignment)
		{
			switch(alignment) {
				case Alignment.Start: return 0.0f;
				case Alignment.Center: return 0.5f;
				case Alignment.End: return 1.0f;
			}
			throw new InvalidOperationException("Invalid alignment value: " + alignment);
		}

		public static Pango.Alignment ToPangoAlignment (this Alignment alignment)
		{
			switch(alignment) {
				case Alignment.Start: return Pango.Alignment.Left;
				case Alignment.Center: return Pango.Alignment.Center;
				case Alignment.End: return Pango.Alignment.Right;
			}
			throw new InvalidOperationException("Invalid alignment value: " + alignment);
		}

		public static Gtk.ResponseType ToResponseType (this Xwt.Command command)
		{
			if (command.Id == Command.Ok.Id)
				return Gtk.ResponseType.Ok;
			if (command.Id == Command.Cancel.Id)
				return Gtk.ResponseType.Cancel;
			if (command.Id == Command.Yes.Id)
				return Gtk.ResponseType.Yes;
			if (command.Id == Command.No.Id)
				return Gtk.ResponseType.No;
			if (command.Id == Command.Close.Id)
				return Gtk.ResponseType.Close;
			if (command.Id == Command.Delete.Id)
				return Gtk.ResponseType.DeleteEvent;
			if (command.Id == Command.Apply.Id)
				return Gtk.ResponseType.Accept;
			if (command.Id == Command.Stop.Id)
				return Gtk.ResponseType.Reject;
			return Gtk.ResponseType.None;
		}

		public static Atk.Role ToAtkRole (this Accessibility.Role role)
		{
			switch (role) {
			case Accessibility.Role.Button:
				return Atk.Role.PushButton;
			case Accessibility.Role.ButtonClose:
				return Atk.Role.PushButton;
			case Accessibility.Role.ButtonMinimize:
				return Atk.Role.PushButton;
			case Accessibility.Role.ButtonMaximize:
				return Atk.Role.PushButton;
			case Accessibility.Role.ButtonFullscreen:
				return Atk.Role.PushButton;
			case Accessibility.Role.Calendar:
				return Atk.Role.Calendar;
			case Accessibility.Role.Cell:
				return Atk.Role.TableCell;
			case Accessibility.Role.CheckBox:
				return Atk.Role.CheckBox;
			case Accessibility.Role.ColorChooser:
				return Atk.Role.ColorChooser;
			case Accessibility.Role.Column:
				return Atk.Role.TableColumnHeader;
			case Accessibility.Role.ComboBox:
				return Atk.Role.ComboBox;
			case Accessibility.Role.Custom:
				return Atk.Role.Unknown;
			case Accessibility.Role.Disclosure:
				return Atk.Role.Arrow;
			case Accessibility.Role.Filler:
				return Atk.Role.Filler;
			case Accessibility.Role.Group:
				return Atk.Role.Panel;
			case Accessibility.Role.Image:
				return Atk.Role.Image;
			case Accessibility.Role.Label:
				return Atk.Role.Label;
			case Accessibility.Role.LevelIndicator:
				return (Atk.Role)101; // ATK_ROLE_LEVEL_BAR
			case Accessibility.Role.Link:
				return Atk.Role.Link;
			case Accessibility.Role.List:
				return Atk.Role.List;
			case Accessibility.Role.Menu:
				return Atk.Role.Menu;
			case Accessibility.Role.MenuBar:
				return Atk.Role.MenuBar;
			case Accessibility.Role.MenuBarItem:
				return Atk.Role.MenuItem; // no difference between item and bar item
			case Accessibility.Role.MenuButton:
				return Atk.Role.PushButton;
			case Accessibility.Role.MenuItem:
				return Atk.Role.MenuItem;
			case Accessibility.Role.MenuItemCheckBox:
				return Atk.Role.CheckMenuItem;
			case Accessibility.Role.MenuItemRadio:
				return Atk.Role.RadioMenuItem;
			case Accessibility.Role.Notebook:
				return Atk.Role.PageTabList;
			case Accessibility.Role.NotebookTab:
				return Atk.Role.PageTab;
			case Accessibility.Role.Paned:
				return Atk.Role.SplitPane;
			case Accessibility.Role.PanedSplitter:
				return (Atk.Role)int.MaxValue; // no matching role > Atk.Role.LastDefined
			case Accessibility.Role.Popup:
				return Atk.Role.PopupMenu;
			case Accessibility.Role.ProgressBar:
				return Atk.Role.ProgressBar;
			case Accessibility.Role.RadioButton:
				return Atk.Role.RadioButton;
			case Accessibility.Role.RadioGroup:
				return (Atk.Role)97; // ATK_ROLE_GROUPING
			case Accessibility.Role.Row:
				return Atk.Role.TableRowHeader;
			case Accessibility.Role.ScrollBar:
				return Atk.Role.ScrollBar;
			case Accessibility.Role.ScrollView:
				return Atk.Role.ScrollPane;
			case Accessibility.Role.Separator:
				return Atk.Role.Separator;
			case Accessibility.Role.Slider:
				return Atk.Role.Slider;
			case Accessibility.Role.SpinButton:
				return Atk.Role.SpinButton;
			case Accessibility.Role.Table:
				return Atk.Role.Table;
			case Accessibility.Role.TextArea:
				return Atk.Role.Text;
			case Accessibility.Role.TextEntry:
				return Atk.Role.Entry;
			case Accessibility.Role.TextEntryPassword:
				return Atk.Role.PasswordText;
			case Accessibility.Role.TextEntrySearch:
				return Atk.Role.Entry;
			case Accessibility.Role.ToggleButton:
				return Atk.Role.ToggleButton;
			case Accessibility.Role.ToolBar:
				return Atk.Role.ToolBar;
			case Accessibility.Role.ToolTip:
				return Atk.Role.ToolTip;
			case Accessibility.Role.Tree:
				return Atk.Role.TreeTable;
			case Accessibility.Role.None:
				return (Atk.Role)int.MaxValue; // no matching role > Atk.Role.LastDefined
			default:
				throw new ArgumentOutOfRangeException ();
			}
		}

		public static Accessibility.Role ToXwtRole (this Atk.Role role)
		{
			switch (role) {
			//case Atk.Role.Invalid:
			//	break;
			//case Atk.Role.AccelLabel:
			//	break;
			//case Atk.Role.Alert:
			//	break;
			//case Atk.Role.Animation:
			//	break;
			case Atk.Role.Arrow:
				return Accessibility.Role.Disclosure;
			case Atk.Role.Calendar:
				return Accessibility.Role.Calendar;
			//case Atk.Role.Canvas:
			//	break;
			case Atk.Role.CheckBox:
				return Accessibility.Role.CheckBox;
			case Atk.Role.CheckMenuItem:
				return Accessibility.Role.MenuItemCheckBox;
			case Atk.Role.ColorChooser:
				return Accessibility.Role.ColorChooser;
			case Atk.Role.ColumnHeader:
				return Accessibility.Role.Column;
			case Atk.Role.ComboBox:
				return Accessibility.Role.ComboBox;
			case Atk.Role.DateEditor:
				return Accessibility.Role.Calendar;
			//case Atk.Role.DesktopIcon:
			//	break;
			//case Atk.Role.DesktopFrame:
			//	break;
			//case Atk.Role.Dial:
			//	break;
			//case Atk.Role.Dialog:
			//	break;
			//case Atk.Role.DirectoryPane:
			//	break;
			//case Atk.Role.DrawingArea:
			//	break;
			//case Atk.Role.FileChooser:
			//	break;
			case Atk.Role.Filler:
				return Accessibility.Role.Filler;
			//case Atk.Role.FontChooser:
			//	break;
			//case Atk.Role.Frame:
			//	break;
			//case Atk.Role.GlassPane:
			//	break;
			//case Atk.Role.HtmlContainer:
			//	break;
			//case Atk.Role.Icon:
			//	break;
			case Atk.Role.Image:
				return Accessibility.Role.Image;
			//case Atk.Role.InternalFrame:
			//	break;
			case Atk.Role.Label:
				return Accessibility.Role.Label;
			//case Atk.Role.LayeredPane:
			//	break;
			case Atk.Role.List:
				return Accessibility.Role.List;
			case Atk.Role.ListItem:
				return Accessibility.Role.Cell;
			case Atk.Role.Menu:
				return Accessibility.Role.Menu;
			case Atk.Role.MenuBar:
				return Accessibility.Role.MenuBar;
			case Atk.Role.MenuItem:
				return Accessibility.Role.MenuItem;
			//case Atk.Role.OptionPane:
			//	break;
			case Atk.Role.PageTab:
				return Accessibility.Role.NotebookTab;
			case Atk.Role.PageTabList:
				return Accessibility.Role.Notebook;
			case Atk.Role.Panel:
				return Accessibility.Role.Group;
			case Atk.Role.PasswordText:
				return Accessibility.Role.TextEntryPassword;
			case Atk.Role.PopupMenu:
				return Accessibility.Role.Popup;
			case Atk.Role.ProgressBar:
				return Accessibility.Role.ProgressBar;
			case Atk.Role.PushButton:
				return Accessibility.Role.Button;
			case Atk.Role.RadioButton:
				return Accessibility.Role.RadioButton;
			case Atk.Role.RadioMenuItem:
				return Accessibility.Role.MenuItemRadio;
			//case Atk.Role.RootPane:
			//	break;
			case Atk.Role.RowHeader:
				return Accessibility.Role.Row;
			case Atk.Role.ScrollBar:
				return Accessibility.Role.ScrollBar;
			case Atk.Role.ScrollPane:
				return Accessibility.Role.ScrollView;
			case Atk.Role.Separator:
				return Accessibility.Role.Separator;
			case Atk.Role.Slider:
				return Accessibility.Role.Slider;
			case Atk.Role.SplitPane:
				return Accessibility.Role.Paned;
			case Atk.Role.SpinButton:
				return Accessibility.Role.SpinButton;
			//case Atk.Role.Statusbar:
			//	break;
			case Atk.Role.Table:
				return Accessibility.Role.Table;
			case Atk.Role.TableCell:
				return Accessibility.Role.Cell;
			case Atk.Role.TableColumnHeader:
				return Accessibility.Role.Column;
			case Atk.Role.TableRowHeader:
				return Accessibility.Role.Row;
			//case Atk.Role.TearOffMenuItem:
			//	break;
			//case Atk.Role.Terminal:
			//	break;
			case Atk.Role.Text:
				return Accessibility.Role.TextArea;
			case Atk.Role.ToggleButton:
				return Accessibility.Role.ToggleButton;
			case Atk.Role.ToolBar:
				return Accessibility.Role.ToolBar;
			case Atk.Role.ToolTip:
				return Accessibility.Role.ToolTip;
			case Atk.Role.Tree:
				return Accessibility.Role.Table;
			case Atk.Role.TreeTable:
				return Accessibility.Role.Table;
			case Atk.Role.Unknown:
				return Accessibility.Role.Custom;
			//case Atk.Role.Viewport:
			//	break;
			//case Atk.Role.Window:
			//	break;
			//case Atk.Role.Header:
			//	break;
			//case Atk.Role.Footer:
			//	break;
			//case Atk.Role.Paragraph:
			//	break;
			//case Atk.Role.Ruler:
			//	break;
			//case Atk.Role.Application:
			//	break;
			//case Atk.Role.Autocomplete:
			//	break;
			//case Atk.Role.Editbar:
			//	break;
			//case Atk.Role.Embedded:
			//	break;
			case Atk.Role.Entry:
				return Accessibility.Role.TextEntry;
			//case Atk.Role.Chart:
			//	break;
			//case Atk.Role.Caption:
			//	break;
			//case Atk.Role.DocumentFrame:
			//	break;
			//case Atk.Role.Heading:
			//	break;
			//case Atk.Role.Page:
			//	break;
			//case Atk.Role.Section:
			//	break;
			//case Atk.Role.RedundantObject:
			//	break;
			//case Atk.Role.Form:
			//	break;
			case Atk.Role.Link:
				return Accessibility.Role.Link;
			//case Atk.Role.InputMethodWindow:
			//	break;
			//case Atk.Role.LastDefined:
			//	break;
			case (Atk.Role)97: // ATK_ROLE_GROUPING
				return Accessibility.Role.RadioGroup;
			case (Atk.Role)101: // ATK_ROLE_LEVEL_BAR
				return Accessibility.Role.LevelIndicator;
			default:
				return Accessibility.Role.None;
			}
		}
	}
}

