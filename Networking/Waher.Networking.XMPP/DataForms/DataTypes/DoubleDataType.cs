using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;

namespace Waher.Networking.XMPP.DataForms.DataTypes
{
	/// <summary>
	/// Double Data Type (xs:double)
	/// </summary>
	public class DoubleDataType : DataType
	{
		/// <summary>
		/// Public instance of data type.
		/// </summary>
		public static readonly DoubleDataType Instance = new DoubleDataType();

		/// <summary>
		/// Double Data Type (xs:double)
		/// </summary>
		public DoubleDataType()
			: this("xs:double")
		{
		}

		/// <summary>
		/// Double Data Type (xs:double)
		/// </summary>
		/// <param name="DataType">Data Type</param>
		public DoubleDataType(string DataType)
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
			if (CommonTypes.TryParse(Value, out double Result))
				return Result;
			else
				return null;
		}
	}
}
