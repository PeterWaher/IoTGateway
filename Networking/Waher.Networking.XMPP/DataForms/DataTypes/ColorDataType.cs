using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Waher.Content;

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
			byte R, G, B;

			if (Value.Length != 6)
				return null;

			if (!byte.TryParse(Value.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out R))
				return null;

			if (!byte.TryParse(Value.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out G))
				return null;

			if (!byte.TryParse(Value.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out B))
				return null;

			return new ColorReference(R, G, B);
		}
	}
}
