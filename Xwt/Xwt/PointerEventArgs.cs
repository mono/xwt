// 
// PointerEventArgs.cs
//  
// Author:
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

namespace Xwt
{

	/// <summary>
	/// Generic pointer event arguments for mouse, touch, pen etc.
	/// Pointer Events provide all the usual properties present in Mouse Events (position, button states, etc.) 
	/// in addition to new properties for other forms of input: pressure, contact geometry, tilt, etc
	/// <remarks>this class follows loosly the recommondation http://www.w3.org/TR/pointerevents </remarks>
	/// </summary>
	public class PointerEventArgs : ButtonEventArgs
	{

		public PointerEventArgs (PointerType pointerType)
		{
			this.PointerType = pointerType;
		}

		/// <summary>
		/// Indicates the device type that caused the event (mouse, pen, touch, etc.). 
		/// If a user agent is to fire a pointer event for a mouse, pen stylus, or touch input device, 
		/// then the value of pointerType MUST be of PointerType Mouse, Pen or Touch.
		/// If the device type cannot be detected by the user agent, then the value MUST be PointerType.Unknown
		/// If a user agent supports pointer device types other than those listed above, the value MUST be PointerType.Custom
		/// </summary>
		public PointerType PointerType { get; protected set; }

		/// <summary>
		/// The width (magnitude on the X axis), in pixels, of the contact geometry of the pointer. 
		/// This value MAY be updated on each event for a given pointer. 
		/// For devices which have a contact geometry but the actual geometry is not reported by the hardware, 
		/// a default value SHOULD be provided by the user agent to approximate the geometry typical of that pointer type. 
		/// Otherwise, the value MUST be 0. 
		/// </summary>
		public double Width { get; set; }

		/// <summary>
		/// The height (magnitude on the Y axis),  in pixels, of the contact geometry of the pointer. 
		/// This value MAY be updated on each event for a given pointer. 
		/// For devices which have a contact geometry but the actual geometry is not reported by the hardware, 
		/// a default value SHOULD be provided by the user agent to approximate the geometry typical of that pointer type. 
		/// Otherwise, the value MUST be 0. 
		/// </summary>
		public double Height { get; set; }

		/// <summary>
		/// The normalized pressure of the pointer input in the range of [0,1], 
		/// where 0 and 1 represent the minimum and maximum pressure the hardware is capable of detecting, respectively. 
		/// For hardware that does not support pressure, including but not limited to mouse, 
		/// the value MUST be 0.5 when in the active buttons state and 0 otherwise. 
		/// </summary>
		public double Pressure { get; set; }

		/// <summary>
		/// The plane angle (in degrees, in the range of [-90,90]) between the Y-Z plane and 
		/// the plane containing both the transducer (e.g. pen stylus) axis and the Y axis. 
		/// A positive tiltX is to the right. 
		/// tiltX can be used along with tiltY to represent the tilt away from the normal of a transducer 
		/// with the digitizer. 
		/// For devices that do not report tilt, the value MUST be 0.
		/// </summary>
		public int TiltX { get; set; }

		/// <summary>
		/// The plane angle (in degrees, in the range of [-90,90]) between the X-Z plane and 
		/// the plane containing both the transducer (e.g. pen stylus) axis and the X axis. 
		/// A positive tiltY is towards the user. 
		/// tiltY can be used along with tiltX to represent the tilt away from the normal of a transducer 
		/// with the digitizer. 
		/// For devices that do not report tilt, the value MUST be 0. 
		/// </summary>
		public int TiltY { get; set; }

		/// <summary>
		/// When the event occurred
		/// </summary>
		public long Timestamp { get; protected set; }

		/// <summary>
		/// Indicates if the pointer represents the primary pointer of this pointer type.
		/// </summary>
		public bool IsPrimary { get; set; }

		/// <summary>
		/// A unique identifier for the pointer causing the event. 
		/// This identifier MUST be unique from all other active pointers at the time. 
		/// A user agent MAY recycle previously retired values for pointerId from previous active pointers, 
		/// if necessary.
		/// </summary>
		public int PointerId { get; set; }

	}

	public enum PointerType
	{
		Unknown,
		Mouse,
		Pen,
		Touch,
		Custom
	}

}