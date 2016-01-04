using System;
using System.Drawing;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Waher.Networking.XMPP.DataForms.DataTypes
{
	/// <summary>
	/// Color Data Type (xdc:color)
	/// </summary>
	public class ColorDataType : DataType
	{
		/// <summary>
		/// Color Data Type (xdc:color)
		/// </summary>
		/// <param name="TypeName">Type Name</param>
		public ColorDataType(string DataType)
			: base(DataType)
		{
		}

		/// <summary>
		/// <see cref="DataType.Parse"/>
		/// </summary>
		internal override object Parse(string Value)
		{
			int R, G, B;

			if (Value.Length != 6)
				return null;

			if (!int.TryParse(Value.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out R))
				return null;

			if (!int.TryParse(Value.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out G))
				return null;

			if (!int.TryParse(Value.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out B))
				return null;

			return Color.FromArgb(R, G, B);
		}
	}
}
