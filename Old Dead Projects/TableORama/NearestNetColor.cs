﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using LORUtils;

namespace UtilORama
{
	class NearestColor
	{
		// 140 total, 7 per line (thus 20 lines)
		//!TODO figure out efficient way to extract this from .Net instead of using constants
		//! Special note for Light-O-Rama: Changed "Lime" to "Green" and "Green" to "DarkGreen"

		public static string[] ColorNames = {
			"AliceBlue","AntiqueWhite","Aqua","Aquamarine","Azure","Beige","Bisque",
			"Black","BlanchedAlmond","Blue","BlueViolet","Brown","BurlyWood","CadetBlue",
			"Chartreuse","Chocolate","Coral","CornflowerBlue","Cornsilk","Crimson","Cyan",
			"DarkBlue","DarkCyan","DarkGoldenrod","DarkGray","DarkGreen","DarkKhaki","DarkMagena",
			"DarkOliveGreen","DarkOrange","DarkOrchid","DarkRed","DarkSalmon","DarkSeaGreen","DarkSlateBlue",
			"DarkSlateGray","DarkTurquoise","DarkViolet","DeepPink","DeepSkyBlue","DimGray","DodgerBlue",
			"Firebrick","FloralWhite","ForestGreen","Fuschia","Gainsboro","GhostWhite","Gold",
			"Goldenrod","Gray","DarkGreen","GreenYellow","Honeydew","HotPink","IndianRed",
			"Indigo","Ivory","Khaki","Lavender","LavenderBlush","LawnGreen","LemonChiffon",
			"LightBlue","LightCoral","LightCyan","LightGoldenrodYellow","LightGreen","LightGray","LightPink",
			"LightSalmon","LightSeaGreen","LightSkyBlue","LightSlateGray","LightSteelBlue","LightYellow","Green",
			"LimeGreen","Linen","Magenta","Maroon","MediumAquamarine","MediumBlue","MediumOrchid",
			"MediumPurple","MediumSeaGreen","MediumSlateBlue","MediumSpringGreen","MediumTurquoise","MediumVioletRed","MidnightBlue",
			"MintCream","MistyRose","Moccasin","NavajoWhite","Navy","OldLace","Olive",
			"OliveDrab","Orange","OrangeRed","Orchid","PaleGoldenrod","PaleGreen","PaleTurquoise",
			"PaleVioletRed","PapayaWhip","PeachPuff","Peru","Pink","Plum","PowderBlue",
			"Purple","Red","RosyBrown","RoyalBlue","SaddleBrown","Salmon","SandyBrown",
			"SeaGreen","Seashell","Sienna","Silver","SkyBlue","SlateBlue","SlateGray",
			"Snow","SpringGreen","SteelBlue","Tan","Teal","Thistle","Tomato",
			"Turquoise","Violet","Wheat","White","WhiteSmoke","Yellow","YellowGreen"};

		// These colors are in .Net or HTML format Red-Grn-Blu
		public static Int32[] ColorVals = {
			0xF0F8FF,0xFAEBD7,0x00FFFF,0x7FFFD4,0xF0FFFF,0xF5F5DC,0xFFE4C4,
			0x000000,0xFFEBCD,0x0000FF,0x8A2BE2,0xA52A2A,0xDEB887,0x5F9EA0,
			0x7FFF00,0xD2691E,0xFF7F50,0x6495ED,0xFFF8DC,0xDC143C,0x00FFFF,
			0x00008B,0x008B8B,0xB8860B,0xA9A9A9,0x006400,0xBDB76B,0x8B008B,
			0x556B2F,0xFF8C00,0x9932CC,0x8B0000,0xE9967A,0x8FBC8B,0x483D8B,
			0x2F4F4F,0x00CED1,0x9400D3,0xFF1493,0x00BFFF,0x696969,0x1E90FF,
			0xB22222,0xFFFAF0,0x228B22,0xFF00FF,0xDCDCDC,0xF8F8FF,0xFFD700,
			0xDAA520,0x808080,0x008000,0xADFF2F,0xF0FFF0,0xFF69B4,0xCD5C5C,
			0x4B0082,0xFFFFF0,0xF0E68C,0xE6E6FA,0xFFF0F5,0x7CFC00,0xFFFACD,
			0xADD8E6,0xF08080,0xE0FFFF,0xFAFAD2,0xD3D3D3,0x90EE90,0xFFB6C1,
			0xFFA07A,0x20B2AA,0x87CEFA,0x778899,0xB0C4DE,0xFFFFE0,0x00FF00,
			0x32CD32,0xFAF0E6,0xFF00FF,0x800000,0x66CDAA,0x0000CD,0xBA55D3,
			0x9370DB,0x3CB371,0x7B68EE,0x00FA9A,0x48D1CC,0xC71585,0x191970,
			0xF5FFFA,0xFFE4E1,0xFFE4B5,0xFFDEAD,0x000080,0xFDF5E6,0x808000,
			0x6B8E23,0xFFA500,0xFF4500,0xDA70D6,0xEEE8AA,0x98FB98,0xAFEEEE,
			0xDB7093,0xFFEFD5,0xFFDAB9,0xCD853F,0xFFC0CB,0xDDA0DD,0xB0E0E6,
			0x800080,0xFF0000,0xBC8F8F,0x4169E1,0x8B4513,0xFA8072,0xF4A460,
			0x2E8B57,0xFFF5EE,0xA0522D,0xC0C0C0,0x87CEEB,0x6A5ACD,0x708090,
			0xFFFAFA,0x00FF7F,0x4682B4,0xD2B48C,0x008080,0xD8BFD8,0xFF6347,
			0x40E0D0,0xEE82EE,0xF5DEB3,0xFFFFFF,0xF5F5F5,0xFFFF00,0x9ACD32};

		// These colors are in LOR format Blu-Grn-Red
		//todo






		public static string FindNearestColorName(Color c)
		{
			int idx = FindNearestColorIndex(c);
			return ColorNames[idx];
		}

		public static int FindNearestColorIndex(Color c)
		{
			int nearestSoFar = 999;
			int smallestDiff = 999;

			for (int n=0; n< ColorNames.Length; n++)
			{
				//int nr = (ColorVals[n] & 0xFF0000) >> 16;
				//int ng = (ColorVals[n] & 0xFF00) >> 8;
				//int nb = (ColorVals[n] & 0xFF);
				//int d = Math.Abs(c.R - nr);
				//d += Math.Abs(c.G - ng);
				//d += Math.Abs(c.B - nb);

				int d = ColorDistance(c, Color.FromArgb(ColorVals[n]));
				if (d<smallestDiff)
				{
					smallestDiff = d;
					nearestSoFar = n;
					if (d == 0) n = ColorNames.Length; // If exact match found, no need to check the rest, force exit of loop
				}
			}
			return nearestSoFar;
		}

		public static int ColorDistance(Color c1, Color c2)
		{
			int d = Math.Abs(c1.R - c2.R);
			d += Math.Abs(c1.G - c2.G);
			d += Math.Abs(c1.B - c2.B);
			return d;
		}

		public static bool ColorMatch (Color c1, Color c2)
		{
			int d = ColorDistance(c1, c2);
			if (d == 0) return true; else return false;
		}

		public static string FindNearestColorName(int LORcolor)
		{
			Color c = utils.Color_LORtoNet(LORcolor);
			int idx = FindNearestColorIndex(c);
			return ColorNames[idx];
		}

		public static int FindNearestColorIndex(int LORcolor)
		{
			int nearestSoFar = 999;
			int smallestDiff = 999;

			for (int n = 0; n < ColorNames.Length; n++)
			{
				int d = ColorDistance(LORcolor, Color.FromArgb(ColorVals[n]));
				if (d < smallestDiff)
				{
					smallestDiff = d;
					nearestSoFar = n;
					if (d == 0) n = ColorNames.Length; // If exact match found, no need to check the rest, force exit of loop
				}
			}
			return nearestSoFar;
		}

		public static int ColorDistance(int LORcolor, Color NetColor)
		{
			int lr = LORcolor & 0x0000FF;
			int lg = LORcolor & 0x00FF00 >> 8;
			int lb = LORcolor & 0xFF0000 >> 16;
			int d = Math.Abs(lr - NetColor.R);
			d += Math.Abs(lg - NetColor.G);
			d += Math.Abs(lb - NetColor.B);
			return d;
		}

		public static bool ColorMatch(int LORcolor, Color NetColor)
		{
			int d = ColorDistance(LORcolor, NetColor);
			if (d == 0) return true; else return false;
		}

	}
}
