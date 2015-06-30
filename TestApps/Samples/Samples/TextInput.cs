using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xwt;
using Xwt.Drawing;

namespace Samples
{
	public class TextInput : VBox
	{
		public class TextEditor : Canvas
		{
			string text;

			public TextEditor ()
			{
				this.TextInput += HandleTextInput;
				this.ButtonPressed += HandleButtonPressed;

				CanGetFocus = true;
				WidthRequest = 300;
				HeightRequest = 300;
			}

			void HandleButtonPressed (object sender, ButtonEventArgs e)
			{
				SetFocus();
			}

			void HandleTextInput (object sender, TextInputEventArgs e)
			{
				text += e.Text;
				e.Handled = true;

				QueueDraw ();
			}

			protected override void OnDraw (Context ctx, Rectangle dirtyRect)
			{
				base.OnDraw (ctx, dirtyRect);

				ctx.Rectangle (0, 0, Size.Width, Size.Height);
				ctx.SetColor (Colors.LightGray);
				ctx.Fill ();

				ctx.SetColor (Colors.Black);
				using (var layout = new TextLayout (this))
				{
					layout.Text = text;
					ctx.DrawTextLayout (layout, new Point (0, 20));
				}
			}
		}

		public TextInput ()
		{
			PackStart (new Label ("Click the widget below and start typing text"));
			PackStart (new TextEditor ());
		}
	}
}
