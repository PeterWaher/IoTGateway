using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.DataForms.DataTypes
{
	/// <summary>
	/// Long Data Type (xs:long)
	/// </summary>
	public class LongDataType : DataType
	{
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
		/// <param name="TypeName">Type Name</param>
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
			long Result;

			if (long.TryParse(Value, out Result))
				return Result;
			else
				return null;
		}
	}
}
