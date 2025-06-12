using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Xml;
using Waher.Script;
using Waher.Script.Graphs;

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
					rgbaToName[Graph.ToRGBAStyle(Color)] = PI.Name;
				}
			}

			foreach (FieldInfo FI in T.GetRuntimeFields())
			{
				if (FI.FieldType == typeof(SKColor))
				{
					SKColor Color = (SKColor)FI.GetValue(null);
					stringToColor[FI.Name] = Color;
					rgbaToName[Graph.ToRGBAStyle(Color)] = FI.Name;
				}
			}
		}

		/// <summary>
		/// Color attribute
		/// </summary>
		/// <param name="AttributeName">Attribute name.</param>
		/// <param name="Value">Attribute value.</param>
		/// <param name="Document">Document hosting the attribute.</param>
		public ColorAttribute(string AttributeName, SKColor Value, Layout2DDocument Document)
			: base(AttributeName, Value, Document)
		{
		}

		/// <summary>
		/// Color attribute
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="AttributeName">Attribute name.</param>
		/// <param name="Document">Document hosting the attribute.</param>
		public ColorAttribute(XmlElement E, string AttributeName, Layout2DDocument Document)
			: base(E, AttributeName, true, Document)
		{
		}

		/// <summary>
		/// Color attribute
		/// </summary>
		/// <param name="AttributeName">Attribute name.</param>
		/// <param name="Expression">Expression.</param>
		/// <param name="Document">Document hosting the attribute.</param>
		public ColorAttribute(string AttributeName, Expression Expression, Layout2DDocument Document)
			: base(AttributeName, Expression, Document)
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
			string s = Graph.ToRGBAStyle(Value);

			if (rgbaToName.TryGetValue(s, out string s2))
				return s2;
			else
				return s;
		}

		/// <summary>
		/// Copies the attribute object if undefined, or defined by an expression.
		/// Returns a reference to itself, if preset (set by a constant value).
		/// </summary>
		/// <param name="ForDocument">Document that will host the new attribute.</param>
		/// <returns>Attribute reference.</returns>
		public ColorAttribute CopyIfNotPreset(Layout2DDocument ForDocument)
		{
			if (this.HasPresetValue)
				return this;
			else
				return new ColorAttribute(this.Name, this.Expression, ForDocument);
		}

	}
}
