using System;

namespace Xwt.Accessibility
{
	public class AccessibleFields
	{

		public IDataField<string> Label { get; set; }
		public IDataField<string> Identifier { get; set; }
		public IDataField<bool> IsAccessible { get; set; }
		public IDataField<string> Title { get; set; }
		public IDataField<string> Description { get; set; }
		public IDataField<string> Value { get; set; }
		public IDataField<Uri> Uri { get; set; }
		public IDataField<Role> Role { get; set; }
		public IDataField<string> RoleDescription { get; set; }
		public IDataField<Rectangle> Bounds { get; set; }
		
		public AccessibleFields ()
		{

		}
	}
}
