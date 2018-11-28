using System;

namespace Xwt.Accessibility
{
	public class AccessibleFields
	{

		public DataField<string> Label { get; set; }
		public DataField<string> Identifier { get; set; }
		public DataField<bool> IsAccessible { get; set; }
		public DataField<string> Title { get; set; }
		public DataField<string> Description { get; set; }
		public DataField<string> Value { get; set; }
		public DataField<Uri> Uri { get; set; }
		public DataField<Role> Role { get; set; }
		public DataField<string> RoleDescription { get; set; }
		public DataField<Rectangle> Bounds { get; set; }
		
		internal AccessibleFields ()
		{

		}
	}
}
