using System;

namespace Waher.Networking.XMPP.DataForms.DataTypes
{
	/// <summary>
	/// Long Data Type (xs:long)
	/// </summary>
	public class LongDataType : DataType
	{
		/// <summary>
		/// Public instance of data type.
		/// </summary>
		public static readonly LongDataType Instance = new LongDataType();

		/// <summary>
		/// Long Data Type (xs:long)
		/// </summary>
		public LongDataType()
			: this("xs:long")
		{
		}

		/// <summary>
		/// Long Data Type (xs:long)
		/// </summary>
		/// <param name="DataType">Data Type</param>
		public LongDataType(string DataType)
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
			if (long.TryParse(Value, out long Result))
				return Result;
			else
				return null;
		}
	}
}
