using System;

namespace Waher.Networking.XMPP.DataForms.DataTypes
{
	/// <summary>
	/// Byte Data Type (xs:byte)
	/// </summary>
	public class ByteDataType : DataType
	{
		/// <summary>
		/// Public instance of data type.
		/// </summary>
		public static readonly ByteDataType Instance = new ByteDataType();

		/// <summary>
		/// Byte Data Type (xs:byte)
		/// </summary>
		public ByteDataType()
			: this("xs:byte")
		{
		}

		/// <summary>
		/// Byte Data Type (xs:byte)
		/// </summary>
		/// <param name="DataType">Data Type</param>
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
			if (sbyte.TryParse(Value, out sbyte Result))
				return Result;
			else
				return null;
		}
	}
}
