// 
// Iconeria.cs
//  
// Authors:
//	Lytico (http://www.limada.org)
//
// Copyright (c) 2015 http://www.limada.org
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using Xwt.Drawing;
using System;
using System.Linq;
using System.Linq.Expressions;
using Xwt;

namespace Xwt.Iconerias
{

	public partial class Iconeria
	{

		public virtual Color FillColor { get; set; }

		public virtual Color StrokeColor { get; set; }

		public virtual bool Fill { get; set; }

		public virtual bool Stroke { get; set; }

		public virtual bool StrokeFirst { get; set; }

		public virtual Size DefaultSize { get; set; }

		public virtual void PaintIcon (Context c, double size, double x, double y, Action<Context> icon)
		{
			c.Save ();

			// settings needs to be adjusted from the SVG path values, those works well for FontAwesome
			var border = size / 10;
			c.Translate (border + x, size * .8 + y);
			// c.Translate(border + x, size - border + y);
			var scale = 2500;
			c.Scale (size / scale, -size / scale);

			icon (c);

			c.SetLineWidth (size / 50);

			if (Stroke && StrokeFirst && Fill) {
				c.SetColor (StrokeColor);
				c.StrokePreserve ();
			}

			if (Fill) {
				c.SetColor (FillColor);
				if (Stroke && !StrokeFirst)
					c.FillPreserve ();
				else
					c.Fill ();
			}

			if (Stroke && !StrokeFirst) {
				c.SetColor (StrokeColor);
				c.Stroke ();
			}

			c.Restore ();

		}

		public virtual void ForEach (Action<Action<Context>, string, string> visit)
		{
			foreach (var iconMethod in this.GetType().GetMethods()
                .Where(m => m.IsDefined(typeof(IconAttribute), true))
                .OrderBy(m => m.Name)) {
				var att = iconMethod.GetCustomAttributes (typeof(IconAttribute), true).First ()
                          as IconAttribute;

				var icon = Delegate.CreateDelegate (typeof(Action<Context>), this, iconMethod) as Action<Context>;
				visit (icon, att.Name, att.Id);
			}
		}

		public Image AsImage (Action<Context> icon, double size)
		{
			var ib = new ImageBuilder (size, size);
			this.PaintIcon (ib.Context, size, 0, 0, icon);
			var img = ib.ToBitmap (ImageFormat.ARGB32);
			return img;
		}
	}

}