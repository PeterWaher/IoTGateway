using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.DataForms.DataTypes
{
	/// <summary>
	/// DateTime Data Type (xs:dateTime)
	/// </summary>
	public class DateTimeDataType : DataType
	{
		/// <summary>
		/// Public instance of data type.
		/// </summary>
		public static readonly DateTimeDataType Instance = new DateTimeDataType();

		/// <summary>
		/// DateTime Data Type (xs:dateTime)
		/// </summary>
		public DateTimeDataType()
			: this("xs:dateTime")
		{
		}

		/// <summary>
		/// DateTime Data Type (xs:dateTime)
		/// </summary>
		/// <param name="DataType">Data Type</param>
		public DateTimeDataType(string DataType)
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
			int i = Value.IndexOf('T');

			if (i == 10)
			{
				DateTime? DT = DateDataType.ParseDate(Value.Substring(0, 10));
				
				if (DT.HasValue && TimeSpan.TryParse(Value.Substring(11), out TimeSpan TS))
					return DT.Value + TS;
			}

			if (DateTime.TryParse(Value, out DateTime DT2))
				return DT2;
			else
				return null;
		}
	}
}
