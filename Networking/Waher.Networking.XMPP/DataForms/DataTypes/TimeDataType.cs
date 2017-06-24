using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.DataForms.DataTypes
{
	/// <summary>
	/// Time Data Type (xs:time)
	/// </summary>
	public class TimeDataType : DataType
	{
		/// <summary>
		/// Public instance of data type.
		/// </summary>
		public static readonly TimeDataType Instance = new TimeDataType();

		/// <summary>
		/// Time Data Type (xs:time)
		/// </summary>
		public TimeDataType()
			: this("xs:time")
		{
		}

		/// <summary>
		/// Time Data Type (xs:time)
		/// </summary>
		/// <param name="DataType">Data Type</param>
		public TimeDataType(string DataType)
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
			if (TimeSpan.TryParse(Value, out TimeSpan Result))
				return Result;
			else
				return null;
		}
	}
}
