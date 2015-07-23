using MonoMac.Foundation;

namespace Xwt.Mac
{
	public static class NSStringAttributeKey
	{
		public static NSString ForegroundColor
		{
			get { return NSAttributedString.ForegroundColorAttributeName; }
		}

		public static NSString UnderlineStyle
		{
			get { return NSAttributedString.UnderlineStyleAttributeName; }
		}

		public static NSString BackgroundColor
		{
			get { return NSAttributedString.BackgroundColorAttributeName; }
		}

		public static NSString Obliqueness
		{
			get { return NSAttributedString.ObliquenessAttributeName; }
		}

		public static NSString Link
		{
			get { return NSAttributedString.LinkAttributeName; }
		}

		public static NSString StrikethroughStyle
		{
			get { return NSAttributedString.StrikethroughStyleAttributeName; }
		}

		public static NSString Font
		{
			get { return NSAttributedString.FontAttributeName; }
		}
	}
}

