using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.DataForms.DataTypes
{
	/// <summary>
	/// String Data Type (xs:string)
	/// </summary>
	public class StringDataType : DataType
	{
		/// <summary>
		/// String Data Type (xs:string)
		/// </summary>
		/// <param name="TypeName">Type Name</param>
		public StringDataType(string DataType)
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
			return Value;
		}
	}
}
