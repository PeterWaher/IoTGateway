using System;
using Waher.Content;

namespace Waher.Networking.XMPP.DataForms.DataTypes
{
	/// <summary>
	/// Boolean Data Type (xs:boolean)
	/// </summary>
	public class BooleanDataType : DataType
	{
		/// <summary>
		/// Public instance of data type.
		/// </summary>
		public static readonly BooleanDataType Instance = new BooleanDataType();

		/// <summary>
		/// Boolean Data Type (xs:boolean)
		/// </summary>
		public BooleanDataType()
			: this("xs:boolean")
		{
		}

		/// <summary>
		/// Boolean Data Type (xs:boolean)
		/// </summary>
		/// <param name="DataType">Data Type</param>
		public BooleanDataType(string DataType)
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
			if (CommonTypes.TryParse(Value, out bool Result))
				return Result;
			else
				return null;
		}
	}
}
