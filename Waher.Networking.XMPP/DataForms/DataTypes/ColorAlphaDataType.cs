using System;
using System.Drawing;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Waher.Networking.XMPP.DataForms.DataTypes
{
	/// <summary>
	/// ColorAlpha Data Type (xdc:colorAlpha)
	/// </summary>
	public class ColorAlphaDataType : DataType
	{
		/// <summary>
		/// ColorAlpha Data Type (xdc:colorAlpha)
		/// </summary>
		/// <param name="TypeName">Type Name</param>
		public ColorAlphaDataType(string DataType)
			: base(DataType)
		{
		}

		/// <summary>
		/// <see cref="DataType.Parse"/>
		/// </summary>
		internal override object Parse(string Value)
		{
			int R, G, B, A;

			if (Value.Length != 8)
				return null;

			if (!int.TryParse(Value.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out R))
				return null;

			if (!int.TryParse(Value.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out G))
				return null;

			if (!int.TryParse(Value.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out B))
				return null;

			if (!int.TryParse(Value.Substring(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out A))
				return null;

			return Color.FromArgb(A, R, G, B);
		}
	}
}
