// 
// Colors.cs
//  
// Author:
//       Lytico 
// 
// Copyright (c) 2012 Lytico (http://limada.sourceforge.net)
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

using System.Collections.Generic;
using System.Globalization;
using System;

namespace Xwt.Drawing
{
	public static class Colors
	{
		public static readonly Color Transparent = Color.FromBytes (255, 255, 255, 0);
		public static readonly Color AliceBlue = Color.FromBytes (240, 248, 255, 255);
		public static readonly Color AntiqueWhite = Color.FromBytes (250, 235, 215, 255);
		public static readonly Color Aqua = Color.FromBytes (0, 255, 255, 255);
		public static readonly Color Aquamarine = Color.FromBytes (127, 255, 212, 255);
		public static readonly Color Azure = Color.FromBytes (240, 255, 255, 255);
		public static readonly Color Beige = Color.FromBytes (245, 245, 220, 255);
		public static readonly Color Bisque = Color.FromBytes (255, 228, 196, 255);
		public static readonly Color Black = Color.FromBytes (0, 0, 0, 255);
		public static readonly Color BlanchedAlmond = Color.FromBytes (255, 235, 205, 255);
		public static readonly Color Blue = Color.FromBytes (0, 0, 255, 255);
		public static readonly Color BlueViolet = Color.FromBytes (138, 43, 226, 255);
		public static readonly Color Brown = Color.FromBytes (165, 42, 42, 255);
		public static readonly Color BurlyWood = Color.FromBytes (222, 184, 135, 255);
		public static readonly Color CadetBlue = Color.FromBytes (95, 158, 160, 255);
		public static readonly Color Chartreuse = Color.FromBytes (127, 255, 0, 255);
		public static readonly Color Chocolate = Color.FromBytes (210, 105, 30, 255);
		public static readonly Color Coral = Color.FromBytes (255, 127, 80, 255);
		public static readonly Color CornflowerBlue = Color.FromBytes (100, 149, 237, 255);
		public static readonly Color Cornsilk = Color.FromBytes (255, 248, 220, 255);
		public static readonly Color Crimson = Color.FromBytes (220, 20, 60, 255);
		public static readonly Color Cyan = Color.FromBytes (0, 255, 255, 255);
		public static readonly Color DarkBlue = Color.FromBytes (0, 0, 139, 255);
		public static readonly Color DarkCyan = Color.FromBytes (0, 139, 139, 255);
		public static readonly Color DarkGoldenrod = Color.FromBytes (184, 134, 11, 255);
		public static readonly Color DarkGray = Color.FromBytes (169, 169, 169, 255);
		public static readonly Color DarkGreen = Color.FromBytes (0, 100, 0, 255);
		public static readonly Color DarkKhaki = Color.FromBytes (189, 183, 107, 255);
		public static readonly Color DarkMagenta = Color.FromBytes (139, 0, 139, 255);
		public static readonly Color DarkOliveGreen = Color.FromBytes (85, 107, 47, 255);
		public static readonly Color DarkOrange = Color.FromBytes (255, 140, 0, 255);
		public static readonly Color DarkOrchid = Color.FromBytes (153, 50, 204, 255);
		public static readonly Color DarkRed = Color.FromBytes (139, 0, 0, 255);
		public static readonly Color DarkSalmon = Color.FromBytes (233, 150, 122, 255);
		public static readonly Color DarkSeaGreen = Color.FromBytes (143, 188, 139, 255);
		public static readonly Color DarkSlateBlue = Color.FromBytes (72, 61, 139, 255);
		public static readonly Color DarkSlateGray = Color.FromBytes (47, 79, 79, 255);
		public static readonly Color DarkTurquoise = Color.FromBytes (0, 206, 209, 255);
		public static readonly Color DarkViolet = Color.FromBytes (148, 0, 211, 255);
		public static readonly Color DeepPink = Color.FromBytes (255, 20, 147, 255);
		public static readonly Color DeepSkyBlue = Color.FromBytes (0, 191, 255, 255);
		public static readonly Color DimGray = Color.FromBytes (105, 105, 105, 255);
		public static readonly Color DodgerBlue = Color.FromBytes (30, 144, 255, 255);
		public static readonly Color Firebrick = Color.FromBytes (178, 34, 34, 255);
		public static readonly Color FloralWhite = Color.FromBytes (255, 250, 240, 255);
		public static readonly Color ForestGreen = Color.FromBytes (34, 139, 34, 255);
		public static readonly Color Fuchsia = Color.FromBytes (255, 0, 255, 255);
		public static readonly Color Gainsboro = Color.FromBytes (220, 220, 220, 255);
		public static readonly Color GhostWhite = Color.FromBytes (248, 248, 255, 255);
		public static readonly Color Gold = Color.FromBytes (255, 215, 0, 255);
		public static readonly Color Goldenrod = Color.FromBytes (218, 165, 32, 255);
		public static readonly Color Gray = Color.FromBytes (128, 128, 128, 255);
		public static readonly Color Green = Color.FromBytes (0, 128, 0, 255);
		public static readonly Color GreenYellow = Color.FromBytes (173, 255, 47, 255);
		public static readonly Color Honeydew = Color.FromBytes (240, 255, 240, 255);
		public static readonly Color HotPink = Color.FromBytes (255, 105, 180, 255);
		public static readonly Color IndianRed = Color.FromBytes (205, 92, 92, 255);
		public static readonly Color Indigo = Color.FromBytes (75, 0, 130, 255);
		public static readonly Color Ivory = Color.FromBytes (255, 255, 240, 255);
		public static readonly Color Khaki = Color.FromBytes (240, 230, 140, 255);
		public static readonly Color Lavender = Color.FromBytes (230, 230, 250, 255);
		public static readonly Color LavenderBlush = Color.FromBytes (255, 240, 245, 255);
		public static readonly Color LawnGreen = Color.FromBytes (124, 252, 0, 255);
		public static readonly Color LemonChiffon = Color.FromBytes (255, 250, 205, 255);
		public static readonly Color LightBlue = Color.FromBytes (173, 216, 230, 255);
		public static readonly Color LightCoral = Color.FromBytes (240, 128, 128, 255);
		public static readonly Color LightCyan = Color.FromBytes (224, 255, 255, 255);
		public static readonly Color LightGoldenrodYellow = Color.FromBytes (250, 250, 210, 255);
		public static readonly Color LightGreen = Color.FromBytes (144, 238, 144, 255);
		public static readonly Color LightGray = Color.FromBytes (211, 211, 211, 255);
		public static readonly Color LightPink = Color.FromBytes (255, 182, 193, 255);
		public static readonly Color LightSalmon = Color.FromBytes (255, 160, 122, 255);
		public static readonly Color LightSeaGreen = Color.FromBytes (32, 178, 170, 255);
		public static readonly Color LightSkyBlue = Color.FromBytes (135, 206, 250, 255);
		public static readonly Color LightSlateGray = Color.FromBytes (119, 136, 153, 255);
		public static readonly Color LightSteelBlue = Color.FromBytes (176, 196, 222, 255);
		public static readonly Color LightYellow = Color.FromBytes (255, 255, 224, 255);
		public static readonly Color Lime = Color.FromBytes (0, 255, 0, 255);
		public static readonly Color LimeGreen = Color.FromBytes (50, 205, 50, 255);
		public static readonly Color Linen = Color.FromBytes (250, 240, 230, 255);
		public static readonly Color Magenta = Color.FromBytes (255, 0, 255, 255);
		public static readonly Color Maroon = Color.FromBytes (128, 0, 0, 255);
		public static readonly Color MediumAquamarine = Color.FromBytes (102, 205, 170, 255);
		public static readonly Color MediumBlue = Color.FromBytes (0, 0, 205, 255);
		public static readonly Color MediumOrchid = Color.FromBytes (186, 85, 211, 255);
		public static readonly Color MediumPurple = Color.FromBytes (147, 112, 219, 255);
		public static readonly Color MediumSeaGreen = Color.FromBytes (60, 179, 113, 255);
		public static readonly Color MediumSlateBlue = Color.FromBytes (123, 104, 238, 255);
		public static readonly Color MediumSpringGreen = Color.FromBytes (0, 250, 154, 255);
		public static readonly Color MediumTurquoise = Color.FromBytes (72, 209, 204, 255);
		public static readonly Color MediumVioletRed = Color.FromBytes (199, 21, 133, 255);
		public static readonly Color MidnightBlue = Color.FromBytes (25, 25, 112, 255);
		public static readonly Color MintCream = Color.FromBytes (245, 255, 250, 255);
		public static readonly Color MistyRose = Color.FromBytes (255, 228, 225, 255);
		public static readonly Color Moccasin = Color.FromBytes (255, 228, 181, 255);
		public static readonly Color NavajoWhite = Color.FromBytes (255, 222, 173, 255);
		public static readonly Color Navy = Color.FromBytes (0, 0, 128, 255);
		public static readonly Color OldLace = Color.FromBytes (253, 245, 230, 255);
		public static readonly Color Olive = Color.FromBytes (128, 128, 0, 255);
		public static readonly Color OliveDrab = Color.FromBytes (107, 142, 35, 255);
		public static readonly Color Orange = Color.FromBytes (255, 165, 0, 255);
		public static readonly Color OrangeRed = Color.FromBytes (255, 69, 0, 255);
		public static readonly Color Orchid = Color.FromBytes (218, 112, 214, 255);
		public static readonly Color PaleGoldenrod = Color.FromBytes (238, 232, 170, 255);
		public static readonly Color PaleGreen = Color.FromBytes (152, 251, 152, 255);
		public static readonly Color PaleTurquoise = Color.FromBytes (175, 238, 238, 255);
		public static readonly Color PaleVioletRed = Color.FromBytes (219, 112, 147, 255);
		public static readonly Color PapayaWhip = Color.FromBytes (255, 239, 213, 255);
		public static readonly Color PeachPuff = Color.FromBytes (255, 218, 185, 255);
		public static readonly Color Peru = Color.FromBytes (205, 133, 63, 255);
		public static readonly Color Pink = Color.FromBytes (255, 192, 203, 255);
		public static readonly Color Plum = Color.FromBytes (221, 160, 221, 255);
		public static readonly Color PowderBlue = Color.FromBytes (176, 224, 230, 255);
		public static readonly Color Purple = Color.FromBytes (128, 0, 128, 255);
		public static readonly Color Red = Color.FromBytes (255, 0, 0, 255);
		public static readonly Color RosyBrown = Color.FromBytes (188, 143, 143, 255);
		public static readonly Color RoyalBlue = Color.FromBytes (65, 105, 225, 255);
		public static readonly Color SaddleBrown = Color.FromBytes (139, 69, 19, 255);
		public static readonly Color Salmon = Color.FromBytes (250, 128, 114, 255);
		public static readonly Color SandyBrown = Color.FromBytes (244, 164, 96, 255);
		public static readonly Color SeaGreen = Color.FromBytes (46, 139, 87, 255);
		public static readonly Color SeaShell = Color.FromBytes (255, 245, 238, 255);
		public static readonly Color Sienna = Color.FromBytes (160, 82, 45, 255);
		public static readonly Color Silver = Color.FromBytes (192, 192, 192, 255);
		public static readonly Color SkyBlue = Color.FromBytes (135, 206, 235, 255);
		public static readonly Color SlateBlue = Color.FromBytes (106, 90, 205, 255);
		public static readonly Color SlateGray = Color.FromBytes (112, 128, 144, 255);
		public static readonly Color Snow = Color.FromBytes (255, 250, 250, 255);
		public static readonly Color SpringGreen = Color.FromBytes (0, 255, 127, 255);
		public static readonly Color SteelBlue = Color.FromBytes (70, 130, 180, 255);
		public static readonly Color Tan = Color.FromBytes (210, 180, 140, 255);
		public static readonly Color Teal = Color.FromBytes (0, 128, 128, 255);
		public static readonly Color Thistle = Color.FromBytes (216, 191, 216, 255);
		public static readonly Color Tomato = Color.FromBytes (255, 99, 71, 255);
		public static readonly Color Turquoise = Color.FromBytes (64, 224, 208, 255);
		public static readonly Color Violet = Color.FromBytes (238, 130, 238, 255);
		public static readonly Color Wheat = Color.FromBytes (245, 222, 179, 255);
		public static readonly Color White = Color.FromBytes (255, 255, 255, 255);
		public static readonly Color WhiteSmoke = Color.FromBytes (245, 245, 245, 255);
		public static readonly Color Yellow = Color.FromBytes (255, 255, 0, 255);
		public static readonly Color YellowGreen = Color.FromBytes (154, 205, 50, 255);

		// When adding colours to this file, add the lowercase name to this dictionary
		// The whole dictionary can be recreated with 
		// grep FromBytes main/external/xwt/Xwt/Xwt.Drawing/Colors.cs | cut -d ' ' -f 5 | xargs -L1 -I '$' echo '{"$", $},' | gsed  -E 's/"(.+)"/"\L\1"/g'
		// (install gnu-sed from brew on the Mac)
		static readonly Dictionary<string, Color> namedColors = new Dictionary<string, Color> (StringComparer.InvariantCultureIgnoreCase) {
			{"transparent", Transparent},
			{"aliceblue", AliceBlue},
			{"antiquewhite", AntiqueWhite},
			{"aqua", Aqua},
			{"aquamarine", Aquamarine},
			{"azure", Azure},
			{"beige", Beige},
			{"bisque", Bisque},
			{"black", Black},
			{"blanchedalmond", BlanchedAlmond},
			{"blue", Blue},
			{"blueviolet", BlueViolet},
			{"brown", Brown},
			{"burlywood", BurlyWood},
			{"cadetblue", CadetBlue},
			{"chartreuse", Chartreuse},
			{"chocolate", Chocolate},
			{"coral", Coral},
			{"cornflowerblue", CornflowerBlue},
			{"cornsilk", Cornsilk},
			{"crimson", Crimson},
			{"cyan", Cyan},
			{"darkblue", DarkBlue},
			{"darkcyan", DarkCyan},
			{"darkgoldenrod", DarkGoldenrod},
			{"darkgray", DarkGray},
			{"darkgreen", DarkGreen},
			{"darkkhaki", DarkKhaki},
			{"darkmagenta", DarkMagenta},
			{"darkolivegreen", DarkOliveGreen},
			{"darkorange", DarkOrange},
			{"darkorchid", DarkOrchid},
			{"darkred", DarkRed},
			{"darksalmon", DarkSalmon},
			{"darkseagreen", DarkSeaGreen},
			{"darkslateblue", DarkSlateBlue},
			{"darkslategray", DarkSlateGray},
			{"darkturquoise", DarkTurquoise},
			{"darkviolet", DarkViolet},
			{"deeppink", DeepPink},
			{"deepskyblue", DeepSkyBlue},
			{"dimgray", DimGray},
			{"dodgerblue", DodgerBlue},
			{"firebrick", Firebrick},
			{"floralwhite", FloralWhite},
			{"forestgreen", ForestGreen},
			{"fuchsia", Fuchsia},
			{"gainsboro", Gainsboro},
			{"ghostwhite", GhostWhite},
			{"gold", Gold},
			{"goldenrod", Goldenrod},
			{"gray", Gray},
			{"green", Green},
			{"greenyellow", GreenYellow},
			{"honeydew", Honeydew},
			{"hotpink", HotPink},
			{"indianred", IndianRed},
			{"indigo", Indigo},
			{"ivory", Ivory},
			{"khaki", Khaki},
			{"lavender", Lavender},
			{"lavenderblush", LavenderBlush},
			{"lawngreen", LawnGreen},
			{"lemonchiffon", LemonChiffon},
			{"lightblue", LightBlue},
			{"lightcoral", LightCoral},
			{"lightcyan", LightCyan},
			{"lightgoldenrodyellow", LightGoldenrodYellow},
			{"lightgreen", LightGreen},
			{"lightgray", LightGray},
			{"lightpink", LightPink},
			{"lightsalmon", LightSalmon},
			{"lightseagreen", LightSeaGreen},
			{"lightskyblue", LightSkyBlue},
			{"lightslategray", LightSlateGray},
			{"lightsteelblue", LightSteelBlue},
			{"lightyellow", LightYellow},
			{"lime", Lime},
			{"limegreen", LimeGreen},
			{"linen", Linen},
			{"magenta", Magenta},
			{"maroon", Maroon},
			{"mediumaquamarine", MediumAquamarine},
			{"mediumblue", MediumBlue},
			{"mediumorchid", MediumOrchid},
			{"mediumpurple", MediumPurple},
			{"mediumseagreen", MediumSeaGreen},
			{"mediumslateblue", MediumSlateBlue},
			{"mediumspringgreen", MediumSpringGreen},
			{"mediumturquoise", MediumTurquoise},
			{"mediumvioletred", MediumVioletRed},
			{"midnightblue", MidnightBlue},
			{"mintcream", MintCream},
			{"mistyrose", MistyRose},
			{"moccasin", Moccasin},
			{"navajowhite", NavajoWhite},
			{"navy", Navy},
			{"oldlace", OldLace},
			{"olive", Olive},
			{"olivedrab", OliveDrab},
			{"orange", Orange},
			{"orangered", OrangeRed},
			{"orchid", Orchid},
			{"palegoldenrod", PaleGoldenrod},
			{"palegreen", PaleGreen},
			{"paleturquoise", PaleTurquoise},
			{"palevioletred", PaleVioletRed},
			{"papayawhip", PapayaWhip},
			{"peachpuff", PeachPuff},
			{"peru", Peru},
			{"pink", Pink},
			{"plum", Plum},
			{"powderblue", PowderBlue},
			{"purple", Purple},
			{"red", Red},
			{"rosybrown", RosyBrown},
			{"royalblue", RoyalBlue},
			{"saddlebrown", SaddleBrown},
			{"salmon", Salmon},
			{"sandybrown", SandyBrown},
			{"seagreen", SeaGreen},
			{"seashell", SeaShell},
			{"sienna", Sienna},
			{"silver", Silver},
			{"skyblue", SkyBlue},
			{"slateblue", SlateBlue},
			{"slategray", SlateGray},
			{"snow", Snow},
			{"springgreen", SpringGreen},
			{"steelblue", SteelBlue},
			{"tan", Tan},
			{"teal", Teal},
			{"thistle", Thistle},
			{"tomato", Tomato},
			{"turquoise", Turquoise},
			{"violet", Violet},
			{"wheat", Wheat},
			{"white", White},
			{"whitesmoke", WhiteSmoke},
			{"yellow", Yellow},
			{"yellowgreen", YellowGreen},
		};

		public static bool TryGetNamedColor (string name, out Color color)
		{
			return namedColors.TryGetValue (name, out color);
		}
	}
}