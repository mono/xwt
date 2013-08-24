using System;
using System.IO;

using Xwt.Backends;

namespace Xwt.Formats
{
	public class PlainTextFormat : TextFormat
	{
		public override void Parse (Stream input, IRichTextBuffer buffer)
		{
			using (var reader = new StreamReader (input)) {
				buffer.EmitStartParagraph (0);
				buffer.EmitText (reader.ReadToEnd (), RichTextInlineStyle.Normal);
				buffer.EmitEndParagraph ();
			}
		}
	}
}

