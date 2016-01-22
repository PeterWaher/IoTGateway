using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.DataForms.DataTypes
{
	/// <summary>
	/// Byte Data Type (xs:byte)
	/// </summary>
	public class ByteDataType : DataType
	{
		/// <summary>
		/// Byte Data Type (xs:byte)
		/// </summary>
		/// <param name="TypeName">Type Name</param>
		public ByteDataType(string DataType)
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
			sbyte Result;

			if (sbyte.TryParse(Value, out Result))
				return Result;
			else
				return null;
		}
	}
}
