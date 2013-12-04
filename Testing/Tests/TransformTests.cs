// 
// TransformTests.cs
//  
// Author:
//       Hywel Thomas <hywel.w.thomas@gmail.com>
// 
// Copyright (c) 2011 Xamarin Inc
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using Xwt;
using Xwt.Drawing;
using NUnit.Framework;

namespace Xwt
{
	/// <summary>
	/// Checks the backend Context transforms against the local Drawing.Matrix operations
	/// (which are tested separately), and access to the Current Transform via GetCTM
	/// </summary>
	/// <remarks>
	/// The backend matrix formats may differ from the Drawing.Matrix one, so a check of
	/// the functional equivalence of the local and backend transformations is important
	/// </remarks>
	[TestFixture]
	public class TransformTests
	{
		// There is only a single context used in all tests,
		// so it MUST be Saved and Restored for each test.

		const double DELTA = 0.000001d;

		ImageBuilder ib = null;
		Context context = null;

		[TestFixtureSetUp]
		public void Init ()
		{
			ib = new ImageBuilder (1, 1);
			context = ib.Context;
			Matrix m1 = Matrix.Identity;
			Matrix m2 = context.GetCTM ();
			CheckMatrix (m1, m2);
		}

		[TestFixtureTearDown]
		public void Cleanup ()
		{
			if (context != null)
				context.Dispose ();
			if (ib != null)
				ib.Dispose ();
		}

		private Context Context {
			get { return context; }
		}

		void CheckMatrix (Matrix expected, Matrix actual)
		{
			Assert.AreEqual (expected.M11, actual.M11, DELTA);
			Assert.AreEqual (expected.M12, actual.M12, DELTA);
			Assert.AreEqual (expected.M21, actual.M21, DELTA);
			Assert.AreEqual (expected.M22, actual.M22, DELTA);
			Assert.AreEqual (expected.OffsetX, actual.OffsetX, DELTA);
			Assert.AreEqual (expected.OffsetY, actual.OffsetY, DELTA);
		}



		[Test]
		public void Translate ()
		{
			Matrix m1, m2;
			Context ctx = Context;
			ctx.Save ();

			// Check with a range of -ve and +ve offsets
			for (double tx = -1; tx <= 1; tx += 0.25) {
				ctx.Save ();
				ctx.Translate (tx, 0);
				m1 = Matrix.Identity;
				m1.Translate (tx, 0);
				m2 = ctx.GetCTM ();
				CheckMatrix (m1, m2);
				ctx.Restore ();
			}
			for (double ty = -1; ty <= 1; ty += 0.25) {
				ctx.Save ();
				ctx.Translate (0, ty);
				m1 = Matrix.Identity;
				m1.Translate (0, ty);
				m2 = ctx.GetCTM ();
				CheckMatrix (m1, m2);
				ctx.Restore ();
			}
			ctx.Restore ();
		}

		[Test]
		public void Scale ()
		{
			Matrix m1, m2;
			Context ctx = Context;
			ctx.Save ();

			// Scaling with a zero scale-factor results in a non-invertible matrix
			// This fails with Cairo, so avoid zero as one of the test values
			for (double scaleX = -1.25; scaleX <= 1.25; scaleX += 0.5) {
				ctx.Save ();
				ctx.Scale (scaleX, 1);
				m1 = Matrix.Identity;
				m1.Scale (scaleX, 1);
				m2 = ctx.GetCTM ();
				CheckMatrix (m1, m2);
				ctx.Restore ();
			}
			for (double scaleY = -1.25; scaleY <= 1.25; scaleY += 0.5) {
				ctx.Save ();
				ctx.Scale (1, scaleY);
				m1 = Matrix.Identity;
				m1.Scale (1, scaleY);
				m2 = ctx.GetCTM ();
				CheckMatrix (m1, m2);
				ctx.Restore ();
			}
			ctx.Restore ();
		}

		[Test]
		public void Rotate ()
		{
			Matrix m1, m2;
			Context ctx = Context;
			ctx.Save ();

			for (double theta = 0; theta <= 360; theta += 30) {
				ctx.Save ();
				ctx.Rotate (theta);
				m1 = Matrix.Identity;
				m1.Rotate (theta);
				m2 = ctx.GetCTM ();
				CheckMatrix (m1, m2);
				ctx.Restore ();
			}
			ctx.Restore ();
		}

		[Test]
		public void RotateAndTranslate ()
		{
			// Transforms are Prepended to the CTM, so they are done in reverse order.
			// At present, this must be stated explicitly in the Matrix calls, but may
			// be worth changing so that Prepend is the default (to match the Backends).

			Matrix m1, m2;
			Context ctx = Context;
			ctx.Save ();

			for (double theta = 30; theta <= 360; theta += 30) {
				ctx.Save ();
				ctx.Translate (100, 0);		// done last
				ctx.Rotate (theta);			// done first (about the origin)

				m1 = Matrix.Identity;
				m1.Translate (100, 0);
				m1.Rotate (theta);
				m2 = ctx.GetCTM ();

				CheckMatrix (m1, m2);
				ctx.Restore ();
			}
			ctx.Restore ();
		}

		[Test]
		public void ModifyCTM ()
		{
			// Checks that Matrix and Context transforms match, and that applying
			// a matrix transform to the Context CTM produces the expected result

			double theta = 30;
			double x = 50;
			double y = 30;

			Context ctx = Context;
			ctx.Save ();

			Matrix m1 = Matrix.Identity;
			Matrix ctm;

			// Apply and compare several Matrix and Context transforms
			ctx.Save ();
			ctx.Scale (2, 3);
			m1.Scale (2, 3);
			ctm = ctx.GetCTM ();
			CheckMatrix (m1, ctm);

			ctx.Translate (x, y);
			m1.Translate (x, y);
			ctm = ctx.GetCTM ();
			CheckMatrix (m1, ctm);

			ctx.Rotate (theta);
			m1.Rotate (theta);
			ctm = ctx.GetCTM ();
			CheckMatrix (m1, ctm);

			// Check ModifyCTM matches combined Matrix Transform
			ctx.Restore ();
			ctx.ModifyCTM (m1);
			ctm = ctx.GetCTM ();
			CheckMatrix (m1, ctm);

			ctx.Restore ();
		}

	}
}

