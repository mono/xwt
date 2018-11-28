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
			INSAccessibility accessibleCell = cell as INSAccessibility;
			if (accessibleCell == null)
				return;

			ICellDataSource source = cell.CellContainer;
			if (frontend.AccessibleFields.Label != null)
				accessibleCell.AccessibilityLabel = GetValue (source, frontend.AccessibleFields.Label);
			if (frontend.AccessibleFields.Identifier != null)
				accessibleCell.AccessibilityIdentifier = GetValue (source, frontend.AccessibleFields.Identifier);
			if (frontend.AccessibleFields.Description != null)
				accessibleCell.AccessibilityHelp = GetValue (source, frontend.AccessibleFields.Description);
			if (frontend.AccessibleFields.Title != null)
				accessibleCell.AccessibilityTitle = GetValue (source, frontend.AccessibleFields.Title);
			if (frontend.AccessibleFields.IsAccessible != null)
				accessibleCell.AccessibilityElement = GetValue (source, frontend.AccessibleFields.IsAccessible);
			if (frontend.AccessibleFields.Value != null)
				accessibleCell.AccessibilityValue = new NSString (GetValue (source, frontend.AccessibleFields.Value));
			if (frontend.AccessibleFields.Uri != null)
				accessibleCell.AccessibilityUrl = new NSUrl (GetValue (source, frontend.AccessibleFields.Uri).AbsoluteUri);
			if (frontend.AccessibleFields.Bounds != null)
				accessibleCell.AccessibilityFrame = GetValue (source, frontend.AccessibleFields.Bounds).ToCGRect ();
			
			if (frontend.AccessibleFields.Role != null) {
				var role = GetValue (source, frontend.AccessibleFields.Role);
				if (role == Role.Filler) {
					accessibleCell.AccessibilityElement = false;
				} else {
					accessibleCell.AccessibilityElement = true;
					accessibleCell.AccessibilityRole = role.GetMacRole ();
					accessibleCell.AccessibilitySubrole = role.GetMacSubrole ();
				}
			}
			if (frontend.AccessibleFields.RoleDescription != null)
				accessibleCell.AccessibilityRoleDescription = GetValue (source, frontend.AccessibleFields.RoleDescription);
		}


		static T GetValue<T> (ICellDataSource source, IDataField<T> field, T defaultValue = default(T))
		{
			var result = source.GetValue (field);
			return result == null || result == DBNull.Value ? defaultValue : (T) result;
		}
	}

}

