using System;
using System.Globalization;
using SkiaSharp;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Graphs.Functions.Colors
{
	/// <summary>
	/// Returns a color value from a string.
	/// </summary>
	public class Color : FunctionOneScalarVariable
	{
		/// <summary>
		/// Returns a color value from a string.
		/// </summary>
		/// <param name="Name">Name of color.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Color(ScriptNode Name, int Start, int Length, Expression Expression)
			: base(Name, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "Name" };
			}
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get
			{
				return "Color";
			}
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(string Argument, Variables Variables)
		{
			if (TryParse(Argument, out SKColor Color))
				return new ObjectValue(Color);
			else
				throw new ScriptRuntimeException("Unable to parse color.", this);
		}

		/// <summary>
		/// Tries to parse a string containing a color name.
		/// </summary>
		/// <param name="s">String value.</param>
		/// <param name="Color">Color, if found.</param>
		/// <returns>If the string was successfully parsed.</returns>
		public static bool TryParse(string s, out SKColor Color)
		{
			switch (s.ToLower())
			{
				case "aliceblue": Color = SKColors.AliceBlue; break;
				case "palegreen": Color = SKColors.PaleGreen; break;
				case "palegoldenrod": Color = SKColors.PaleGoldenrod; break;
				case "orchid": Color = SKColors.Orchid; break;
				case "orangered": Color = SKColors.OrangeRed; break;
				case "orange": Color = SKColors.Orange; break;
				case "olivedrab": Color = SKColors.OliveDrab; break;
				case "olive": Color = SKColors.Olive; break;
				case "oldlace": Color = SKColors.OldLace; break;
				case "navy": Color = SKColors.Navy; break;
				case "navajowhite": Color = SKColors.NavajoWhite; break;
				case "moccasin": Color = SKColors.Moccasin; break;
				case "mistyrose": Color = SKColors.MistyRose; break;
				case "mintcream": Color = SKColors.MintCream; break;
				case "midnightblue": Color = SKColors.MidnightBlue; break;
				case "mediumvioletred": Color = SKColors.MediumVioletRed; break;
				case "mediumturquoise": Color = SKColors.MediumTurquoise; break;
				case "mediumspringgreen": Color = SKColors.MediumSpringGreen; break;
				case "lightslategray": Color = SKColors.LightSlateGray; break;
				case "lightsteelblue": Color = SKColors.LightSteelBlue; break;
				case "lightyellow": Color = SKColors.LightYellow; break;
				case "lime": Color = SKColors.Lime; break;
				case "limegreen": Color = SKColors.LimeGreen; break;
				case "linen": Color = SKColors.Linen; break;
				case "paleturquoise": Color = SKColors.PaleTurquoise; break;
				case "magenta": Color = SKColors.Magenta; break;
				case "mediumaquamarine": Color = SKColors.MediumAquamarine; break;
				case "mediumblue": Color = SKColors.MediumBlue; break;
				case "mediumorchid": Color = SKColors.MediumOrchid; break;
				case "mediumpurple": Color = SKColors.MediumPurple; break;
				case "mediumseagreen": Color = SKColors.MediumSeaGreen; break;
				case "mediumslateblue": Color = SKColors.MediumSlateBlue; break;
				case "maroon": Color = SKColors.Maroon; break;
				case "palevioletred": Color = SKColors.PaleVioletRed; break;
				case "papayawhip": Color = SKColors.PapayaWhip; break;
				case "peachpuff": Color = SKColors.PeachPuff; break;
				case "snow": Color = SKColors.Snow; break;
				case "springgreen": Color = SKColors.SpringGreen; break;
				case "steelblue": Color = SKColors.SteelBlue; break;
				case "tan": Color = SKColors.Tan; break;
				case "teal": Color = SKColors.Teal; break;
				case "thistle": Color = SKColors.Thistle; break;
				case "slategray": Color = SKColors.SlateGray; break;
				case "tomato": Color = SKColors.Tomato; break;
				case "violet": Color = SKColors.Violet; break;
				case "wheat": Color = SKColors.Wheat; break;
				case "white": Color = SKColors.White; break;
				case "whitesmoke": Color = SKColors.WhiteSmoke; break;
				case "yellow": Color = SKColors.Yellow; break;
				case "yellowgreen": Color = SKColors.YellowGreen; break;
				case "turquoise": Color = SKColors.Turquoise; break;
				case "lightskyblue": Color = SKColors.LightSkyBlue; break;
				case "slateblue": Color = SKColors.SlateBlue; break;
				case "silver": Color = SKColors.Silver; break;
				case "peru": Color = SKColors.Peru; break;
				case "pink": Color = SKColors.Pink; break;
				case "plum": Color = SKColors.Plum; break;
				case "powderblue": Color = SKColors.PowderBlue; break;
				case "purple": Color = SKColors.Purple; break;
				case "red": Color = SKColors.Red; break;
				case "skyblue": Color = SKColors.SkyBlue; break;
				case "rosybrown": Color = SKColors.RosyBrown; break;
				case "saddlebrown": Color = SKColors.SaddleBrown; break;
				case "salmon": Color = SKColors.Salmon; break;
				case "sandybrown": Color = SKColors.SandyBrown; break;
				case "seagreen": Color = SKColors.SeaGreen; break;
				case "seashell": Color = SKColors.SeaShell; break;
				case "sienna": Color = SKColors.Sienna; break;
				case "royalblue": Color = SKColors.RoyalBlue; break;
				case "lightseagreen": Color = SKColors.LightSeaGreen; break;
				case "lightsalmon": Color = SKColors.LightSalmon; break;
				case "lightpink": Color = SKColors.LightPink; break;
				case "crimson": Color = SKColors.Crimson; break;
				case "cyan": Color = SKColors.Cyan; break;
				case "darkblue": Color = SKColors.DarkBlue; break;
				case "darkcyan": Color = SKColors.DarkCyan; break;
				case "darkgoldenrod": Color = SKColors.DarkGoldenrod; break;
				case "darkgray": Color = SKColors.DarkGray; break;
				case "cornsilk": Color = SKColors.Cornsilk; break;
				case "darkgreen": Color = SKColors.DarkGreen; break;
				case "darkmagenta": Color = SKColors.DarkMagenta; break;
				case "darkolivegreen": Color = SKColors.DarkOliveGreen; break;
				case "darkorange": Color = SKColors.DarkOrange; break;
				case "darkorchid": Color = SKColors.DarkOrchid; break;
				case "darkred": Color = SKColors.DarkRed; break;
				case "darksalmon": Color = SKColors.DarkSalmon; break;
				case "darkkhaki": Color = SKColors.DarkKhaki; break;
				case "darkseagreen": Color = SKColors.DarkSeaGreen; break;
				case "cornflowerblue": Color = SKColors.CornflowerBlue; break;
				case "chocolate": Color = SKColors.Chocolate; break;
				case "antiquewhite": Color = SKColors.AntiqueWhite; break;
				case "aqua": Color = SKColors.Aqua; break;
				case "aquamarine": Color = SKColors.Aquamarine; break;
				case "azure": Color = SKColors.Azure; break;
				case "beige": Color = SKColors.Beige; break;
				case "bisque": Color = SKColors.Bisque; break;
				case "coral": Color = SKColors.Coral; break;
				case "black": Color = SKColors.Black; break;
				case "blue": Color = SKColors.Blue; break;
				case "blueviolet": Color = SKColors.BlueViolet; break;
				case "brown": Color = SKColors.Brown; break;
				case "burlywood": Color = SKColors.BurlyWood; break;
				case "cadetblue": Color = SKColors.CadetBlue; break;
				case "chartreuse": Color = SKColors.Chartreuse; break;
				case "blanchedalmond": Color = SKColors.BlanchedAlmond; break;
				case "transparent": Color = SKColors.Transparent; break;
				case "darkslateblue": Color = SKColors.DarkSlateBlue; break;
				case "darkturquoise": Color = SKColors.DarkTurquoise; break;
				case "indianred": Color = SKColors.IndianRed; break;
				case "indigo": Color = SKColors.Indigo; break;
				case "ivory": Color = SKColors.Ivory; break;
				case "khaki": Color = SKColors.Khaki; break;
				case "lavender": Color = SKColors.Lavender; break;
				case "lavenderblush": Color = SKColors.LavenderBlush; break;
				case "hotpink": Color = SKColors.HotPink; break;
				case "lawngreen": Color = SKColors.LawnGreen; break;
				case "lightblue": Color = SKColors.LightBlue; break;
				case "lightcoral": Color = SKColors.LightCoral; break;
				case "lightcyan": Color = SKColors.LightCyan; break;
				case "lightgoldenrodyellow": Color = SKColors.LightGoldenrodYellow; break;
				case "lightgray": Color = SKColors.LightGray; break;
				case "lightgreen": Color = SKColors.LightGreen; break;
				case "lemonchiffon": Color = SKColors.LemonChiffon; break;
				case "darkslategray": Color = SKColors.DarkSlateGray; break;
				case "honeydew": Color = SKColors.Honeydew; break;
				case "green": Color = SKColors.Green; break;
				case "darkviolet": Color = SKColors.DarkViolet; break;
				case "deeppink": Color = SKColors.DeepPink; break;
				case "deepskyblue": Color = SKColors.DeepSkyBlue; break;
				case "dimgray": Color = SKColors.DimGray; break;
				case "dodgerblue": Color = SKColors.DodgerBlue; break;
				case "firebrick": Color = SKColors.Firebrick; break;
				case "greenyellow": Color = SKColors.GreenYellow; break;
				case "floralwhite": Color = SKColors.FloralWhite; break;
				case "fuchsia": Color = SKColors.Fuchsia; break;
				case "gainsboro": Color = SKColors.Gainsboro; break;
				case "ghostwhite": Color = SKColors.GhostWhite; break;
				case "gold": Color = SKColors.Gold; break;
				case "goldenrod": Color = SKColors.Goldenrod; break;
				case "gray": Color = SKColors.Gray; break;
				case "forestgreen": Color = SKColors.ForestGreen; break;
				case "empty": Color = SKColors.Empty; break;
				default:
					byte R, G, B;

					if (s.Length == 6)
					{
						if (byte.TryParse(s.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out R) &&
							byte.TryParse(s.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out G) &&
							byte.TryParse(s.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out B))
						{
							Color = new SKColor(R, G, B);
							return true;
						}
					}
					else if (s.Length == 8)
					{
						if (byte.TryParse(s.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out R) &&
							byte.TryParse(s.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out G) &&
							byte.TryParse(s.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out B) &&
							byte.TryParse(s.Substring(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte A))
						{
							Color = new SKColor(R, G, B, A);
							return true;
						}
					}
					else if (s.Length == 7 && s[0] == '#')
					{
						if (byte.TryParse(s.Substring(1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out R) &&
							byte.TryParse(s.Substring(3, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out G) &&
							byte.TryParse(s.Substring(5, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out B))
						{
							Color = new SKColor(R, G, B);
							return true;
						}
					}
					else if (s.Length == 9 && s[0] == '#')
					{
						if (byte.TryParse(s.Substring(1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out R) &&
							byte.TryParse(s.Substring(3, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out G) &&
							byte.TryParse(s.Substring(5, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out B) &&
							byte.TryParse(s.Substring(7, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte A))
						{
							Color = new SKColor(R, G, B, A);
							return true;
						}
					}

					Color = SKColors.Empty;
					return false;
			}

			return true;
		}
	}
}
