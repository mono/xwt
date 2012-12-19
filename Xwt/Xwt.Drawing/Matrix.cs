// 
// Matrix.cs
//  
// Authors:
//	Chris Toshok (toshok@ximian.com)
//	Lytico (http://www.limada.org)
//
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
// Copyright (c) 2012 http://www.limada.org
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
// https://github.com/mono/mono/blob/master/mcs/class/WindowsBase/System.Windows.Media/Matrix.cs
// by lytico to be used with 
// https://github.com/mono/xwt
// 
using System;
using System.Globalization;

namespace Xwt.Drawing
{
	[Serializable]
	public class Matrix
	{

		double m11;
		double m12;
		double m21;
		double m22;
		double offsetX;
		double offsetY;

		public Matrix (double m11, double m12, double m21, double m22, double offsetX, double offsetY)
		{
			this.m11 = m11;
			this.m12 = m12;
			this.m21 = m21;
			this.m22 = m22;
			this.offsetX = offsetX;
			this.offsetY = offsetY;
		}

		public Matrix (Matrix m) : this(m.M11, m.M12, m.M21, m.M22, m.OffsetX, m.OffsetY)
		{
		}

		public Matrix ()
		{
			SetIdentity ();
		}

		public void Append (Matrix matrix)
		{
			var _m11 = m11 * matrix.M11 + m12 * matrix.M21;
			var _m12 = m11 * matrix.M12 + m12 * matrix.M22;
			var _m21 = m21 * matrix.M11 + m22 * matrix.M21;
			var _m22 = m21 * matrix.M12 + m22 * matrix.M22;

			var _offsetX = offsetX * matrix.M11 + offsetY * matrix.M21 + matrix.OffsetX;
			var _offsetY = offsetX * matrix.M12 + offsetY * matrix.M22 + matrix.OffsetY;

			m11 = _m11;
			m12 = _m12;
			m21 = _m21;
			m22 = _m22;
			offsetX = _offsetX;
			offsetY = _offsetY;
		}

		public bool Equals (Matrix value)
		{
			return (m11 == value.M11 &&
				m12 == value.M12 &&
				m21 == value.M21 &&
				m22 == value.M22 &&
				offsetX == value.OffsetX &&
				offsetY == value.OffsetY);
		}

		public override bool Equals (object o)
		{
			if (!(o is Matrix))
				return false;

			return Equals ((Matrix)o);
		}

		public static bool Equals (Matrix matrix1, Matrix matrix2)
		{
			return matrix1.Equals (matrix2);
		}

		public override int GetHashCode ()
		{
			int h = m11.GetHashCode ();
			h = (h << 5) - h + m21.GetHashCode ();
			h = (h << 5) - h + m12.GetHashCode ();
			h = (h << 5) - h + m22.GetHashCode ();
			h = (h << 5) - h + offsetX.GetHashCode ();
			h = (h << 5) - h + offsetY.GetHashCode ();
			return h;
		}

		public void Invert ()
		{
			if (!HasInverse)
				throw new InvalidOperationException ("Transform is not invertible.");

			var d = Determinant;

			/* 1/(ad-bc)[d -b; -c a] */

			var _m11 = m22;
			var _m12 = -m12;
			var _m21 = -m21;
			var _m22 = m11;

			var _offsetX = m21 * offsetY - m22 * offsetX;
			var _offsetY = m12 * offsetX - m11 * offsetY;

			m11 = _m11 / d;
			m12 = _m12 / d;
			m21 = _m21 / d;
			m22 = _m22 / d;
			offsetX = _offsetX / d;
			offsetY = _offsetY / d;
		}

		public static Matrix Multiply (Matrix trans1, Matrix trans2)
		{
			return trans1 * trans2;
		}

		public static bool operator == (Matrix matrix1, Matrix matrix2)
		{
			if (Matrix.ReferenceEquals (matrix1, matrix2))
				return true;
			if (Matrix.ReferenceEquals (matrix1, null))
				return false; 
			return matrix1.Equals (matrix2);
		}

		public static bool operator != (Matrix matrix1, Matrix matrix2)
		{
			return !(matrix1 == matrix2);
		}

		public static Matrix operator * (Matrix trans1, Matrix trans2)
		{
			var result = new Matrix (trans1);
			result.Append (trans2);
			return result;
		}

		public void Prepend (Matrix matrix)
		{
			var _m11 = matrix.M11 * m11 + matrix.M12 * m21;
			var _m12 = matrix.M11 * m12 + matrix.M12 * m22;
			var _m21 = matrix.M21 * m11 + matrix.M22 * m21;
			var _m22 = matrix.M21 * m12 + matrix.M22 * m22;

			var _offsetX = matrix.OffsetX * m11 + matrix.OffsetY * m21 + offsetX;
			var _offsetY = matrix.OffsetX * m12 + matrix.OffsetY * m22 + offsetY;

			m11 = _m11;
			m12 = _m12;
			m21 = _m21;
			m22 = _m22;
			offsetX = _offsetX;
			offsetY = _offsetY;
		}

		public void Rotate (double angle)
		{
			// R_theta==[costheta -sintheta; sintheta costheta],	
			var theta = angle * pi180;
			var cos = Math.Cos (theta);
			var sin = Math.Sin (theta);
            
			var _m11 = cos * this.m11 + sin * this.m21;
			var _m12 = cos * this.m12 + sin * this.m22;
			var _m21 = cos * this.m21 - sin * this.m11;
			var _m22 = cos * this.m22 - sin * this.m12;

			this.m11 = _m11;
			this.m12 = _m12;
			this.m21 = _m21;
			this.m22 = _m22;
		}

		public void RotateAt (double angle, double centerX, double centerY)
		{
			Translate (-centerX, -centerY);
			Rotate (angle);
			Translate (centerX, centerY);
		}

		public void RotateAtPrepend (double angle, double centerX, double centerY)
		{
			var m = Matrix.Identity;
			m.RotateAt (angle, centerX, centerY);
			Prepend (m);
		}

		public void RotatePrepend (double angle)
		{
			var m = Matrix.Identity;
			m.Rotate (angle);
			Prepend (m);
		}

		public void Scale (double scaleX, double scaleY)
		{
			var scale = new Matrix (scaleX, 0, 0, scaleY, 0, 0);
			Append (scale);
		}

		public void ScaleAt (double scaleX, double scaleY, double centerX, double centerY)
		{
			Translate (-centerX, -centerY);
			Scale (scaleX, scaleY);
			Translate (centerX, centerY);
		}

		public void ScaleAtPrepend (double scaleX, double scaleY, double centerX, double centerY)
		{
			var m = Matrix.Identity;
			m.ScaleAt (scaleX, scaleY, centerX, centerY);
			Prepend (m);
		}

		public void ScalePrepend (double scaleX, double scaleY)
		{
			var m = Matrix.Identity;
			m.Scale (scaleX, scaleY);
			Prepend (m);
		}

		public void SetIdentity ()
		{
			m11 = m22 = 1.0;
			m12 = m21 = 0.0;
			offsetX = offsetY = 0.0;
		}

		const double pi180 = Math.PI / 180;

		public void Skew (double skewX, double skewY)
		{
			var skew_m = new Matrix (1, Math.Tan (skewY * pi180),
                            Math.Tan (skewX * pi180), 1,
                            0, 0);
			Append (skew_m);
		}

		public void SkewPrepend (double skewX, double skewY)
		{
			var m = Matrix.Identity;
			m.Skew (skewX, skewY);
			Prepend (m);
		}

		public override string ToString ()
		{
			if (IsIdentity)
				return this.GetType ().Name + "=Identity";
			else
				return string.Format (this.GetType ().Name + "{{M11={0} M12={1} M21={2} M22={3} OffsetX={4} OffsetY={5}}}",
                              m11.ToString (CultureInfo.InvariantCulture),
                              m12.ToString (CultureInfo.InvariantCulture),
                              m21.ToString (CultureInfo.InvariantCulture),
                              m22.ToString (CultureInfo.InvariantCulture),
                              offsetX.ToString (CultureInfo.InvariantCulture),
                              offsetY.ToString (CultureInfo.InvariantCulture));
		}

		public Point Transform (Point point)
		{
			return new Point (point.X * m11 + point.Y * m21 + offsetX,
                point.X * m12 + point.Y * m22 + offsetY);
		}

		public void Transform (Point[] points)
		{
			double x;
			double y;
			var len = points.Length;
			for (int i = 0; i < len; i++) {
				x = points [i].X;
				y = points [i].Y;
				points [i].X = x * m11 + y * m21 + offsetX;
				points [i].Y = x * m12 + y * m22 + offsetY;
			}
		}

		public Point TransformVector (Point vector)
		{
			return new Point (vector.X * m11 + vector.Y * m21, vector.X * m12 + vector.Y * m22);
		}

		public void TransformVector (Point[] vectors)
		{
			double x;
			double y;
			var len = vectors.Length;
			for (int i = 0; i < len; i++) {
				x = vectors [i].X;
				y = vectors [i].Y;
				vectors [i].X = x * m11 + y * m21;
				vectors [i].Y = x * m12 + y * m22;
			}
		}

		public void Translate (double offsetX, double offsetY)
		{
			this.offsetX += offsetX;
			this.offsetY += offsetY;
		}

		public void TranslatePrepend (double offsetX, double offsetY)
		{
			var m = Matrix.Identity;
			m.Translate (offsetX, offsetY);
			Prepend (m);
		}

		public double Determinant {
			get { return m11 * m22 - m12 * m21; }
		}

		public bool HasInverse {
			get { return Determinant != 0; }
		}

		public static Matrix Identity {
			get { return new Matrix (1.0, 0.0, 0.0, 1.0, 0.0, 0.0); }
		}

		public bool IsIdentity {
			get {
				return (m11 == 1.0d && m12 == 0.0d &&
					m21 == 0.0d && m22 == 1.0d &&
					offsetX == 0.0d && offsetY == 0.0d);
			}
		}

		public double M11 {
			get { return m11; }
			set { m11 = value; }
		}

		public double M12 {
			get { return m12; }
			set { m12 = value; }
		}

		public double M21 {
			get { return m21; }
			set { m21 = value; }
		}

		public double M22 {
			get { return m22; }
			set { m22 = value; }
		}

		public double OffsetX {
			get { return offsetX; }
			set { offsetX = value; }
		}

		public double OffsetY {
			get { return offsetY; }
			set { offsetY = value; }
		}

	}

}
