// 
// MatrixTest.cs
//  
// Authors:
//	Chris Toshok (toshok@ximian.com)
//	Lytico (http://www.limada.org)
//	Hywel Thomas <hywel.w.thomas@gmail.com>
//
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
// Copyright (c) 2013 http://www.limada.org
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
// Remark: this file was refactored from 
// https://github.com/mono/mono/blob/master/mcs/class/WindowsBase/Test/System.Windows.Media/MatrixTest.cs
// by lytico to be used with 
// https://github.com/mono/xwt
//
using System;
using Xwt;
using Xwt.Drawing;
using NUnit.Framework;

namespace Xwt
{
	[TestFixture]
	public class MatrixTest
	{
		const double DELTA = 0.000000001d;

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
		public void TestAccessors ()
		{
			Matrix m = new Matrix (1, 2, 3, 4, 5, 6);
			Assert.AreEqual (1, m.M11);
			Assert.AreEqual (2, m.M12);
			Assert.AreEqual (3, m.M21);
			Assert.AreEqual (4, m.M22);
			Assert.AreEqual (5, m.OffsetX);
			Assert.AreEqual (6, m.OffsetY);
		}

		[Test]
		public void Append ()
		{
			// NB pick test matrices so that Append and Prepend results differ
			var m = new Matrix (1, 2, 3, 4, 5, 6);
			m.Append (new Matrix (6, 5, 4, 3, 2, 1));
			CheckMatrix (new Matrix (14, 11, 34, 27, 56, 44), m);
		}

		[Test]
		public void Equals ()
		{
			var m = new Matrix (1, 2, 3, 4, 5, 6);
			Assert.IsTrue (m.Equals (new Matrix (1, 2, 3, 4, 5, 6)));
			Assert.IsFalse (m.Equals (new Matrix (0, 2, 3, 4, 5, 6)));
			Assert.IsFalse (m.Equals (new Matrix (1, 0, 3, 4, 5, 6)));
			Assert.IsFalse (m.Equals (new Matrix (1, 2, 0, 4, 5, 6)));
			Assert.IsFalse (m.Equals (new Matrix (1, 2, 3, 0, 5, 6)));
			Assert.IsFalse (m.Equals (new Matrix (1, 2, 3, 4, 0, 6)));
			Assert.IsFalse (m.Equals (new Matrix (1, 2, 3, 4, 5, 0)));

			Assert.IsFalse (m.Equals (0));
			Assert.IsTrue (m.Equals ((object)m));
			Assert.IsFalse (m.Equals (null));
			Assert.IsFalse (m == null);
			Assert.IsFalse (null == m);
		}

		[Test]
		public void Determinant ()
		{
			Assert.AreEqual (0, (new Matrix (2, 2, 2, 2, 0, 0)).Determinant);
			Assert.AreEqual (-6, (new Matrix (1, 4, 2, 2, 0, 0)).Determinant);
			Assert.AreEqual (1, (new Matrix (1, 0, 0, 1, 0, 0)).Determinant);
			Assert.AreEqual (1, (new Matrix (1, 0, 0, 1, 5, 5)).Determinant);
			Assert.AreEqual (-1, (new Matrix (0, 1, 1, 0, 5, 5)).Determinant);
		}

		[Test]
		public void HasInverse ()
		{
			/* same matrices as in Determinant() */
			Assert.IsFalse ((new Matrix (2, 2, 2, 2, 0, 0)).HasInverse);
			Assert.IsTrue ((new Matrix (1, 4, 2, 2, 0, 0)).HasInverse);
			Assert.IsTrue ((new Matrix (1, 0, 0, 1, 0, 0)).HasInverse);
			Assert.IsTrue ((new Matrix (1, 0, 0, 1, 5, 5)).HasInverse);
			Assert.IsTrue ((new Matrix (0, 1, 1, 0, 5, 5)).HasInverse);
		}

		[Test]
		public void IsIdentity ()
		{
			Assert.IsTrue (Matrix.Identity.IsIdentity);
			Assert.IsFalse ((new Matrix (1, 0, 0, 1, 5, 5)).IsIdentity);
			Assert.IsFalse ((new Matrix (5, 5, 5, 5, 5, 5)).IsIdentity);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		// "Transform is not invertible."
        public void InvertException1 ()
		{
			var m = new Matrix (2, 2, 2, 2, 0, 0);
			m.Invert ();
		}

		[Test]
		public void Invert ()
		{
			var m = new Matrix (1, 0, 0, 1, 0, 0);
			m.Invert ();
			CheckMatrix (new Matrix (1, 0, 0, 1, 0, 0), m);

			m = new Matrix (1, 0, 0, 1, 5, 5);
			m.Invert ();
			CheckMatrix (new Matrix (1, 0, 0, 1, -5, -5), m);

			m = new Matrix (1, 0, 0, 2, 5, 5);
			m.Invert ();
			CheckMatrix (new Matrix (1, 0, 0, 0.5, -5, -2.5), m);

			m = new Matrix (0, 2, 4, 0, 5, 5);
			m.Invert ();
			CheckMatrix (new Matrix (0, 0.25, 0.5, 0, -2.5, -1.25), m);
		}

		[Test]
		public void Identity ()
		{
			CheckMatrix (new Matrix (1, 0, 0, 1, 0, 0), Matrix.Identity);
		}

		[Test]
		public void Multiply ()
		{
			CheckMatrix (new Matrix (5, 0, 0, 5, 10, 10),
                     Matrix.Multiply (new Matrix (1, 0, 0, 1, 2, 2),
                              new Matrix (5, 0, 0, 5, 0, 0)));

			CheckMatrix (new Matrix (0, 0, 0, 0, 10, 10),
                     Matrix.Multiply (new Matrix (1, 0, 0, 1, 0, 0),
                              new Matrix (0, 0, 0, 0, 10, 10)));
		}

		[Test]
		public void Prepend ()
		{
			// NB pick matrices so that Append and Prepend give different results
			var m = new Matrix (1, 2, 3, 4, 5, 6);
			m.Prepend (new Matrix (6, 5, 4, 3, 2, 1));
			CheckMatrix (new Matrix (21, 32, 13, 20, 10, 14), m);
		}

		[Test]
		public void Rotate ()
		{
			var m = new Matrix (1, 2, 3, 4, 5, 6);
			m.Rotate (33);

			CheckMatrix (new Matrix (2.47258767299051,
                         3.85589727595096,
                         1.97137266882125,
                         2.26540420175164,
                         5, 6), m);
		}

		[Test]
		public void RotateAppend ()
		{
			var m = Matrix.Identity;
			m.RotateAppend (45);
			CheckMatrix (new Matrix (0.707106781186548,
                         0.707106781186547,
                         -0.707106781186547,
                         0.707106781186548, 0, 0), m);

			m = new Matrix (1, 2, 3, 4, 5, 6);
			m.RotateAppend (33);
			CheckMatrix (new Matrix (-0.25060750208463,
                         2.22198017090588,
                         0.337455563776164,
                         4.98859937682678,
                         0.925518629636958,
                         7.75521858274768), m);
		}

		[Test]
		public void RotateAt ()
		{
			var m = new Matrix (1, 2, 3, 4, 5, 6);
			m.RotateAt (33, 5, 5);

			CheckMatrix (new Matrix (2.47258767299051,
                         3.85589727595096,
                         1.97137266882125,
                         2.26540420175164,
                         2.78019829094125,
                         5.39349261148701), m);
		}

		[Test]
		public void RotateAtAppend ()
		{
			var m = new Matrix (1, 2, 3, 4, 5, 6);
			m.RotateAtAppend (33, 5, 5);

			CheckMatrix (new Matrix (-0.25060750208463,
                         2.22198017090588,
                         0.337455563776164,
                         4.98859937682678,
                         4.45536096498497,
                         5.83867056794542), m);
		}

		[Test]
		public void RotateQuadrants ()
		{
			var matrix = new Matrix ();
			
			Action<double, double, double, double, double> prove = (angle, m11, m12, m21, m22) => {
				matrix.SetIdentity ();
				matrix.RotateAppend (angle);
				CheckMatrix (new Matrix (m11, m12, m21, m22, 0, 0), matrix);
			};
			prove (90, 0, 1, -1, 0);
			prove (-90, 0, -1, 1, 0);
			prove (180, -1, 0, 0, -1);
			prove (-180, -1, 0, 0, -1);
			prove (270, 0, -1, 1, 0);
			prove (-270, 0, 1, -1, 0);
			prove (0, 1, 0, 0, 1);
			prove (360, 1, 0, 0, 1);
			prove (-360, 1, 0, 0, 1);
			prove (360 + 90, 0, 1, -1, 0);
			
		}

		[Test]
		public void Scale ()
		{
			var m = Matrix.Identity;
			m.Scale (5, 6);
			CheckMatrix (new Matrix (5, 0, 0, 6, 0, 0), m);

			m = new Matrix (1, 2, 2, 1, 3, 3);
			m.Scale (5, 5);
			CheckMatrix (new Matrix (5, 10, 10, 5, 3, 3), m);
		}

		[Test]
		public void ScaleAppend ()
		{
			var m = Matrix.Identity;
			m.ScaleAppend (5, 6);
			CheckMatrix (new Matrix (5, 0, 0, 6, 0, 0), m);

			m = new Matrix (1, 2, 2, 1, 3, 3);
			m.ScaleAppend (5, 5);
			CheckMatrix (new Matrix (5, 10, 10, 5, 15, 15), m);
		}

		[Test]
		public void ScaleAt ()
		{
			var m = new Matrix (1, 0, 0, 1, 2, 2);
			m.ScaleAt (2, 2, 0, 0);
			CheckMatrix (new Matrix (2, 0, 0, 2, 2, 2), m);

			m = new Matrix (1, 0, 0, 1, 2, 2);
			m.ScaleAt (2, 2, 4, 4);
			CheckMatrix (new Matrix (2, 0, 0, 2, -2, -2), m);

			m = new Matrix (1, 0, 0, 1, 2, 2);
			m.ScaleAt (2, 2, 2, 2);
			CheckMatrix (new Matrix (2, 0, 0, 2, 0, 0), m);
		}

		[Test]
		public void ScaleAtAppend ()
		{
			var m = new Matrix (1, 0, 0, 1, 2, 2);
			m.ScaleAtAppend (2, 2, 0, 0);
			CheckMatrix (new Matrix (2, 0, 0, 2, 4, 4), m);

			m = new Matrix (1, 0, 0, 1, 2, 2);
			m.ScaleAtAppend (2, 2, 4, 4);
			CheckMatrix (new Matrix (2, 0, 0, 2, 0, 0), m);

			m = new Matrix (1, 0, 0, 1, 2, 2);
			m.ScaleAtAppend (2, 2, 2, 2);
			CheckMatrix (new Matrix (2, 0, 0, 2, 2, 2), m);
		}

		[Test]
		public void SetIdentity ()
		{
			var m = new Matrix (5, 5, 5, 5, 5, 5);
			m.SetIdentity ();
			CheckMatrix (Matrix.Identity, m);
		}

		[Test]
		public void Skew ()
		{
			var m = Matrix.Identity;
			m.Skew (10, 15);
			CheckMatrix (new Matrix (1,
                         0.267949192431123,
                         0.176326980708465,
                         1, 0, 0), m);

			m = new Matrix (1, 2, 2, 1, 3, 3);
			m.Skew (10, 15);
			CheckMatrix (new Matrix (1.53589838486225,
                         2.26794919243112,
                         2.17632698070847,
                         1.35265396141693,
                         3, 3), m);
		}

		[Test]
		public void SkewAppend ()
		{
			var m = Matrix.Identity;
			m.SkewAppend (10, 15);
			CheckMatrix (new Matrix (1,
                         0.267949192431123,
                         0.176326980708465,
                         1, 0, 0), m);

			m = new Matrix (1, 2, 2, 1, 3, 3);
			m.SkewAppend (10, 15);
			CheckMatrix (new Matrix (1.35265396141693,
                         2.26794919243112,
                         2.17632698070847,
                         1.53589838486225,
                         3.52898094212539,
                         3.80384757729337), m);
		}

		[Test]
		public void ToStringTest ()
		{
			var m = new Matrix (1, 2, 3, 4, 5, 6);
			Assert.AreEqual ("Matrix{M11=1 M12=2 M21=3 M22=4 OffsetX=5 OffsetY=6}", m.ToString ());
			m = Matrix.Identity;
			Assert.AreEqual ("Matrix=Identity", m.ToString ());
		}

		[Test]
		public void PointTransform ()
		{
			var m = new Matrix (2, 0, 0, 2, 4, 4);

			var p = new Point (5, 6);

			Assert.AreEqual (new Point (14, 16), m.Transform (p));

			var ps = new Point[10];
			for (int i = 0; i < ps.Length; i++)
				ps [i] = new Point (3 * i, 2 * i);

			m.Transform (ps);

			for (int i = 0; i < ps.Length; i++)
				Assert.AreEqual (m.Transform (new Point (3 * i, 2 * i)), ps [i]);
		}

		[Test]
		public void VectorTransform ()
		{
			var m = new Matrix (2, 0, 0, 2, 4, 4);

			var p = new Point (5, 6);

			Assert.AreEqual (new Point (10, 12), m.TransformVector (p));

			var ps = new Point[10];
			for (int i = 0; i < ps.Length; i++)
				ps [i] = new Point (3 * i, 2 * i);

			m.TransformVector (ps);

			for (int i = 0; i < ps.Length; i++)
				Assert.AreEqual (m.TransformVector (new Point (3 * i, 2 * i)), ps [i]);
		}

		[Test]
		public void TranslateAppend ()
		{
			var m = new Matrix (1, 0, 0, 1, 0, 0);
			m.TranslateAppend (5, 5);
			CheckMatrix (new Matrix (1, 0, 0, 1, 5, 5), m);

			m = new Matrix (2, 0, 0, 2, 0, 0);
			m.TranslateAppend (5, 5);
			CheckMatrix (new Matrix (2, 0, 0, 2, 5, 5), m);
		}

		[Test]
		public void Translate ()
		{
			var m = new Matrix (1, 0, 0, 1, 0, 0);
			m.Translate (5, 5);
			CheckMatrix (new Matrix (1, 0, 0, 1, 5, 5), m);

			m = new Matrix (2, 0, 0, 2, 0, 0);
			m.Translate (5, 5);
			CheckMatrix (new Matrix (2, 0, 0, 2, 10, 10), m);
		}

	}
}

