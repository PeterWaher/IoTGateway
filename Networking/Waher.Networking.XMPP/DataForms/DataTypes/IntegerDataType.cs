using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace Waher.Networking.XMPP.DataForms.DataTypes
{
	/// <summary>
	/// Integer Data Type (xs:integer)
	/// </summary>
	public class IntegerDataType : DataType
	{
		/// <summary>
		/// Integer Data Type (xs:integer)
		/// </summary>
		public IntegerDataType()
			: this("xs:integer")
		{
		}

		/// <summary>
		/// Integer Data Type (xs:integer)
		/// </summary>
		/// <param name="TypeName">Type Name</param>
		public IntegerDataType(string DataType)
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
			BigInteger Result;

			if (BigInteger.TryParse(Value, out Result))
				return Result;
			else
				return null;
		}
	}
}
