using System.Globalization;
using Waher.Content;

namespace Waher.Networking.XMPP.DataForms.DataTypes
{
	/// <summary>
	/// ColorAlpha Data Type (xdc:colorAlpha)
	/// </summary>
	public class ColorAlphaDataType : DataType
	{
		/// <summary>
		/// Public instance of data type.
		/// </summary>
		public static readonly ColorAlphaDataType Instance = new ColorAlphaDataType();

		/// <summary>
		/// ColorAlpha Data Type (xdc:colorAlpha)
		/// </summary>
		public ColorAlphaDataType()
			: this("xdc:colorAlpha")
		{
		}

		/// <summary>
		/// ColorAlpha Data Type (xdc:colorAlpha)
		/// </summary>
		/// <param name="DataType">Data Type</param>
		public ColorAlphaDataType(string DataType)
			: base(DataType)
		{
		}

		/// <summary>
		/// Parses a string.
		/// </summary>
		/// <param name="Value">String value.</param>
		/// <returns>Parsed value, if possible, null otherwise.</returns>
		public override object Parse(string Value)
		{
			if (TryParse(Value, out ColorReference Result))
				return Result;
			else
				return null;
		}

		/// <summary>
		/// Tries to parse a string into a color reference value.
		/// </summary>
		/// <param name="Value">String-representation of color.</param>
		/// <param name="Color">Parsed color reference.</param>
		/// <returns>If successful.</returns>
		public static bool TryParse(string Value, out ColorReference Color)
		{
			Color = null;
			if (Value.Length != 8)
				return false;

			if (!byte.TryParse(Value.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte R))
				return false;

			if (!byte.TryParse(Value.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte G))
				return false;

			if (!byte.TryParse(Value.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte B))
				return false;

			if (!byte.TryParse(Value.Substring(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte A))
				return false;

			Color = new ColorReference(R, G, B, A);
			return true;
		}
	}
}
