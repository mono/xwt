using System;
using NUnit.Framework;

namespace Xwt
{
	public class GtkIntegrationTests : XwtTest
	{
		[Test]
		public void NativeWindowFrameHasCorrectScreenBounds ()
		{
			var nativeWindow = new Gtk.Window ("Foo");
			nativeWindow.Resize (450, 320);
			nativeWindow.Move (13, 50);
			nativeWindow.ShowAll ();

			WaitForEvents ();

			var window = Toolkit.CurrentEngine.WrapWindow (nativeWindow);
			var bounds = window.ScreenBounds;
			Assert.AreEqual (450, bounds.Width);
			Assert.AreEqual (320, bounds.Height);
			Assert.AreEqual (13, bounds.X);
			Assert.AreEqual (50, bounds.Y);

			nativeWindow.Move (30, 100);
			WaitForEvents ();
			bounds = window.ScreenBounds;
			Assert.AreEqual (30, bounds.X);
			Assert.AreEqual (100, bounds.Y);
			Assert.AreEqual (450, bounds.Width);
			Assert.AreEqual (320, bounds.Height);

			nativeWindow.Resize (100, 100);
			WaitForEvents ();
			bounds = window.ScreenBounds;
			Assert.AreEqual (30, bounds.X);
			Assert.AreEqual (100, bounds.Y);
			Assert.AreEqual (100, bounds.Width);
			Assert.AreEqual (100, bounds.Height);
		}
	}
}

