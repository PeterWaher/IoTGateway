using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;

namespace Waher.Networking.XMPP.DataForms.DataTypes
{
	/// <summary>
	/// Decimal Data Type (xs:decimal)
	/// </summary>
	public class DecimalDataType : DataType
	{
		/// <summary>
		/// Public instance of data type.
		/// </summary>
		public static readonly DecimalDataType Instance = new DecimalDataType();

		/// <summary>
		/// Decimal Data Type (xs:decimal)
		/// </summary>
		public DecimalDataType()
			: this("xs:decimal")
		{
		}

		/// <summary>
		/// Decimal Data Type (xs:decimal)
		/// </summary>
		/// <param name="DataType">Data Type</param>
		public DecimalDataType(string DataType)
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
			if (CommonTypes.TryParse(Value, out decimal Result))
				return Result;
			else
				return null;
		}
	}
}
