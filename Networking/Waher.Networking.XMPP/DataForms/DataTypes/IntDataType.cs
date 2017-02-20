using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.DataForms.DataTypes
{
	/// <summary>
	/// Int Data Type (xs:int)
	/// </summary>
	public class IntDataType : DataType
	{
		/// <summary>
		/// Public instance of data type.
		/// </summary>
		public static readonly IntDataType Instance = new IntDataType();

		/// <summary>
		/// Int Data Type (xs:int)
		/// </summary>
		public IntDataType()
			: this("xs:int")
		{
		}

		/// <summary>
		/// Int Data Type (xs:int)
		/// </summary>
		/// <param name="TypeName">Type Name</param>
		public IntDataType(string DataType)
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
			int Result;

			if (int.TryParse(Value, out Result))
				return Result;
			else
				return null;
		}
	}
}
