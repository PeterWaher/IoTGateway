using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.DataForms.DataTypes
{
	/// <summary>
	/// Short Data Type (xs:short)
	/// </summary>
	public class ShortDataType : DataType
	{
		/// <summary>
		/// Public instance of data type.
		/// </summary>
		public static readonly ShortDataType Instance = new ShortDataType();

		/// <summary>
		/// Short Data Type (xs:short)
		/// </summary>
		public ShortDataType()
			: this("xs:short")
		{
		}

		/// <summary>
		/// Short Data Type (xs:short)
		/// </summary>
		/// <param name="TypeName">Type Name</param>
		public ShortDataType(string DataType)
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
			short Result;

			if (short.TryParse(Value, out Result))
				return Result;
			else
				return null;
		}
	}
}
