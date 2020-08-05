using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Xml;
using SkiaSharp;
using Waher.Content;

namespace Waher.Layout.Layout2D.Model.Attributes
{
	/// <summary>
	/// Color attribute
	/// </summary>
	public class ColorAttribute : Attribute<SKColor>
	{
		private static readonly Dictionary<string, SKColor> stringToColor;
		private static readonly Dictionary<string, string> rgbaToName;

		static ColorAttribute()
		{
			stringToColor = new Dictionary<string, SKColor>();
			rgbaToName = new Dictionary<string, string>();
			
			Type T = typeof(SKColors);

			foreach (PropertyInfo PI in T.GetRuntimeProperties())
			{
				if (PI.PropertyType == typeof(SKColor))
				{
					SKColor Color = (SKColor)PI.GetValue(null);
					stringToColor[PI.Name] = Color;
					rgbaToName[ToRGBA(Color)] = PI.Name;
				}
			}

			foreach (FieldInfo FI in T.GetRuntimeFields())
			{
				if (FI.FieldType == typeof(SKColor))
				{
					SKColor Color = (SKColor)FI.GetValue(null);
					stringToColor[FI.Name] = Color;
					rgbaToName[ToRGBA(Color)] = FI.Name;
				}
			}
		}

		private static string ToRGBA(SKColor Color)
		{
			StringBuilder Result = new StringBuilder();

			Result.Append('#');
			Result.Append(Color.Red.ToString("x2"));
			Result.Append(Color.Green.ToString("x2"));
			Result.Append(Color.Blue.ToString("x2"));

			if (Color.Alpha != 255)
				Result.Append(Color.Alpha.ToString("x2"));

			return Result.ToString();
		}

		/// <summary>
		/// Color attribute
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="AttributeName">Attribute name.</param>
		public ColorAttribute(XmlElement E, string AttributeName)
			: base(E, AttributeName, true)
		{
		}

		/// <summary>
		/// Tries to parse a string value
		/// </summary>
		/// <param name="StringValue">String value for attribute.</param>
		/// <param name="Value">Parsed value, if successful.</param>
		/// <returns>If the value could be parsed.</returns>
		public override bool TryParse(string StringValue, out SKColor Value)
		{
			if (StringValue.StartsWith("#"))
			{
				switch (StringValue.Length)
				{
					case 7:
						byte R, G, B;

						if (byte.TryParse(StringValue.Substring(1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out R) &&
							byte.TryParse(StringValue.Substring(3, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out G) &&
							byte.TryParse(StringValue.Substring(5, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out B))
						{
							Value = new SKColor(R, G, B);
							return true;
						}
						break;

					case 9:
						if (byte.TryParse(StringValue.Substring(1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out R) &&
							byte.TryParse(StringValue.Substring(3, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out G) &&
							byte.TryParse(StringValue.Substring(5, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out B) &&
							byte.TryParse(StringValue.Substring(7, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte A))
						{
							Value = new SKColor(R, G, B, A);
							return true;
						}
						break;
				}

				Value = SKColor.Empty;
				return false;
			}
			else
				return stringToColor.TryGetValue(StringValue, out Value);
		}

		/// <summary>
		/// Converts a value to a string.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>String representation.</returns>
		public override string ToString(SKColor Value)
		{
			string s = ToRGBA(Value);

			if (rgbaToName.TryGetValue(s, out string s2))
				return s2;
			else
				return s;
		}

	}
}
