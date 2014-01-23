// 
// Matrix.cs
//  
// Authors:
//	Chris Toshok (toshok@ximian.com)
//	Lytico (http://www.limada.org)
//  Hywel Thomas <hywel.w.thomas@gmail.com>
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
// Remarks: this file was refactored from 
// https://github.com/mono/mono/blob/master/mcs/class/WindowsBase/System.Windows.Media/Matrix.cs
// by lytico to be used with https://github.com/mono/xwt
// 
// The default order for multiple matrix operations has been changed from
// Append to Prepend to be consistent with the Drawing.Context transform
// order as implemented in each of the ContextBackends.
// 
using System;
using System.Globalization;

namespace Xwt.Drawing
{
	[Serializable]
	public class Matrix
	{

		public double M11 { get; set; }
		public double M12 { get; set; }
		public double M21 { get; set; }
		public double M22 { get; set; }
		public double OffsetX { get; set; }
		public double OffsetY { get; set; }

		public Matrix (double m11, double m12, double m21, double m22, double offsetX, double offsetY)
		{
			this.M11 = m11;
			this.M12 = m12;
			this.M21 = m21;
			this.M22 = m22;
			this.OffsetX = offsetX;
			this.OffsetY = offsetY;
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
			Append (matrix.M11, matrix.M12, matrix.M21, matrix.M22, matrix.OffsetX, matrix.OffsetY);
		}
		
		protected void Append (double m11, double m12, double m21, double m22, double offsetX, double offsetY)
		{
			var _m11 = M11 * m11 + M12 * m21;
			var _m12 = M11 * m12 + M12 * m22;
			var _m21 = M21 * m11 + M22 * m21;
			var _m22 = M21 * m12 + M22 * m22;
			
			var _offsetX = OffsetX * m11 + OffsetY * m21 + offsetX;
			var _offsetY = OffsetX * m12 + OffsetY * m22 + offsetY;
			
			M11 = _m11;
			M12 = _m12;
			M21 = _m21;
			M22 = _m22;
			OffsetX = _offsetX;
			OffsetY = _offsetY;
		}

		public bool Equals (Matrix value)
		{
			if (value == null)
				return false;
			return (M11 == value.M11 &&
				M12 == value.M12 &&
				M21 == value.M21 &&
				M22 == value.M22 &&
				OffsetX == value.OffsetX &&
				OffsetY == value.OffsetY);
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
			int h = M11.GetHashCode ();
			h = (h << 5) - h + M21.GetHashCode ();
			h = (h << 5) - h + M12.GetHashCode ();
			h = (h << 5) - h + M22.GetHashCode ();
			h = (h << 5) - h + OffsetX.GetHashCode ();
			h = (h << 5) - h + OffsetY.GetHashCode ();
			return h;
		}

		public void Invert ()
		{
			if (!HasInverse)
				throw new InvalidOperationException ("Transform is not invertible.");

			var d = Determinant;

			/* 1/(ad-bc)[d -b; -c a] */

			var _m11 = M22;
			var _m12 = -M12;
			var _m21 = -M21;
			var _m22 = M11;

			var _offsetX = M21 * OffsetY - M22 * OffsetX;
			var _offsetY = M12 * OffsetX - M11 * OffsetY;

			M11 = _m11 / d;
			M12 = _m12 / d;
			M21 = _m21 / d;
			M22 = _m22 / d;
			OffsetX = _offsetX / d;
			OffsetY = _offsetY / d;
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
			Prepend (matrix.M11, matrix.M12, matrix.M21, matrix.M22, matrix.OffsetX, matrix.OffsetY);
		}

		protected void Prepend (double m11, double m12, double m21, double m22, double offsetX, double offsetY)
		{
			var _m11 = m11 * M11 + m12 * M21;
			var _m12 = m11 * M12 + m12 * M22;
			var _m21 = m21 * M11 + m22 * M21;
			var _m22 = m21 * M12 + m22 * M22;
			
			var _offsetX = offsetX * M11 + offsetY * M21 + OffsetX;
			var _offsetY = offsetX * M12 + offsetY * M22 + OffsetY;
			
			M11 = _m11;
			M12 = _m12;
			M21 = _m21;
			M22 = _m22;
			OffsetX = _offsetX;
			OffsetY = _offsetY;
		}

		public void Rotate (double angle)
		{
			angle = angle % 360;
			if (angle == 0)
				return;
			if (angle == 90 || angle == -270)
				Prepend (0, 1, -1, 0, 0, 0);
			else if (angle == -90 || angle == 270)
				Prepend (0, -1, 1, 0, 0, 0);
			else if (angle == 180 || angle == -180)
				Prepend (-1, 0, 0, -1, 0, 0);
			else {
				var theta = angle * pi180;
				var cos = Math.Cos (theta);
				var sin = Math.Sin (theta);
				Prepend (cos, sin, -sin, cos, 0, 0);
			}
		}

		public void RotateAt (double angle, double centerX, double centerY)
		{
			Translate (centerX, centerY);
			Rotate (angle);
			Translate (-centerX, -centerY);		// done first
		}

		public void RotateAppend (double angle)
		{
			angle = angle % 360;
			if (angle == 90 || angle == -270)
				Append (0, 1, -1, 0, 0, 0);
			else if (angle == -90 || angle == 270)
				Append (0, -1, 1, 0, 0, 0);
			else if (angle == 180 || angle == -180)
				Append (-1, 0, 0, -1, 0, 0);
			else if (angle == 0)
				return;
			else {
				var theta = angle * pi180;
				var cos = Math.Cos (theta);
				var sin = Math.Sin (theta);
				Append (cos, sin, -sin, cos, 0, 0);
			}
		}

		public void RotateAtAppend (double angle, double centerX, double centerY)
		{
			TranslateAppend (-centerX, -centerY);
			RotateAppend (angle);
			TranslateAppend (centerX, centerY);
		}

		public void Scale (double scaleX, double scaleY)
		{
			Prepend (scaleX, 0, 0, scaleY, 0, 0);
		}

		public void ScaleAt (double scaleX, double scaleY, double centerX, double centerY)
		{
			Translate (centerX, centerY);
			Scale (scaleX, scaleY);
			Translate (-centerX, -centerY);		// done first
		}

		public void ScaleAppend (double scaleX, double scaleY)
		{
			Append (scaleX, 0, 0, scaleY, 0, 0);
		}

		public void ScaleAtAppend (double scaleX, double scaleY, double centerX, double centerY)
		{
			TranslateAppend (-centerX, -centerY);
			ScaleAppend (scaleX, scaleY);
			TranslateAppend (centerX, centerY);
		}

		public void SetIdentity ()
		{
			M11 = M22 = 1.0;
			M12 = M21 = 0.0;
			OffsetX = OffsetY = 0.0;
		}

		const double pi180 = Math.PI / 180;

		public void Skew (double skewX, double skewY)
		{
			Prepend (1, Math.Tan (skewY * pi180),
			        Math.Tan (skewX * pi180), 1,
			        0, 0);
		}

		public void SkewAppend (double skewX, double skewY)
		{
			Append (1, Math.Tan (skewY * pi180),
			        Math.Tan (skewX * pi180), 1,
			        0, 0);
		}

		public override string ToString ()
		{
			if (IsIdentity)
				return this.GetType ().Name + "=Identity";
			else
				return string.Format (this.GetType ().Name + "{{M11={0} M12={1} M21={2} M22={3} OffsetX={4} OffsetY={5}}}",
                              M11.ToString (CultureInfo.InvariantCulture),
                              M12.ToString (CultureInfo.InvariantCulture),
                              M21.ToString (CultureInfo.InvariantCulture),
                              M22.ToString (CultureInfo.InvariantCulture),
                              OffsetX.ToString (CultureInfo.InvariantCulture),
                              OffsetY.ToString (CultureInfo.InvariantCulture));
		}

		public Point Transform (Point point)
		{
			return new Point (point.X * M11 + point.Y * M21 + OffsetX,
                point.X * M12 + point.Y * M22 + OffsetY);
		}

		public void Transform (Point[] points)
		{
			double x;
			double y;
			var len = points.Length;
			var m11 = this.M11;
			var m12 = this.M12;
			var m21 = this.M21;
			var m22 = this.M22;
			var offsetX = this.OffsetX;
			var offsetY = this.OffsetY;
			
			for (int i = 0; i < len; i++) {
				x = points [i].X;
				y = points [i].Y;
				points [i].X = x * m11 + y * m21 + offsetX;
				points [i].Y = x * m12 + y * m22 + offsetY;
			}
		}

		public Point TransformVector (Point vector)
		{
			return new Point (vector.X * M11 + vector.Y * M21, vector.X * M12 + vector.Y * M22);
		}

		public void TransformVector (Point[] vectors)
		{
			double x;
			double y;
			var len = vectors.Length;
			var m11 = this.M11;
			var m12 = this.M12;
			var m21 = this.M21;
			var m22 = this.M22;
			
			for (int i = 0; i < len; i++) {
				x = vectors [i].X;
				y = vectors [i].Y;
				vectors [i].X = x * m11 + y * m21;
				vectors [i].Y = x * m12 + y * m22;
			}
		}

		public void Translate (double offsetX, double offsetY)
		{
			OffsetX += M11*offsetX + M21*offsetY;
			OffsetY += M12*offsetX + M22*offsetY;
		}

		public void TranslateAppend (double offsetX, double offsetY)
		{
			OffsetX += offsetX;
			OffsetY += offsetY;
		}

		public double Determinant {
			get { return M11 * M22 - M12 * M21; }
		}

		public bool HasInverse {
			get { return Determinant != 0; }
		}

		public static Matrix Identity {
			get { return new Matrix (1.0, 0.0, 0.0, 1.0, 0.0, 0.0); }
		}

		public bool IsIdentity {
			get {
				return (M11 == 1.0d && M12 == 0.0d &&
					M21 == 0.0d && M22 == 1.0d &&
					OffsetX == 0.0d && OffsetY == 0.0d);
			}
		}

	}

}
