using System;
using System.Numerics;

namespace Waher.Networking.XMPP.DataForms.DataTypes
{
	/// <summary>
	/// Integer Data Type (xs:integer)
	/// </summary>
	public class IntegerDataType : DataType
	{
		/// <summary>
		/// Public instance of data type.
		/// </summary>
		public static readonly IntegerDataType Instance = new IntegerDataType();

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
		/// <param name="DataType">Data Type</param>
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
			if (BigInteger.TryParse(Value, out BigInteger Result))
				return Result;
			else
				return null;
		}
	}
}
