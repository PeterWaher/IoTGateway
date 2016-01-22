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
		/// Double Data Type (xs:double)
		/// </summary>
		/// <param name="TypeName">Type Name</param>
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
			double Result;

			if (CommonTypes.TryParse(Value, out Result))
				return Result;
			else
				return null;
		}
	}
}
