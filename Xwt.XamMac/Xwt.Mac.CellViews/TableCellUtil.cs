using System;
using AppKit;
using Foundation;
using Xwt.Accessibility;
using Xwt.Backends;

namespace Xwt.Mac
{
	static class TableCellUtil
	{
		public static void ApplyAcessibilityProperties (ICellRenderer cell, ICellViewFrontend frontend)
		{
			if (frontend.AccessibleFields == null)
				return;

			INSAccessibility accessibleCell = cell as INSAccessibility;
			if (accessibleCell == null)
				return;

			ICellDataSource source = cell.CellContainer;
			var label = GetValue (source, frontend.AccessibleFields.Label);
			if (label != null)
				accessibleCell.AccessibilityLabel = (string)label;
			var identifier = GetValue (source, frontend.AccessibleFields.Identifier);
			if (identifier != null)
				accessibleCell.AccessibilityIdentifier = (string)identifier;
			var description = GetValue (source, frontend.AccessibleFields.Description);
			if (description != null)
				accessibleCell.AccessibilityHelp = (string)description;
			var title = GetValue (source, frontend.AccessibleFields.Title);
			if (title != null)
				accessibleCell.AccessibilityTitle = (string)title;
			var isAccessible = GetValue (source, frontend.AccessibleFields.IsAccessible);
			if (isAccessible != null)
				accessibleCell.AccessibilityElement = (bool)isAccessible;
			var value = GetValue (source, frontend.AccessibleFields.Value);
			if (value != null)
				accessibleCell.AccessibilityValue = new NSString ((string)value);
			var uri = GetValue (source, frontend.AccessibleFields.Uri);
			if (uri != null)
				accessibleCell.AccessibilityUrl = new NSUrl (((Uri)uri).AbsoluteUri);
			var bounds = GetValue (source, frontend.AccessibleFields.Bounds);
			if (bounds != null)
				accessibleCell.AccessibilityFrame = ((Rectangle)bounds).ToCGRect ();
			
			var role = GetValue (source, frontend.AccessibleFields.Role);
			if (role != null) {
				if ((Role)role == Role.Filler) {
					accessibleCell.AccessibilityElement = false;
				} else {
					accessibleCell.AccessibilityElement = true;
					accessibleCell.AccessibilityRole = ((Role)role).GetMacRole ();
					accessibleCell.AccessibilitySubrole = ((Role)role).GetMacSubrole ();
				}
			}
			var roleDescription = GetValue (source, frontend.AccessibleFields.RoleDescription);
			if (roleDescription != null)
				accessibleCell.AccessibilityRoleDescription = (string)roleDescription;
		}

		static object GetValue (ICellDataSource source, IDataField field)
		{
			if (field == null)
				return null;
			return source.GetValue (field);
		}
	}

}

